using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Implementations
{
    public class UserContext : BaseContext<UserContext>
    {
        public string CurrentUserName { get; set; }
    }
}
