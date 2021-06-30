namespace Datacom.IRIS.Common.DynamicProxy
{
    public class WcfClient<T> : System.ServiceModel.ClientBase<T> where T : class
    {
        public object Invoke(string methodName, params object[] parameters)
        {
            var clientMethod = base.Channel.GetType().GetMethod(methodName);
            return clientMethod == null ? null : clientMethod.Invoke(base.Channel, parameters);
        }
    }
}