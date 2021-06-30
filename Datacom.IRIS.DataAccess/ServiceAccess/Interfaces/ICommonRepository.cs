using System;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.DTO;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface ICommonRepository : IRepositoryBase
    {
        List<IRISObject> GetIrisObjects(params long[] idValues);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        IRISObject GetIrisObject(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Note GetNoteByID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Note> GetNotesByIrisObjectID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<string, string> GetIrisMessages();

        [DoNotGenerateBusinessWrapper]
        List<Favourite> GetFavouritesByUser(string loginName);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        void DeleteFavouritesForObject(string objectType, long objectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod=SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<AppSetting> GetAllAppSettings();

        [DoNotGenerateBusinessWrapper]
        long? GetObjectIDFromBusinessID(string businessID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Configuration , PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        AppSetting GetAppSettingById(long id);

        [DoNotGenerateBusinessWrapper]
        AppSetting GetAppSettingByKey(string key);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Permission, ObjectTypeIDParameterName = "objectTypeID", PermissionName = PermissionNames.Edit)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string GetNextBusinessID(long objectTypeID, bool isFinancial = false);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Permission, ObjectTypeCode = ReferenceDataValueCodes.ObjectType.Contact, PermissionName = PermissionNames.CreateFinancialCustomer)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string GetNextFinancialCustomerCode();

        //This interface is the same as GetNextFinancialCustomerCode() except the security check is different.
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Permission, ObjectTypeCode = ReferenceDataValueCodes.ObjectType.JointFinancialCustomer, PermissionName = PermissionNames.Edit)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string GetNextFinancialCustomerCodeForJFC();

        bool IsBusinessIDUnique(long currentIrisID, string businessID);

        bool IsSymbolNameUnique(long currentQuestionDefinitionID, string symbolName);

        bool IsCDFTaskDefinitionUnique(long cdfListID, List<string> taskDefIDs);

        bool IsCDFTaskNameUnique(long cdfListID, string name);

        bool IsAuthorisationIRISIDUnique(long currentIrisID, long currentActivityID, string authorisationIRISID);

        bool IsFavoriteTitleUnique(Favourite favourite);

        bool IsFINCustCodeUnique(long currentIrisID, string finCustCode);

        bool IsFINProjectCodeUnique(long currentIrisID, string finProjectCode);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Status GetStatusByID(long statusId);    

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Status> GetStatusesByID(long irisObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Status GetLatestStatusByIrisId(long irisObjectId);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        GetIRISDataResult GetIRISData(string storedProcName, IEnumerable<SqlParameter> parameters);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)] //permission to IRISObject already done in Pre-check. Hence set Post Check to be none
        List<EventInstanceData> GetEventList(long irisObjectID, bool toDoTasksOnly);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName="irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)] //permission to IRISObject already done in Pre-check. Hence set Post Check to be none
        List<EventInstanceData> GetEventList(long irisObjectID, bool toDoTasksOnly, int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        int GetEventListCount(long irisObjectID, bool toDoTasksOnly);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)] 
        List<TaskInstanceData> GetTaskList(long irisObjectID, bool toDoTasksOnly);

        [DoNotGenerateBusinessWrapper]
        List<NavigationHistory> GetNavigationHistoriesByUser(User user);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        void DeleteNavigationHistoriesByIRISObjectID(long IRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        IRISDetailsDTO GetDetailsByIrisObjectIdOrActivityRelationshipId(long irisObjectID, long activityRelationshipId, long taskId);

        [DoNotGenerateBusinessWrapper]
        void CreateSystemNote(Note systemNote);

        [DoNotGenerateBusinessWrapper]
        List<KeyValuePair<long, string>> GetActiveSecurityGroupForUser(long userID, List<long> irisObjectIdsWithOfficerResponsible);

        [DoNotGenerateBusinessWrapper]
        bool GroupHasFunctionToObjectType(long groupID, string objectTypeCode);

        [DoNotGenerateBusinessWrapper]
        string GetOnlineServiceObjectDetailsXML(long? ObjectID, string ObjectTypeCode);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<QuestionDefinition> GetCDFColumnFields(string subClass1REF);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<GeneralRegister> GetGeneralRegistersBySubClass(Contact contact, string objectType, string subClass1REF, bool isHarbourMaster );

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Answer> GetGeneralRegisterCDFDetails(string objectType, string subClass1REF, long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string GetOtherIdentfierByIRISObjectID(long irisObjectId);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        IEnumerable<OtherIdentifier> GetOtherIdentfiers(long irisObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.BulkReassign, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<long> ListIRISObjectIDsForOfficerResponsible(long userId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.BulkReassign, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<long?> ListTaskInstancesAssignedToUser(long userId, long groupID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.BulkReassign, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<long?> BulkReassignOfficerResponsible(long fromOfficerResponsibleID, long toOfficerResponsibleID, long groupID, string currentUserAccountName, long userID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.RollForwardAnnualCharges, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<BatchRollForwardAnnualChargeResult> RollForwardAnnualCharges(long fromYear, long fromPeriod, long toYear, long toPeriod, string currentUserAccountName);

        //Security is copied from GetIRISData() above
        // No security check needed because this is only retrieving a template
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        EmailTemplate GetEmailTemplate(string templateName);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Configuration, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        CustomLink GetCustomLinkByID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.BulkReassign, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<CustomLink> GetAllCustomLinks();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<CustomLink> GetCustomLinksByIRISObjectID(long objectTypeREFID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool IsHoliday(DateTime date);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        DateTime CalculateLGOIMADueDate(string LGOIMADueDateStr, Request request);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        IQueryable<ObjectInspectionTypeMapping> QueryInspectionTypeMappings();
    }
}
