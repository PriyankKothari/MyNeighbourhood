using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Datacom.IRIS.Common.Interfaces;

namespace Datacom.IRIS.Common.Implementations
{
    /// <summary>
    ///  The Composite Application Library provides an event mechanism that enables communications 
    ///  between loosely coupled components in the application. This mechanism, based on the event aggregator service, 
    ///  allows publishers and subscribers to communicate through events and still do not have a direct 
    ///  reference to each other.
    /// </summary>
  
    public abstract class BaseEvent
    {
        private readonly List<IEventSubscription> _subscriptions = new List<IEventSubscription>();

        protected ICollection<IEventSubscription> Subscriptions
        {
            get
            {
                return _subscriptions.AsReadOnly();
            }
        }

        protected virtual SubscriptionToken Subscribe(IEventSubscription eventSubscription)
        {
            eventSubscription.SubscriptionToken = new SubscriptionToken();

            lock (_subscriptions)
            {
                _subscriptions.Add(eventSubscription);
            }

            return eventSubscription.SubscriptionToken;
        }

        protected virtual void Publish(params object[] arguments)
        {
            // some times more than one event subscription, which MAY be the cause for random overlay (IRIS-4408)
            // limit the number to one so that overlay is only opened once.
            //var execution = PruneAndReturnStrategies().FirstOrDefault();

            //if (execution != null)
            //{
            //    execution(arguments);
            //}


            // revert back to pre-IRIS-4408 with some debugging info
            List<Action<object[]>> executionStrategies = PruneAndReturnStrategies();
            UtilLogger.Instance.LogInfo(string.Format("EventAggregator Published with {0}, number of stratergies: {1}", arguments[0], executionStrategies.Count));

            foreach (var executionStrategy in executionStrategies)
            {
                executionStrategy(arguments);
            }
        }

        public virtual void Unsubscribe(SubscriptionToken token)
        {
            lock (_subscriptions)
            {
                IEventSubscription subscription = _subscriptions.FirstOrDefault(evt => evt.SubscriptionToken == token);

                if (subscription != null)
                {
                    _subscriptions.Remove(subscription);
                }
            }
        }

        public virtual bool Contains(SubscriptionToken token)
        {
            lock (_subscriptions)
            {
                IEventSubscription subscription = _subscriptions.FirstOrDefault(evt => evt.SubscriptionToken == token);

                return (subscription != null);
            }
        }

        private List<Action<object[]>> PruneAndReturnStrategies()
        {
            List<Action<object[]>> returnList = new List<Action<object[]>>();

            lock (_subscriptions)
            {
                for (int i = _subscriptions.Count - 1; i >= 0; i--)
                {
                    Action<object[]> listItem = _subscriptions[i].GetExecutionStrategy();

                    if (listItem == null)
                    {
                        _subscriptions.RemoveAt(i);
                    }
                    else
                    {
                        returnList.Add(listItem);
                    }
                }
            }

            return returnList;
        }
    }

    public abstract class BaseEvent<TPayload> : BaseEvent
    {
        public virtual SubscriptionToken Subscribe(Action<TPayload> action)
        {
            return Subscribe(action, false);
        }

        public virtual SubscriptionToken Subscribe(Action<TPayload> action, bool keepSubscriberReferenceAlive)
        {
            return Subscribe(action, keepSubscriberReferenceAlive, delegate { return true; });
        }

        public virtual SubscriptionToken Subscribe(Action<TPayload> action, bool keepSubscriberReferenceAlive, Predicate<TPayload> filter)
        {
            IDelegateReference actionReference = new DelegateReference(action, keepSubscriberReferenceAlive);
            IDelegateReference filterReference = new DelegateReference(filter, keepSubscriberReferenceAlive);

            EventSubscription<TPayload> subscription = new EventSubscription<TPayload>(actionReference, filterReference);

            return base.Subscribe(subscription);
        }

        public virtual void Publish(TPayload payload)
        {
            base.Publish(payload);
        }

        public virtual void Unsubscribe(Action<TPayload> subscriber)
        {
            lock (Subscriptions)
            {
                IEventSubscription eventSubscription = Subscriptions.Cast<EventSubscription<TPayload>>().FirstOrDefault(evt => evt.Action == subscriber);

                if (eventSubscription != null)
                {
                    Subscriptions.Remove(eventSubscription);
                }
            }
        }

        public virtual bool Contains(Action<TPayload> subscriber)
        {
            IEventSubscription eventSubscription;

            lock (Subscriptions)
            {
                eventSubscription = Subscriptions.Cast<EventSubscription<TPayload>>().FirstOrDefault(evt => evt.Action == subscriber);
            }

            return eventSubscription != null;
        }
    }

