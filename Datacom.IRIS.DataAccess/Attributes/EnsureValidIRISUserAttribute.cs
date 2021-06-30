using System;

namespace Datacom.IRIS.DataAccess.Attributes
{
    /// <summary>
    /// When this attribute is present, the auto-generated business layer will ensure the call is made from a valid IRISUser.
    /// This attribute is only relevant if security check method is set to none for both CheckWhen.Before and CheckWhen.After
    /// For other types of security check method, IRISUser identity is always checked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class EnsureValidIRISUserAttribute : Attribute
    {
    }
}