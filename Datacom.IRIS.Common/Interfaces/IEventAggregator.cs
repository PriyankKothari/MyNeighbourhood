using Datacom.IRIS.Common.Implementations;

namespace Datacom.IRIS.Common.Interfaces
{
    using System;

    public interface IEventSubscription
    {
        SubscriptionToken SubscriptionToken { get; set; }

        Action<object[]> GetExecutionStrategy();
    }

    public interface IEventAggregator
    {
        TEventType GetEvent<TEventType>() where TEventType : BaseEvent;
    }

    public interface IDelegateReference
    {
        Delegate Target
        {
            get;
        }
    }
}
