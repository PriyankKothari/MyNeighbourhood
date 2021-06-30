using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Configuration;

namespace Datacom.IRIS.Common.Utils
{
    public static class GlobalUtils
    {
        public static string AssemblyVersionNumber()
        {
            // Use the AssemblyFileVersion as this includes additional informationn on CI builds and is always incremented (AssemblyVersion will not be incremented for some files)
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            AssemblyFileVersionAttribute versionAttribute = (AssemblyFileVersionAttribute)Attribute.GetCustomAttribute(executingAssembly, typeof(AssemblyFileVersionAttribute));
            return versionAttribute.Version;
        }

        /// <summary>
        ///    Finds all method attributes inside a given Type and returns a list of 
        ///    tuples. Key for each tuple is the attribute instance and value is a
        ///    reference to its method.
        /// </summary>
        public static IEnumerable<Tuple<T, MethodInfo>> GetAttributeDecoratedMethods<T>(Type targetClass) where T : Attribute
        {
            MethodInfo[] methods = targetClass.GetMethods();
            foreach (MethodInfo method in methods)
            {
                T councilAttribute = Attribute.GetCustomAttribute(method, typeof(T), false) as T;
                if (councilAttribute != null)
                {
                    yield return new Tuple<T, MethodInfo>(councilAttribute, method);
                }
            }
        }

        /// <summary>
        ///    Returns true if object is an instance of a given type. This method is
        ///    useful to match Types or test if an object implements a given interface.
        /// </summary>
        public static bool IsInstanceOfType<T>(this T obj, Type type) where T : class
        {
            Type objectInstanceType = obj.GetType();
            return type.IsAssignableFrom(objectInstanceType);
        }

        public static int? ToNullableInt32(this string s)
        {
            int i;
            if (Int32.TryParse(s, out i)) return i;
            return null;
        }

        public static long? ToNullableInt64(this string s)
        {
            long i;
            if (Int64.TryParse(s, out i)) return i;
            return null;
        }

        public static double? ToNullableDouble(this string s)
        {
            double i;
            if (string.IsNullOrEmpty(s))
                return null;
            if (Double.TryParse(s, out i)) return i;
                return double.MinValue;
        }

        public static int ToInteger(this string s)
        {
            int i;
            return int.TryParse(s, out i) ? i : int.MinValue;
        }

        public static long ToLong(this string s)
        {
            long i;
            return long.TryParse(s, out i) ? i : long.MinValue;
        }

        public static bool ToBool(this string s)
        {
            bool i;
            return bool.TryParse(s, out i) && i;
        }

        public static string GetAppSettingsValue(string webConfigKey)
        {
            return WebConfigurationManager.AppSettings[webConfigKey];
        }

        public static bool GetAppSettingsValueAsBoolean(string webConfigKey)
        {
            bool result;
            Boolean.TryParse(GetAppSettingsValue(webConfigKey), out result);
            return result;
        }
    }
}
