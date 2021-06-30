using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

namespace Datacom.IRIS.Common.Utils
{
    public static class ReflectionUtils
    {
        /// <summary>
        /// Gets all customs attributes of type T from a Type, methodInfo, ...
        /// </summary>
        //public static T[] GetCustomAttributes<T>(this MemberInfo memberInfo) where T : Attribute
        //{
        //    return memberInfo.GetCustomAttributes(typeof(T), false).Cast<T>().ToArray();
        //}

        /// <summary>
        /// Gets custom attribute of type T from a Type.  
        /// If the return type is string and attribute does not exist then it returns the fieldName.
        /// Used for Constants and Static fields only.
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <typeparam name="TOut">return type</typeparam>
        /// <typeparam name="TAttribute">Attribute to be retrieved</typeparam>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="valueSelector">Function to </param>
        /// <returns></returns>
        public static TOut GetConstantFieldAttributeValue<T, TOut, TAttribute>(string fieldName, Func<TAttribute, TOut> valueSelector) where TAttribute : Attribute
        {
            var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            if (fieldInfo == null) return typeof(TOut) == typeof(string) ? (TOut)(object)fieldName :  default(TOut);
            var att = fieldInfo.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;
            return att != null ? valueSelector(att) : typeof(TOut) == typeof(string) ? (TOut)(object)fieldName :default(TOut);
        }

        public static Type[] GetTypesInNamespace(this Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => t.IsClass 
                                                    && t.IsPublic 
                                                    && String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)
                                            ).ToArray();
        }

        public static Type[] GetInterfaceTypesInNamespace(this Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => t.IsInterface
                                                    && t.IsPublic
                                                    && String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)
                                            ).ToArray();
        }


        public static string GetRecursiveMemberName(this MemberExpression expression)
        {
            string expr = string.Empty;

            while (expression != null)
            {
                expr = expression.Member.Name + (expr != string.Empty ? "." + expr : string.Empty);
                expression = expression.Expression as MemberExpression;
            }

            return expr;
        }
    }
}
