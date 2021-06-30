using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Exceptions
{
    public class IRISUnauthorizedAccessException : IRISException
    {
        public IRISUnauthorizedAccessException(string message)
            : base(message)
        {
        }

        public IRISUnauthorizedAccessException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
