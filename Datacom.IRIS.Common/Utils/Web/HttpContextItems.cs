using System;
using System.ServiceModel;
using System.Web;

namespace Datacom.IRIS.Common.Utils.Web
{
    /// <summary>
    ///    The purpose of this class is to store the returned value of an action into
    ///    the HTTPContext Items hashtable, and always return the cached object on 
    ///    every subsequent call in the future
    /// </summary>
    public static class HttpContextItems
    {
        public static T Get<T>(string key, Func<T> action)
        {
            // This is expected to be null when WCF makes a call to this method
            if (HttpContext.Current == null)
            {
                return action.Invoke();
            }

            // This is called by the Web project
            object obj = HttpContext.Current.Items[key];
            if (obj == null)
            {
                HttpContext.Current.Items[key] = action.Invoke();
            }

            return (T)HttpContext.Current.Items[key];
        }
    }
}