    public  class EventAggregator : IEventAggregator
    {
        private readonly List<BaseEvent> _events = new List<BaseEvent>();
        private readonly ReaderWriterLockSlim _rwl = new ReaderWriterLockSlim();

        public TEventType GetEvent<TEventType>() where TEventType : BaseEvent
        {
            _rwl.EnterUpgradeableReadLock();

            try
            {
                TEventType eventInstance = _events.SingleOrDefault(evt => evt.GetType() == typeof(TEventType)) as TEventType;

                if (eventInstance == null)
                {
                    _rwl.EnterWriteLock();

                    try
                    {
                        eventInstance = _events.SingleOrDefault(evt => evt.GetType() == typeof(TEventType)) as TEventType;

                        if (eventInstance == null)
                        {
                            eventInstance = Activator.CreateInstance<TEventType>();
                            _events.Add(eventInstance);
                        }
                    }
                    finally
                    {
                        _rwl.ExitWriteLock();
                    }
                }

                return eventInstance;
            }
            finally
            {
                _rwl.ExitUpgradeableReadLock();
            }
        }
    }

    public class SubscriptionToken : IEquatable<SubscriptionToken>
    {
        private readonly Guid _token = Guid.NewGuid();

        public bool Equals(SubscriptionToken other)
        {
            return (other != null) && Equals(_token, other._token);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || Equals(obj as SubscriptionToken);
        }

        public override int GetHashCode()
        {
            return _token.GetHashCode();
        }

        public override string ToString()
        {
            return _token.ToString();
        }
    }

    public class DelegateReference : IDelegateReference
    {
        private readonly Delegate _delegate;
        private readonly WeakReference _weakReference;
        private readonly MethodInfo _method;
        private readonly Type _delegateType;

        public DelegateReference(Delegate @delegate, bool keepReferenceAlive)
        {
            if (@delegate == null)
            {
                throw new ArgumentNullException("delegate");
            }

            if (keepReferenceAlive)
            {
                _delegate = @delegate;
            }
            else
            {
                _weakReference = new WeakReference(@delegate.Target);
                _method = @delegate.Method;
                _delegateType = @delegate.GetType();
            }
        }

        public Delegate Target
        {
            get
            {
                return _delegate ?? TryGetDelegate();
            }
        }

        private Delegate TryGetDelegate()
        {
            if (_method.IsStatic)
            {
                return Delegate.CreateDelegate(_delegateType, null, _method);
            }

            object target = _weakReference.Target;

            return (target != null) ? Delegate.CreateDelegate(_delegateType, target, _method) : null;
        }
    }

    public class EventSubscription<TPayload> : IEventSubscription
    {
        private readonly IDelegateReference _actionReference;
        private readonly IDelegateReference _filterReference;

        public EventSubscription(IDelegateReference actionReference, IDelegateReference filterReference)
        {
            if (actionReference == null)
            {
                throw new ArgumentNullException("actionReference");
            }

            if (filterReference == null)
            {
                throw new ArgumentNullException("filterReference");
            }

            if (!(actionReference.Target is Action<TPayload>))
            {
                throw new ArgumentException("Invalid delegate rerefence type.", "actionReference");
            }

            if (!(filterReference.Target is Predicate<TPayload>))
            {
                throw new ArgumentException("Invalid delegate rerefence type.", "filterReference");
            }

            _actionReference = actionReference;
            _filterReference = filterReference;
        }

        public Action<TPayload> Action
        {
            get
            {
                return (Action<TPayload>)_actionReference.Target;
            }
        }

        public Predicate<TPayload> Filter
        {
            get
            {
                return (Predicate<TPayload>)_filterReference.Target;
            }
        }

        public SubscriptionToken SubscriptionToken
        {
            get;
            set;
        }

        public virtual Action<object[]> GetExecutionStrategy()
        {
            Action<TPayload> action = Action;
            Predicate<TPayload> filter = Filter;

            if (action != null && filter != null)
            {
                return arguments =>
                {
                    TPayload argument = default(TPayload);

                    if (arguments != null && arguments.Length > 0 && arguments[0] != null)
                    {
                        argument = (TPayload)arguments[0];
                    }

                    if (filter(argument))
                    {
                        InvokeAction(action, argument);
                    }
                };
            }

            return null;
        }

        protected virtual void InvokeAction(Action<TPayload> action, TPayload argument)
        {
            action(argument);
        }
    }
}
