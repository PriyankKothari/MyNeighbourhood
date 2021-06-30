using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Datacom.IRIS.Common;
using Datacom.IRIS.Common.Implementations;
using EFTracingProvider;

namespace Datacom.IRIS.DataAccess
{
    /// <summary>
    ///    Entity Framework has a public provider model which makes it possible for provider writers to support 
    ///    different databases (e.g. Oracle, MySQL, etc). The provider interface used by Entity Framework is 
    ///    stackable, which means it’s possible to write a provider which will wrap another provider and intercept 
    ///    communication between Entity Framework and the original provider.
    /// 
    ///    This class extends the default IRIS Context class and wrap a special tracing provider to do interesting
    ///    things such as:
    ///       (a) Examining query trees and commands before they are executed
    ///       (b) Controlling connections, commands, transactions, data readers, etc. 
    /// 
    ///    http://code.msdn.microsoft.com/EFProviderWrappers
    /// </summary>
    public class IRISTracingContext : IRISContext
    {
        /// <summary>
        ///    Internal list used to keep track of all queries successfully run for a given context connection
        /// </summary>
        public readonly List<SQLResponse> SqlResponseList = new List<SQLResponse>();

        private const string TableNamesPattern = "^*\\s* [FROM|JOIN] \\s* (\\[.*\\].\\[.*\\]) \\s* AS .*";

        public IRISTracingContext() : base(EFTracingProviderUtils.CreateTracedEntityConnection(ConnectionString))
        {
            // We will always only have one tracing connection; listen to it
            EFTracingConnection efTracingConnection = Connection.GetTracingConnections().Single();
            efTracingConnection.CommandFinished += efTracingConnection_CommandFinished;
            efTracingConnection.CommandFailed += efTracingConnection_CommandFinished;
        }

        private void efTracingConnection_CommandFinished(object sender, CommandExecutionEventArgs e)
        {
            string queryString = e.ToTraceString();
            //SQLResponse sqlResponse = new SQLResponse(queryString, e.Duration, e.Status == CommandExecutionStatus.Finished);
            //SqlResponseList.Add(sqlResponse);

            //UtilLogger.Instance.LogSQL(sqlResponse);
            UtilLogger.Instance.Instrument(string.Format("DataRepository -> SQLServer ;Elapsed Time:{0}; Level:{1}; StrLen:{2}; Tables:{3}",
                                            e.Duration.TotalMilliseconds, "3", queryString.Length, TableNamesInQuery(queryString)));
        }

        /// <summary>
        ///    Given a EF SQL query, extract that table names which are used in the query. This is used
        ///    as a useful header title for every query that we expose in the UI. Making it easy for developers
        /// </summary>
        private static string TableNamesInQuery(string query)
        {
            HashSet<string> set = new HashSet<string>();
            Regex regexObj = new Regex(TableNamesPattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            foreach (Match matchResult in regexObj.Matches(query))
            {
                GroupCollection groups = matchResult.Groups;
                set.Add(groups[1].Value);
            }

            return string.Join(", ", set);
        }
    }
}