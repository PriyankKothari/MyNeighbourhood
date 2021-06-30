using Datacom.IRIS.Common;
using Datacom.IRIS.Common.DependencyInjection;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Unity;


namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class CachedReferenceDataRepository : RepositoryStore, IReferenceDataRepository
    {
        public List<ReferenceDataValue> GetReferenceDataListByCollectionCode(string code)
        {
            var collection = Context.ReferenceDataCollection.SingleOrDefault(x => x.Code.ToLower() == code.ToLower());
            return Context.ReferenceDataValue.Where(a => a.CollectionID == collection.ID).ToList();
        }

        public List<ReferenceDataCollection> GetAllReferenceDataCollectionsWithValues()
        {
            return GetCachedAllReferenceDataCollectionsWithValues();
        }
        public List<ReferenceDataValue> GetReferenceDataValuesByCode(string code)
        {
            return Context.ReferenceDataValue.Where(x => x.Code == code && x.IsCurrent).ToList();
        }

        public List<ReferenceDataValueAttribute> GetReferenceDataValueAttributesUncached(long referenceDataValueID)
        {
            return Context.ReferenceDataValueAttribute.Where(a => a.ValueID == referenceDataValueID).ToList();
        }

        public List<ReferenceDataValue> GetObjectTypesWithAttributesFromDatabase()
        {
            return (from referenceDataValue in Context.ReferenceDataValue
                join refDataCollection in Context.ReferenceDataCollection on referenceDataValue.CollectionID equals
                    refDataCollection.ID
                where refDataCollection.Code.ToLower() == ReferenceDataCollectionCode.IrisObjects.ToLower()
                select new
                {
                    referenceDataValue,
                    referenceDataValue.ReferenceDataValueAttribute
                }).Select(x => x.referenceDataValue).Distinct().ToList();
        }

        public List<ReferenceDataValue> GetObjectTypesWithAttributes()
        {
            return GetCachedObjectTypesWithAttributes();
        }
        public List<ReferenceDataCollection> GetSubClass123CollectionsWithAttributes()
        {
            return GetCachedSubClass123CollectionsWithAttributes();
        }
        public List<ReferenceDataValue> GetSubClassification1List(string objectType)
        {
            throw new NotImplementedException(
                "This is not implemented because it was created for use by Self Service which does not use Cache. If you need to implement the caching for this method then you could do use the GetSubClass123CollectionsWithAttributes method as this gets all data needed.");
        }
        public List<ReferenceDataValue> GetSubClassification2List(string objectType, string subClassification1Code)
        {
            throw new NotImplementedException(
                "This is not implemented because it was created for use by Self Service which does not use Cache. If you need to implement the caching for this method then you could do use the GetSubClass123CollectionsWithAttributes method as this gets all data needed.");
        }
        public List<ReferenceDataValue> GetSubClassification3List(string objectTypeCode, string subClassification1Code, string subClassification2Code)
        {
            throw new NotImplementedException(
                "This is not implemented because it was created for use by Self Service which does not use Cache. If you need to implement the caching for this method then you could do use the GetSubClass123CollectionsWithAttributes method as this gets all data needed.");
        }

        /// <summary>
        ///    This method gets is only by the Admin screen for edit purpose.
        ///    Hence no caching is used.
        /// </summary>
        public ReferenceDataCollection GetReferenceDataCollectionByIDUncached(long id)
        {

            return Context.ReferenceDataCollection
                .Include(r => r.ParentReferenceDataCollection)
                .SingleOrDefault(c => c.ID == id);
        }

        /// <summary>
        ///    This method is used only by the Admin screen for edit purpose.
        ///    Hence no caching is used.
        /// </summary>
        public List<ReferenceDataValue> GetReferenceDataValuesUncached(long collectionId, bool includeInactive, long? parentValueId)
        {
            var queryable = Context.ReferenceDataValue
                .Where(r => r.CollectionID == collectionId);

            if (!includeInactive)
                queryable = queryable.Where(r => r.IsCurrent);

            if (parentValueId.HasValue)
                queryable = queryable.Where(r => r.ParentValueID == parentValueId);

            return queryable.ToList();
        }

        /// <summary>
        ///    This method is used only by the Admin screen for edit purpose.
        ///    Hence no caching is used.
        /// </summary>
        public ReferenceDataValue GetReferenceDataValueByIDUncached(long id)
        {
            return Context.ReferenceDataValue.Include(r => r.ReferenceDataValueWording).Single(r => r.ID == id)
                .TrackAll();
        }
        
        /// <summary>
        /// This method always return a fresh copy of ReferenceDataCollection.
        /// This is used by the Admin Screen only
        /// </summary>
        /// <returns></returns>
        public List<ReferenceDataCollection> GetReferenceDataCollectionsUncached()
        {
            return Context.ReferenceDataCollection.ToList();
        }


        /// <summary>
        ///    Given a collection code, returns a specific reference data value for a given reference
        ///    reference data value code. This method is typically used for the UI to be able to fetch
        ///    the ref data value ID of a specific code which requires specific behaviour to be
        ///    applied. For example, if a Contact's title is set to "Full Name", then both First Name
        ///    and Last Name properties will be set to required.
        /// </summary>
        public List<ReferenceDataValue> GetReferenceDataValuesForCollection(string collectionCode, params string[] referenceDataValueCodeList)
        {
            var refValues = GetReferenceDataValuesForCollection(collectionCode);

            if (referenceDataValueCodeList != null && referenceDataValueCodeList.Length > 0)
            {
                var referenceDataValueCodeListLowerCase = referenceDataValueCodeList.Select(x => x.ToLower()).ToList();
                return refValues.Where(r => referenceDataValueCodeListLowerCase.Contains(r.Code.ToLower())).ToList();
            }

            return refValues.ToList();
        }

        /// <summary>
        ///    Selects an individual reference data value. This method should ideally be used by the
        ///    UI in "one-off" areas were a single translation of an ID is required. If more than one
        ///    ID to value is required, then developer is better of to use another method in common
        ///    that fetches back a collection of reference data values in one transaction.
        /// </summary>
        /// <param name="referecentDataValueID"></param>
        /// <returns></returns>
        public ReferenceDataValue GetReferenceDataValueByID(long? referecentDataValueID)
        {

            return GetAllReferenceDataValues().SingleOrDefault(r => r.ID == referecentDataValueID);
        }

        public ReferenceDataValue GetReferenceDataValueByCollectionCodeAndCode(string collectionCode, string code)
        {
            return GetReferenceDataValuesForCollection(collectionCode).Single(r => r.Code.ToLower() == code.ToLower());
        }

        /// <summary>
        /// This method is implemented as a work-around for creating location.
        /// Please use GetReferenceDataValueByCollectionCodeAndCode(string collectionCode, string code) method.
        /// </summary>
        /// <param name="collectionCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public ReferenceDataValue GetReferenceDataValueByCollectionCodeAndCodeFromDatabase(string collectionCode,
            string code)
        {
            return Context.ReferenceDataValue.Include(rdv => rdv.ReferenceDataCollection).SingleOrDefault(r =>
                r.Code.ToLower().Equals(code.ToLower()) &&
                r.ReferenceDataCollection.Code.ToLower().Equals(collectionCode.ToLower()));
        }

        public ReferenceDataValue GetReferenceDataValueByCollectionCodeAndDisplayValue(string collectionCode, string displayValue)
        {
            return GetReferenceDataValuesForCollection(collectionCode)
                .Single(r => r.DisplayValue.ToLower() == displayValue.ToLower());
        }

        public List<ReferenceDataValue> GetReferenceDataValuesByIDs(long?[] referenceDataValueIDs)
        {
            return GetAllReferenceDataValues()
                .Where(r => referenceDataValueIDs.Contains(r.ID))
                .ToList();
        }

        public ReferenceDataValue GetReferenceDataValueForCollection(string collectionCode, long referecentDataValueID)
        {
            return GetReferenceDataValuesForCollection(collectionCode)
                .SingleOrDefault(r => r.ID == referecentDataValueID);
        }

        public ReferenceDataValue GetReferenceDataValueForCodeCollectionCodeParentRefValue(string valueCode,
            string collectionCode, long parentReferecentDataValueID)
        {
            return GetReferenceDataValuesForCollection(collectionCode)
                .SingleOrDefault(r =>
                    r.Code.ToLower() == valueCode.ToLower() && r.ParentValueID == parentReferecentDataValueID);
        }

        public ReferenceDataValue GetReferenceDataValueForDisplayValueCollectionCodeParentRefValue(string displayValue,
            string collectionCode, long parentReferecentDataValueID)
        {
            return GetReferenceDataValuesForCollection(collectionCode)
                .SingleOrDefault(r =>
                    r.DisplayValue.ToLower() == displayValue.ToLower() &&
                    r.ParentValueID == parentReferecentDataValueID);
        }

        public List<ReferenceDataValue> GetReferenceDataValuesForParentValueID(string collectionCode, long parentReferecentDataValueID)
        {
            return GetReferenceDataValuesForCollection(collectionCode)
                .Where(r => r.ParentValueID == parentReferecentDataValueID)
                .ToList();
        }

        public List<ReferenceDataValue> GetReferenceDataValuesForCollectionCodeParentValueCode(string collectionCode, string parentReferecentDataValueCode)
        {
            return GetReferenceDataValuesForCollection(collectionCode).Where(r =>
                r.ParentReferenceDataValue.Code.ToLower() == parentReferecentDataValueCode.ToLower()).ToList();
        }

        public List<ReferenceDataValue> GetReferenceDataValuesForParentValueIDList(string collectionCode, long[] parentReferecentDataValueIDs)
        {
            return GetReferenceDataValuesForCollection(collectionCode)
                .Where(r => r.ParentValueID != null && parentReferecentDataValueIDs.Contains(r.ParentValueID.Value))
                .ToList();
        }

        /// <summary>
        ///    This method is used to retrieve a reference data value with a given code under a different
        ///    object type code. This is used for very specific scenarios.
        /// 
        ///    An Application, Authorisation and Activity all have the same SubClass1 display values but with
        ///    different ID's. Only thing common between them is the Code.
        /// 
        ///    This method will retrieve a reference data value with a given Code, that needs to be looked up
        ///    under a given object type (which will typically either be App, Auth or Activity)
        /// </summary>
        /// <param name="objectTypeCode">Object Type to look under e.g. "Application"</param>
        /// <param name="refDataValueCode">SubClass1 Code value that we are fetching "ResourceConsent"</param>
        /// <returns></returns>
        public ReferenceDataValue GetSubClass1RefForParentObjectTypeCode(string objectTypeCode, string refDataValueCode)
        {
            var objectTypeReferenceDataValueId =
                GetReferenceDataValuesForCollection(ReferenceDataCollectionCode.IrisObjects, objectTypeCode).Single()
                    .ID;

            return GetReferenceDataValueForCodeCollectionCodeParentRefValue(refDataValueCode,
                ReferenceDataCollectionCode.IrisObjectsSubClass1, objectTypeReferenceDataValueId);
        }


        /// <summary>
        ///   There are certain situations where we need to pass no collection code list and return all collection values
        ///   Such as Administration module Ref Data section, so this method is desgined to work both ways; with or without params
        /// </summary>
        public List<ReferenceDataCollection> GetReferenceDataCollections(string[] collectionCodesList)
        {
            if (collectionCodesList == null)
                return null;
            
            if (collectionCodesList.Length < 1)
                return GetCachedAllReferenceDataCollectionsWithValues();
            
            var collectionCodesListLowerCase = collectionCodesList.Select(x => x.ToLower()).ToList();
            return GetCachedAllReferenceDataCollectionsWithValues()
                .Where(c => collectionCodesListLowerCase.Contains(c.Code.ToLower()))
                .ToList();
        }

        /// <summary>
        ///  This method return the requested reference data collection with ReferenceDataValues and their 
        ///  ReferenceDataValues.ParentReferenceDataValue. the ReferenceDataValues.ParentReferenceDataValue.ParentReferenceDataValue 
        ///  (and ReferenceDataValues.ParentReferenceDataValue.ParentReferenceDataValue.ParentReferenceDataValue) can be included by
        ///  defining the levelOfParent parameter. 
        /// 
        ///  If levelOfParent less than 1, the method will only include ReferenceDataValues.ParentReferenceDataValue
        ///  If levelOfParent bigger than 3, the method will only include up to
        ///  ReferenceDataValues.ParentReferenceDataValue.ParentReferenceDataValue.ParentReferenceDataValue
        /// 
        ///  NOTE: the "levelOfParent" difine the level for all the requesting collection in collectionCodesList.
        /// </summary>
        /// <param name="collectionCodesList"></param>
        /// <param name="levelOfParent"></param>
        /// <returns></returns>
        public List<ReferenceDataCollection> GetReferenceDataCollectionsWithParentValues(string[] collectionCodesList, int levelOfParent)
        {
            //we are returning all parent levels in the cached Collection List, so it's no different from calling GetReferenceDataCollections(collectionCodesList)
            return GetReferenceDataCollections(collectionCodesList);
        }

        public bool IsReferenceDataValueInCollectionUncached(long referenceDataValueId, long collectionId)
        {
            return (from refValue in Context.ReferenceDataValue
                    where refValue.ID == referenceDataValueId && refValue.CollectionID == collectionId
                    select refValue.ID
                ).Count() == 1;
        }

        public bool IsReferenceDataValueDisplayValueUniqueUncached(ReferenceDataValue testValue)
        {
            return !(from refValue in Context.ReferenceDataValue
                    where
                        refValue.DisplayValue == testValue.DisplayValue
                        && refValue.CollectionID == testValue.CollectionID
                        && (refValue.ParentValueID == testValue.ParentValueID ||
                            refValue.ParentValueID == null && testValue.ParentValueID == null)
                        && refValue.ID != testValue.ID
                    select refValue.ID
                ).Any();
        }

        public bool IsReferenceDataValueCodeUniqueUncached(ReferenceDataValue testValue)
        {
            return !(from refValue in Context.ReferenceDataValue
                    where
                        refValue.Code == testValue.Code
                        && refValue.CollectionID == testValue.CollectionID
                        && (refValue.ParentValueID == testValue.ParentValueID ||
                            refValue.ParentValueID == null && testValue.ParentValueID == null)
                        && refValue.ID != testValue.ID
                    select refValue.ID
                ).Any();
        }

        public void LoadReferenceDataCacheData(bool forAdminSite)
        {    
            //Create a new IObjectContext before loading each cache object otherwise all objects in the cache will be linked up
            using (IObjectContext context = DIContainer.Container.Resolve<IObjectContext>())
            {
                GetCachedAllReferenceDataCollectionsWithValues(context);
            }
            using (IObjectContext context = DIContainer.Container.Resolve<IObjectContext>())
            {
                GetCachedObjectTypesWithAttributes(context);
            }
            using (IObjectContext context = DIContainer.Container.Resolve<IObjectContext>())
            {
                GetCachedSubClass123CollectionsWithAttributes(context);
            }
        }            

        public List<ParentReferenceDataSummary> ListReferenceDataCollectionParentValuesUncached(long collectionID)
        {
            return Context.ListReferenceDataCollectionParentValues(collectionID).ToList();
        }

        private ReferenceDataCollection GetReferenceDataCollection(string collectionCode)
        {
            if (collectionCode == null) return null;

            return GetCachedAllReferenceDataCollectionsWithValues()
                .Single(c => collectionCode.ToLower() == c.Code.ToLower());
        }

        private IEnumerable<ReferenceDataValue> GetReferenceDataValuesForCollection(string collectionCode)
        {
            return GetReferenceDataCollection(collectionCode).ReferenceDataValues;
        }

        private IEnumerable<ReferenceDataValue> GetAllReferenceDataValues()
        {
            return GetCachedAllReferenceDataCollectionsWithValues()
                .SelectMany(c => c.ReferenceDataValues);
        }

        private static object _cachedCollectionSyncLock = new object();
        private static List<ReferenceDataCollection> _cachedAllCollectionsWithValues;

        private List<ReferenceDataCollection> GetCachedAllReferenceDataCollectionsWithValues()
        {
            return GetCachedAllReferenceDataCollectionsWithValues(Context);
        }

        private List<ReferenceDataCollection> GetCachedAllReferenceDataCollectionsWithValues(IObjectContext context)
        {
            lock (_cachedCollectionSyncLock)
            {
                if (_cachedAllCollectionsWithValues != null)
                {
                    //UtilLogger.Instance.LogDebug("Cache Get for [GetCachedAllReferenceDataCollectionsWithValues] with DataSource LocalCopy.");
                    return _cachedAllCollectionsWithValues.ReturnFromCache();
                }
           
                Action<bool> onCacheInvalidated = (doRecache) =>
                {
                    lock (_cachedCollectionSyncLock)
                    {
                        CacheHelper.RemoveCacheEntry<ReferenceDataValue>(CacheConstants.ReferenceDataCollections_Values);
                        CacheHelper.RemoveCacheEntry<ReferenceDataCollection>(CacheConstants.ReferenceDataCollections_Collections);
                        _cachedAllCollectionsWithValues = null;
                    }

                    //Recache.
                    if (doRecache)
                    {
                        using (var repository = RepositoryMap.ReferenceDataRepository)
                        {
                            repository.GetAllReferenceDataCollectionsWithValues();
                        }
                    }
                };

                //Because EF object materialisation is too slow for the call Context.ReferenceDataValue (Takes ~13s),
                //we'll first set MergeOption to MergeOption.NoTracking and register the SqlDependency with dummy calls before doing the real call.
                //This is to avoid notification subscription fires before we finish grabbing the data.
                //Using MergeOption.NoTracking will stop the auto fix up happen that links collection to children collections and reference data value to children reference data values.
                context.ReferenceDataValue.MergeOption = MergeOption.NoTracking;
                context.ReferenceDataCollection.MergeOption = MergeOption.NoTracking;

                // NOTE: DON"T DELETE
                // It makes use of the Entity framework auto-fixup feature.  Once the entity has been pulled into the 
                // EF object graph, it will auto hook up the relationships. This is to work around the fact that caching  
                // (relying on sql dependency)  does not work with Left join.
                var dummy1 = context.ReferenceDataValue.FromCached(CacheConstants.ReferenceDataCollections_Values, onCacheInvalidated).ToList();
                var dummy2 = context.ReferenceDataCollection.FromCached(CacheConstants.ReferenceDataCollections_Collections, onCacheInvalidated).ToList();


                //Turn merge option back on and do the 'real call'
                context.ReferenceDataValue.MergeOption = MergeOption.AppendOnly;
                context.ReferenceDataCollection.MergeOption = MergeOption.AppendOnly;
                _cachedAllCollectionsWithValues = context.ReferenceDataCollection.Include("ReferenceDataValues").ToList();

                return _cachedAllCollectionsWithValues.ReturnFromCache();

            }
        }


        private static object _cachedObjectTypesSyncLock = new object();
        private static List<ReferenceDataValue> _cachedObjectTypes;


        private List<ReferenceDataValue> GetCachedObjectTypesWithAttributes()
        {
            return GetCachedObjectTypesWithAttributes(Context);
        }


        private List<ReferenceDataValue> GetCachedObjectTypesWithAttributes(IObjectContext context)
        {
            lock (_cachedObjectTypesSyncLock)
            {
                if (_cachedObjectTypes != null)
                {
                    //UtilLogger.Instance.LogDebug("Cache Get for [GetCachedObjectTypesWithAttributes] with DataSource LocalCopy.");
                    return _cachedObjectTypes.ReturnFromCache();
                }

                //set up cache invalidated action to invalidate all related cache entries and recache.
                Action<bool> onCacheInvalidated = (doRecache) =>            
                {                
                    lock (_cachedObjectTypesSyncLock)
                    {
                        CacheHelper.RemoveCacheEntry<ReferenceDataValueAttribute>(CacheConstants.ObjectTypes_Attributes);
                        CacheHelper.RemoveCacheEntry<ReferenceDataValue>(CacheConstants.ObjectTypes_ReferenceDataValues);
                        _cachedObjectTypes = null;
                    }
                    //recache
                    if (doRecache)
                    {
                        using (var repository = RepositoryMap.ReferenceDataRepository)
                        {
                            repository.GetObjectTypesWithAttributes();
                        }
                    }
                };

                // NOTE: DON"T DELETE
                // It makes use of the Entity framework auto-fixup feature.  Once the entity has been pulled into the 
                // EF object graph, it will auto hook up the relationships. This is to work around the fact that caching  
                // (relying on sql dependency)  does not work with Left join.
                var referenceDataValueAttributes = context.ReferenceDataValueAttribute.FromCached(CacheConstants.ObjectTypes_Attributes, onCacheInvalidated).ToList();

                var referenceDataValues = (from refDataValue in context.ReferenceDataValue
                        join refDataCollection in context.ReferenceDataCollection on refDataValue.CollectionID equals
                            refDataCollection.ID
                        where refDataCollection.Code.ToLower() == ReferenceDataCollectionCode.IrisObjects.ToLower()
                        select refDataValue)
                    .FromCached(CacheConstants.ObjectTypes_ReferenceDataValues, onCacheInvalidated).ToList();

                _cachedObjectTypes = referenceDataValues;

                return _cachedObjectTypes.ReturnFromCache();
            }
        }

        private static object _cachedSubClassesSyncLock = new object();
        private static List<ReferenceDataCollection> _cachedSubClasses;
        
        private List<ReferenceDataCollection> GetCachedSubClass123CollectionsWithAttributes()
        {
            return GetCachedSubClass123CollectionsWithAttributes(Context);
        }

        private List<ReferenceDataCollection> GetCachedSubClass123CollectionsWithAttributes(IObjectContext context)
        {
            lock (_cachedSubClassesSyncLock)
            {
                if (_cachedSubClasses != null)
                {
                    //UtilLogger.Instance.LogDebug("Cache Get for [GetCachedSubClass123CollectionsWithAttributes] with DataSource LocalCopy.");
                    return _cachedSubClasses.ReturnFromCache();
                }
                    

                //set up cache invalidated action to invalidate all related cache entries and recache.
                Action<bool> onCacheInvalidated = (doRecache) =>
                {
                    lock (_cachedSubClassesSyncLock)
                    {
                        CacheHelper.RemoveCacheEntry<ReferenceDataValueAttribute>(CacheConstants.SubClasses_Attributes);
                        CacheHelper.RemoveCacheEntry<ReferenceDataCollectionAndValue>(CacheConstants.SubClasses_ReferenceDataValues);
                        _cachedSubClasses = null;
                    }
                    //recache
                    if (doRecache)
                    {
                        using (var repository = RepositoryMap.ReferenceDataRepository)
                        {
                            repository.GetSubClass123CollectionsWithAttributes();
                        }
                    }
                
                };

                // NOTE: DON"T DELETE
                // It makes use of the Entity framework auto-fixup feature.  Once the entity has been pulled into the 
                // EF object graph, it will auto hook up the relationships. This is to work around the fact that caching  
                // (relying on sql dependency)  does not work with Left join.
                var referenceDataValueAttributes = (from refDataAttributes in context.ReferenceDataValueAttribute
                                                    where refDataAttributes.ID == refDataAttributes.ID   //this is a dummy criteria to distinguish this cache entry from the one in GetCachedObjectTypesWithAttributes
                                                    select refDataAttributes).FromCached(CacheConstants.SubClasses_Attributes, onCacheInvalidated).ToList();

                var refDataCollectionsAndValues = (from refDataCollection in context.ReferenceDataCollection
                        join refDataValue in context.ReferenceDataValue on refDataCollection.ID equals refDataValue
                            .CollectionID
                        where (refDataCollection.Code.ToLower() ==
                               ReferenceDataCollectionCode.IrisObjectsSubClass1.ToLower() ||
                               refDataCollection.Code.ToLower() ==
                               ReferenceDataCollectionCode.IrisObjectsSubClass2.ToLower() ||
                               refDataCollection.Code.ToLower() ==
                               ReferenceDataCollectionCode.IrisObjectsSubClass3.ToLower())
                        select new ReferenceDataCollectionAndValue
                        {
                            Collection = refDataCollection,
                            ReferenceDataValue = refDataValue
                        })
                    .FromCached(CacheConstants.SubClasses_ReferenceDataValues, onCacheInvalidated);
                    
                _cachedSubClasses = refDataCollectionsAndValues.Select(x => x.Collection).Distinct().ToList();
                
                return _cachedSubClasses.ReturnFromCache();
            }
           
        }


        private class ReferenceDataCollectionAndValue
        {
            public ReferenceDataCollection Collection { get; set; }
            public ReferenceDataValue ReferenceDataValue { get; set; }
        }
    }
}
