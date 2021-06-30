using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Datacom.IRIS.Common
{
    public interface ILogger
    {
        void LogError(string message);

        void LogInfo(string message);

        void LogDebug(string message);

        //void LogTrace(string message);
    }
}
