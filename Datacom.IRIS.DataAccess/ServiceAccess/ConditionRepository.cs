using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common.Utils;


namespace Datacom.IRIS.DataAccess.ServiceAccess
{
 
    public class ConditionRepository : RepositoryStore, IConditionRepository
    {
		#region Conditions

        public List<LibraryCondition> GetAllLibraryConditions(bool includeInactive)
        {
            return Context.LibraryCondition
                          .Include(lc => lc.ConditionTypeREF)
                          .Where(lc => !lc.IsDeleted && (includeInactive || lc.IsActive))
                          .ToList().TrackAll();
        }

        public LibraryCondition GetLibraryConditionByID(long libraryConditionID)
        {
            return Context.LibraryCondition
                .Include(lc => lc.ConditionTypeREF)
                .Where(lc => lc.ID == libraryConditionID).Single().TrackAll();
        }

        public List<Condition> GetAllConditionsWithinConditionSchedule(long conditionID, bool currentOnly)
        {
            long conditionScheduleID = Context.ConditionScheduleConditions.Where(x => x.ConditionID == conditionID).Select(x => x.ConditionScheduleID).FirstOrDefault();
            return Context.ConditionScheduleConditions
                          .Include(c => c.Condition.ConditionType)
                          .Where(c => c.ConditionScheduleID == conditionScheduleID)
                          .Select(c => c.Condition)
                          .Where(c => !c.IsDeleted && (c.ToDate == null || !currentOnly))
                          .ToList();
        }

        public bool IsConditionLibraryInUse(long libraryConditionID)
        {
            return Context.Conditions.Any(c => c.ToDate == null && c.LibraryConditionID == libraryConditionID && !c.IsDeleted);
        }


        public List<Condition> GetAllConditionsForAuthorisation(long authorisationID, bool currentOnly)
        {
            var conditions= Context.AuthorisationConditions
                            .Include(ac=>ac.Condition.ConditionType)
                            .Where(ac => ac.AuthorisationID == authorisationID);

            return conditions.Select(c => c.Condition)
                             .Where(c => !c.IsDeleted && (c.ToDate == null || !currentOnly))
                             .ToList();
        }

        public List<Condition> GetAllConditionsWithinAuthorisation(long conditionID, bool currentOnly)
        {
            long authorisationID = Context.AuthorisationConditions.Where(x => x.ConditionID == conditionID).Select(x => x.AuthorisationID).FirstOrDefault();
            return GetAllConditionsForAuthorisation(authorisationID, currentOnly);
        }

        public List<Condition> GetAllConditionsForActivity(long activityID, bool currentOnly)
        {
            var conditions = Context.ActivityConditions
                            .Include(ac => ac.Condition.ConditionType)
                            .Where(ac => ac.ActivityID == activityID);

            return conditions.Select(c => c.Condition)
                             .Where(c => !c.IsDeleted && (c.ToDate == null || !currentOnly))
                             .ToList();
        }

        public List<Condition> GetAllConditionsWithinActivity(long conditionID, bool currentOnly)
        {
            long activityID = Context.ActivityConditions.Where(x => x.ConditionID == conditionID).Select(x => x.ActivityID).FirstOrDefault();
            return GetAllConditionsForActivity(activityID, currentOnly);
        }


        public Condition GetConditionByID(long conditionID)
        {
            //Note: we retrieve the AuthorisationConditions and AuthorisationConditions and ConditionScheduleConditions
            // to know which auth/activity we are dealing with when modifying a condition
            var condition = Context.Conditions
                            .Include(c => c.ConditionType)
                            .Include(c => c.Parameters)
                            .Include("Parameters.ParameterTypeREF")
                            .Include("Parameters.UnitREF")
                            .Include(c => c.AuthorisationConditions)//used to know which auth we are dealing with
                            .Include(c => c.ActivityConditions)
                            .Include(c => c.ConditionScheduleConditions)
                            .Single(c => c.ID == conditionID && !c.IsDeleted).TrackAll();

            condition.Parameters.ClearSoftlyDeletedItems();

            return condition.TrackAll();
        }

        #endregion


        #region Condition Schedule

        public IRISObject GetIRISObjectByConditionScheduleID(long conditionScheduleID)
        {
            var auth = Context.ConditionSchedules
                .Include(a => a.IRISObject)
                .Include(a => a.IRISObject.ObjectTypeREF)
                .Single(a => a.ID == conditionScheduleID);
            return auth.IRISObject;
        }

        public ConditionSchedule GetConditionScheduleByID (long conditionScheduleId)
        {
            var conditionSchedule = Context.ConditionSchedules
                            .Include(c => c.IRISObject)
                            .Include(c => c.IRISObject.ObjectTypeREF)
                            .Include(c => c.IRISObject.SubClass1REF)
                            .Single(c => c.ID == conditionScheduleId).TrackAll();

            var conditionScheduleConditionQuery = from cond in Context.ConditionScheduleConditions
                                        .Include(c => c.Condition)
                                        .Include(c => c.Condition.ConditionType)
                                        .Where(c => c.ConditionScheduleID == conditionScheduleId)
                                        select cond;

            var conditionScheduleConditions = conditionScheduleConditionQuery.Where(ac => !ac.Condition.IsDeleted);
            conditionSchedule.ConditionScheduleConditions.AddTrackableCollection(conditionScheduleConditions.ToList());

            return conditionSchedule.TrackAll();
        }

        #endregion
    }


}
