using System;
using System.Collections;
using System.Reflection;

namespace Datacom.IRIS.Common
{
    public static class EnumUtils
    {
        public static T StringToEnum<T>(string name)
        {
            return (T)Enum.Parse(typeof(T), name);
        }
    }
}