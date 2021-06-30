using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.Common;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IReferenceDataRepository : IRepositoryBase 
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetReferenceDataListByCollectionCode(string code);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValueAttribute> GetReferenceDataValueAttributesUncached(long referenceDataValueID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetObjectTypesWithAttributes();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetObjectTypesWithAttributesFromDatabase();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataCollection> GetSubClass123CollectionsWithAttributes();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetSubClassification1List(string objectTypeCode);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetSubClassification2List(string objectTypeCode, string subClassification1Code);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetSubClassification3List(string objectTypeCode, string subClassification1Code, string subClassification2Code);

        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValue GetReferenceDataValueForCollection(string collectionCode, long referecentDataValueID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValue GetSubClass1RefForParentObjectTypeCode(string objectTypeCode, string refDataValueCode);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValue GetReferenceDataValueForCodeCollectionCodeParentRefValue(string valueCode, string collectionCode, long parentReferecentDataValueID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValue GetReferenceDataValueForDisplayValueCollectionCodeParentRefValue(string displayValue, string collectionCode, long parentReferecentDataValueID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataCollection> GetReferenceDataCollections(string[] collectionCodesList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetReferenceDataValuesByIDs(long?[] referenceDataValueIDList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValue GetReferenceDataValueByID(long? referecentDataValueID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValue GetReferenceDataValueByCollectionCodeAndCode(string collectionCode, string code);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValue GetReferenceDataValueByCollectionCodeAndCodeFromDatabase(string collectionCode, string code);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValue GetReferenceDataValueByCollectionCodeAndDisplayValue(string collectionCode, string displayValue);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetReferenceDataValuesForCollection(string collectionCode, params string[] referenceDataValueCodeList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetReferenceDataValuesForParentValueID(string collectionCode, long parentReferecentDataValueID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetReferenceDataValuesForCollectionCodeParentValueCode(string collectionCode, string parentReferecentDataValueCode);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetReferenceDataValuesForParentValueIDList(string collectionCode, long[] parentReferecentDataValueIDs);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataCollection> GetReferenceDataCollectionsWithParentValues(string[] collectionCodesList, int levelOfParent);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.ReferenceData, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataCollection> GetReferenceDataCollectionsUncached();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.ReferenceData, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataCollection GetReferenceDataCollectionByIDUncached(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.ReferenceData, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetReferenceDataValuesUncached(long collectionId, bool includeInactive, long? parentValueId);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValue GetReferenceDataValueByIDUncached(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        void LoadReferenceDataCacheData(bool forAdminSite);

        [DoNotGenerateBusinessWrapper]
        List<ReferenceDataCollection> GetAllReferenceDataCollectionsWithValues();

        [DoNotGenerateBusinessWrapper]
        bool IsReferenceDataValueInCollectionUncached(long referenceDataValueId, long collectionId);

        [DoNotGenerateBusinessWrapper]
        bool IsReferenceDataValueDisplayValueUniqueUncached(ReferenceDataValue testValue);

        [DoNotGenerateBusinessWrapper]
        bool IsReferenceDataValueCodeUniqueUncached(ReferenceDataValue testValue);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ParentReferenceDataSummary> ListReferenceDataCollectionParentValuesUncached(long collectionID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValue> GetReferenceDataValuesByCode(string code);
    }
}
