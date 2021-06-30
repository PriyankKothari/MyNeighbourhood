using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Exceptions
{
    public class IRISException : Exception
    {
        public IRISException(string message) : base(message)
        {

        }

        public IRISException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
