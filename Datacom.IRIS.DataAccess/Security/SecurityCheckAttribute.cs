using System;
using Datacom.IRIS.Common.Utils;

namespace Datacom.IRIS.DataAccess.Security
{
    /// <summary>
    /// Specifies how data security access should be enforced for a method
    /// </summary>
    public enum SecurityCheckMethod
    {
        /// <summary>
        ///    Default check: user permissions will be checked against
        ///     - the type of data returned by the protected method (for pre-retrieval checks)
        ///     - the actual data returned for post-retrieval checks
        /// </summary>
        Default,

        /// <summary>
        ///    When used, a custom security check method (specified by the user) will be called
        /// </summary>
        Custom,

        /// <summary>
        ///    Access is allowed if the user has access to a given function/permission
        /// </summary>
        FunctionPermission,

        /// <summary>
        ///    Access is allowed if the user has the speficic permission name for the type
        ///    of data that is being returned
        /// </summary>
        Permission,

        /// <summary>
        ///  Access is allowed if the user has access to the IRISIObject with IRISObjectId specified in the input parameter
        /// </summary>
        IRISObject,


        /// <summary>
        ///  Access is allowed if the user has access to the IRISObject with specific ObjectTypeCode and LinkId
        /// </summary>
        IRISObjectViaLinkID,

       
        /// <summary>
        ///    Access is allowed if the user has access to a given function/permission OR IRISObject
        /// </summary>
        FunctionPermissionOrIRISObject,

        /// <summary>
        ///    No check is going to be performed: the data is unprotected
        /// </summary>
        None

    }

    /// <summary>
    /// Specifies when the check defined by the associated SecurityCheck attribute should be applied (before retrieving data or after)
    /// </summary>
    public enum CheckWhen
    {
        /// <summary>
        /// Check before retrieving the data
        /// </summary>
        Before,
        /// <summary>
        /// Check after retrieving the data
        /// </summary>
        After
    }

    /// <summary>
    /// This attribute specify how the data returned by a method is protected against non-permtted user access and 
    /// should decorate all methods in the DomainRepositoryStore.
    /// 
    /// Refer to the AuthorisationManager in the business layer to see how this attribute is used
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SecurityCheckAttribute : Attribute
    {
        /// <summary>
        /// Specifies when the check defined this attribute should be applied (before retrieving data or after)
        /// </summary>
        public CheckWhen CheckWhen { get; set; }

        /// <summary>
        /// Defines the access to the associated method is going to be validated.
        /// </summary>
        public SecurityCheckMethod CheckMethod { get; set; }

        /// <summary>
        /// Gets or sets the name of the method to invoke before calling the associated method.
        /// This only applies if the CheckMethod property is set to Custom
        /// </summary>
        public string MethodToInvoke { get; set; }

        /// <summary>
        /// The function that the user must have to access the data (applicable if CheckMethod is FunctionPermission only)
        /// </summary>
        public string FunctionName { get; set; }

        /// <summary>
        /// The permission that the user must have to access the data (applicable if CheckMethod is FunctionPermission or CheckMethod is IRISObject)
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// The permission that the user must have on the IRISObject to access the data (only applicable if CheckMethod is FunctionPermissionORIRISObject)
        /// </summary>
        public string IRISObjectPermissionName { get; set; }

        /// <summary>
        /// The permission that the user must have on the specified admin Function to access the data (only applicable if CheckMethod is FunctionPermissionORIRISObject)
        /// </summary>
        public string FunctionPermissionName { get; set; }        

        /// <summary>
        /// IRIS Object ID parameter name.  This is used for doing pre-retreival security check to see if user has access to the IRISObject (applicable if CheckMethod is IRISObject)
        /// </summary>
        public string IRISObjectIDParameterName { get; set; }
        
        /// <summary>
        /// IRISObject.LinkID parameter name.   This is used for doing pre-retrieval security check to see if user has access to the IRISObject (applicable if CheckMethod is IRISObjectWithLinkID) 
        /// </summary>
        public string LinkIDParameterName { get; set; }

        /// <summary>
        /// IRISObject.ObjectTypeREF.Code. This is used for doing pre-retrieval security check to see if user has access to the IRISObject (applicable if CheckMethod is IRISObjectWithLinkID) 
        /// or if user has access to the ObjectType (applicable if CheckMethod is Default or Permission)
        /// </summary>
        public string ObjectTypeCode { get; set; }
        
        /// <summary>
        /// Object Type ID parameter name.  This is used for doing pre-retrieval security check to see if user has access to the ObjectType (appliable if CheckMethod is Default or Permission)
        /// </summary>
        public string ObjectTypeIDParameterName { get; set; }

        public override string ToString()
        {
            string result = string.Format("CheckWhen: '{0}', ", CheckWhen);
            result += string.Format("CheckMethod: '{0}'", CheckMethod);
            result += MethodToInvoke.Eval(c => string.Format(", MethodToInvoke: '{0}'", c));
            result += FunctionName.Eval(c => string.Format(", FunctionName: '{0}'", c));
            result += PermissionName.Eval(c => string.Format(", PermissionName: '{0}'", c));
            result += IRISObjectIDParameterName.Eval(c => string.Format(", IRISObjectIDParameterName: '{0}'", c));
            result += LinkIDParameterName.Eval(c => string.Format(", LinkIDParameterName: '{0}'", c));
            result += ObjectTypeCode.Eval(c => string.Format(", ObjectTypeCode: '{0}'", c));
            result += ObjectTypeIDParameterName.Eval(c => string.Format(", ObjectTypeIDParameterName: '{0}'", c));

            return string.Format("[{0}]", result);
        }
    }
}