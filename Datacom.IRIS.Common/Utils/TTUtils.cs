using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace Datacom.IRIS.Common.Utils
{
    public static class TTUtils
    {
        public static string NaturalReturnName(this Type type)
        {
            return type.ToString().NaturalReturnName();
        }

        public static string NaturalReturnName(this string typeName)
        {
            return typeName
                     .Replace("`1", "").Replace("`2", "").Replace("`3", "")
                     .Replace("[", "<").Replace("]", ">")
                     .Replace("<>", "[]").Trim().Trim(',');
        }

        /// <summary>
        ///    This method will compile a ParameterInfo[] object into a flat string to be used
        ///    typically in a TT template that is trying to generate code on the fly. Examples
        ///    of outputs:
        ///      (a) string variable1, string variable2
        ///      (b) params string[] variable1
        /// </summary>
        public static string ParametersListAsString(this ParameterInfo[] pars, bool includeType = true)
        {
            string parameters = string.Empty;
            
            foreach (ParameterInfo param in pars)
            {
                bool isParams = Attribute.IsDefined(param, typeof(ParamArrayAttribute));
                parameters = parameters + string.Format("{0}{1}{2}, ", 
                    isParams && includeType ? "params " : string.Empty,
                    includeType ? param.ParameterType + " " : string.Empty, 
                    param.Name);
            }
            
            return parameters.NaturalReturnName();
        }

        /// <summary>
        ///    This method will compile a ParameterInfo[] object into an List of parameter to be used
        ///    typically in a TT template that is trying to generate code on the fly. Examples
        ///    of outputs:
        ///      (a) string variable1, string variable2
        ///      (b) params string[] variable1
        /// </summary>
        public static List<string> ParametersList(this ParameterInfo[] pars, bool includeType = true)
        {
            List<string> parameters = new List<string>();

            foreach (ParameterInfo param in pars)
            {
                bool isParams = Attribute.IsDefined(param, typeof(ParamArrayAttribute));
                parameters.Add( string.Format("{0}{1}{2}",
                    isParams && includeType ? "params " : string.Empty,
                    includeType ? param.ParameterType + " " : string.Empty,
                    param.Name).NaturalReturnName());
            }

            return parameters;
        }


        public static bool IsVoid(this MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof (void);
        }
    }
}
