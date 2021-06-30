using Datacom.IRIS.Common.DependencyInjection;
using Datacom.IRIS.Common.Implementations;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DomainModel.Domain;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Unity;

namespace Datacom.IRIS.DataAccess
{
    public abstract class RepositoryStore : IDisposable, IRepositoryBase
    {
        protected IObjectContext Context { get; private set; }

        protected RepositoryStore()
        {
            Context = DIContainer.Container.Resolve<IObjectContext>();
        }

        public void Dispose()
        {
            if (Context != null)
                Context.Dispose();
        }

        public void EnsureDatabaseExists()
        {
            if (Context.DatabaseExists())
                return;

            Context.CreateDatabase();
        }

        public void EnsureCleanDatabaseExists()
        {
            if (Context.DatabaseExists())
                Context.DeleteDatabase();

            Context.CreateDatabase();
        }

        /// <summary>Returns a domain object by id with the option to retrieve additional IRIS details in an easy to use key word fashion.</summary>
        /// <typeparam name="T">Domain object type e.g. Application</typeparam>
        /// <param name="id">object id</param>
        /// <param name="additionalDetails">A list of keywords used to simplify the retrieval of commonly used objects e.g. SubClass1-2, LatestStatus</param>
        /// <returns>Hydrated domain object</returns>
        /// <example>
        /// 
        ///     This:
        /// 
        ///     var res = GetById<Activity>(activityActivityID, "SubClass2-3,Application.SubClass1,Application.LatestStatus,Application.Holds,Application.Timeframes");
        /// 
        ///     Replaces:
        /// 
        ///     var activity = Context.Activities
        ///    .Include(x => x.IRISObject)
        ///    .Include(x => x.IRISObject.ObjectTypeREF)
        ///    .Include(x => x.IRISObject.SubClass2REF)
        ///    .Include(x => x.IRISObject.SubClass3REF)
        ///    .Include("Application.IRISObject")
        ///    .Include("Application.IRISObject.ObjectTypeREF")
        ///    .Include("Application.IRISObject.SubClass1REF")
        ///    .Include("Application.IRISObject.Statuses")
        ///    .Include("Application.IRISObject.Statuses.StatusREF")
        ///    .Include("Application.IRISObject.Clock")
        ///    .Include("Application.IRISObject.Clock.Holds")
        ///    .Include("Application.IRISObject.Clock.Holds.Reason")
        ///    .Include("Application.IRISObject.Clock.Timeframes")
        ///    .Include("Application.IRISObject.Clock.Timeframes.TimeFrameType")
        ///    .SingleOrDefault(x => x.ID == activityActivityID);
        ///     
        /// </example>
        /// TODO: [Optionally] Rather than using a string, create a static class DSL to include/expand keywords e.g. AdditionalDetails.SubClass1-2.SubClass1("Application").And("SomeOtherIRISObject")
        /// TODO: [More interestingly] AdditionalDetails.WithContactSubLinks()
        public T GetById<T>(long id, string additionalDetails = null) where T : class, IDomainObjectBase
        {
            // Use .include with IQueryable http://stackoverflow.com/questions/5256692/linq-to-entities-include-method-not-found
            // Dynamic Include statements http://stackoverflow.com/questions/10001061/dynamic-include-statements-for-eager-loading-in-a-query-ef-4-3-1
            // EF Fix-up http://blogs.msdn.com/b/alexj/archive/2009/10/13/tip-37-how-to-do-a-conditional-include.aspx

            var objectQuery = Context.EntitySet<T>() as ObjectQuery<T>;

            if (objectQuery == null) return null;

            ExpandKeyWords(additionalDetails).ForEach(item =>
            {
                objectQuery = objectQuery.Include(item);    
            });

            if (objectQuery is IDeletable)
                return objectQuery.Single(x => x.ID == id && !(x as IDeletable).IsDeleted);

            return objectQuery.SingleOrDefault(x => x.ID == id);
        }

