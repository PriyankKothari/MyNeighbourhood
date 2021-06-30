using System;
using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.Common.Utils;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.DTO;
using Datacom.IRIS.DomainModel.DTO.Workflow;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.Workflow.DomainModel;
using System.Web.UI.WebControls;


namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class WorkflowRepository : RepositoryStore, IWorkflowRepository
    {
        const int WorkflowQueryLimit = 500;

        private IQueryable<TaskInstanceData> GetTaskQueryForList(long irisObjectID, bool excludeCompletedTask, bool excludeTaskWithNoDueDate)
        {
            //int userPrefixLength = .Length;
            var queryable = from task in Context.TaskInstances
                            join completedBy in Context.User on task.CompletedBy equals completedBy.AccountName into CompletedByUser
                            join assignedTo in Context.User on task.AssignedTo equals (WorkflowConstants.UserPrefix +
                                                                                       assignedTo.AccountName) into AssignedToUser
                            from completedUser in CompletedByUser.DefaultIfEmpty()
                            from assignedToUser in AssignedToUser.DefaultIfEmpty()
                            where ((task.WorkflowInstance.Status !=
                                    Datacom.Workflow.DomainModel.DomainConstants.WorkflowStatus.Cancelled) ||
                                   task.CompletedDate != null) //exclude cancel workflow that has never been completed   
                                   && task.IRISObjectID == irisObjectID
                            select
                                new TaskInstanceData()
                                {
                                    ID = task.ID,
                                    IRISObjectID = task.IRISObjectID,

                                    IRISObjectTypeCode = task.IRISObject.ObjectTypeREF.Code,
                                    IRISObjectTypeDisplayValue = task.IRISObject.ObjectTypeREF.DisplayValue,
                                    IRISObjectTypeID = task.IRISObject.ObjectTypeID,
                                    IRISObjectSubClass1ID = task.IRISObject.SubClass1ID,
                                    IRISObjectSubClass2ID = task.IRISObject.SubClass2ID,

                                    IRISObjectHasAdditionalDetails = task.IRISObject.HasAdditionalDetails,

                                    SecurityContextIRISObjectID = task.IRISObject.SecurityContextIRISObjectID,
                                    SecurityContextIRISObjectTypeCode =
                                        task.IRISObject.SecurityContextIRISObject.ObjectTypeREF.Code,
                                    SecurityContextIRISObjectTypeID = task.IRISObject.SecurityContextIRISObject.ObjectTypeID,
                                    SecurityContextIRISObjectSubClass1ID = task.IRISObject.SecurityContextIRISObject.SubClass1ID,
                                    SecurityContextIRISObjectSubClass2ID = task.IRISObject.SecurityContextIRISObject.SubClass2ID,

                                    IRISObjectLinkID = task.IRISObject.LinkID,
                                    IRISObjectLinkDetails = task.IRISObject.LinkDetails,
                                    IRISObjectBusinessID = task.IRISObject.BusinessID,
                                    DueDate = task.DueDate,
                                    CompletedDate = task.CompletedDate,
                                    CompletedTime = task.CompletedTime,
                                    AssignedTo = task.AssignedTo,
                                    AssignedToUserDisplayName =
                                        (assignedToUser != null) ? assignedToUser.DisplayName : task.AssignedTo,
                                    CompletedBy = completedUser.DisplayName,
                                    TaskInstanceName = task.Name,
                                    TaskDefinitionName = task.TaskDefinition.Name,
                                    RequiresTime = task.TaskDefinition.RequiresTime,
                                    TaskOutcomes = task.TaskDefinition.PossibleOutcomes,
                                    InstanceOutcome = task.Outcome,
                                    TaskNotes = task.Notes,
                                    WorkflowDefinitionName = task.TaskDefinition.WorkflowDefinition.Name,
                                    TaskTypeDisplayValue = task.TaskInteractionTypeREF.DisplayValue,
                                    WorkflowInstanceStatus = task.WorkflowInstance.Status,
                                    CreatedByUserAccountName = task.CreatedBy,
                                    InstanceType = EventInstanceType.Task
                                };

            if (excludeCompletedTask)
                queryable = queryable.Where(x => x.CompletedDate == null);  //exclude completed task means exclude tasks with a completed date.

            if (excludeTaskWithNoDueDate)
                queryable = queryable.Where(x => x.DueDate > DateTime.MinValue);  //task with no due date has due date set to min value

            return queryable;
        }

        private int GetTaskQueryForListCount(long irisObjectID, bool excludeCompletedTask, bool excludeTaskWithNoDueDate)
        {
            //int userPrefixLength = .Length;
            return (from task in Context.TaskInstances
                    join completedBy in Context.User on task.CompletedBy equals completedBy.AccountName into CompletedByUser
                    join assignedTo in Context.User on task.AssignedTo equals (WorkflowConstants.UserPrefix +
                                                                               assignedTo.AccountName) into AssignedToUser
                    from completedUser in CompletedByUser.DefaultIfEmpty()
                    from assignedToUser in AssignedToUser.DefaultIfEmpty()
                    where ((task.WorkflowInstance.Status !=
                            Datacom.Workflow.DomainModel.DomainConstants.WorkflowStatus.Cancelled) ||
                           task.CompletedDate != null) //exclude cancel workflow that has never been completed   
                           && task.IRISObjectID == irisObjectID
                           && (excludeCompletedTask == false || task.CompletedDate == null)
                           && (excludeTaskWithNoDueDate == false || task.DueDate > DateTime.MinValue)
                    select task).Count();

        }

        private IQueryable<TaskInstanceData> GetTasksList(bool excludeCompletedTask,
            bool excludeTaskWithNoDueDate, string username, IQueryable<string> colleagueGroups,
            IQueryable<string> teamGroups, int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection)
        {
            var tasksQueryable = Context.TaskInstances
                .GroupJoin(Context.User,
                    task => task.CompletedBy,
                    user => user.AccountName,
                    (task, completedByUser) => new { task, completedByUser })
                .GroupJoin(Context.User,
                    task => task.task.AssignedTo,
                    user => WorkflowConstants.UserPrefix + user.AccountName,
                    (task, assignedToUser) => new { task, assignedToUser })
                .SelectMany(task => task.task.completedByUser.DefaultIfEmpty(),
                    (task, completedByUser) => new { task, completedByUser })
                .SelectMany(task => task.task.assignedToUser.DefaultIfEmpty(),
                    (task, assignedToUser) => new { task, assignedToUser })
                .Where(task =>
                    (task.task.task.task.task.WorkflowInstance.Status != DomainConstants.WorkflowStatus.Cancelled ||
                     task.task.task.task.task.CompletedDate != null) &&
                    (!excludeCompletedTask || task.task.task.task.task.CompletedDate == null) &&
                    (!excludeTaskWithNoDueDate || task.task.task.task.task.DueDate > DateTime.MinValue) &&
                    (task.task.task.task.task.AssignedTo == username || string.IsNullOrEmpty(username) && (
                         colleagueGroups.Contains(task.task.task
                             .task.task.AssignedTo) ||
                         teamGroups.Contains(task.task.task.task
                             .task.AssignedTo))))
                //.OrderBy(task => task.task.task.task.task.ID)
                .Select(task =>
                    new TaskInstanceData()
                    {
                        ID = task.task.task.task.task.ID,
                        IRISObjectID = task.task.task.task.task.IRISObjectID,
                        IRISObjectTypeCode = task.task.task.task.task.IRISObject.ObjectTypeREF.Code,
                        IRISObjectTypeDisplayValue = task.task.task.task.task.IRISObject.ObjectTypeREF.DisplayValue,
                        IRISObjectTypeID = task.task.task.task.task.IRISObject.ObjectTypeID,
                        IRISObjectSubClass1ID = task.task.task.task.task.IRISObject.SubClass1ID,
                        IRISObjectSubClass2ID = task.task.task.task.task.IRISObject.SubClass2ID,
                        IRISObjectHasAdditionalDetails = task.task.task.task.task.IRISObject.HasAdditionalDetails,
                        SecurityContextIRISObjectID = task.task.task.task.task.IRISObject.SecurityContextIRISObjectID,
                        SecurityContextIRISObjectTypeCode = task.task.task.task.task.IRISObject
                            .SecurityContextIRISObject.ObjectTypeREF.Code,
                        SecurityContextIRISObjectTypeID =
                            task.task.task.task.task.IRISObject.SecurityContextIRISObject.ObjectTypeID,
                        SecurityContextIRISObjectSubClass1ID =
                            task.task.task.task.task.IRISObject.SecurityContextIRISObject.SubClass1ID,
                        SecurityContextIRISObjectSubClass2ID =
                            task.task.task.task.task.IRISObject.SecurityContextIRISObject.SubClass2ID,
                        IRISObjectLinkID = task.task.task.task.task.IRISObject.LinkID,
                        IRISObjectLinkDetails = task.task.task.task.task.IRISObject.LinkDetails,
                        IRISObjectBusinessID = task.task.task.task.task.IRISObject.BusinessID,
                        DueDate = task.task.task.task.task.DueDate,
                        CompletedDate = task.task.task.task.task.CompletedDate,
                        CompletedTime = task.task.task.task.task.CompletedTime,
                        AssignedTo = task.task.task.task.task.AssignedTo,
                        AssignedToUserDisplayName = (task.assignedToUser != null)
                            ? task.assignedToUser.DisplayName
                            : task.task.task.task.task.AssignedTo,
                        CompletedBy = task.task.completedByUser.DisplayName,
                        TaskInstanceName = task.task.task.task.task.Name,
                        TaskDefinitionName = task.task.task.task.task.TaskDefinition.Name,
                        DisplayName = task.task.task.task.task.TaskDefinition.Name == WorkflowConstants.AdHocTaskName
                            ? task.task.task.task.task.Name
                            : task.task.task.task.task.TaskDefinition.WorkflowDefinition.Name + " " + task.task.task.task.task.Name,
                        RequiresTime = task.task.task.task.task.TaskDefinition.RequiresTime,
                        TaskOutcomes = task.task.task.task.task.TaskDefinition.PossibleOutcomes,
                        InstanceOutcome = task.task.task.task.task.Outcome,
                        TaskNotes = task.task.task.task.task.Notes,
                        WorkflowDefinitionName = task.task.task.task.task.TaskDefinition.WorkflowDefinition.Name,
                        TaskTypeDisplayValue = task.task.task.task.task.TaskInteractionTypeREF.DisplayValue,
                        WorkflowInstanceStatus = task.task.task.task.task.WorkflowInstance.Status,
                        CreatedByUserAccountName = task.task.task.task.task.CreatedBy,
                        InstanceType = EventInstanceType.Task
                    }).SortBy(sortExpression, sortDirection)
                .Skip(startRowIndex.GetValueOrDefault())
                .Take(maximumRows.GetValueOrDefault());

            return tasksQueryable;
        }

        private int GetTasksListCount(bool excludeCompletedTask,
            bool excludeTaskWithNoDueDate, string username, IQueryable<string> colleagueGroups,
            IQueryable<string> teamGroups)
        {
            return Context.TaskInstances
                .GroupJoin(Context.User,
                    task => task.CompletedBy,
                    user => user.AccountName,
                    (task, completedByUser) => new { task, completedByUser })
                .GroupJoin(Context.User,
                    task => task.task.AssignedTo,
                    user => WorkflowConstants.UserPrefix + user.AccountName,
                    (task, assignedToUser) => new { task, assignedToUser })
                .SelectMany(task => task.task.completedByUser.DefaultIfEmpty(),
                    (task, completedByUser) => new { task, completedByUser })
                .SelectMany(task => task.task.assignedToUser.DefaultIfEmpty(),
                    (task, assignedToUser) => new { task, assignedToUser })
                .Where(task =>
                    (task.task.task.task.task.WorkflowInstance.Status != DomainConstants.WorkflowStatus.Cancelled ||
                     task.task.task.task.task.CompletedDate != null) &&
                    (!excludeCompletedTask || task.task.task.task.task.CompletedDate == null) &&
                    (!excludeTaskWithNoDueDate || task.task.task.task.task.DueDate > DateTime.MinValue) &&
                    (task.task.task.task.task.AssignedTo == username || string.IsNullOrEmpty(username) && (
                         colleagueGroups.Contains(task.task.task
                             .task.task.AssignedTo) ||
                         teamGroups.Contains(task.task.task.task
                             .task.AssignedTo)))).ToList().Count;
        }

        private IQueryable<string> GetColleaguesAccountNameQuery(long userId, string groupId, bool includeCurrentUserInResults)
        {
            long selectedGroupId = groupId.ParseAsLong();
            return
            from userGroup in Context.UserGroup
            join userGroup2 in Context.UserGroup on userGroup.GroupID equals userGroup2.GroupID
            join user in Context.User on userGroup2.UserID equals user.ID
            where (userGroup.UserID == userId
                   && userGroup.GroupID == (selectedGroupId == 0 ? userGroup.GroupID : selectedGroupId) //either the selected group or all groups
                   && userGroup2.UserID != (includeCurrentUserInResults ? -999 : userId) && !userGroup2.IsDeleted)  //exclude the specified userId if require - -999 is an invalid user id.
            select WorkflowConstants.UserPrefix + user.AccountName;
        }

        private IQueryable<string> GetGroupNameQuery(long userId, string groupId)
        {
            long selectedGroupId = groupId.ParseAsLong();

            return from userGroup in Context.UserGroup
                   join gp in Context.Group on userGroup.GroupID equals gp.ID
                   where (!userGroup.IsDeleted &&
                          userGroup.UserID == userId &&
                          userGroup.GroupID == (selectedGroupId == 0 ? userGroup.GroupID : selectedGroupId))  //either the specified group id or all group
                   select WorkflowConstants.GroupPrefix + gp.Name;

        }

        public List<TaskInstanceData> GetTaskList(long irisObjectID, bool excludeCompletedTask)
        {
            return GetTaskQueryForList(irisObjectID, excludeCompletedTask, false)
                .ToList();
        }

        public int GetTaskListCount(long irisObjectID, bool excludeCompletedTask)
        {
            return GetTaskQueryForListCount(irisObjectID, excludeCompletedTask, false);
        }

        public string GetTaskOutcomeMappedValue(long taskDefinitionID, string outcome)
        {
            var mapping = Context.TaskDefinitionOutcomeMappings.SingleOrDefault(m => m.TaskDefinitionID == taskDefinitionID && m.Outcome == outcome);
            if (mapping != null)
            {
                return mapping.MappedValue;
            }
            return null;
        }

        public string GetTaskOutcomeFromMappedValue(long taskDefinitionID, string mappedValue)
        {
            var mappings = Context.TaskDefinitionOutcomeMappings.Where(m => m.TaskDefinitionID == taskDefinitionID && m.MappedValue == mappedValue).ToList();
            if (mappings.Count == 1)
            {
                return mappings.First().Outcome;
            }
            return null;
        }

        /// <summary>
        /// Get the next imcomplete task with a due date set and has the earliest due date. and is not suspended
        /// </summary>
        /// <param name="irisObjectID"></param>
        /// <returns></returns>
        public TaskInstanceData GetTheEarliestDueIncompleteTaskInstance(long irisObjectID)
        {
            return GetTaskQueryForList(irisObjectID, true, true)  //ie. exclude completed task and exclude task with no due date
                .Where(t => t.WorkflowInstanceStatus != Datacom.Workflow.DomainModel.DomainConstants.WorkflowStatus.Suspended)
                .OrderBy(t => t.DueDate)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets a task list for a specified user (My Tasks)
        /// </summary>
        public List<TaskInstanceData> GetTaskList(User user, int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection)
        {
            //The full username including domain (e.g. DOMAIN\\User)
            // Note that we manually prefix here as we cannot use WorkflowHelper in a LINQ to SQL query
            //exclude completed task but include task with no due date
            return GetTasksList(true, false, WorkflowConstants.UserPrefix + user.AccountName,
                new List<string>().AsQueryable(), new List<string>().AsQueryable(),
                startRowIndex, maximumRows, sortExpression, sortDirection).ToList();
        }

        /// <summary>
        /// Gets a count of tasks for a specified user (My Tasks)
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int GetUserTaskListCount(User user)
        {
            return GetTasksListCount(true, false, WorkflowConstants.UserPrefix + user.AccountName,
                new List<string>().AsQueryable(), new List<string>().AsQueryable());
        }

        /// <summary>
        /// Gets a task list for a specified users colleagues' (Colleagues' Tasks)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="groupId">The group Id </param>
        /// <param name="startRowIndex"></param>
        /// <param name="maximumRows"></param>
        /// <returns></returns>
        public List<TaskInstanceData> GetTasksForColleagues(User user, string groupId, int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection)
        {
            long userId = user.ID;
            var colleaguesQuery = GetColleaguesAccountNameQuery(userId, groupId, false);

            //exclude completed task but include task with no due date
            return
                GetTasksList(true, false, string.Empty, colleaguesQuery, new List<string>().AsQueryable(),
                        startRowIndex, maximumRows, sortExpression, sortDirection)
                    .OrderBy(t => t.ID).ToList();
        }

        /// <summary>
        /// Gets a count of tasks for a specified users colleagues (Colleagues' Tasks)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public int GetColleaguesTaskListCount(User user, string groupId)
        {
            var colleaguesQuery = GetColleaguesAccountNameQuery(user.ID, groupId, false);

            return GetTasksListCount(true, false, string.Empty, colleaguesQuery, new List<string>().AsQueryable());
        }

        // Gets a task list for a specified users team (Team Tasks)
        public List<TaskInstanceData> GetTasksForTeam(User user, string groupId, int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection)
        {
            long userId = user.ID;
            var groupQuery = GetGroupNameQuery(userId, groupId);

            //exclude completed task but include task with no due date
            return GetTasksList(true, false, string.Empty, new List<string>().AsQueryable(), groupQuery,
                    startRowIndex, maximumRows, sortExpression, sortDirection)
                .ToList();
        }

        /// <summary>
        /// Gets a count of tasks for a specified users team (Team Tasks)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public int GetTeamsTaskListCount(User user, string groupId)
        {
            var groupQuery = GetGroupNameQuery(user.ID, groupId);

            //exclude completed task but include task with no due date
            return GetTasksListCount(true, false, string.Empty, new List<string>().AsQueryable(), groupQuery);
        }

        /// <summary>
        /// Gets all tasks (All Tasks)
        /// </summary>
        /// <param name="username">The full username including domain (e.g. DOMAIN\\User)</param>
        /// <param name="groupId">The group Id </param>
        /// <returns></returns>
        public List<TaskInstanceData> GetTasksForAll(User user, string groupId, int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection)
        {
            long userId = user.ID;
            var colleaguesQuery = GetColleaguesAccountNameQuery(userId, groupId, true);
            var groupQuery = GetGroupNameQuery(userId, groupId);
            return GetTasksList(true, false, string.Empty, colleaguesQuery, groupQuery, startRowIndex,
                    maximumRows, sortExpression, sortDirection) //exclude completed task but include task with no due date
                .ToList();
        }

        /// <summary>
        /// Gets a count of all tasks (All Tasks)
        /// </summary>
        /// <param name="user"></param>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public int GetAllTaskLitCount(User user, string groupId)
        {
            var colleaguesQuery = GetColleaguesAccountNameQuery(user.ID, groupId, true);
            var groupQuery = GetGroupNameQuery(user.ID, groupId);
            return GetTasksListCount(true, false, string.Empty, colleaguesQuery, groupQuery);
        }

        public TaskInstance GetLastTaskInstance()
        {
            var data = (from task in Context.TaskInstances
                        select
                            new
                            {
                                TaskInstance = task
                            }).AsEnumerable().LastOrDefault();
            return data.TaskInstance;
        }

        public string GetTaskInstanceNameByWorkflowInstanceId(long workflowInstanceId)
        {
            var data = (from task in Context.TaskInstances
                        where task.WorkflowInstanceID == workflowInstanceId
                        select new
                        {
                            TaskInstance = task
                        }).AsEnumerable().SingleOrDefault();

            if (data == null)
            {
                var workflowInstance = (from task in Context.WorkflowInstances
                                        join task2 in Context.WorkflowInstances on task.CalledByInstanceUID equals task2
                                            .WorkflowInstanceUID
                                        where task.ID == workflowInstanceId
                                        select new { WorkflowInstance = task2 }).AsEnumerable().FirstOrDefault();

                if (workflowInstance != null)
                {
                    var adhocWorkflowInstances = (from task in Context.WorkflowInstances
                                                  where task.CalledByInstanceUID == workflowInstance.WorkflowInstance.WorkflowInstanceUID && task.WorkflowDefinitionID == 1
                                                  select new
                                                  {
                                                      WorkflowInstance = task
                                                  }).OrderBy(x => x.WorkflowInstance.DateCreated).ToList();

                    if (adhocWorkflowInstances != null)
                    {
                        var adhocTaskInstances = (from task in Context.TaskInstances
                                                  where task.WorkflowInstanceID == workflowInstance.WorkflowInstance.ID
                                                        && task.WasAsynchronousFlag == true
                                                        && task.WorkflowHostName == "SpecialTaskHost"
                                                  select new
                                                  {
                                                      TaskInstance = task
                                                  }).OrderBy(x => x.TaskInstance.DateCreated).ToList();

                        data = adhocTaskInstances[adhocWorkflowInstances.FindIndex(x => x.WorkflowInstance.ID == workflowInstanceId)];
                    }

                }

            }
            return data == null ? "<blank>" : data.TaskInstance.Name;
        }
        public TaskInstance GetTaskByID(long taskID)
        {
            var data = (from task in Context.TaskInstances
                        join completedBy in Context.User on task.CompletedBy equals completedBy.AccountName into CompletedByUser
                        join assignedTo in Context.User on task.AssignedTo equals (WorkflowConstants.UserPrefix + assignedTo.AccountName) into AssignedToUser
                        join escalatedTo in Context.User on task.EscalateTo equals (WorkflowConstants.UserPrefix + escalatedTo.AccountName) into EscalatedToUser
                        join createdBy in Context.User on task.CreatedBy equals createdBy.AccountName into CreatedByUser
                        from completedUser in CompletedByUser.DefaultIfEmpty()
                        from assignedToUser in AssignedToUser.DefaultIfEmpty()
                        from createdByUser in CreatedByUser.DefaultIfEmpty()
                        from escalatedToUser in EscalatedToUser.DefaultIfEmpty()
                        where task.ID == taskID
                        select
                        new
                        {
                            TaskInstance = task,
                            AssignedToDisplay = (assignedToUser != null) ? assignedToUser.DisplayName : task.AssignedTo,
                            CompletedByDisplay = completedUser.DisplayName,
                            CreatedByDisplay = createdByUser.DisplayName,
                            EscalatedToDisplay = (escalatedToUser != null) ? escalatedToUser.DisplayName : task.EscalateTo,

                            //The following statements have the same effect as calling .Include(t => t.TaskDefinition) etc.
                            task.TaskDefinition,
                            task.TaskDefinition.DocumentTemplate,
                            task.IRISObject,
                            task.IRISObject.ObjectTypeREF,
                            task.IRISObject.SubClass1REF,
                            task.IRISObject.SubClass2REF,
                            task.IRISObject.SubClass3REF,
                            SecurityContextIRISObject = task.IRISObject.SecurityContextIRISObject,
                            SecurityContextIRISObjectTypeRef = task.IRISObject.SecurityContextIRISObject.ObjectTypeREF,
                            task.WorkflowInstance,
                            task.WorkflowInstance.WorkflowDefinition,
                            task.TaskBusinessTypeREF,
                            task.TaskInteractionTypeREF,
                        }).AsEnumerable().FirstOrDefault();

            var taskInstance = data.TaskInstance;

            //populate displaynames for the view screen
            taskInstance.AssignedToDisplayName = data.AssignedToDisplay;
            taskInstance.CompletedByDisplayName = data.CompletedByDisplay;
            taskInstance.CreatedByDisplayName = data.CreatedByDisplay;
            taskInstance.EscalatedToDisplayName = data.EscalatedToDisplay;
            return taskInstance.TrackAll();
        }

        public List<TaskInstance> GetTaskListByIDs(List<long> taskIDs)
        {
            var result = Context.TaskInstances
                 .Include(t => t.TaskDefinition)
                 .Include(t => t.IRISObject)
                 .Include(t => t.IRISObject.ObjectTypeREF)
                 .Include(t => t.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                 .Include(t => t.WorkflowInstance)
                 .Include(t => t.WorkflowInstance.WorkflowDefinition)
                 .Include(t => t.TaskBusinessTypeREF)
                 .Include(t => t.TaskInteractionTypeREF)
                 .Where(t => taskIDs.Contains(t.ID))
                 .ToList();

            // CR288 track the original dates for reschedule validation.
            result.ForEach(t =>
            {
                t.OriginalDueDate = t.DueDate;
                t.OriginalPromptDate = t.PromptDate;
            });

            return result.TrackAll();
        }

        public TaskDefinition GetTaskDefinitionByID(long taskDefinitionID)
        {
            return Context.TaskDefinitions.Single(td => td.ID == taskDefinitionID).TrackAll();
        }

        public TaskDefinition GetTaskDefinitionByName(string taskDefinitionName)
        {
            return Context.TaskDefinitions.Single(td => td.Name == taskDefinitionName);
        }

        public WorkflowDefinition GetWorkflowDefinitionByID(long workflowDefinitionID)
        {
            return Context.WorkflowDefinitions
                .Include(x => x.TaskDefinitions)
                .Include("TaskDefinitions.TaskInstance")
                .Single(td => td.ID == workflowDefinitionID).TrackAll();
        }

        public WorkflowDefinition GetWorkflowDefinitionById(long workflowDefinitionID)
        {
            return Context.WorkflowDefinitions
                .Include(x => x.TaskDefinitions)
                .Include("TaskDefinitions.TaskInstance")
                .Single(td => td.ID == workflowDefinitionID).TrackAll();
        }

        public WorkflowDefinition GetWorkflowDefinitionByName(string workflowName, string disambiguation)
        {
            return Context.WorkflowDefinitions.Where(row => row.Name == workflowName
                                                             && ((row.Disambiguation ?? "") == (disambiguation ?? "")))
                                              .FirstOrDefault();
        }

        public List<WorkflowDefinition> GetWorkflowDefinitionList(bool enabledOnly)
        {
            return (from w in Context.WorkflowDefinitions where !enabledOnly || w.IsEnabledFlag select w).ToList().TrackAll();
        }

        public List<string> GetWorkflowNamesForCallContext(string workflowCallContext, long irisObjectID)
        {
            IRISObject irisObject = Context.IRISObject
                                        .Include(i => i.ObjectTypeREF)
                                        .Include(i => i.SubClass1REF)
                                        .Include(i => i.SubClass2REF)
                                        .FirstOrDefault(i => i.ID == irisObjectID);
            if (irisObject != null)
            {
                List<WorkflowCallMapping> workflowCallMappings = GetWorkflowCallMappingList(workflowCallContext, irisObject);
                return workflowCallMappings
                        .Select(e => IRISWorkflowDefinition.CreateSafeName(e.WorkflowDefinition.Name, e.WorkflowDefinition.ID))
                        .ToList();
            }

            return new List<string>();
        }

        public List<WorkflowCallMapping> GetWorkflowCallMappingList(string workflowCallContextCode, IRISObject irisObject)
        {
            // lookup Call Context
            ReferenceDataValue callContextREF = Context.ReferenceDataValue
                                                    .Include(v => v.ReferenceDataCollection)
                                                    .Where(v => v.ReferenceDataCollection.Code == ReferenceDataCollectionCode.WorkflowCallContexts)
                                                    .Where(v => v.Code == workflowCallContextCode)
                                                    .FirstOrDefault();
            if (callContextREF == null)
            {
                throw new ArgumentException(string.Format("Call Context {0} not found", workflowCallContextCode));
            }

            // get all WorkflowCallMappings for CallContext / irisObject.ObjectType
            // Filter by Date!
            DateTime now = DateTime.Now.Date;
            List<WorkflowCallMapping> allWorkflowCallMappings = Context.WorkflowCallMappings
                                                                        .Include(e => e.WorkflowDefinition)
                                                                        .Where(e => e.WorkflowCallContextREFID == callContextREF.ID)
                                                                        .Where(e => e.ObjectTypeREFID == irisObject.ObjectTypeID)
                                                                        .Where(e => e.StartDate <= now && now <= (e.EndDate ?? now))
                                                                        .Where(e => e.WorkflowDefinition.IsEnabledFlag == true)
                                                                        .ToList();

            // The rules/pseduo code is:
            // If there is one or more call mapping(s) exist matching the irisobject's Object Type & SubClass1 & SubClass2, these matching workflow definition(s) will be invoked.
            // Else check if there is one or more call mapping(s) exist matching the iris object's Object Type & SubClass1. If yes, these matching workflow definition(s) will be invoked.
            // Otherwise check if there is one or more call mapping(s) exist matching the iris object's Object Type. If yes, these matching workflow definition(s) will be invoked.
            List<WorkflowCallMapping> resultWorkflowCallMappings = new List<WorkflowCallMapping>();
            resultWorkflowCallMappings = allWorkflowCallMappings.Where(e => e.SubClassification1REFID == irisObject.SubClass1ID)
                                                                .Where(e => e.SubClassification2REFID == irisObject.SubClass2ID)
                                                                .ToList();

            if (resultWorkflowCallMappings.Count == 0)
            {
                resultWorkflowCallMappings = allWorkflowCallMappings.Where(e => e.SubClassification1REFID == irisObject.SubClass1ID)
                                                                    .Where(e => !e.SubClassification2REFID.HasValue) // we don't want to invoke any workflow mapping to other subclass2
                                                                    .ToList();
            }
            else
            {
                return resultWorkflowCallMappings;
            }

            if (resultWorkflowCallMappings.Count == 0)
            {
                resultWorkflowCallMappings = allWorkflowCallMappings.Where(e => !e.SubClassification1REFID.HasValue) // we don't want to invoke any workflow mapping to other subclass1
                                                                    .Where(e => !e.SubClassification2REFID.HasValue) // we don't want to invoke any workflow mapping to other subclass2
                                                                    .ToList();
            }

            return resultWorkflowCallMappings;
        }

        // TODO: Move this to a helper
        private long? GetReferenceDataValueID(string collectionCode, string valueCode)
        {
            var collection = Context.ReferenceDataCollection.Include(r => r.ReferenceDataValues).Where(c => c.Code == collectionCode).FirstOrDefault();

            if (collection != null)
            {
                var value = collection.ReferenceDataValues.Where(v => v.Code == valueCode).FirstOrDefault();
                if (value != null)
                {
                    return value.ID;
                }
            }
            return null;
        }

        public TaskInstance CreateUserTask(
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
           )
        {
            long? taskTypeID = GetReferenceDataValueID(ReferenceDataCollectionCode.TaskTypes,
                                                       ReferenceDataValueCodes.TaskType.UserTask);

            //taskDefinition is _AdHocTask. Leave as is.
            var taskDefinition = GetTaskDefinitionByName(Datacom.Workflow.Constants.AdHocTaskName);

            TaskInstance taskInstance = new TaskInstance
            {
                IRISObjectID = irisObjectId,
                Name = name,
                Description = description,
                AssignedTo = assignTo,
                SendAssignedAlertFlag = sendAssignedAlertFlag,
                DueDate = dueDate,
                DoEscalationFlag = doEscalationFlag,
                EscalationDate = doEscalationFlag ? dueDate : (DateTime?)null,
                PromptDate = promptDate,
                SendPromptAlertFlag = sendPromptAlertFlag,
                CreatedBy = createdBy,
                Notes = notes,
                CompletedBy = completedBy,
                CompletedDate = completedDate,
                Outcome = WorkflowConstants.Outcomes.Completed,
                TaskInteractionTypeREFID = interactionType,
                TaskBusinessTypeREFID = (long)taskTypeID,
                TaskDefinition = taskDefinition,
                CompletedTime = completedTime
            };
            Context.TaskInstances.AddObject(taskInstance);
            Context.SaveChanges();
            ////this is no longer required because securitycheck method is set to none to bypass data security check for this Create method.
            ////populate security context for returning data security check.  
            //taskInstance.IRISObject = (Context.IRISObject
            //                                  .Include(x => x.ObjectTypeREF)
            //                                  .Include(x => x.SecurityContextIRISObject.ObjectTypeREF)
            //                                  .Where(x => x.ID == irisObjectId)).Single();

            return taskInstance;
        }

        public List<WorkflowCallInstance> GetRunningWorkflowsForObjectAndCallContext(long irisObjectID, string workflowCallContextCode)
        {
            var workflowCallInstances = from callInstance in Context.WorkflowCallInstances.Where(i => i.IRISObjectID == irisObjectID)
                                        from irisObject in Context.IRISObject.Where(o => o.ID == callInstance.IRISObjectID)
                                        from workflowCallContext in Context.ReferenceDataValue.Where(v => callInstance.WorkflowCallContextREFID == v.ID && v.Code == workflowCallContextCode)
                                        from workflowDefinition in Context.WorkflowDefinitions.Where(d => callInstance.WorkflowDefinitionID == d.ID)
                                        from workflowInstance in Context.WorkflowInstances.Where(i => workflowDefinition.ID == i.WorkflowDefinitionID && irisObjectID == i.IRISObjectID)
                                        from instance in Context.InstancesTable.Where(i => workflowInstance.WorkflowInstanceUID == i.Id).Where(i => i.IsCompleted != true)
                                        select new { callInstance, irisObject, workflowCallContext, workflowDefinition, workflowInstance };

            return workflowCallInstances.Select(i => i.callInstance).ToList();
        }

        public WorkflowCallInstance CreateWorkflowCallInstance(long irisObjectID, long workflowCallContextID, long workflowDefinitionID)
        {
            WorkflowCallInstance workflowCallInstance = new WorkflowCallInstance()
            {
                EventFiredDateTime = DateTime.Now,
                IRISObjectID = irisObjectID,
                WorkflowCallContextREFID = workflowCallContextID,
                WorkflowDefinitionID = workflowDefinitionID
            };
            Context.WorkflowCallInstances.AddObject(workflowCallInstance);
            Context.SaveChanges();

            return workflowCallInstance;
        }

        public List<TaskDefinition> GetTaskDefinitionList(long workflowDefinitionID)
        {
            var taskDefinitions = (from taskDefinition in Context.TaskDefinitions
                                   where taskDefinition.WorkflowDefinitionID == workflowDefinitionID
                                   select taskDefinition);

            return taskDefinitions.ToList();
        }

        public List<EmailTemplate> GetEmailTemplateList()
        {
            // TODO (CHRIS): Consider creating simple ID, Name object for generic drop downlist population
            var emailTemplates = from emailTemplate in Context.EmailTemplates
                                 select emailTemplate;

            return emailTemplates.ToList();
        }

        public List<WorkflowRunningInstance> GetWorkflowRunningInstanceListByDomainObjectID(long IRISObjectID, bool includeUserTask)
        {
            var runningInstances = from WorkflowRunningInstance in Context.WorkflowRunningInstances
                                   where WorkflowRunningInstance.IRISObjectID == IRISObjectID &&
                                         WorkflowRunningInstance.WorkflowInstanceStatus != Workflow.DomainModel.DomainConstants.WorkflowStatus.Cancelled &&
                                         WorkflowRunningInstance.WorkflowInstanceStatus != Workflow.DomainModel.DomainConstants.WorkflowStatus.Closed &&
                                         (includeUserTask || WorkflowRunningInstance.IsUserTask == false)
                                   select WorkflowRunningInstance;

            return runningInstances.ToList();
        }

        public List<WorkflowRunningInstance> GetWorkflowRunningInstanceListByIRISObjectID(long irisObjectID, bool includeAll, bool includeSpecialTask = false)
        {

            var runningInstances = from instance in Context.WorkflowRunningInstances
                                   where (includeSpecialTask || instance.WorkflowType != Workflow.DomainModel.DomainConstants.WorkflowTypes.SpecialTask) &&
                                         instance.IRISObjectID == irisObjectID
                                   select instance;

            if (!includeAll)
                runningInstances = from instance in runningInstances
                                   where instance.WorkflowInstanceStatus != Workflow.DomainModel.DomainConstants.WorkflowStatus.Cancelled &&
                                         instance.WorkflowInstanceStatus != Workflow.DomainModel.DomainConstants.WorkflowStatus.Closed
                                   select instance;

            return runningInstances.OrderByDescending(row => row.DateCreated).ToList();
        }

        public List<WorkflowRunningInstance> GetWorkflowRunningInstanceList(string textFilter, string statusFilter, bool includeSpecialTask = false)
        {
            var runningInstances = from WorkflowRunningInstance in Context.WorkflowRunningInstances
                                   where (includeSpecialTask || WorkflowRunningInstance.WorkflowType != Workflow.DomainModel.DomainConstants.WorkflowTypes.SpecialTask)
                                   select WorkflowRunningInstance;

            // Apply text based filter to across selection of text based fields
            if (!string.IsNullOrWhiteSpace(textFilter))
            {
                var filteredByText = from filteredByTextRow in runningInstances
                                     where (filteredByTextRow.LinkDetails.Contains(textFilter)
                                         || filteredByTextRow.WorkflowDefinitionName.Contains(textFilter)
                                         || filteredByTextRow.RestartPoint.Contains(textFilter)
                                         || filteredByTextRow.BusinessID.Contains(textFilter))
                                     select filteredByTextRow;
                runningInstances = filteredByText;
            }

            // Apply status filter 
            if (!string.IsNullOrWhiteSpace(statusFilter))
            {
                var filteredByStatus = from filteredByStatusRow in runningInstances
                                       where statusFilter.Contains(filteredByStatusRow.WorkflowInstanceStatus)
                                       select filteredByStatusRow;

                runningInstances = filteredByStatus;
            }

            // Most recent first
            var orderResults = runningInstances.OrderByDescending(row => row.DateCreated);

            // Limit query, as filter should be used to bring back a managable list
            var limitedResults = orderResults.Take(WorkflowQueryLimit);

            return limitedResults.ToList();
        }

        public WorkflowRunningInstance GetWorkflowRunningInstancebyID(long workflowInstanceID, long? irisObjectID)
        {
            var result = from instance in Context.WorkflowRunningInstances
                         where instance.WorkflowInstanceID == workflowInstanceID &&
                               (irisObjectID == null || instance.IRISObjectID == irisObjectID.Value)
                         select instance;
            return result.First();
        }

        public List<WorkflowDefinitionsData> GetWorkflowsForIRISObject(long irisObjectID)
        {
            DateTime now = DateTime.Now.Date;

            var workflowDefinitions =
                from o in Context.IRISObject.Where(ob => ob.ID == irisObjectID)
                from we in Context.WorkflowCallMappings.Where(w => (w.ObjectTypeREFID == o.ObjectTypeID && w.SubClassification1REFID == o.SubClass1ID
                && w.SubClassification2REFID == o.SubClass2ID) ||
                (w.ObjectTypeREFID == o.ObjectTypeID && w.SubClassification1REFID == o.SubClass1ID && w.SubClassification2REFID == null) ||
                (w.ObjectTypeREFID == o.ObjectTypeID && w.SubClassification1REFID == null && w.SubClassification2REFID == null))
                from wd in Context.WorkflowDefinitions.Where(d => we.WorkflowDefinitionID == d.ID)
                where o.ID == irisObjectID
                    && we.StartDate <= now
                    && (we.EndDate == null || we.EndDate >= now)
                select new WorkflowDefinitionsData()
                {
                    ID = wd.ID
                    ,
                    WorkflowDefinitionName = wd.Name
                    ,
                    WorkflowDefinitionDisambiguation = wd.Disambiguation
                };

            return workflowDefinitions.ToList();
        }

        public List<WorkflowTaskDefinitionsData> GetWorkflowRestartPointsForIRISObject(long irisObjectID)
        {
            DateTime now = DateTime.Now.Date;

            var workflowTaskDefinitions =
                from o in Context.IRISObject
                join we in Context.WorkflowCallMappings on new { ObjectTypeID = o.ObjectTypeID, SubClass1ID = o.SubClass1ID, SubClass2ID = o.SubClass2ID }
                                          equals new { ObjectTypeID = we.ObjectTypeREFID, SubClass1ID = we.SubClassification1REFID, SubClass2ID = we.SubClassification2REFID }
                join wd in Context.WorkflowDefinitions on we.WorkflowDefinitionID equals wd.ID
                join td in Context.TaskDefinitions on wd.ID equals td.WorkflowDefinitionID
                where o.ID == irisObjectID
                    && we.StartDate <= now
                    && (we.EndDate == null || we.EndDate >= now)
                select new WorkflowTaskDefinitionsData
                {
                    WorkflowDefinitionID = wd.ID
                    ,
                    TaskDefinitionID = td.ID
                    ,
                    TaskDefinitionName = td.Name
                };

            // Create a (Beginning) start point for every workflow and add to the result
            List<WorkflowDefinitionsData> workflowDefinitions = GetWorkflowsForIRISObject(irisObjectID);

            var beginningRows = from row in workflowDefinitions
                                select new WorkflowTaskDefinitionsData
                                {
                                    WorkflowDefinitionID = row.ID
                                    ,
                                    TaskDefinitionID = WorkflowConstants.RestartAtBeginningID
                                    ,
                                    TaskDefinitionName = WorkflowConstants.RestartAtBeginningText
                                };

            var allRows = workflowTaskDefinitions.ToList().Union(beginningRows);

            return allRows.OrderBy(row => row.WorkflowDefinitionID)
                          .ThenBy(row => row.TaskDefinitionID).ToList();
        }

        public List<TaskDefinition> CopyWorkflowTaskDefinitions(long sourceWorkflowDefinitionID, long targetWorkflowDefinitionID, string modifiedBy, DateTime lastModified)
        {
            Context.Workflow_CopyTaskDefinitions(sourceWorkflowDefinitionID, targetWorkflowDefinitionID, modifiedBy, lastModified);


            // retrun the copied tasks
            var taskDefinitions = from taskDefinition in Context.TaskDefinitions
                                  where taskDefinition.WorkflowDefinitionID == targetWorkflowDefinitionID
                                  select taskDefinition;

            return taskDefinitions.ToList();
        }

        public List<WorkflowCallMapping> CopyWorkflowCallMappings(long sourceWorkflowDefinitionID, long targetWorkflowDefinitionID, DateTime startDate, string createdBy, DateTime dateCreated)
        {
            Context.Workflow_CopyCallMappings(sourceWorkflowDefinitionID, targetWorkflowDefinitionID, startDate, createdBy, dateCreated);

            // Return the copied call mappings
            var workflowCallMappings = from workflowCallMapping in Context.WorkflowCallMappings
                                       where workflowCallMapping.WorkflowDefinitionID == targetWorkflowDefinitionID
                                       select workflowCallMapping;

            return workflowCallMappings.ToList();
        }

        public WorkflowCallMapping GetWorkflowCallMappingByID(long workflowCallMappingID)
        {
            return Context.WorkflowCallMappings.Single(workflowCallMapping => workflowCallMapping.ID == workflowCallMappingID).TrackAll();
        }

        /// <summary>
        /// Get a list of workflow call mapping data for the Workflow call Mappings screen.        
        /// </summary>
        /// <param name="showExpired">Whether to include WorkflowCallMapping that has a past end date.</param>
        /// <returns></returns>
        public List<WorkflowCallMappingData> GetWorkflowCallMappingDataList(bool showExpired)
        {
            IQueryable<WorkflowCallMapping> queryable = showExpired ?
                                                  Context.WorkflowCallMappings :
                                                  Context.WorkflowCallMappings.Where(wfe => (wfe.EndDate >= DateTime.Today.Date || wfe.EndDate == null));

            return queryable
                    .Select(workflowCallMapping => new WorkflowCallMappingData
                    {
                        WorkflowCallMappingID = workflowCallMapping.ID,
                        ObjectTypeDisplayName = workflowCallMapping.ObjectTypeREF.DisplayValue,
                        SubClass1DisplayName = workflowCallMapping.SubClassification1REF.DisplayValue,
                        SubClass2DisplayName = workflowCallMapping.SubClassification2REF.DisplayValue,
                        WorkflowCallContextDisplayName = workflowCallMapping.WorkflowCallContextREF.DisplayValue,
                        WorkflowDefinitionName = workflowCallMapping.WorkflowDefinition.Name,
                        WorkflowDefinitionDisambiguation = workflowCallMapping.WorkflowDefinition.Disambiguation,
                        WorkflowCallMappingStartDate = workflowCallMapping.StartDate,
                        WorkflowCallMappingEndDate = workflowCallMapping.EndDate
                    })
                    .OrderBy(x => x.ObjectTypeDisplayName)
                    .ToList();
        }

        public List<WorkflowDefinitionsData> GetWorkflowDefinitionDataList()
        {
            return (from wd in Context.WorkflowDefinitions
                    join referenceDataValue in Context.ReferenceDataValue on wd.DomainObjectTypeName equals referenceDataValue.Code
                    where referenceDataValue.ReferenceDataCollection.Code == ReferenceDataCollectionCode.IrisObjects
                    select new WorkflowDefinitionsData
                    {
                        ID = wd.ID,
                        WorkflowDefinitionName = wd.Name,
                        WorkflowDefinitionDisambiguation = wd.Disambiguation,
                        ParentValueID = referenceDataValue.ID  //related ObjectType ReferenceDataValue
                    }).ToList();
        }

        public List<WorkflowRunningInstance> GetAsyncWorkflowRunningInstance(WorkflowRunningInstance parentInstance)
        {

            var runningInstances = from instance in Context.WorkflowInstances
                                   join runningInstance in Context.WorkflowRunningInstances on instance.ID equals runningInstance.WorkflowInstanceID
                                   join definition in Context.WorkflowDefinitions on instance.WorkflowDefinitionID equals definition.ID
                                   where instance.IRISObjectID == parentInstance.IRISObjectID &&
                                         instance.Status != Workflow.DomainModel.DomainConstants.WorkflowStatus.Cancelled &&
                                         instance.Status != Workflow.DomainModel.DomainConstants.WorkflowStatus.Closed &&
                                         definition.Name != WorkflowConstants.AdHocTaskName &&
                                         instance.CalledByInstanceUID == parentInstance.WorkflowInstanceUID
                                   select runningInstance;


            return runningInstances.ToList();
        }

        public List<WorkflowRunningInstance> GetOrphanAsyncWorkflowRunningInstanceCleanUpList()
        {
            var asyncInstances = from asyncInstance in Context.WorkflowInstances
                                 join parentInstance in Context.WorkflowInstances on asyncInstance.CalledByInstanceUID equals parentInstance.WorkflowInstanceUID
                                 join asyncRunningInstance in Context.WorkflowRunningInstances on asyncInstance.ID equals asyncRunningInstance.WorkflowInstanceID
                                 join asyncWorkflowDeifnition in Context.WorkflowDefinitions on asyncInstance.WorkflowDefinitionID equals asyncWorkflowDeifnition.ID
                                 where parentInstance.Status == Workflow.DomainModel.DomainConstants.WorkflowStatus.Cancelled &&
                                       asyncInstance.Status != Workflow.DomainModel.DomainConstants.WorkflowStatus.Cancelled &&
                                       asyncInstance.Status != Workflow.DomainModel.DomainConstants.WorkflowStatus.Closed &&
                                       asyncWorkflowDeifnition.Name == WorkflowConstants.AdHocTaskName
                                 select asyncRunningInstance;

            return asyncInstances.ToList();
        }

        public List<long> GeIRISObjectIDsHasWorkflowRunningInstnace(List<long> irisObjectIDsList)
        {
            return Context.WorkflowInstances.Where(x => irisObjectIDsList.Contains(x.IRISObjectID))
                    .Select(x => x.IRISObjectID)
                    .ToList();
        }
    }
}
