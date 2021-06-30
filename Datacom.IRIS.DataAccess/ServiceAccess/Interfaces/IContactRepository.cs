using System;
using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IContactRepository : IRepositoryBase
    {
        [DoNotGenerateBusinessWrapperAttribute]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Contact> GetChangedContacts(DateTime changedSince);

        [DoNotGenerateBusinessWrapperAttribute]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Contact> GetChangedContacts(DateTime changedSince, bool contactCallUpdateContactFinCustomerOnly = false);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)] //None - because it can be for object type Contact or Joint Financial Customer. However, it doesn't really matter becasue everyone has view access to Contact/JFC anyway.
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Contact GetContactByID(long id, bool isJointFinancialCustomer);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Contact GetContactByIRISObjectID(long irisObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactGroupMember> GetContactGroupMembersByContactID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Contact> GetNotDuplicateContacts(long contactID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)] //None - because we are retrieving a list of JFC contacts not contact
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Contact> ListJointFinancialCustomersAssociatedWithContacts(params long[] associatedContactIds);

        //Due to performance consideration, use EnsureValidIRISUser instead of setting CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectIdList" 
        //because everyone has read access to contact anyway
        [EnsureValidIRISUser] 
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<long, string> GetContactWarnings(List<long> irisObjectIdList);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        ContactGroup GetContactGroupByID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        ContactGroup GetContactGroupByIRISObjectID(long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool ContactGroupHasMember(long contactGroupID);

        bool IsContactGroupNameUnique(ContactGroup contactGroup);
        bool IsContactOnlineServiceAccountUserIDUnique(OnlineServicesAccount onlineServiceAccount);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]  //set precheck to be none because we want to check the user permission to a ContactGroup, not a ContactGroupMember
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]  
        List<ContactGroupMember> GetContactGroupMembersByActivityObjectRelationshipIDs(List<long> activityObjectRelationshipIDs);
        
        //Get a mapping of ActivityObjectRelationshipID with ContactGroupMemberID -- No security check since we are only returning IDs
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<long, long> GetContactGroupMemberIDsByActivityObjectRelationshipIDs(List<long> activityObjectRelationshipIDs);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)] //set precheck to be none because we want to check the user permission to a ContactGroup, not a ContactGroupMember
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)] 
        ContactGroupMember GetContactGroupMemberById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        ContactGroup GetContactGroupByIDWithQuestionsAndAnswers(long contactGroupMemberID);

       

        //[EnsureValidIRISUser] //Cannot set to require IRISUser because this is called by the interface
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Contact GetContactByIDReadOnly(long id);  //a read-only version - ie. no TrackAll 

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactSubLink> GetContactSubLinksByRelationshipIDs(List<long> relationshipIDs, long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactGroupQuestionDefinition> GetContactGroupQuestionDefinitionsByContactGroupIRISObjectID(long irisObjectID);

        /// <summary>
        ///  This method is only used in PostEntityChangeHook.SyncCDFsForContact().
        ///  We bypass the security checking since it only return the contactID only.
        /// </summary>
        /// <param name="irisObjectID"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        long GetContactIDByIRISObjectID(long irisObjectID);

        /// <summary>
        ///  TODO: security need to be defined
        /// </summary>
        /// <param name="contactBulkLoadItemGuid"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ContactBulkLoadItem GetContactBulkLoadItem(Guid contactBulkLoadItemGuid);

        /// <summary>
        ///  TODO: security need to be defined
        /// </summary>
        /// <param name="bulkLoadBatchGuid"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactBulkLoadItem> GetFailedContactBulkLoadItems(Guid bulkLoadBatchGuid);

        /// <summary>
        ///  TODO: security need to be defined
        /// </summary>
        /// <param name="bulkLoadBatchGuid"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        int GetIncompletedBulkLoadItemCount(Guid bulkLoadBatchGuid);

        /// <summary>
        ///  TODO: security need to be defined
        /// </summary>
        /// <param name="bulkLoadBatchGuid"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        void DeleteContactBulkLoadItems(Guid bulkLoadBatchGuid);

        Contact GetContactBySphereUserID(string sphereUserID);

    }
}