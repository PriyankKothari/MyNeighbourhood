using System;
using System.Linq;
using Datacom.IRIS.Common.Implementations;
using Datacom.IRIS.Common.Utils;
using Microsoft.Practices.EnterpriseLibrary.Logging;
using System.Diagnostics;
using Microsoft.Practices.EnterpriseLibrary.Validation;
using System.Web;
using System.Text.RegularExpressions;

namespace Datacom.IRIS.Common
{
    public class UtilLogger : ILogger
    {
        public static UtilLogger Instance
        {
            get { return _instance ?? (_instance = new UtilLogger()); }
        }

        private static UtilLogger _instance;
        
        public void LogError(string message)
        {            

                if (HttpContext.Current != null && HttpContext.Current.Request != null)
                {
                    HttpBrowserCapabilities bc = HttpContext.Current.Request.Browser;
                    var userAgent = HttpContext.Current.Request.UserAgent;
                    var browserName = bc.Browser;
                    var browserVersion = bc.Version;
                    var info = "IRIS Version Number : " + GlobalUtils.AssemblyVersionNumber();
                    if (browserName != "Unknown")                
                        info = info + Environment.NewLine +  "Browser :  " + browserName + " " + bc.Version + Environment.NewLine;
                    message = info + message;
                    
                }
            WriteToLog(message, TraceEventType.Error, new[] { LoggingCategory.DebugCategory, LoggingCategory.ReleaseCategory, LoggingCategory.ErrorCategory });
        }

        public void LogInfo(string message)
        {
            WriteToLog(message, TraceEventType.Information, new[] { LoggingCategory.DebugCategory, LoggingCategory.ReleaseCategory });
        }

        public void LogDebug(string message)
        {
            WriteToLog(message, TraceEventType.Verbose, new[] { LoggingCategory.DebugCategory });
        }

        
        public void LogDebug(string message, params object[] args)
        {
            LogDebug(string.Format(message, args));
        }

        public void LogWorkflow(string message, params object[] args)
        {
            LogWorkflow(string.Format(message,args));
        }

        public void LogWorkflow(string message)
        {
            WriteToLog(message, TraceEventType.Information, new[] { LoggingCategory.WorkflowCategory });
        }

        
        public void LogValidationErrors(object entity, ValidationResults results)
        {
            if (!results.IsValid)
            {
                string reasons = results.Cast<TranslatedValidationResult>().Aggregate("", (current, error) => current + (string.Format("{0}  * [{1}] {2} (FieldKey: {3})", Environment.NewLine, error.Message, error.TranslatedMessage, error.Key)));
                string validationErrors = string.Format("Entity of type '{0}' failed validation for the following reasons: {1}", entity.GetType(), reasons);
                LogDebug(validationErrors);    
            }
        }

        public void LogStackStrace()
        {
            StackTrace stackTrace = new StackTrace();
            LogDebug(stackTrace.ToString());
        }

        public void LogMiniProfiling(string message)
        {
            WriteToLog(message, TraceEventType.Verbose, new[] { LoggingCategory.MiniProfilerCategory });
        }

        private static void WriteToLog(string message, TraceEventType severity, string[] categories)
        {
           
            
            LogEntry entry = new LogEntry { Message = message, Severity = severity };

            foreach (string c in categories)
                entry.Categories.Add(c);

            Logger.Write(entry);
        }


        /// <summary>
        ///    If instrumentation is turned on for the project, wrap the action with a stopwatch
        ///    timer and log the results. Otherwise run the action directly anyway instead.
        /// </summary>
        public void Instrument(Action action, Func<Stopwatch, string> logMessage)
        {
            if (GlobalUtils.GetAppSettingsValueAsBoolean("Instrument"))
            {
                // Wrap given action with a stopwatch timer
                Stopwatch stopwatch = Stopwatch.StartNew();
                action.Invoke();
                stopwatch.Stop();

                // Pass stopwatch to logging function
                string message = logMessage.Invoke(stopwatch);
                Instrument(message);
            }
            else
            {
                action.Invoke();
            }
        }

        public void Instrument(string message, params object[] args)
        {
            if (GlobalUtils.GetAppSettingsValueAsBoolean("Instrument"))
            {
                WriteToLog(message, TraceEventType.Verbose, new[] { LoggingCategory.InstrumentationCategory });
            }
        }
    }
}
