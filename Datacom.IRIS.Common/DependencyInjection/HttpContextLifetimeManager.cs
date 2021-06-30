using System;
using System.Web;
using Unity.Lifetime;

namespace Datacom.IRIS.Common.DependencyInjection
{
    public class HttpContextLifetimeManager : LifetimeManager, IDisposable
    {
        private readonly string _itemName = "HttpContextLifetimeManager_" + Guid.NewGuid();

        public override void RemoveValue(ILifetimeContainer container = null)
        {
            var disposable = GetValue() as IDisposable;
            HttpContext.Current.Items.Remove(_itemName);

            if (disposable != null)
                disposable.Dispose();
        }

        public override void SetValue(object newValue, ILifetimeContainer container = null)
        {
            HttpContext.Current.Items[_itemName] = newValue;
        }

        public void Dispose()
        {
            RemoveValue();
        }

        public override object GetValue(ILifetimeContainer container = null)
        {
            return HttpContext.Current.Items[_itemName];
        }

        protected override LifetimeManager OnCreateLifetimeManager()
        {
            return new HttpContextLifetimeManager();
        }
    }
}
