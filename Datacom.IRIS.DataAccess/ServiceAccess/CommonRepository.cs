using Datacom.IRIS.Common;
using Datacom.IRIS.Common.Utils;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.DTO;
using Datacom.Workflow.DomainModel;
using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Linq;
using System.Transactions;
using Datacom.IRIS.DomainModel.Domain.Constants;
using System.Web.UI.WebControls;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class CommonRepository : RepositoryStore, ICommonRepository
    {
        /// <summary>
        ///    This is a utility method that is used by the business layer for validation purposes.
        ///    It will not be exposed publicly outside the business layer, and hence it has no security
        ///    check attributes
        /// </summary>
        public List<IRISObject> GetIrisObjects(params long[] idValues)
        {
            return Context.IRISObject
                .Include(c => c.ObjectTypeREF)
                .Include(c => c.SubClass1REF)
                .Include(c => c.SubClass2REF)
                .Include(c => c.SubClass3REF)
                .Include("ActivityObjectRelationships.ContactSublink")
                .Where(i => idValues.Contains(i.ID))
                .ToList().TrackAll();
        }

        public IRISObject GetIrisObject(long id)
        {
            return Context.IRISObject
                .Include(c => c.ObjectTypeREF)
                .Include(c => c.SecurityContextIRISObject)
                .Include(c => c.SecurityContextIRISObject.ObjectTypeREF)
                   .Where(i => id == i.ID).FirstOrDefault();
        }

        public Note GetNoteByID(long id)
        {
            var note = Context.Note
                .Include(n => n.IRISObject)
                .Include(n => n.IRISObject.ObjectTypeREF)
                .Include(n => n.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                .Include(n => n.TypeREF)
                .Include(n => n.ByUser)
                .Where(n => n.ID == id && !n.IsDeleted)
                .Single().TrackAll();

            note.ModifiedByDisplayName = Context.User.Single(x => x.AccountName == note.ModifiedBy).DisplayName;
            return note;
        }

        public List<Note> GetNotesByIrisObjectID(long id)
        {
            var notes = Context.Note
                .Include(n => n.IRISObject)
                .Where(n => n.IRISObject.ID == id && !n.IsDeleted).ToList();

            return notes;
        }

        public List<EventInstanceData> GetEventList(long irisObjectID, bool toDoTasksOnly)
        {
            List<EventInstanceData> eventList = GetTaskQueryForListByIrisObject(irisObjectID, toDoTasksOnly);

            if (!toDoTasksOnly)
                eventList.AddRange(GetNoteQueryForListByIrisObject(irisObjectID).ToList());

            return eventList;
        }
        
        /// <summary>
        /// Overloaded method that implements sorting. Used by events grid in ViewObjectEvents control. 
        /// </summary>
        /// <param name="irisObjectID"></param>
        /// <param name="toDoTasksOnly"></param>
        /// <param name="startRowIndex"></param>
        /// <param name="maximumRows"></param>
        /// <param name="sortExpression"></param>
        /// <param name="sortDirection"></param>
        /// <returns></returns>
        public List<EventInstanceData> GetEventList(long irisObjectID, bool toDoTasksOnly, int? startRowIndex, int? maximumRows, string sortExpression, SortDirection sortDirection)
        {
            List<EventInstanceData> eventList = GetTaskQueryForListByIrisObject(irisObjectID, toDoTasksOnly);

            if (!toDoTasksOnly)
                eventList.AddRange(GetNoteQueryForListByIrisObject(irisObjectID).ToList());

            return eventList.SortBy(sortExpression, sortDirection)
                .Skip(startRowIndex.GetValueOrDefault())
                    .Take(maximumRows.GetValueOrDefault())
                    .ToList(); 
        }

        public int GetEventListCount(long irisObjectID, bool toDoTasksOnly)
        {
            int count = GetTaskQueryForListByIrisObjectCount(irisObjectID, toDoTasksOnly);

            if (!toDoTasksOnly)
                count += GetNoteQueryForListByIrisObjectCount(irisObjectID);

            return count;
        }

        public List<TaskInstanceData> GetTaskList(long irisObjectID, bool toDoTasksOnly)
        {
            List<TaskInstanceData> taskList = RepositoryMap.WorkflowRepository.GetTaskList(irisObjectID, toDoTasksOnly); 

            return taskList;
        }

        public Dictionary<string, string> GetIrisMessages()
        {
            return (from i in Context.IRISMessage select i).FromCached(CacheConstants.IrisErrorMessages).ToDictionary(u => u.Code, u => u.Message);
        }

        public List<Favourite> GetFavouritesByUser(string loginName)
        {
            return (from n in Context.Favourite where n.UserLoginName == loginName && !n.IsDeleted select n).ToList().TrackAll();
        }

        public void DeleteFavouritesForObject(string objectType, long objectID)
        {
            Context.Favourite.ForEach(f =>
            {
                if (f.Type == objectType && f.QueryString.Split('=').Contains(objectID.ToString()))
                {
                    f.IsDeleted = true;
                }
            });

            Context.SaveChanges();
        }

        public List<AppSetting> GetAllAppSettings()
        {
            return Context.AppSetting.FromCached(CacheConstants.AppSettings).ToList();
        }

        public long? GetObjectIDFromBusinessID(string businessID)
        {
            var obj = Context.IRISObject.SingleOrDefault(o => o.BusinessID == businessID);
            return obj == null ? (long?)null : obj.LinkID.Value;
        }

        public AppSetting GetAppSettingById(long id)
        {
            return (from n in Context.AppSetting where n.ID == id select n).SingleOrDefault().TrackAll();
        }

        public AppSetting GetAppSettingByKey(string key)
        {
            key = key.ToLower();
            return GetAllAppSettings().Single(a => a.Key.ToLower() == key);  //make use of GetAllAppSettings() as it is a cahced
        }

        public Status GetLatestStatusByIrisId(long irisObjectId)
        {
            return Context.Statuses
                .Include(s => s.StatusREF)
                .Include(s => s.IRISObject.ObjectTypeREF)
                .Include(s => s.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                .Where(s => s.IRISObjectID == irisObjectId && !s.IsDeleted)
                .OrderByDescending(s => s.StatusDate)
                .ThenByDescending(a => a.ID)
                .Take(1)
                .SingleOrDefault();
        }

        public Status GetStatusByID(long statusId)
        {
            var dbquery = from status in Context.Statuses.Where(s => s.ID == statusId && !s.IsDeleted)
                          from refData in Context.ReferenceDataValue.Where(r => r.ID == status.StatusREFID)
                          from irisObject in Context.IRISObject.Where(i => i.ID == status.IRISObjectID)
                          from objectTypeRef in Context.ReferenceDataValue.Where(o => o.ID == status.IRISObject.ObjectTypeID)
                          from user in Context.User
                            .Where(u => u.AccountName == status.CreatedBy)
                            .DefaultIfEmpty()
                          select new
                          {
                              status,
                              irisObject,
                              refData,
                              user,
                              objectTypeRef,
                              irisObject.SecurityContextIRISObject,
                              irisObject.SecurityContextIRISObject.ObjectTypeREF
                          };

            return dbquery.AsEnumerable().Select(anon =>
            {
                anon.status.CreatedByDisplayName = anon.user != null ? anon.user.DisplayName : anon.status.CreatedBy;
                return anon.status;
            })
            .Distinct().Single().TrackAll();
        }

        public List<Status> GetStatusesByID(long irisObjectId)
        {
            // NOTE: sorting order is defined at each per gridview
            var dbquery = from status in Context.Statuses
                              .Where(s => s.IRISObjectID == irisObjectId && !s.IsDeleted)
                          from refData in Context.ReferenceDataValue.Where(r => r.ID == status.StatusREFID)
                          from irisObject in Context.IRISObject.Where(i => i.ID == status.IRISObjectID)
                          from user in Context.User
                            .Where(u => u.AccountName == status.CreatedBy)
                            .DefaultIfEmpty()
                          select new
                          {
                              status,
                              irisObject,
                              refData,
                              user,
                              irisObject.SecurityContextIRISObject,
                              irisObject.SecurityContextIRISObject.ObjectTypeREF
                          };

            return dbquery.AsEnumerable().Select(anon =>
            {
                anon.status.CreatedByDisplayName = anon.user != null ? anon.user.DisplayName : anon.status.CreatedBy;
                return anon.status;
            })
            .Distinct().ToList();
        }

        private static object _lockObject = new object();

        public string GetNextBusinessID(long objectTypeID, bool isFinancial = false)
        {
            //TODO - this should use transaction instead of lock as we need to cater for web farm scenario.
            //TODO - evaluate whether we need both lock and transactionscope
            lock (_lockObject)
            {
                IRISBusinessIDPattern businessIdPatternEntity = null;

                bool success = TransactionScopedQuery(() =>
                {
                    businessIdPatternEntity = Context.IRISBusinessIDPattern
                        .SingleOrDefault(p => p.ObjectTypeID == objectTypeID && p.IsFinancial == isFinancial);

                    if (businessIdPatternEntity != null)
                    {
                        businessIdPatternEntity.Counter += 1;
                        ApplyEntityChanges(businessIdPatternEntity);
                    }
                });

                return success && businessIdPatternEntity != null ? businessIdPatternEntity.GenerateBusinessID() : null;
            }
        }

        private static object _lockObjectFinCustCode = new object();
        public string GetNextFinancialCustomerCode()
        {
            //Note: We use a lock to ensure that 2 codes generated at the same time do not have the same reference number
            lock (_lockObjectFinCustCode)
            {
                Sequence sequence = null;
                bool success = TransactionScopedQuery(() =>
                {
                    sequence = Context.Sequence.Single(s => s.Key == SequencesCodes.NextFinancialCustomerCode).TrackAll();
                    if (sequence != null)
                    {
                        sequence.Value += 1;
                        ApplyEntityChanges(sequence);
                    }
                });

                return success && sequence != null ? sequence.Value.ToString() : null;
            }
        }

        public string GetNextFinancialCustomerCodeForJFC()
        {
            //ie. the same call as GetNextFinancialCustomerCode(). Two interfaces are created because of different security context check.
            return GetNextFinancialCustomerCode();
        }

        public bool IsBusinessIDUnique(long currentIrisID, string businessID)
        {
            return (from i in Context.IRISObject where i.BusinessID == businessID && i.ID != currentIrisID select i).Count() == 0;
        }

        public bool IsSymbolNameUnique(long currentQuestionDefinitionID, string symbolName)
        {
            return !(from qd in Context.QuestionDefinition.Where(qd => qd.SymbolName == symbolName && qd.ID != currentQuestionDefinitionID && !qd.IsDeleted)
                     from lqd in Context.CDFListQuestionDefinition.Where(x => x.QuestionDefinitionID == qd.ID)
                     from list in Context.CDFList.Where(l => !l.IsDeleted && l.ID == lqd.CDFListID)
                     select qd).Any();
        }

        public bool IsCDFTaskDefinitionUnique(long cdfListID, List<string> taskDefIDs)
        {
            var query = Context.CDFList.Where(l => !l.IsDeleted && l.ID != cdfListID && l.TaskDefinitionIDs != null && l.TaskDefinitionIDs.Length > 0).Select(x => x.TaskDefinitionIDs).ToList();
            return !query.SelectMany(x => x.Split('|')).Any(taskDefIDs.Contains);
        }

        public bool IsCDFTaskNameUnique(long cdfListID, string name)
        {
            return !Context.CDFList.Any(l => l.ID != cdfListID && !l.IsDeleted && l.ListName == name && l.TaskDefinitionIDs != null && l.TaskDefinitionIDs.Length > 0);
        }

        public bool IsAuthorisationIRISIDUnique(long currentIrisID, long currentActivityID, string authorisationIRISID)
        {
            bool uniqueInIrisObject = (from i in Context.IRISObject
                                       from a in Context.Authorisations.Where(a => a.IRISObjectID == i.ID).DefaultIfEmpty()
                                       where i.ID != currentIrisID
                                       && i.BusinessID == authorisationIRISID &&
                                       (
                                            (a.ActivityID == null)
                                            // if the authorisation is created from an activity, the Activity.AuthorisationIRISID can be the same as the Authorisation.IRISBusinessID [CR283]
                                            || (a.ActivityID != null && a.ActivityID != currentActivityID)
                                          )
                                       select i).Count() == 0;
            bool uniqueInActivity = (from i in Context.Activities where i.AuthorisationIrisID == authorisationIRISID && i.ID != currentActivityID select i).Count() == 0;
            return uniqueInIrisObject && uniqueInActivity;
        }

        public bool IsFavoriteTitleUnique(Favourite favourite)
        {
            return (from i in Context.Favourite where i.IsDeleted == false && i.Title.ToLower() == favourite.Title.ToLower() && i.UserLoginName == favourite.UserLoginName select i).Count() == 0;
        }

        public bool IsFINCustCodeUnique(long currentIrisID, string finCustCode)
        {
            return (from i in Context.IRISObject where i.FINCustCode == finCustCode && i.ID != currentIrisID select i).Count() == 0;
        }

        public bool IsFINProjectCodeUnique(long currentIrisID, string finProjectCode)
        {
            return (from i in Context.IRISObject where i.FINProjectCode == finProjectCode && i.ID != currentIrisID select i).Count() == 0;
        }

        public void CreateSystemNote(Note systemNote)
        {
            IRISContext context = new IRISContext();
            context.Note.AddObject(systemNote);
            context.AutomaticallySetAuditFields = false;
            context.SaveChanges();
        }

        public bool GroupHasFunctionToObjectType(long groupID, string objectTypeCode)
        {
            var functionQuery = from grouPermission in Context.GroupPermission.Where(gp => !gp.IsDeleted && gp.GroupID == groupID)
                                from function in Context.Function.Where(f => f.ID == grouPermission.FunctionID && !f.IsDeleted)
                                from objectTypeREF in Context.ReferenceDataValue.Where(r => r.Code == objectTypeCode && r.ID == function.ObjectID)
                                select new { function };
            return functionQuery.Any();
        }

        public List<KeyValuePair<long, string>> GetActiveSecurityGroupForUser(long userID, List<long> irisObjectIdsWithOfficerResponsible)
        {
            // list of groups the user belongs to
            var groupQuery = from userGroup in Context.UserGroup.Where(ug => ug.UserID == userID && !ug.IsDeleted)
                             from gp in Context.Group.Where(g => g.ID == userGroup.GroupID && !g.IsDeleted)
                             select new { gp };

            var groups = groupQuery.AsEnumerable().Select(x => x.gp).ToList();

            List<KeyValuePair<long, string>> result = new List<KeyValuePair<long, string>>();
            groups.ForEach(g =>
            {
                var activeRecordCount = GetActiveRecordCountForSecurityGroup(g.ID, irisObjectIdsWithOfficerResponsible);
                var activeTaskCount = GetTaskCountForSecurityGroup(g.ID, userID);
                if (activeRecordCount != 0 || activeTaskCount != 0)
                {
                    result.Add(new KeyValuePair<long, string>(g.ID, string.Format("{0} ({1} {2} + {3} {4})", g.Name, activeRecordCount, activeRecordCount > 1 ? "Records" : "Record", activeTaskCount, activeTaskCount > 1 ? "Tasks" : "Task")));
                }
            });
            return result.OrderBy(x => x.Value).ToList();
        }

        private int GetTaskCountForSecurityGroup(long groupID, long userID)
        {
            // get the list of object types the groups has permmision for
            var query = from groupPermission in Context.GroupPermission.Where(gp => gp.GroupID == groupID && !gp.IsDeleted)
                        from permission in Context.Permission.Where(p => (p.Name == PermissionNames.Edit || p.Name == PermissionNames.Event) && p.ID == groupPermission.PermissionID)
                        from function in Context.Function.Where(f => f.ID == groupPermission.FunctionID && !f.IsDeleted)
                        select new { function };
            var functions = query.Select(x => x.function).Distinct().ToList();

            // incompleted tasks assigned to the user
            var tasksQuery = from user in Context.User.Where(x => x.ID == userID)
                             from task in Context.TaskInstances.Where(x => x.AssignedTo == ("User:" + user.AccountName) && !x.CompletedDate.HasValue)
                             from wf in Context.WorkflowInstances.Where(w => w.ID == task.WorkflowInstanceID && (w.Status == DomainConstants.WorkflowStatus.Active || w.Status == DomainConstants.WorkflowStatus.Waiting))
                             from obj in Context.IRISObject.Where(o => task.IRISObjectID == o.ID)
                             from securityContextObj in Context.IRISObject.Where(s => s.ID == obj.SecurityContextIRISObjectID).DefaultIfEmpty()
                             select new { task, obj, securityContextObj };



            int count = 0;
            tasksQuery.ForEach(o =>
            {
                var securityContext = o.securityContextObj ?? o.obj;
                if (functions.Any(f => FunctionAppliesToObject(f, securityContext)))
                {
                    count++;
                }
            });
            return count;
        }

        private int GetActiveRecordCountForSecurityGroup(long groupID, List<long> irisObjectIdsWithOfficerResponsible)
        {
            // get the list of object types the groups has permmision for
            var query = from groupPermission in Context.GroupPermission.Where(gp => gp.GroupID == groupID && !gp.IsDeleted)
                        from permission in Context.Permission.Where(p => p.Name == PermissionNames.Edit && p.ID == groupPermission.PermissionID)
                        from function in Context.Function.Where(f => f.ID == groupPermission.FunctionID && !f.IsDeleted)
                        select new { function };
            var functions = query.Select(x => x.function).Distinct().ToList();

            // filter irisObjectIdsWithOfficerResponsible with 'Open' Status and include security context object
            var activeObjects = from obj in Context.IRISObject.Where(x => irisObjectIdsWithOfficerResponsible.Contains(x.ID))
                                from latestStatus in
                                    (from status in Context.Statuses
                                     where !status.IsDeleted && status.IRISObjectID == obj.ID
                                     orderby status.StatusDate descending, status.StatusTime descending, status.ID descending
                                     select status
                                    ).Take(1).DefaultIfEmpty()
                                from latestStatusREF in Context.ReferenceDataValue.Where(r => r.ID == latestStatus.StatusREFID).DefaultIfEmpty()
                                from statusRefAttrib in Context.ReferenceDataValueAttribute.Where(a => a.ValueID == latestStatusREF.ID && a.AttrName == ReferenceDataValueAttributeCodes.IsOpenStatus && a.AttrValue.ToLower() == "true").DefaultIfEmpty()
                                from securityContextObj in Context.IRISObject.Where(s => s.ID == obj.SecurityContextIRISObjectID).DefaultIfEmpty()
                                where (latestStatus.StatusREFID != 0 && statusRefAttrib.AttrName == ReferenceDataValueAttributeCodes.IsOpenStatus && statusRefAttrib.AttrValue.ToLower() == "true")
                                        || (securityContextObj.ID != 0)
                                select new { obj, securityContextObj };

            int count = 0;
            activeObjects.ForEach(o =>
            {
                var securityContext = o.securityContextObj ?? o.obj;
                if (functions.Any(f => FunctionAppliesToObject(f, securityContext)))
                {
                    count++;
                }
            });

            return count;
        }

        private bool FunctionAppliesToObject(Function f, IRISObject o)
        {
            return (f.ObjectID == o.ObjectTypeID && f.SubClassID == null && f.SubClass2ID == null) ||
                   (f.ObjectID == o.ObjectTypeID && f.SubClassID == o.SubClass1ID && f.SubClass2ID == null) ||
                   (f.ObjectID == o.ObjectTypeID && f.SubClassID == o.SubClass1ID && f.SubClass2ID == o.SubClass2ID);
        }

        /// <summary>
        ///    Utility function that encapsulates transaction management complexity
        ///    away from the user. Simply call and pass in an action with n number
        ///    of database calls. They will all be run as part of one transaction.
        /// </summary>
        private bool TransactionScopedQuery(Action action)
        {
            bool success = false;

            // Define a transaction scope for the operations.
            using (TransactionScope scope = new TransactionScope())
            {
                // Need to use a try statement so that 'success=true' is not run if there is an exception
                try
                {
                    action.Invoke();

                    // Save changes pessimistically. This means that changes 
                    // must be accepted manually once the transaction succeeds.
                    SaveChanges(SaveOptions.DetectChangesBeforeSave);

                    // Mark the transaction as complete.
                    scope.Complete();
                    success = true;
                }
                catch { }
            }

            if (success)
            {
                Context.AcceptAllChanges();
            }

            return success;
        }

        public List<QuestionDefinition> GetCDFColumnFields(string subClass1REF)
        {
            List<QuestionDefinition> listQd = new List<QuestionDefinition>();
            string objectType = "GeneralRegister";
            var collectionId = Context.ReferenceDataCollection.SingleOrDefault(x => x.Code == "ObjectType").ID;
            var objectTypeId = Context.ReferenceDataValue.SingleOrDefault(x => x.Code == objectType && x.CollectionID == collectionId).ID;
            var subClass1ID = Context.ReferenceDataValue.SingleOrDefault(x => x.ParentValueID == objectTypeId && x.Code == subClass1REF).ID;
            var cdfLists = Context.CDFList.Include("CDFListQuestionDefinitions").Where(x => x.ObjectTypeREFID == objectTypeId && x.SubClassification1REFID == subClass1ID && !x.IsDeleted).ToList();
            foreach (var cdfList in cdfLists)
            {
                foreach (var qd in cdfList.CDFListQuestionDefinitions)
                {
                    QuestionDefinition def = Context.QuestionDefinition.SingleOrDefault(x => x.ID == qd.QuestionDefinitionID && !String.IsNullOrEmpty(x.QuestionName));
                    if (def != null)
                    {
                        def.CDFListID = qd.CDFListID;
                        listQd.Add(def);
                    }
                }
            }
            return listQd.OrderBy(x => x.CDFListID).ThenBy(a => a.OrderNumber).ToList();
        }

        #region Stored Procedure Calls

        /// <summary>
        ///    Call to the CalculateElapsedWorkingDays Stored Procedure 
        /// </summary>
        public int CalculateElapsedWorkingDays(long? irisObjectID)
        {
            ObjectParameter elapsedWorkingDays = new ObjectParameter("ElapsedWorkingDays", typeof(int));
            Context.CalculateElapsedWorkingDays(irisObjectID, elapsedWorkingDays);
            return elapsedWorkingDays.Value != null && !Convert.IsDBNull(elapsedWorkingDays.Value) ? (int)elapsedWorkingDays.Value : 0;
        }

        /// <summary>
        ///    Call to the CalculateWorkingDays Stored Procedure 
        /// </summary>
        public int CalculateWorkingDays(DateTime? startDate, DateTime? endDate, long? applicationID)
        {
            ObjectParameter numWorkingDays = new ObjectParameter("numWorkingDays", typeof(int));
            Context.CalculateWorkingDays(startDate, endDate, applicationID, numWorkingDays);
            return numWorkingDays.Value != null && !Convert.IsDBNull(numWorkingDays.Value) ? (int)numWorkingDays.Value : 0;
        }

        /// <summary>
        ///    Call to the CalculateDueDate Stored Procedure 
        /// </summary>
        public DateTime? CalculateDueDate(long? IRISObjectID)
        {
            ObjectParameter dueDate = new ObjectParameter("DueDate", typeof(DateTime));
            Context.CalculateDueDate(IRISObjectID, dueDate);
            return dueDate.Value != null && !Convert.IsDBNull(dueDate.Value) ? (DateTime)dueDate.Value : (DateTime?)null;
        }

        /// <summary>
        ///    Call to the GetBusinessDataXML Stored Procedure 
        /// </summary>
        public string GetBusinessDataXML(long? IRISObjectID, string ObjectTypeCode, long? ParentIRISObjectId = null)
        {
            var result = Context.GetBusinessDataXML(IRISObjectID, ObjectTypeCode, ParentIRISObjectId);
            return string.Join("", result);
        }

        public bool IsHoliday(DateTime date)
        {
            var result = Context.NonWorkDay.Where(x => (date >= x.FromDate) && (date <= x.ToDate));
            if (result.Any())
            {
                return true;
            }
            return false;
        }

        public DateTime CalculateLGOIMADueDate(string LGOIMADueDateStr, Request request)
        {
            DateTime res = new DateTime();
            if (request != null)
            {
                long AddDays = Convert.ToInt64(LGOIMADueDateStr);
                int i = 1;
                DateTime requestDate = request.RequestDate;
                DateTime LGOIMADueDateVal = requestDate;
                while (i <= AddDays)
                {
                    LGOIMADueDateVal = LGOIMADueDateVal.AddDays(1);
                    if (!IsHoliday(LGOIMADueDateVal))
                    {
                        i++;
                    }
                }
                res = LGOIMADueDateVal;
            }
            return res;
        }

        public List<GeneralRegister> GetGeneralRegistersBySubClass(Contact contact, string objectType, string subClass1REF, bool isHarbourMaster)
        {
            var collectionId = Context.ReferenceDataCollection.SingleOrDefault(x => x.Code == "ObjectType").ID;
            var objectTypeId = Context.ReferenceDataValue.SingleOrDefault(x => x.Code == objectType && x.CollectionID == collectionId).ID;
            var subClass1REFID = Context.ReferenceDataValue.SingleOrDefault(x => x.Code == subClass1REF && x.ParentValueID == objectTypeId).ID;
            List<GeneralRegister> result = new List<GeneralRegister>();
            result = Context.GeneralRegisters.Include("IRISObject.Statuses.StatusREF")
                    .Include("IRISObject.SubClass1REF").Include("IRISObject.ActivityObjectRelationships.ContactSubLink")
                    .Where(x => x.IRISObject.SubClass1ID == subClass1REFID && (x.IRISObject.ActivityObjectRelationships.Count(
                                a => a.ContactSubLink.ContactID == contact.ID && a.CurrentTo == null) > 0)).ToList();
            return result;
        }

        public List<Answer> GetGeneralRegisterCDFDetails(string objectType, string subClass1REF, long irisObjectID)
        {
            List<Answer> answers = new List<Answer>();
            var collectionId = Context.ReferenceDataCollection.SingleOrDefault(x => x.Code == "ObjectType").ID;
            var objectTypeId = Context.ReferenceDataValue.SingleOrDefault(x => x.Code == objectType && x.CollectionID == collectionId).ID;
            var subClass1ID = Context.ReferenceDataValue.SingleOrDefault(x => x.Code == subClass1REF && x.ParentValueID == objectTypeId).ID;
            var cdfLists = Context.CDFList.Where(x => x.ObjectTypeREFID == objectTypeId && x.SubClassification1REFID == subClass1ID && !x.IsDeleted).ToList();
            foreach (var cdfList in cdfLists)
            {
                var cdfListQd = Context.CDFListQuestionDefinition.Include("QuestionDefinition").Where(x => x.CDFListID == cdfList.ID).ToList();
                foreach (var listQd in cdfListQd.OrderBy(x => x.QuestionDefinition.OrderNumber))
                {
                    var answer = Context.Answer.Include("QuestionDefinition").SingleOrDefault(x => x.QuestionID == listQd.QuestionDefinitionID &&
                        x.IRISObjectID == irisObjectID && !x.IsDeleted && !String.IsNullOrEmpty(listQd.QuestionDefinition.QuestionName));
                    if (answer != null)
                    {
                        answers.Add(answer);
                    }
                    if (answer == null && !String.IsNullOrEmpty(listQd.QuestionDefinition.QuestionName))
                    {
                        answers.Add(answer);
                    }
                }
            }
            return answers;
        }

        public string GetOtherIdentfierByIRISObjectID(long irisObjectId)
        {
            string result = "";
            var otherIdentifier = Context.OtherIdentifiers.Include("IdentifierContextREF").Where(x => x.IRISObjectID == irisObjectId && x.IdentifierContextREF.Code == ReferenceDataValueCodes.IdentifierContexts.OnlineServicesReference).ToList();
            if (otherIdentifier.Count > 0)
            {
                result = otherIdentifier.OrderByDescending(x => x.ID).First().OtherIdentifierText;
            }
            return result;
        }

        public IEnumerable<OtherIdentifier> GetOtherIdentfiers(long irisObjectId)
        {
            return Context.OtherIdentifiers.Include("IdentifierContextREF")
                .Where(x => x.IRISObjectID == irisObjectId).ToList();
        }

        /// <summary>
        ///    Call to the GetOnlineServiceObjectDetailsXML Stored Procedure 
        /// </summary>
        public string GetOnlineServiceObjectDetailsXML(long? ObjectID, string ObjectTypeCode)
        {
            var result = Context.GetOnlineServiceObjectDetailsXML(ObjectID, ObjectTypeCode);
            return string.Join("", result);
        }

        /// <summary>
        ///    Call to the DeleteObject Stored Procedure 
        /// </summary>
        public void DeleteObject(long? ObjectID, string ObjectTypeCode)
        {
            Context.DeleteObject(ObjectID, ObjectTypeCode);
        }

        /// <summary>
        ///    Call to the UpdateActivityObjectRelationshipReference Stored Procedure 
        /// </summary>
        public void UpdateActivityObjectRelationshipReference(string tableName, string columnName, long? oldLinkID, long? newLinkID)
        {
            Context.UpdateActivityObjectRelationshipReference(tableName, columnName, oldLinkID, newLinkID);
        }

        /// <summary>
        ///    Call to the GetMergedToContactID Stored Procedure 
        /// </summary>
        public long GetMergedToContactID(long ContactID)
        {
            ObjectParameter MergedToContactID = new ObjectParameter("MergedToContactID", typeof(long));
            Context.GetMergedToContactID(ContactID, MergedToContactID);
            return MergedToContactID.Value != null && !Convert.IsDBNull(MergedToContactID.Value) ? (long)MergedToContactID.Value : 0;

        }

        /// <summary>
        ///    Call to the DeleteSpatialLocation Stored Procedure 
        /// </summary>
        public void DeleteSpatialLocation(long IRISObjectID)
        {
            Context.DeleteSpatialLocation(IRISObjectID);
        }

        /// <summary>
        ///    Call to the DeleteSpatialLocation Stored Procedure 
        /// </summary>
        public void GetLatestSingleSpatialLocation(long IRISObjectID, out double xcoordinate, out double ycoordinate)
        {
            ObjectParameter xcoordinateParameter = new ObjectParameter("Xcoordinate", typeof(double));
            ObjectParameter ycoordinateParameter = new ObjectParameter("Ycoordinate", typeof(double));

            Context.GetLatestSingleSpatialLocation(IRISObjectID, xcoordinateParameter, ycoordinateParameter);

            if (Convert.IsDBNull(xcoordinateParameter.Value) || Convert.IsDBNull(ycoordinateParameter.Value))
            {
                xcoordinate = 0;
                ycoordinate = 0;
            }
            else
            {
                xcoordinate = (double)xcoordinateParameter.Value;
                ycoordinate = (double)ycoordinateParameter.Value;
            }
        }

        /// <summary>
        ///    Call to the CalculateInformationSwornDueDate Stored Procedure 
        /// </summary>
        public DateTime? CalculateInformationSwornDueDate(long? enforcementID)
        {
            ObjectParameter swornDueDate = new ObjectParameter("DueDate", typeof(DateTime));
            Context.CalculateInformationSwornDueDate(enforcementID, swornDueDate);
            return swornDueDate.Value != null && !Convert.IsDBNull(swornDueDate.Value) ? (DateTime)swornDueDate.Value : (DateTime?)null;
        }

        public DateTime? CalculateFinalDateForFilingChargingDocuments(long? enforcementID)
        {
            ObjectParameter finalDate = new ObjectParameter("DueDate", typeof(DateTime));
            Context.CalculateFinalDateForFilingChargingDocuments(enforcementID, finalDate);
            return finalDate.Value != null && !Convert.IsDBNull(finalDate.Value) ? (DateTime)finalDate.Value : (DateTime?)null;
        }

        /// <summary>
        ///    Call to the CalculateInfringementNoticeServedDueDate Stored Procedure 
        /// </summary>
        public DateTime? CalculateInfringementNoticeServedDueDate(long? irisObjectID)
        {
            ObjectParameter infirngementDueDate = new ObjectParameter("DueDate", typeof(DateTime));
            Context.CalculateInfringementNoticeServedDueDate(irisObjectID, infirngementDueDate);
            return infirngementDueDate.Value != null && !Convert.IsDBNull(infirngementDueDate.Value) ? (DateTime)infirngementDueDate.Value : (DateTime?)null;
        }

        public GetIRISDataResult GetIRISData(string storedProcName, IEnumerable<SqlParameter> parameters)
        {
            // Confirm that stored proc exists in the ExposedStoredProcedures table.
            ExposedStoredProcedure exposedStoredProcedure = Context.ExposedStoredProcedures.SingleOrDefault(e => e.StoredProcedureName == storedProcName);

            if (exposedStoredProcedure != null) //check if the stored proc exists in the table ExposedStoredProcedure
            {
                return new GetIRISDataResult()
                {
                    IsExposedStoredProcedure = true,
                    SQLResult = Context.ExecuteSPReturnScalar(storedProcName, parameters.ToArray())
                };
            }
            else
            {
                return new GetIRISDataResult()
                {
                    IsExposedStoredProcedure = false,
                    SQLResult = null
                };
            }
        }

        public List<NavigationHistory> GetNavigationHistoriesByUser(User user)
        {
            return Context.NavigationHistories
                          .Include(x => x.IRISObject)
                          .Include(x => x.IRISObject.ObjectTypeREF)
                          .Where(x => x.UserID == user.ID)
                          .OrderByDescending(x => x.LastVisitedDttm)
                          .ToList()
                          .TrackAll();
        }

        public void DeleteNavigationHistoriesByIRISObjectID(long IRISObjectID)
        {
            var deletedHistories = Context.NavigationHistories.Where(x => x.IRISObjectID == IRISObjectID);
            deletedHistories.ForEach(x => Context.NavigationHistories.DeleteObject(x));
            SaveChanges();
        }

        public IRISDetailsDTO GetDetailsByIrisObjectIdOrActivityRelationshipId(long irisObjectID, long activityRelationshipId, long taskId)
        {
            var isDetailsForContactSublink = activityRelationshipId != 0;
            var isObjectEvent = taskId != 0;
            var result = new IRISDetailsDTO
            {
                KeyValues = new List<KeyValuePair<string, string>>(),
                ObjectTypeCode = Context.IRISObject.Include(x => x.ObjectTypeREF).Single(x => x.ID == irisObjectID).ObjectTypeREF.Code
            };

            GetTheObjectsThatNeedToCallFrontEndFunctionsToFormatTheirValues(irisObjectID, result);

            if (isDetailsForContactSublink)
            {
                GetKeyValuesAndObjectsForContactSublinks(activityRelationshipId, result);
            }
            else if (isObjectEvent)
            {
                TaskInstance task = Context.TaskInstances.Where(x => x.ID == taskId).Single();
                if (task != null)
                {
                    if (!task.Notes.IsNullOrEmpty())
                    {
                        result.KeyValues.Add(new KeyValuePair<string, string>("Notes:", task.Notes));
                    }
                }
            }
            else
            {
                Context.IRISObjectDetails.Where(x => x.IRISObjectID == irisObjectID).ToList()
                    .ForEach(x => result.KeyValues.Add(new KeyValuePair<string, string>(x.Key, x.Value)));
            }

            return result;
        }

        private void GetKeyValuesAndObjectsForContactSublinks(long activityRelationshipId, IRISDetailsDTO result)
        {
            var subLinkDetails =
                (from relationship in Context.ActivityObjectRelationship.Where(x => x.ID == activityRelationshipId)
                 let sublink = relationship.ContactSubLink
                 let contactAddress = sublink.ContactAddress
                 let address = contactAddress.Address
                 let phone = sublink.PhoneNumber
                 let email = sublink.Email
                 let website = sublink.Website
                 let country = Context.Address.OfType<OverseasAddress>().FirstOrDefault(x => x.ID == address.ID).CountryREF
                 let deliveryAddressType =
                     Context.Address.OfType<DeliveryAddress>().FirstOrDefault(x => x.ID == address.ID).DeliveryAddressTypeREF
                 let unitType =
                  Context.Address.OfType<UrbanRuralAddress>().FirstOrDefault(x => x.ID == address.ID).UnitTypeREF
                 let floorType =
                   Context.Address.OfType<UrbanRuralAddress>().FirstOrDefault(x => x.ID == address.ID).FloorTypeREF
                 select new { relationship, sublink, contactAddress, address, country, phone, email, website, deliveryAddressType, unitType, floorType }).ToList();


            result.HasSublinkDetails = subLinkDetails.Select(x => x.sublink).SingleOrDefault() != null;
            if (result.HasSublinkDetails)
            {
                //Only display these values for types that have sublinks i.e. not for contacts.
                // These address is passed a an object because we need format them using the display adapters which are in the web project, it would be better as a method on the object.                  
                result.ContactAddress = subLinkDetails.Select(x => x.contactAddress).SingleOrDefault();

                result.KeyValues.Add(new KeyValuePair<string, string>("Sublinked Phone Number",
                                                                      subLinkDetails.Select(x => x.phone).SingleOrDefault() ==
                                                                      null
                                                                          ? "None"
                                                                          : subLinkDetails.Select(x => x.phone).Single().
                                                                                FormattedNumber));
                result.KeyValues.Add(new KeyValuePair<string, string>("Sublinked Email",
                                                                      subLinkDetails.Select(x => x.email).SingleOrDefault() ==
                                                                      null
                                                                          ? "None"
                                                                          : subLinkDetails.Select(x => x.email).Single().IsCurrent ?
                                                                                subLinkDetails.Select(x => x.email).Single().Value : DisplayConstants.NonCurrent + subLinkDetails.Select(x => x.email).Single().Value
                                                                          ));
                result.KeyValues.Add(new KeyValuePair<string, string>("Sublinked Website",
                                                                      subLinkDetails.Select(x => x.website).SingleOrDefault() ==
                                                                      null
                                                                          ? "None"
                                                                          : subLinkDetails.Select(x => x.website).Single().URL));
            }
            var description = subLinkDetails.Select(x => x.relationship).SingleOrDefault() == null
                                  ? ""
                                  : subLinkDetails.Select(x => x.relationship).Single().Description ?? "";
            description = description.Trim();
            result.KeyValues.Add(new KeyValuePair<string, string>("Link Description", description));
        }

        private void GetTheObjectsThatNeedToCallFrontEndFunctionsToFormatTheirValues(long irisObjectID, IRISDetailsDTO result)
        {
            if (result.ObjectTypeCode == ReferenceDataValueCodes.ObjectType.RegimeActivity)
            {
                result.RegimeActivitySchedule =
                    (from reqAct in Context.RegimeActivity.Where(x => x.IRISObjectID == irisObjectID)
                     from schedule in Context.RegimeActivitySchedule.Where(x => x.ID == reqAct.RegimeActivityScheduleID)
                     from scheduleType in Context.ReferenceDataValue.Where(x => x.ID == schedule.ScheduleTypeREFID)
                     select new { schedule, scheduleType }).AsEnumerable()
                     .Select(x => x.schedule).SingleOrDefault();
            }

            if (result.ObjectTypeCode == ReferenceDataValueCodes.ObjectType.EnforcementAllegedOffence)
            {
                result.EnforcementAllegedOffence =
                    Context.EnforcementAllegedOffences.Include(x => x.OffenceDateTypeREF).SingleOrDefault(x => x.IRISObjectID == irisObjectID);
            }
        }

        #endregion

        public List<long> ListIRISObjectIDsForOfficerResponsible(long userId)
        {
            var results = Context.ListIRISObjectIDsForOfficerResponsible(userId).Where(o => o.HasValue).Select(o => o.Value);
            return results.ToList();
        }

        public List<long?> ListTaskInstancesAssignedToUser(long userId, long groupID)
        {
            var results = Context.ListTaskInstancesAssignedToUser(userId, groupID);
            return results.ToList();
        }

        public List<long?> BulkReassignOfficerResponsible(long fromOfficerResponsibleID, long toOfficerResponsibleID, long groupID, string currentUserAccountName, long userID)
        {
            var results = Context.BulkReassignOfficerResponsible(fromOfficerResponsibleID, toOfficerResponsibleID, currentUserAccountName, userID, groupID);
            return results.ToList();
        }

        public List<BatchRollForwardAnnualChargeResult> RollForwardAnnualCharges(long fromYear, long fromPeriod, long toYear, long toPeriod, string currentUserAccountName)
        {
            var results = Context.RollForwardAnnualCharges(fromYear, fromPeriod, toYear, toPeriod, currentUserAccountName);
            return results.ToList();
        }

        public EmailTemplate GetEmailTemplate(string templateName)
        {
            EmailTemplate emailTemplate = Context.EmailTemplates.SingleOrDefault(t => t.Name == templateName);
            if (emailTemplate == null)
            {
                UtilLogger.Instance.LogWorkflow("GetEmailTemplate: Could not find EmailTemplate Name {0}", templateName);
            }
            return emailTemplate;
        }

        public CustomLink GetCustomLinkByID(long id)
        {
            return Context.CustomLinks.Include(x => x.ObjectTypeREF).Single(x => x.ID == id);
        }

        public List<CustomLink> GetAllCustomLinks()
        {
            return Context.CustomLinks.Include(x => x.ObjectTypeREF).Include(c => c.ReferenceDataValue1)
                    .Include(c => c.ReferenceDataValue2)
                    .Include(c => c.ReferenceDataValue3).Where(x => !x.IsDeleted)
                .ToList();
        }

        public List<CustomLink> GetCustomLinksByIRISObjectID(long objectTypeREFID)
        {

            
            return Context.CustomLinks.Include(x => x.ObjectTypeREF).Where(x => x.ObjectTypeREFID == objectTypeREFID && !x.IsDeleted && x.IsActive).ToList();
        }

        public IQueryable<ObjectInspectionTypeMapping> QueryInspectionTypeMappings()
        {
            return Context.ObjectInspectionTypeMappings;
        }

        #region Private Methods
        private List<EventInstanceData> GetNoteQueryForListByIrisObject(long irisObjectID)
        {
            //int userPrefixLength = .Length;
            var queryable = from note in Context.Note
                            join completedBy in Context.User on note.ByID equals completedBy.ID into CompletedByUser
                            from completedUser in CompletedByUser.DefaultIfEmpty()
                            where note.IRISObjectID == irisObjectID && !note.IsDeleted

                            select
                               new NoteInstanceData()
                               {
                                   ID = note.ID,
                                   IRISObjectID = note.IRISObjectID,

                                   IRISObjectTypeCode = note.IRISObject.ObjectTypeREF.Code,
                                   IRISObjectTypeDisplayValue = note.IRISObject.ObjectTypeREF.DisplayValue,
                                   IRISObjectTypeID = note.IRISObject.ObjectTypeID,
                                   IRISObjectSubClass1ID = note.IRISObject.SubClass1ID,
                                   IRISObjectSubClass2ID = note.IRISObject.SubClass2ID,

                                   SecurityContextIRISObjectID = note.IRISObject.SecurityContextIRISObjectID,
                                   SecurityContextIRISObjectTypeCode = note.IRISObject.SecurityContextIRISObject.ObjectTypeREF.Code,
                                   SecurityContextIRISObjectTypeID = note.IRISObject.SecurityContextIRISObject.ObjectTypeID,
                                   SecurityContextIRISObjectSubClass1ID = note.IRISObject.SecurityContextIRISObject.SubClass1ID,
                                   SecurityContextIRISObjectSubClass2ID = note.IRISObject.SecurityContextIRISObject.SubClass2ID,

                                   IRISObjectLinkID = note.IRISObject.LinkID,
                                   IRISObjectLinkDetails = note.IRISObject.LinkDetails,
                                   IRISObjectBusinessID = note.IRISObject.BusinessID,

                                   Date = note.Date,
                                   Time = note.Time,
                                   NoteText = note.NoteText,
                                   NoteType = note.TypeREF.DisplayValue,
                                   CompletedBy = completedUser.DisplayName,
                                   InstanceType = EventInstanceType.Note,
                                   CreatedByUserAccountName = note.CreatedBy
                               };

            return queryable.ToList().Cast<EventInstanceData>().ToList();
        }

        private int GetNoteQueryForListByIrisObjectCount(long irisObjectID)
        {
            //int userPrefixLength = .Length;
            return (from note in Context.Note
                    join completedBy in Context.User on note.ByID equals completedBy.ID into CompletedByUser
                    from completedUser in CompletedByUser.DefaultIfEmpty()
                    where note.IRISObjectID == irisObjectID && !note.IsDeleted

                    select note).Count();
                               
        }

        private List<EventInstanceData> GetTaskQueryForListByIrisObject(long irisObjectID, bool toDoTasksOnly)
        {
            var taskInstanceData = RepositoryMap.WorkflowRepository.GetTaskList(irisObjectID, toDoTasksOnly);
            return taskInstanceData.Cast<EventInstanceData>().ToList();
        }

        private int GetTaskQueryForListByIrisObjectCount(long irisObjectID, bool toDoTasksOnly)
        {
            return RepositoryMap.WorkflowRepository.GetTaskListCount(irisObjectID, toDoTasksOnly);
        }
        #endregion
    }
}
