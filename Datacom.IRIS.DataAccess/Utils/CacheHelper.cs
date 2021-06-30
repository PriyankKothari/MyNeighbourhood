using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using LinqToCache;
using System;
using Datacom.IRIS.Common;
using Datacom.IRIS.Common.DependencyInjection;
using Datacom.IRIS.Common.Utils;

namespace Datacom.IRIS.DataAccess.Utils
{
    public static class CacheHelper
    {
        private static bool _cacheEnabled = false;

        public static IEnumerable<T> FromCached<T>(this IQueryable<T> query, string cacheKey)
        {
            return query.FromCached(cacheKey, (recache) => { });
        }

        public static IEnumerable<T> FromCached<T>(this IQueryable<T> query, string cacheKey, Action<bool> onCacheInvalidated)
        {
            if (_cacheEnabled)
            {
                CachedQueryOptions options = new CachedQueryOptions();

                EventHandler<CachedQueryEventArgs> invalidateCacheHandler = null;
                invalidateCacheHandler = delegate(object sender, CachedQueryEventArgs args)
                {
                    UtilLogger.Instance.LogDebug(string.Format("Invalidate cache for [{0}] - Source:{1}; Type:{2}; Info:{3}.", args.CacheKey, args.NotificationEventArgs.Source, args.NotificationEventArgs.Type, args.NotificationEventArgs.Info));

                    bool recache = true; //bydefault we will recache the query
                    //Log extra warning if this is a query notification subscription error. 
                    //See http://msdn.microsoft.com/en-us/library/ms189308.aspx for a list fo defined error.
                    //and if it is Source:Statement/Info:query, check http://msdn.microsoft.com/en-us/library/ms181122(v=sql.105).aspx for the requirements of supported SELECT statements.
                    if (args.NotificationEventArgs.Source == SqlNotificationSource.Statement)
                    {
                        UtilLogger.Instance.LogError(string.Format("WARNING: Subscribe to query notification failed for [{0}].  Auto recache is disabled. [Source:{1}; Type:{2}; Info:{3}]",
                                                     args.CacheKey, args.NotificationEventArgs.Source, args.NotificationEventArgs.Type, args.NotificationEventArgs.Info));
                        recache = false;
                    }

                    //first unregister the delegate
                    options.OnInvalidated -= invalidateCacheHandler;

                    //invoke onCacheInvalidated asynchronously
                    onCacheInvalidated.BeginInvoke(recache, AsyncCallback, onCacheInvalidated);
                };

                options.OnInvalidated += invalidateCacheHandler;

                var results = (new CachedQuery<T> { Query = query, Key = cacheKey, Options = options }).ToList();
                if (options.DataSource == ECachedQuerySource.FromQuery)
                    UtilLogger.Instance.LogDebug(string.Format("Cache Miss for [{0}]. Get from Query.", cacheKey));
                return results;
            }
            else
            {
                if (GlobalUtils.GetAppSettingsValueAsBoolean(ConfigSettings.EnableCaching))
                    throw new ApplicationException("Error - SqlDependency.Start() must be called before using caching service.");
                else
                    return query.AsEnumerable();
            }
        }

        public static void RemoveCacheEntry<T>(string cacheKey)
        {
            UtilLogger.Instance.LogDebug(string.Format("Manually remove cache entry [{0}].", cacheKey));
            CachedQuery<T>.Remove(cacheKey);
        }

        public static void StartSqlDependency()
        {
            //use existing entityconnection connection string
            SqlDependency.Start(ContextExtensions.ConnectionString());
            _cacheEnabled = true;
        }

        public static void StopSqlDependency()
        {
            //use existing entityconnection connection string
            SqlDependency.Stop(ContextExtensions.ConnectionString());
            _cacheEnabled = false;
        }

        //using yield return to return a new list because the cached static list can be invalidate to null at any time.
        public static List<T> ReturnFromCache<T>(this IEnumerable<T> input)
        {
            return new List<T>(input.YieldReturn());

        }

        private static IEnumerable<T> YieldReturn<T>(this IEnumerable<T> input)
        {
            foreach (T t in input)
            {
                yield return t;
            }
        }

        private static void AsyncCallback(IAsyncResult asyncResult)
        {
            try
            {
                Action<bool> asyncAction = (Action<bool>)asyncResult.AsyncState;
                IRISDisposableTransientLifetimeManager.DisposeTransientItems();
                asyncAction.EndInvoke(asyncResult);
            }
            catch (Exception ex)
            {
                UtilLogger.Instance.LogError("Cache Helper Asynchronous callback  caught following error: " + ex.GetBaseException());
                UtilLogger.Instance.LogStackStrace();
            }
          
        }


    }
}
