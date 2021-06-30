using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Implementations
{
    /// <summary>
    ///    The purpose of this class is to use as a dummy context that unit tests
    ///    can set when invoking business logic to set different values for the business
    ///    tier to extract - this is typically used for impersonation - a way to allow
    ///    unit tests to specify the user they want to impersinate in running their tests
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseContext<T> : IDisposable where T : BaseContext<T>
    {
        protected BaseContext()
        {
            _currentContext = (T)this;
        }

        /// <summary>
        ///    A thread static exists only for the current invoker, and not for the full lifetime of the application
        /// </summary>
        [ThreadStatic]
        private static T _currentContext;

        public static T Current
        {
            get
            {
                return _currentContext;
            }
        }

        public void Dispose()
        {
            _currentContext = null;
        }
    }
}
