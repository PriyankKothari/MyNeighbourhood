using System.Security.Principal;
using Castle.DynamicProxy;

namespace Datacom.IRIS.Common.DynamicProxy
{
    public class WcfInvocationInterceptor<T> : IInterceptor where T : class
    {
        private readonly WcfClient<T> _wcfClient;

        /// <summary>
        ///    Default factory constructor will always create a WCF client that allows 
        ///    for impersonation to occur
        /// </summary>
        public WcfInvocationInterceptor(WcfClient<T> wcfClient) : this(wcfClient, TokenImpersonationLevel.Impersonation)
        {
            // Do nothing...
        }

        public WcfInvocationInterceptor(WcfClient<T> wcfClient, TokenImpersonationLevel allowedImpersonationLevel)
        {
            _wcfClient = wcfClient;
            _wcfClient.ChannelFactory.Credentials.Windows.AllowedImpersonationLevel = allowedImpersonationLevel;
        }

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            invocation.ReturnValue = _wcfClient.Invoke(invocation.Method.Name, invocation.Arguments);
        }

        #endregion
    }
}