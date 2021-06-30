using Datacom.IRIS.Common;
using Datacom.IRIS.Common.DependencyInjection;
using Datacom.IRIS.Common.Utils;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.EntityClient;
using System.Text;
using Unity;

namespace Datacom.IRIS.DataAccess.Utils
{
    /// <summary>
    /// Contains extensions to invoke SPs from the current context
    /// </summary>
    static class ContextExtensions
    {
        /// <summary>
        /// Executes a SP that returns a list of scalar values
        /// </summary>
        /// <typeparam name="T">The type of values returned</typeparam>
        /// <param name="context">The current context</param>
        /// <param name="spName">The name of the SP to execute</param>
        /// <param name="parameters">arguments to pass wo the SP</param>
        /// <returns></returns>
        public static List<T> ExecuteListScalarSP<T>(this IObjectContext context, string spName, params DbParameter[] parameters)
            where T : struct
        {
            List<T> list = new List<T>();
            using (var cmd = GetCommandForSP(context, spName, parameters))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add((T)reader.GetValue(0));
                    }
                    reader.Close();
                }

                cmd.Connection.Close();
            }

            return list;
        }

        /// <summary>
        /// Execute a SP that returns a scalar string (which can be an XML)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string ExecuteSPReturnScalar(this IObjectContext context, string spName, params DbParameter[] parameters)
        {
            StringBuilder stringBuilder = new StringBuilder();
            using (var cmd = GetCommandForSP(context, spName, parameters))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            stringBuilder.Append(reader[0]);
                        }
                    }
                    reader.Close();
                }
                cmd.Connection.Close();
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Execute a SP that return a DataTable - used for SP's that Councils can override and return any number of columns
        /// </summary>
        /// <param name="context"></param>
        /// <param name="spName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static DataTable ExecuteSPReturnDataTable(this IObjectContext context, string spName, params DbParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (var cmd = GetCommandForSP(context, spName, parameters))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    dt.Load(reader);
                }
                cmd.Connection.Close();
            }
            return dt;
        }


        /// <summary>
        /// Prepares a command to execute a stored procedure
        /// </summary>
        /// <param name="context">The current context</param>
        /// <param name="spName">The name of the SP to execute</param>
        /// <param name="parameters">arguments to pass wo the SP</param>
        /// <returns></returns>
        private static DbCommand GetCommandForSP(this IObjectContext context, string spName, params DbParameter[] parameters)
        {
            EntityConnection dc = (EntityConnection)context.Connection;
            DbConnection connection = dc.StoreConnection;

            DbCommand cmd = connection.CreateCommand();
            if (connection.State == System.Data.ConnectionState.Closed)
                connection.Open();

            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            cmd.CommandText = spName;

            int? customisedCommandTimeout =  GlobalUtils.GetAppSettingsValue(ConfigSettings.DatabaseCommandTimeout).ToNullableInt32();

            if (customisedCommandTimeout.HasValue)
                cmd.CommandTimeout = customisedCommandTimeout.Value;

            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    cmd.Parameters.Add(p);
                }
            }

            return cmd;
        }

        public static string ConnectionString()
        {
            using (var context = DIContainer.Container.Resolve<IObjectContext>())
            {
                EntityConnection dc = (EntityConnection)context.Connection;
                return dc.StoreConnection.ConnectionString;
            }
        }
    }
}
