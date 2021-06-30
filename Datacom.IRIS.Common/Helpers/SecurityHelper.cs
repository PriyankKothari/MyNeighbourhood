using System;
using System.ServiceModel;
using System.Web;
using Datacom.IRIS.Common.Implementations;
using Datacom.IRIS.Common.Utils;

namespace Datacom.IRIS.Common
{
    /// <summary>
    /// Contains helper methods and properties related to the User identity
    /// </summary>
    public class SecurityHelper
    {

        #region Logged in user details

        /// <summary>
        ///    This method returns the name of the current logged in user. It does so by looking in three
        ///    different objects:
        ///    (a) UserContext - this is a special class that is create specifically for use with unit tests.
        ///        Any unit test will need to wrap its logic within it to impersoniate the desier user running
        ///    (b) ServiceSecurityContext - this object is populated only if impersonation is turned on and the
        ///        business layer is being access from behind a WCF tier
        ///    (c) HTTPContext - this object is populated only if the webapp is talking directly to the business tier
        /// </summary>
        public static string CurrentUserName
        {
            get
            {
                return UserContext.Current.Eval(x => x.CurrentUserName) // Set by our unit tests
                    ?? ServiceSecurityContext.Current.Eval(x => x.WindowsIdentity).Eval(x => x.Name) // Set by WCF only
                    ?? HttpContext.Current.Eval(x => x.User.Identity).Eval(x => x.Name); // Set by web app
            }
        }

        /// <summary>
        /// Gets the logged in user's account name without domain like goutamg
        /// </summary>
        public static string LoggedInUserAccountName
        {
            get { return CurrentUserName.Substring(CurrentUserName.LastIndexOf('\\') + 1); }
        }

        /// <summary>
        /// Gets the logged in user's domain 
        /// </summary>
        public static string LoggedInUserDomain
        {
            get { return CurrentUserName.Substring(0, CurrentUserName.IndexOf("\\")); }
        }

        #endregion
       
    }
}
