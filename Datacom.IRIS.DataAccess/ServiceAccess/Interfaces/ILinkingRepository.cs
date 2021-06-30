using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.DTO;
using Datacom.IRIS.DomainModel.Domain;
using System.Web.UI.WebControls;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface ILinkingRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.RelationshipTypes, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ActivityObjectRelationshipType GetActivityObjectRelationshipTypeById(long id);

        //no permission check is required, this is used similiar to reference data
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)] 
        ActivityObjectRelationshipType GetActivityObjectRelationshipWithCode(string fromObjectTypeCode, string toObjectTypeCode, string code = null);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ActivityObjectRelationshipType> GetAllActivityObjectRelationshipTypes();

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ActivityObjectRelationshipType> GetAllActivityObjectRelationshipTypesFromDatabase();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke="FilterActivityObjectRelationshipAccess")]
        ActivityObjectRelationship GetRelationshipDetailsByID(long id);

        //Precheck is checking one end of the relationship relation, post check is checking the other end of the link.
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "criteria")]  //note - RelationshipLinkCriteria is implementing IIRISObjectID interface in order for this to work.
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<RelationshipEntry> GetRelationshipLinks(RelationshipLinkCriteria criteria);

        //Primary use is to list objects linked a contact for changing sublinks.
        //Precheck is checking one end of the relationship relation, post check is checking the other end of the link.
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "contactIRISObjectId")]  //note - RelationshipLinkCriteria is implementing IIRISObjectID interface in order for this to work.
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<ChangeSublinksItem> GetRelationshipLinksByContactSublinkType(long contactIRISObjectId, string sublinkType, long? sublinkTypeId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ActivityObjectRelationship> GetRelatedLinksForObject(long IRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterGetRelatedIrisObjectsLookup")]
        ILookup<long, KeyValuePair<long, ActivityObjectRelationship>> GetRelatedIrisObjects(long irisObjectId, long[] objectTypeIDList);  

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ILookup<long, KeyValuePair<long, ActivityObjectRelationship>> GetRelatedIrisObjectsForManagementSiteIds(long[] managementSiteIds, long[] objectTypeIDList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName="irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterGetRelatedIrisObjectsActivityObjectRelationship")]
        List<ActivityObjectRelationship> GetRelatedIrisObjects(long irisObjectId, long objectTypeID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterGetRelatedIrisObjects")]
        Dictionary<string, List<IRISObject>> GetRelatedIrisObjects(long irisObjectId, string[] objectTypeCodeList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterGetRelatedIrisObjectsActivityObjectRelationship")]
        List<ActivityObjectRelationship> GetActivityObjectRelationshipByIrisObjectIDs(long irisObjectID1, long irisObjectID2);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<LinkContactInformation> GetContactLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string HasLinkedContactsWithNonCurrentAddressesAndEmails(long irisObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string HasServiceAddressWithANonCurrentAddress(long irisObjectId, bool isAuth);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<LinkInformation> GetLocationLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<LinkInformation> GetLocationGroupLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<LinkInformation> GetConditionScheduleLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<LinkInformation> GetApplicationLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<LinkInformation> GetAuthorisationLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<LinkInformation> GetAuthorisationGroupLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<LinkInformation> GetContactGroupLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships);

        ActivityObjectRelationship HasExistingCurrentActivityRelationship(ActivityObjectRelationship relationship);

        bool HasExistingCurrentActivityRelationshipType(long id, long? objectTypeId, long? relatedObjectId, string relationship);

        bool HasExistingCurrentActivityRelationshipTypeCode(long id, long? objectTypeId, long? relatedObjectId, string relationship);

        void PopulateLinkDetails(long? id);

        void PopulateObjectID(long? objectId, long? irisObjectId);

        //TODO - ideally we should verify the calling user has access to the IRISObjects related to the passed in relationshipIDs
        //See the commented out code.
        //But since this is only found out after the original CR0142 was concluded and it takes certain effort to retest, it is decided to leave it as TODO for the time being.
        //Please address this TODO - including the TODO in CustomSecurityChecks and LinkingRepository before this interface is reused elsewhere.
        //Note that all of the commented out code is UNTESTED.
        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        //[SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke="FilterContactSubLinksBasedOnLinkedObjectsPermissions")]
        List<ContactSubLink> GetContactSubLinks(List<long> relationshipIDs);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName="irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasAnyLinkedObjectsHaveWarning(long irisObjectID, bool checkLinkedContacts, bool checkLinkedLocations, bool includeNonCurrentObjects);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "locationGroupIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasAnyLinkedObjectsHaveWarningForLocationGroup(long locationGroupIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasRelationshipLinksFor(long irisObjectID, LinkedObjectType linkedObjectType, bool includeNonCurrent);

        //Precheck is checking one end of the relationship relation, stored procedure being called will check the other end of the link.
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "criteria")]  //note - RelationshipLinkCriteria is implementing IIRISObjectID interface in order for this to work.
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<LinkedObjectDTO> ListRelationshipLinksWithPaging([UseCurrentUserInBusinessWrapper]User user, RelationshipLinkCriteria criteria, PagingCriteria pagingCriteria);

        //Both precheck and postcheck is set to none because permission check is done in the stored procedure level to only return the records
        //the user have permission to.
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]  
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ActivityObjectRelationship> ListActivityObjectRelationsipByIDs([UseCurrentUserInBusinessWrapper]User user, List<long> activityObjectRelationshipIDs);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ThirdPartyInvolvementRow> GetThirdPartyInvolvementGridRows(long irisObjectID, int? startRowIndex, int? maximumRows);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        int GetThirdPartyInvolvementGridRowsCount(long irisObjectID);

        //Both precheck and postcheck is set to none because permission check is done in the stored procedure level to only return the records
        //the user have permission to.
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "applicationIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ActivityObjectRelationship GetRelationshipWIthThirdPartyInvolvement(long relationshipID, long applicationIRISObjectID);

        // Get the ProgrammeSubjects of ProgrammeRegime associated with Regime
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ActivityObjectRelationship> GetProgrammeRegimeSubjects(long IRISObjectID, string[] objectTypeCodeList);
    }
}