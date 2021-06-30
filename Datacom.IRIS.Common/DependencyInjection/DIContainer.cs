using Unity;

namespace Datacom.IRIS.Common.DependencyInjection
{
    public class DIContainer
    {
        private static IUnityContainer _container;

        public static IUnityContainer Container
        {
            get
            {
                if (_container == null)
                    _container = new UnityContainer();

                return _container;
            }
            set
            {
                _container = value;
            }
        }
    }
}
