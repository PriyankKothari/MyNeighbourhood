using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.DTO.Workflow;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.Common;
using Datacom.IRIS.DomainModel.DTO;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IWorkflowRepository : IRepositoryBase
    {
        [DoNotGenerateBusinessWrapper]
        List<TaskInstanceData> GetTaskList(long irisObjectID, bool excludeCompletedTask);

        int GetTaskListCount(long irisObjectID, bool excludeCompletedTask);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<TaskInstanceData> GetTaskList([UseCurrentUserInBusinessWrapper] User user, int? startRowIndex,
            int? maximumRows, string sortExpression, SortDirection sortDirection);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        int GetUserTaskListCount(User user);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<TaskInstanceData> GetTasksForColleagues([UseCurrentUserInBusinessWrapper] User user, string groupId,
            int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        int GetColleaguesTaskListCount(User user, string groupId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<TaskInstanceData> GetTasksForTeam([UseCurrentUserInBusinessWrapper] User user, string groupId,
            int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        int GetTeamsTaskListCount(User user, string groupId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<TaskInstanceData> GetTasksForAll([UseCurrentUserInBusinessWrapper] User user, string groupId,
            int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        int GetAllTaskLitCount(User user, string groupId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        TaskInstance GetTaskByID(long taskID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        String GetTaskInstanceNameByWorkflowInstanceId(long workflowInstanceId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        TaskInstance GetLastTaskInstance();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<TaskInstance> GetTaskListByIDs(List<long> taskIDs);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Workflows, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        TaskDefinition GetTaskDefinitionByID(long taskDefinitionID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Workflows, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        WorkflowDefinition GetWorkflowDefinitionByID(long workflowDefinitionID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        WorkflowDefinition GetWorkflowDefinitionById(long workflowDefinitionID);

        [DoNotGenerateBusinessWrapper]
        WorkflowDefinition GetWorkflowDefinitionByName(string workflowName, string disambiguation);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Workflows, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<WorkflowDefinition> GetWorkflowDefinitionList(bool enabledOnly);

        [DoNotGenerateBusinessWrapper]
        List<WorkflowCallMapping> GetWorkflowCallMappingList(string workflowCallContextCode, IRISObject irisObject);

        [DoNotGenerateBusinessWrapper]
        List<string> GetWorkflowNamesForCallContext(string workflowCallContextCode, long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, PermissionName = PermissionNames.Edit, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]   //No need for post check as pre check has checked for Edit Perission. This saves us from populating SecurityContext in the repository layer. 
        TaskInstance CreateUserTask
            (
                long irisObjectId,
                string name,
                string description,
                string createdBy,
                string assignTo,
                bool sendAssignedAlertFlag,
                DateTime dueDate,
                bool doEscalationFlag,
                DateTime? promptDate,
                bool sendPromptAlertFlag,
                string completedBy,
                DateTime? completedDate,
                string notes,
                bool isInteractiveTask,
                DateTime? interactionDate,
                long? interactionType,
                string interactionBy,
                TimeSpan? completedTime
            );

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]//permission to IRISObject already done in Pre-check. Hence set Post Check to be none.
        List<Datacom.IRIS.DomainModel.Domain.WorkflowCallInstance> GetRunningWorkflowsForObjectAndCallContext(long irisObjectID, string workflowCallContextCode);

        [DoNotGenerateBusinessWrapper]
        WorkflowCallInstance CreateWorkflowCallInstance(long irisObjectID, long workflowCallContextID, long workflowDefinitionID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Workflows, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<TaskDefinition> GetTaskDefinitionList(long workflowDefinitionID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Workflows, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EmailTemplate> GetEmailTemplateList();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Workflows, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<WorkflowRunningInstance> GetWorkflowRunningInstanceList(string textFilter, string statusFilter, bool includeSpecialTask);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermissionOrIRISObject, IRISObjectIDParameterName = "irisObjectID", IRISObjectPermissionName = PermissionNames.ManageWorkflow, FunctionName = BaseFunctionNames.Workflows, FunctionPermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<WorkflowRunningInstance> GetWorkflowRunningInstanceListByIRISObjectID(long irisObjectID, bool includeAll, bool includeSpecialTask);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermissionOrIRISObject, IRISObjectIDParameterName = "irisObjectID", IRISObjectPermissionName = PermissionNames.ManageWorkflow, FunctionName = BaseFunctionNames.Workflows, FunctionPermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        WorkflowRunningInstance GetWorkflowRunningInstancebyID(long workflowInstanceID, long? irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermissionOrIRISObject, IRISObjectIDParameterName = "irisObjectID", IRISObjectPermissionName = PermissionNames.ManageWorkflow, FunctionName = BaseFunctionNames.Workflows, FunctionPermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<WorkflowDefinitionsData> GetWorkflowsForIRISObject(long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermissionOrIRISObject, IRISObjectIDParameterName = "irisObjectID", IRISObjectPermissionName = PermissionNames.ManageWorkflow, FunctionName = BaseFunctionNames.Workflows, FunctionPermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<WorkflowTaskDefinitionsData> GetWorkflowRestartPointsForIRISObject(long irisObjectID);

        [DoNotGenerateBusinessWrapperAttribute]
        List<WorkflowRunningInstance> GetWorkflowRunningInstanceListByDomainObjectID(long IRISObjectID, bool includeUserTask);

        [DoNotGenerateBusinessWrapperAttribute]
        List<TaskDefinition> CopyWorkflowTaskDefinitions(long sourceWorkflowDefinitionID, long targetWorkflowDefinitionID, string modifiedBy, DateTime lastModified);

        [DoNotGenerateBusinessWrapperAttribute]
        List<WorkflowCallMapping> CopyWorkflowCallMappings(long sourceWorkflowDefinitionID, long targetWorkflowDefinitionID, DateTime startDate, string createdBy, DateTime dateCreated);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Workflows, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        WorkflowCallMapping GetWorkflowCallMappingByID(long workflowCallMappingID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Workflows, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<WorkflowCallMappingData> GetWorkflowCallMappingDataList(bool showExpired);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Workflows, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<WorkflowDefinitionsData> GetWorkflowDefinitionDataList();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string GetTaskOutcomeMappedValue(long taskDefinitionID, string outcome);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string GetTaskOutcomeFromMappedValue(long taskDefinitionID, string mappedValue);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]//permission to IRISObject already done in Pre-check. Hence set Post Check to be none.
        TaskInstanceData GetTheEarliestDueIncompleteTaskInstance(long irisObjectID);

        [DoNotGenerateBusinessWrapper]
        List<WorkflowRunningInstance> GetAsyncWorkflowRunningInstance(WorkflowRunningInstance parentInstance);

        [DoNotGenerateBusinessWrapper]
        List<WorkflowRunningInstance> GetOrphanAsyncWorkflowRunningInstanceCleanUpList();

        /// <summary>
        ///  Pass in a list of IRISObjectIDs, return the IDs which has associated workflowRunningInstance
        /// </summary>
        /// <param name="irisObjectIDsList"></param>
        /// <returns></returns>
        [DoNotGenerateBusinessWrapper]
        List<long> GeIRISObjectIDsHasWorkflowRunningInstnace(List<long> irisObjectIDsList);

    }
}