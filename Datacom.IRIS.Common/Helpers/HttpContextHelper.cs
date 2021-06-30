using System;
using System.Web;
using System.Net;

namespace Datacom.IRIS.Common.Helpers
{
    public static class HttpContextHelper
    {
        public static void ClearCookie(string cookieName, bool onlyClearIfCookieExists)
        {
            if (!onlyClearIfCookieExists || 
                HttpContext.Current.Request.Cookies[cookieName] != null)
            {
                //Set cookie to expire will in effect clear the cookie
                HttpContext.Current.Response.Cookies[cookieName].Expires = DateTime.Now.AddDays(-1);
            }
        }

    }
}
