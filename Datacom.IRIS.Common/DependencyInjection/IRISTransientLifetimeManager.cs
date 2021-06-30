using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Web;
using Unity.Lifetime;

namespace Datacom.IRIS.Common.DependencyInjection
{
    /// <summary>
    /// Inherit from TransientLifetimeManager but keep track of the disposable items for later disposal.
    /// IRIS use this to keep track of the ObjectContext items so that we can call a disposable at end of a HttpRequest/AsyncCallback
    /// </summary>
    public class IRISDisposableTransientLifetimeManager : TransientLifetimeManager
    {
        private const string TransientDisposableItemsKey = "TransientDisposableItems";

        private static List<IDisposable> DisposableTransientItems
        {
            get
            {
                if (HttpContext.Current == null)
                {
                    //Store in CallContext if HttpContext is unavailable
                    var transientItems = CallContext.GetData(TransientDisposableItemsKey) as List<IDisposable>;
                    if (transientItems == null)
                    {
                        transientItems = new List<IDisposable>();
                        CallContext.SetData(TransientDisposableItemsKey, transientItems);
                    }
                    return transientItems;
                }
                else
                {
                    if (HttpContext.Current.Items[TransientDisposableItemsKey] == null)
                        HttpContext.Current.Items[TransientDisposableItemsKey] = new List<IDisposable>();
                    return HttpContext.Current.Items[TransientDisposableItemsKey] as List<IDisposable>;
                }
            }
        }


        public override void SetValue(object newValue, ILifetimeContainer container = null)
        {
            base.SetValue(newValue);

            if (newValue is IDisposable)
            {
                DisposableTransientItems.Add(newValue as IDisposable);  //if the item is disposable, keep a reference of it for later disposal 
            }
        }

        /// <summary>
        /// This is called in Application_EndRequest() in asp.net or in the async callback method to dispose the transient items
        /// </summary>
        public static void DisposeTransientItems()
        {
            if (DisposableTransientItems != null)
            {
                DisposableTransientItems.ForEach(disposable => disposable.Dispose());
            }
        }


    }
}
