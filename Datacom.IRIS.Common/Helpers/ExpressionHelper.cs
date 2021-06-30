using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;

namespace Datacom.IRIS.Common
{
    /// <summary>
    /// Helper to get Property and Path names as strings
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Get the property name as a string
        /// </summary>
        /// <typeparam name="T">Type of Object e.g. User</typeparam>
        /// <param name="func">An Expression containing the property reference e.g. u=>u.FirstName</param>
        /// <returns>The name of the property as a string e.g. "FirstName"</returns>
        public static string GetPropertyName<T>(Expression<Func<T, object>> func)
        {
            var body = func.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("'expression' should be a member expression");
            return body.Member.Name;
        }

        public static string GetMethodName<T>(Expression<Action<T>> action)
        {
            var body = action.Body as MethodCallExpression;
            if (body == null)
                return string.Empty;

            return body.Method.Name;
        }
        public static string GetMethodName<T>(Expression<Action<T>> action, out string parameter1Name)
        {
            parameter1Name = string.Empty;
            var body = action.Body as MethodCallExpression;
            if (body == null)
                return string.Empty;

            var parameters = body.Method.GetParameters();
            if (parameters.Length > 0)
                parameter1Name = parameters[0].Name;
            return body.Method.Name;
        }
        public static string GetMethodName<T, TOutput>(Expression<Func<T, TOutput>> action, out string parameter1Name)
        {
            parameter1Name = string.Empty;
            var body = action.Body as MethodCallExpression;
            if (body == null)
                return string.Empty;

            var parameters = body.Method.GetParameters();
            if (parameters.Length > 0)
                parameter1Name = parameters[0].Name;
            return body.Method.Name;
        }
        public static string GetMethodName<T>(Expression<Action<T>> action, out string parameter1Name, out string parameter2Name)
        {
            parameter1Name = parameter2Name = string.Empty;
            var body = action.Body as MethodCallExpression;
            if (body == null)
                return string.Empty;

            var parameters = body.Method.GetParameters();
            if (parameters.Length > 0)
                parameter1Name = parameters[0].Name;
            if (parameters.Length > 1)
                parameter2Name = parameters[1].Name;
            return body.Method.Name;
        }

        /// <summary>
        /// Gets the full path of a property as a string
        /// </summary>
        /// <typeparam name="T">>Type of Object e.g. User</typeparam>
        /// <param name="func">An Expression containing the property reference e.g. u=>u.Organisation.Name</param>
        /// <returns>>The path of the property as a string e.g. "Organisation.Name"</returns>
        public static string GetPathOfProperty<T>(Expression<Func<T, object>> func)
        {
            return GetPathOfProperty<T, object>(func);
        }
        public static string GetPathOfProperty<T, TResult>(Expression<Func<T, TResult>> func)
        {
            MemberExpression m = func.Body as MemberExpression;
            if (m == null && func.Body.NodeType == ExpressionType.Convert)
                m = (func.Body as UnaryExpression).Operand as MemberExpression;

            if (m == null)
            {
                return string.Empty;
            }

            return recurse(m);
        }

        /// <summary>
        /// Recurse through the expression building up the path
        /// </summary>
        /// <param name="m">The expression to recurse</param>
        /// <returns>the string of the path</returns>
        private static string recurse(MemberExpression m)
        {
            return m.Expression is MemberExpression ?
                recurse((MemberExpression)m.Expression) + "." + m.Member.Name :
                m.Member.Name;
        }

        public static bool SetValue<T>(string property, T context, object value)
        {
            object propertyContext = context;
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            foreach (string prop in props.Take(props.Length - 1))
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                type = pi.PropertyType;

                propertyContext = pi.GetValue(propertyContext, null);
                if (propertyContext == null)
                    return false;
            }

            // final property set...
            PropertyInfo finalProp = type.GetProperty(props.Last());
            finalProp.SetValue(propertyContext, value, null);

            return true;
        }

        public static Action<T, TValue> CreateSetter<T, TValue>(string property)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            ParameterExpression valArg = Expression.Parameter(typeof(TValue), "val");
            Expression expr = arg;
            foreach (string prop in props.Take(props.Length - 1))
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }

            // final property set...
            PropertyInfo finalProp = type.GetProperty(props.Last());
            MethodInfo setter = finalProp.GetSetMethod();

            expr = Expression.Call(expr, setter, valArg);
            return Expression.Lambda<Action<T, TValue>>(expr, arg, valArg).Compile();
        }

        public static Func<T, TValue> CreateGetter<T, TValue>(string property)
        {
            string[] props = property.Split('.');
            Type type = typeof(T);
            ParameterExpression arg = Expression.Parameter(type, "x");
            Expression expr = arg;
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                expr = Expression.Property(expr, pi);
                type = pi.PropertyType;
            }
            return Expression.Lambda<Func<T, TValue>>(expr, arg).Compile();
        }

        public static bool ValidateExpression<T>(string property)
        {
            Type propertyType;
            return ValidateExpression<T>(property, out propertyType);
        }

        public static bool ValidateExpression<T>(string property, out Type propertyType)
        {
            bool valid = true;
            propertyType = null;

            if (string.IsNullOrEmpty(property))
                return valid;

            string[] props = property.Split('.');
            Type type = typeof(T);
            foreach (string prop in props)
            {
                // use reflection (not ComponentModel) to mirror LINQ
                PropertyInfo pi = type.GetProperty(prop);
                if (pi == null)
                {
                    valid = false;
                    break;
                }

                type = pi.PropertyType;
            }

            propertyType = type;
            return valid;
        }
    }
}
