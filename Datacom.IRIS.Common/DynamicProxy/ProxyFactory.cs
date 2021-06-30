using Castle.DynamicProxy;
using System.Security.Principal;

namespace Datacom.IRIS.Common.DynamicProxy
{
    public static class ProxyFactory<T> where T : class
    {
        public static T CreateClientAllowDelegation()
        {
            return Create(TokenImpersonationLevel.Delegation);
        }

        public static T Create(TokenImpersonationLevel allowedImpersonationLevel = TokenImpersonationLevel.Impersonation) //default is to allow impersonation (to be consistent with WcfInvocationInterceptor)
        {
            var proxyGenerator = new ProxyGenerator();
            var wcfClient = new WcfClient<T>();
            var interceptor = new WcfInvocationInterceptor<T>(wcfClient, allowedImpersonationLevel);
            return proxyGenerator.CreateInterfaceProxyWithoutTarget<T>(interceptor);
        }
    }
}