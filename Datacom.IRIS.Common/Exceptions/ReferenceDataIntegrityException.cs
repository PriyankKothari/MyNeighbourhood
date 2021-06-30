using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Exceptions
{
    public class ReferenceDataIntegrityException : IRISException
    {
        public ReferenceDataIntegrityException(string message) : base(message)
        {
            
        }
    }
}
