using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Interfaces
{
    public interface ISecurableRepository
    {
        SecurityContext GetIRISObjectSecurityContext(long irisId);
    }
}
