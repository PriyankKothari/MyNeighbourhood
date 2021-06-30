using System;

namespace Datacom.IRIS.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class DoNotGenerateBusinessWrapperAttribute : Attribute
    {
        
    }
}