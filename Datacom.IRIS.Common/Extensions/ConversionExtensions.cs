using System;

namespace Datacom.IRIS.Common.Extensions
{
    public static class ConversionExtensions
    {
        public static T As<T>(this object obj) where T : IConvertible
        {
            return obj.As(default(T));
        }

        public static T As<T>(this object obj, T defaultValue) where T : IConvertible
        {
            try
            {
                string s = obj == null ? null : obj.ToString();
                if (s != null)
                {
                    var type = typeof(T);
                    var isEnum = typeof(Enum).IsAssignableFrom(type);
                    return (T)(isEnum ?
                        Enum.Parse(type, s, true)
                        : Convert.ChangeType(s, type));
                }
            }
            catch
            {
            }
            return defaultValue;
        }

        public static T? AsNullable<T>(this object obj) where T : struct, IConvertible
        {
            try
            {
                string s = obj == null ? null : obj.ToString();
                if (s != null)
                {
                    var type = typeof(T);
                    var isEnum = typeof(Enum).IsAssignableFrom(type);
                    return (T)(isEnum ?
                        Enum.Parse(type, s, true)
                        : Convert.ChangeType(s, type));
                }
            }
            catch
            {
            }
            return null;
        }         
    }
}