using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class UseCurrentUserInBusinessWrapperAttribute : Attribute
    {
    }
}