        private static List<string> ExpandKeyWords(string additionalDetails)
        {
            var includes = new Dictionary<string, int>
            {
                // Needed for security checks where security is inherited.
                {"IRISObject", 1},
                {"IRISObject.SecurityContextIRISObject", 1},
                {"IRISObject.SecurityContextIRISObject.ObjectTypeREF", 1}
            };

            var additionalDetailsKeywords = (additionalDetails ?? "").Split(',').ToList();
            
            var expandKeyWord = new Func<string, List<string>>(key => 
            {
                //
                // *** Add any new keywords here ***
                // (Duplicates results are ignored below using a dictionary.)
                //
                var result = new List<string>();
                var keyword = key.Contains('.') ? key.Split('.').Last() : key;
                switch (keyword)
                {
                    case "IRISObject":
                        result.Add(key.Replace("IRISObject", "IRISObject.ObjectTypeREF"));  
                        break;
                    case "SubClass1":
                        result.Add(key.Replace("SubClass1", "IRISObject"));
                        result.Add(key.Replace("SubClass1", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("SubClass1", "IRISObject.SubClass1REF"));
                        break;
                    case "SubClass2":
                        result.Add(key.Replace("SubClass2", "IRISObject"));
                        result.Add(key.Replace("SubClass2", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("SubClass2", "IRISObject.SubClass2REF"));
                        break;
                    case "SubClass3":
                        result.Add(key.Replace("SubClass3", "IRISObject"));
                        result.Add(key.Replace("SubClass3", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("SubClass3", "IRISObject.SubClass3REF"));
                        break;
                    case "SubClass1-2":
                        result.Add(key.Replace("SubClass1-2", "IRISObject"));
                        result.Add(key.Replace("SubClass1-2", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("SubClass1-2", "IRISObject.SubClass1REF"));
                        result.Add(key.Replace("SubClass1-2", "IRISObject.SubClass2REF"));
                        break;
                    case "SubClass1-3":
                        result.Add(key.Replace("SubClass1-3", "IRISObject"));
                        result.Add(key.Replace("SubClass1-3", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("SubClass1-3", "IRISObject.SubClass1REF"));
                        result.Add(key.Replace("SubClass1-3", "IRISObject.SubClass2REF"));
                        result.Add(key.Replace("SubClass1-3", "IRISObject.SubClass3REF"));
                        break;
                    case "SubClass2-3":
                        result.Add(key.Replace("SubClass2-3", "IRISObject"));
                        result.Add(key.Replace("SubClass2-3", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("SubClass2-3", "IRISObject.SubClass2REF"));
                        result.Add(key.Replace("SubClass2-3", "IRISObject.SubClass3REF"));
                        break;
                    case "LatestStatus":
                        result.Add(key.Replace("LatestStatus", "IRISObject"));
                        result.Add(key.Replace("LatestStatus", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("LatestStatus", "IRISObject.Statuses"));
                        result.Add(key.Replace("LatestStatus", "IRISObject.Statuses.StatusREF"));
                        break;
                    case "Clock":
                        result.Add(key.Replace("Clock", "IRISObject"));
                        result.Add(key.Replace("Clock", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("Clock", "IRISObject.Clock"));
                        break;
                    case "Holds":
                        result.Add(key.Replace("Holds", "IRISObject"));
                        result.Add(key.Replace("Holds", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("Holds", "IRISObject.Clock"));
                        result.Add(key.Replace("Holds", "IRISObject.Clock.Holds"));
                        result.Add(key.Replace("Holds", "IRISObject.Clock.Holds.Reason"));
                        break;
                    case "Timeframes":
                        result.Add(key.Replace("Timeframes", "IRISObject"));
                        result.Add(key.Replace("Timeframes", "IRISObject.ObjectTypeREF"));
                        result.Add(key.Replace("Timeframes", "IRISObject.Clock"));
                        result.Add(key.Replace("Timeframes", "IRISObject.Clock.Timeframes"));
                        result.Add(key.Replace("Timeframes", "IRISObject.Clock.Timeframes.TimeFrameType"));
                        break;
                    default:
                        result.Add(key);
                        break;
                }
                return result;
            });

            var addItem = new Action<string>(i => expandKeyWord(i).ForEach(key =>
            {
                //Exclude duplicates.
                if (! includes.ContainsKey(key)) includes.Add(key, 1); else includes[key]++;            
            })); 

            additionalDetailsKeywords.ForEach(addItem);

            return includes.Keys.ToList();
        }

        public void ApplyEntityChanges<T>(T entity) where T : class, IDomainObjectBase
        {
            Context.EntitySet<T>().ApplyChanges(entity);
        }

        public void DetachEntity<T>(T entity) where T : class, IDomainObjectBase
        {
            Context.EntitySet<T>().Detach(entity);
        }

        public void AddEntity<T>(T entity) where T : class, IDomainObjectBase
        {
            Context.EntitySet<T>().AddObject(entity);
        }

        public void DeleteEntity<T>(T entity) where T : class, IDomainObjectBase
        {
            Context.EntitySet<T>().DeleteObject(entity);
        }

        public int SaveChanges(SaveOptions saveOptions)
        {
            return Context.SaveChanges(saveOptions);
        }

        public int SaveChanges()
        {
            return this.SaveChanges(SaveOptions.None);
        }

        /// <summary>
        ///    Returns list of all SQL queries that were run for the current Context instance
        /// </summary>
        public List<SQLResponse> SqlResponseList()
        {
            IRISTracingContext irisTracingContext = Context as IRISTracingContext;
            if (irisTracingContext != null)
            {
                return irisTracingContext.SqlResponseList;
            }

            return new List<SQLResponse>();     // EG logging not turned on, return empty list...
        }
    }
}
