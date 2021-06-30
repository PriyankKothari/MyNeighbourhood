using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using System;
using System.Data.Objects;
using System.Linq.Expressions;


namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class ReferenceDataRepository : RepositoryStore, IReferenceDataRepository
    {
        public List<ReferenceDataValue> GetReferenceDataListByCollectionCode(string code)
        {
            var collection = Context.ReferenceDataCollection.SingleOrDefault(x => x.Code.ToLower() == code.ToLower());
            return Context.ReferenceDataValue.Where(s => s.CollectionID == collection.ID).ToList();
        }
        public List<ReferenceDataCollection> GetAllReferenceDataCollectionsWithValues()
        {
            return this.Context.ReferenceDataCollection.Include(rdc => rdc.ReferenceDataValues).ToList();
        }

        public List<ReferenceDataValue> GetReferenceDataValuesByCode(string code)
        {
            return Context.ReferenceDataValue.Where(x => x.Code == code && x.IsCurrent).ToList();
        }

        public List<ReferenceDataValueAttribute> GetReferenceDataValueAttributesUncached(long referenceDataValueID)
        {
            return Context.ReferenceDataValueAttribute.Where(a => a.ValueID == referenceDataValueID).ToList();
        }

        /// <summary>
        /// This method is used only by CreateLocation page to load object type from database than from cache.
        /// All other pages should use GetObjectTypesWithAttributes()
        /// </summary>
        /// <returns></returns>
        public List<ReferenceDataValue> GetObjectTypesWithAttributesFromDatabase()
        {
            return GetObjectTypesWithAttributes();
        }

        public List<ReferenceDataValue> GetObjectTypesWithAttributes()
        {
            return Context.ReferenceDataValue.Include(rdv => rdv.ReferenceDataValueAttribute)
                .Include(rdv => rdv.ReferenceDataCollection)
                .Where(rdv =>
                    rdv.ReferenceDataCollection.Code.ToLower() == ReferenceDataCollectionCode.IrisObjects.ToLower())
                .Distinct().ToList();
        }

        public List<ReferenceDataCollection> GetSubClass123CollectionsWithAttributes()
        {
            return Context.ReferenceDataCollection.Include("ReferenceDataValues")
                .Include("ReferenceDataValues.ReferenceDataValueAttribute").Where(rdc =>
                    rdc.Code.ToLower().Equals(ReferenceDataCollectionCode.IrisObjectsSubClass1.ToLower()) || rdc.Code
                        .ToLower().Equals(ReferenceDataCollectionCode.IrisObjectsSubClass2.ToLower()) ||
                    rdc.Code.ToLower().Equals(ReferenceDataCollectionCode.IrisObjectsSubClass3.ToLower()))
                .Select(rdc => rdc).Distinct().ToList();
        }


        public List<ReferenceDataValue> GetSubClassification1List(string objectTypeCode)
        {
            return (from referenceDataValue in Context.ReferenceDataValue
                join referenceDataCollection in Context.ReferenceDataCollection on referenceDataValue.CollectionID
                    equals
                    referenceDataCollection.ID
                join parentReferenceDataValue in Context.ReferenceDataValue on referenceDataValue.ParentValueID equals
                    parentReferenceDataValue.ID
                join referenceDataValueAttribute in Context.ReferenceDataValueAttribute on referenceDataValue.ID equals
                    referenceDataValueAttribute.ValueID into sr
                from x in sr.DefaultIfEmpty()
                where referenceDataCollection.Code.ToLower() ==
                      ReferenceDataCollectionCode.IrisObjectsSubClass1.ToLower()
                      && parentReferenceDataValue.Code.ToLower() == objectTypeCode.ToLower()
                select new
                {
                    referenceDataCollection,
                    referenceDataValue,
                    referenceDataValue.ReferenceDataValueAttribute
                }).Select(x => x.referenceDataValue).Distinct().ToList();
        }

        public List<ReferenceDataValue> GetSubClassification2List(string objectTypeCode, string subClassification1Code)
        {
            return (from referenceDataValue in Context.ReferenceDataValue
                join referenceDataCollection in Context.ReferenceDataCollection on referenceDataValue.CollectionID
                    equals
                    referenceDataCollection.ID
                join parentReferenceDataValue in Context.ReferenceDataValue on referenceDataValue.ParentValueID equals
                    parentReferenceDataValue.ID
                join grandParentReferenceDataValue in Context.ReferenceDataValue on parentReferenceDataValue
                    .ParentValueID equals grandParentReferenceDataValue.ID
                join referenceDataValueAttribute in Context.ReferenceDataValueAttribute on referenceDataValue.ID equals
                    referenceDataValueAttribute.ValueID into sr
                from x in sr.DefaultIfEmpty()
                where referenceDataCollection.Code.ToLower() ==
                      ReferenceDataCollectionCode.IrisObjectsSubClass2.ToLower()
                      && grandParentReferenceDataValue.Code.ToLower() == objectTypeCode.ToLower()
                      && parentReferenceDataValue.Code.ToLower() == subClassification1Code.ToLower()
                select new
                {
                    referenceDataCollection,
                    referenceDataValue,
                    referenceDataValue.ReferenceDataValueAttribute
                }).Select(x => x.referenceDataValue).Distinct().ToList();
        }

        public List<ReferenceDataValue> GetSubClassification3List(string objectTypeCode, string subClassification1Code, string subClassification2Code)
        {
            return (from referenceDataValue in Context.ReferenceDataValue
                join referenceDataCollection in Context.ReferenceDataCollection on referenceDataValue.CollectionID
                    equals
                    referenceDataCollection.ID
                join parentReferenceDataValue in Context.ReferenceDataValue on referenceDataValue.ParentValueID equals
                    parentReferenceDataValue.ID
                join grandParentReferenceDataValue in Context.ReferenceDataValue on parentReferenceDataValue
                    .ParentValueID equals grandParentReferenceDataValue.ID
                join greatGrandParentReferenceDataValue in Context.ReferenceDataValue on grandParentReferenceDataValue
                    .ParentValueID equals greatGrandParentReferenceDataValue.ID
                join referenceDataValueAttribute in Context.ReferenceDataValueAttribute on referenceDataValue.ID equals
                    referenceDataValueAttribute.ValueID into sr
                from x in sr.DefaultIfEmpty()
                where referenceDataCollection.Code.ToLower() ==
                      ReferenceDataCollectionCode.IrisObjectsSubClass3.ToLower()
                      && greatGrandParentReferenceDataValue.Code.ToLower() == objectTypeCode.ToLower()
                      && grandParentReferenceDataValue.Code.ToLower() == subClassification1Code.ToLower()
                      && parentReferenceDataValue.Code.ToLower() == subClassification2Code.ToLower()
                select new
                {
                    referenceDataCollection,
                    referenceDataValue,
                    referenceDataValue.ReferenceDataValueAttribute
                }).Select(x => x.referenceDataValue).Distinct().ToList();
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
        ///    
        /// </summary>
        public List<ReferenceDataValue> GetReferenceDataValuesUncached(long collectionId, bool includeInactive, long? parentValueId)
        {
            var queryable = Context.ReferenceDataValue
                .Where(r => r.CollectionID == collectionId);

            if (!includeInactive)
                queryable = queryable.Where(r=> r.IsCurrent);

            if (parentValueId.HasValue)
                queryable = queryable.Where(r=> r.ParentValueID == parentValueId);

            return queryable.ToList();
        }


        /// <summary>
        ///    This method is used only by the Admin screen for edit purpose.
        ///    
        /// </summary>
        public ReferenceDataValue GetReferenceDataValueByIDUncached(long id)
        {
            return Context.ReferenceDataValue.Include(r => r.ReferenceDataValueWording).SingleOrDefault(r => r.ID == id)
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
            // Use a dynamic expression that caters for this method to be called without a 
            // list of reference data values being necessarily passed
            Expression<Func<ReferenceDataValue, bool>> expression = r => r.ReferenceDataCollection.Code == collectionCode;
            if (referenceDataValueCodeList != null && referenceDataValueCodeList.Length > 0)
            {
                expression = r =>
                    r.ReferenceDataCollection.Code == collectionCode && referenceDataValueCodeList.Contains(r.Code);
            }

            return Context.ReferenceDataValue.Include(rdv => rdv.ParentReferenceDataValue)
                .Include(rdv => rdv.ParentReferenceDataValue.ParentReferenceDataValue).Include(rdv =>
                    rdv.ParentReferenceDataValue.ParentReferenceDataValue.ParentReferenceDataValue)
                .Include(r => r.ReferenceDataValueAttribute)
                .Where(expression)
                .ToList();
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
            return Context.ReferenceDataValue.Include(rdv => rdv.ReferenceDataCollection)
                .SingleOrDefault(r => r.ID == referecentDataValueID);
        }


        public ReferenceDataValue GetReferenceDataValueByCollectionCodeAndCode(string collectionCode, string code)
        {
            return Context.ReferenceDataValue.Include(rdv => rdv.ReferenceDataCollection).Single(r =>
                r.Code.ToLower() == code.ToLower() &&
                r.ReferenceDataCollection.Code.ToLower() == collectionCode.ToLower());
        }


        /// <summary>
        /// This method is implemented as a work-around for creating location.
        /// Please use GetReferenceDataValueByCollectionCodeAndCode(string collectionCode, string code) method.
        /// </summary>
        /// <param name="collectionCode"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public ReferenceDataValue GetReferenceDataValueByCollectionCodeAndCodeFromDatabase(string collectionCode, string code)
        {
            return Context.ReferenceDataValue.Include(rdv => rdv.ReferenceDataCollection).SingleOrDefault(r =>
                r.Code.ToLower().Equals(code.ToLower()) &&
                r.ReferenceDataCollection.Code.ToLower().Equals(collectionCode.ToLower()));
        }

        public ReferenceDataValue GetReferenceDataValueByCollectionCodeAndDisplayValue(string collectionCode, string displayValue)
        {
            return Context.ReferenceDataValue.Include(rdv => rdv.ReferenceDataCollection).Single(r =>
                r.DisplayValue.ToLower() == displayValue.ToLower() &&
                r.ReferenceDataCollection.Code.ToLower() == collectionCode.ToLower());
        }

        public List<ReferenceDataValue> GetReferenceDataValuesByIDs(long?[] referenceDataValueIDs)
        {
            return Context.ReferenceDataValue.Include(rdv => rdv.ReferenceDataCollection)
                .Where(r => referenceDataValueIDs.Contains(r.ID)).ToList();
        }

        public ReferenceDataValue GetReferenceDataValueForCollection(string collectionCode, long referecentDataValueID)
        {
            return Context.ReferenceDataValue
                .Include(r => r.ReferenceDataValueAttribute)
                .Include(r => r.ReferenceDataCollection)
                .SingleOrDefault(r =>
                    r.ReferenceDataCollection.Code == collectionCode && referecentDataValueID == r.ID);
        }

        public ReferenceDataValue GetReferenceDataValueForCodeCollectionCodeParentRefValue(string valueCode,
            string collectionCode, long parentReferecentDataValueID)
        {
            return Context.ReferenceDataValue
                .Include(r => r.ReferenceDataValueAttribute)
                .Include(r => r.ReferenceDataCollection)
                .SingleOrDefault(r =>
                    r.ReferenceDataCollection.Code.ToLower() == collectionCode.ToLower() &&
                    r.ParentValueID == parentReferecentDataValueID && r.Code.ToLower() == valueCode.ToLower())
                .TrackAll();
        }

        public ReferenceDataValue GetReferenceDataValueForDisplayValueCollectionCodeParentRefValue(string displayValue, string collectionCode, long parentReferecentDataValueID)
        {
            return Context.ReferenceDataValue
                .Include(r => r.ReferenceDataValueAttribute)
                .Include(r => r.ReferenceDataCollection)
                .SingleOrDefault(r =>
                    r.ReferenceDataCollection.Code.ToLower() == collectionCode.ToLower() &&
                    r.ParentValueID == parentReferecentDataValueID &&
                    r.DisplayValue.ToLower() == displayValue.ToLower());
        }

        public List<ReferenceDataValue> GetReferenceDataValuesForParentValueID(string collectionCode, long parentReferecentDataValueID)
        {
            return Context.ReferenceDataValue.Include(r => r.ReferenceDataValueAttribute)
                .Include(r => r.ReferenceDataCollection).Where(r =>
                    r.ReferenceDataCollection.Code.ToLower() == collectionCode.ToLower() &&
                    parentReferecentDataValueID == r.ParentValueID).ToList();
        }

        public List<ReferenceDataValue> GetReferenceDataValuesForCollectionCodeParentValueCode(string collectionCode, string parentReferecentDataValueCode)
        {
            return Context.ReferenceDataValue
                .Include(r => r.ReferenceDataValueAttribute)
                .Include(r => r.ParentReferenceDataValue)
                .Include(r => r.ReferenceDataCollection)
                .Where(r => r.ReferenceDataCollection.Code.ToLower() == collectionCode.ToLower() &&
                            parentReferecentDataValueCode.ToLower() == r.ParentReferenceDataValue.Code.ToLower())
                .ToList();
        }

        public List<ReferenceDataValue> GetReferenceDataValuesForParentValueIDList(string collectionCode, long[] parentReferecentDataValueIDs)
        {
            return Context.ReferenceDataValue.Include(rdv => rdv.ReferenceDataCollection).Where(r =>
                r.ReferenceDataCollection.Code.ToLower() == collectionCode.ToLower() &&
                parentReferecentDataValueIDs.Contains(r.ParentValueID.Value)).ToList();
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
                GetReferenceDataValuesForCollection(ReferenceDataCollectionCode.IrisObjects, objectTypeCode).Single().ID;

            return GetReferenceDataValueForCodeCollectionCodeParentRefValue(refDataValueCode,
                ReferenceDataCollectionCode.IrisObjectsSubClass1, objectTypeReferenceDataValueId);
        }


        /// <summary>
        ///   There are certain situations where we need to pass no collection code list and return all collection values
        ///   Such as Administration module Ref Data section, so this method is designed to work both ways; with or without params
        /// </summary>
        public List<ReferenceDataCollection> GetReferenceDataCollections(string[] collectionCodesList)
        {
            return collectionCodesList == null
                ? null
                : (
                    collectionCodesList.Length < 1
                        ? Context.ReferenceDataCollection
                            .Include(c => c.ReferenceDataValues)
                            .Include("ReferenceDataValues.ReferenceDataValueAttribute")
                        : Context.ReferenceDataCollection
                            .Include(c => c.ReferenceDataValues)
                            .Include("ReferenceDataValues.ReferenceDataValueAttribute")
                            .Where(r => collectionCodesList.Contains(r.Code))
                ).ToList();
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
        ///  NOTE: the "levelOfParent" define the level for all the requesting collection in collectionCodesList.
        /// </summary>
        /// <param name="collectionCodesList"></param>
        /// <param name="levelOfParent"></param>
        /// <returns></returns>
        public List<ReferenceDataCollection> GetReferenceDataCollectionsWithParentValues(string[] collectionCodesList, int levelOfParent)
        {
            levelOfParent = levelOfParent > 3 ? 3 : levelOfParent;
            var levelQuery = "ReferenceDataValues.ParentReferenceDataValue";
            
            for (int i = 1; i < levelOfParent; i++)
            {
                levelQuery += ".ParentReferenceDataValue";
            }

            return Context.ReferenceDataCollection
                .Include(c => c.ReferenceDataValues)
                .Include(levelQuery)
                .Where(r => collectionCodesList.Contains(r.Code))
                .ToList();
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
            //Do nothing.  This is only useful for the CachedReferenceDataRepository.
        }

        public List<ParentReferenceDataSummary> ListReferenceDataCollectionParentValuesUncached(long collectionID)
        {
            return Context.ListReferenceDataCollectionParentValues(collectionID).ToList();
        }
    }
}