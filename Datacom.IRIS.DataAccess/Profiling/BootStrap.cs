using System.Data.EntityClient;
using Datacom.IRIS.Common.DependencyInjection;
using Datacom.IRIS.Common.Utils;
using Microsoft.Practices.Unity;
using StackExchange.Profiling;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.Common;

namespace Datacom.IRIS.DataAccess.Profiling
{
    public static class BootStrap
    {
        public static void Initialise()
        {
            StackExchange.Profiling.MiniProfilerEF.Initialize_EF42();

            if (GlobalUtils.GetAppSettingsValueAsBoolean(ConfigSettings.MiniProfilerInstrument) &&
                GlobalUtils.GetAppSettingsValueAsBoolean(ConfigSettings.MiniProfilerLogToSql))
            {
                //use existing entityconnection connection string
                var connectionString = ContextExtensions.ConnectionString();

                if (!connectionString.Contains("Application Name"))  //adding an application name to the connection string so that it can be traced separately in Sql profiler/monitoring
                    connectionString = "Application Name = IRISMiniProfiler; " + connectionString;

                MiniProfiler.Settings.Storage = new MiniProfilerSqlServerStorage(connectionString);
                
            }

        }
    }
}
