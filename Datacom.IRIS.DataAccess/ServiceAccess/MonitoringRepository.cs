using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.Common.Utils;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.DTO;
using SortDirection = System.Web.UI.WebControls.SortDirection;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class MonitioringRepository : RepositoryStore, IMonitoringRepository
    {
        private const int Level1 = 1;
        private const int Level2 = 2;
        private const int Level3 = 3;

        public Regime GetRegime(long id)
        {
            var result = Context.Regime
                            .Include(r => r.OfficerResponsible)
                            .Include(r => r.FinancialYearREF)
                            .Include(r => r.ServiceAddress.ContactAddress.Address)
                            .Include(r => r.ServiceAddress.Email)
                            .SingleOrDefault(r => r.ID == id);

            if (result == null) return null;

            result.IRISObject = GetIRISObject(result.IRISObjectID, Level3);

            GetRegimeTypeSpecificValues(result);
            LoadPlanAndRuleInformation(result);
            return result.TrackAll();
        }

        // --------------

        public ReferenceDataValueRate GetReferenceDataValueRate(long resourceLabourPositionId, long financialYearId, long resourceunitTypeId)
        {
            ReferenceDataValueRate referenceDataValueRate =
                         Context.ReferenceDataValueRates.FirstOrDefault(x => (x.ResourceID == resourceLabourPositionId || x.LabourPositionID == resourceLabourPositionId)
                         && x.ResouceUnitTypeID == resourceunitTypeId
                         && x.FinancialYearID == financialYearId
                         && !x.IsDeleted);

            //ReferenceDataValueRate referenceDataValueRate =
            //           Context.ReferenceDataValueRates.ToList().SingleOrDefault(x => x.ResourceID == resourceLabourPositionId 
            //           && x.ResouceUnitTypeID == resourceunitTypeId
            //           && x.FinancialYearID == financialYearId
            //           && x.IsDeleted == false);

            return referenceDataValueRate;
        }

        public ReferenceDataValueRate GetReferenceDataValueRateById(long Id)
        {
            return Context.ReferenceDataValueRates.SingleOrDefault(s => s.ID == Id).TrackAll();
        }



        public List<ReferenceDataValueRate> GetReferenceDataValueRateByFinancialYear(long financialYearId)
        {
            var filteredbyYearList = Context.GetReferenceDataValueRatesByFinancialYearId(financialYearId);

            List<ReferenceDataValueRate> estimatedList = new List<ReferenceDataValueRate>();

            foreach (var item in filteredbyYearList)
            {
                estimatedList.Add(new ReferenceDataValueRate
                {
                    ID = item.ID,
                    ResourceID = item.ResourceID,
                    LabourPositionID = item.LabourPositionID,
                    FinancialYearID = item.FinancialYearID,
                    ResouceUnitTypeID = item.ResouceUnitTypeID,
                    RateValue = item.RateValue,
                    CurrentFrom = item.CurrentFrom,
                    CurrentTo = item.CurrentTo,
                    IsDeleted = item.IsDeleted,
                    DateCreated = item.DateCreated,
                    CreatedBy = item.CreatedBy,
                    LastModified = item.LastModified,
                    ModifiedBy = item.ModifiedBy,

                    ResourceRefCode = item.ResourceRefCode,
                    ResourceDisplayValue = item.ResourceDisplayValue,
                    LabourPositionCode = item.LabourPositionCode,
                    LabourPositionDisplayValue = item.LabourPositionDisplayValue,
                    ResouceUnitTypeCode = item.ResouceUnitTypeCode,
                    ResouceUnitTypeDisplayValue = item.ResouceUnitTypeDisplayValue,
                    FinancialYearCode = item.FinancialYearCode,
                    FinancialYearDisplayValue = item.FinancialYearDisplayValue
                });
            }
            return estimatedList;
        }



        // -------------------------

        #region Map Tip Queries

        /// <summary>
        ///    This method returns 'light' Regime objects that are fetched based on IRIS Object IDS.
        ///    This method is invoked by the GIS map tips functionality and simply wants simple
        ///    information about a Regime such as the IRIS ID, Regime Name and Type
        /// </summary>
        public List<Regime> GetRegimesByIRISObjectIDs(List<long> irisObjectIDsList)
        {
            return Context.Regime
                .Include(r => r.IRISObject)
                .Include("IRISObject.ObjectTypeREF")
                .Include("IRISObject.SubClass1REF")
                .Include(r => r.FinancialYearREF)
                .Where(r => irisObjectIDsList.Contains(r.IRISObjectID))
                .ToList();
        }

        /// <summary>
        ///    This method returns 'light' Observation objects that are fetched based on IRIS Object IDS.
        ///    This method is invoked by the GIS map tips functionality and simply wants simple
        ///    information about an Observation such as Regime IRIS ID, and Observation Type
        /// </summary>
        public List<Observation> GetObservationsByIRISObjectIDs(List<long> irisObjectIDsList)
        {
            var dbquery = from observation in Context.Observations
                            .Where(o => irisObjectIDsList.Contains(o.IRISObjectID))
                          from regimeActivity in Context.RegimeActivity
                            .Where(r => r.ID == observation.RegimeActivityID)
                          from regimeActivitySchedule in Context.RegimeActivitySchedule
                            .Where(ras => ras.ID == regimeActivity.RegimeActivityScheduleID)
                          from regime in Context.Regime
                            .Where(r => r.ID == regimeActivitySchedule.RegimeID)
                          from regimeIrisObject in Context.IRISObject
                            .Where(i => i.ID == regime.IRISObjectID)
                          from observationIrisObject in Context.IRISObject
                            .Where(i => i.ID == observation.IRISObjectID)
                          from objectTypeREF in Context.ReferenceDataValue
                            .Where(r => r.ID == observationIrisObject.ObjectTypeID)
                          from subClass1REF in Context.ReferenceDataValue
                            .Where(r => r.ID == observationIrisObject.SubClass1ID)
                          select new
                          {
                              observation,
                              regimeActivity,
                              regimeActivitySchedule,
                              regime,
                              regimeIrisObject,
                              observationIrisObject,
                              objectTypeREF,
                              subClass1REF,
                              observationIrisObject.SecurityContextIRISObject,
                              observationIrisObject.SecurityContextIRISObject.ObjectTypeREF
                          };

            return dbquery.AsEnumerable().Select(s => s.observation).Distinct().ToList();
        }



        /// <summary>
        ///    This method returns 'light' RegimeActivity objects that are fetched based on IRIS Object IDS.
        ///    This method is invoked by the GIS map tips functionality and simply wants simple
        ///    information about a RegimeActivity such as Regime IRIS ID, Activity Name and Type
        /// </summary>
        public List<RegimeActivity> GetRegimeActivitiesByIRISObjectIDs(List<long> irisObjectIDsList)
        {
            var dbquery = from regimeActivity in Context.RegimeActivity
                            .Where(ra => irisObjectIDsList.Contains(ra.IRISObjectID))
                          from regimeActivitySchedule in Context.RegimeActivitySchedule
                            .Where(ras => ras.ID == regimeActivity.RegimeActivityScheduleID)
                          from regime in Context.Regime
                            .Where(r => r.ID == regimeActivitySchedule.RegimeID)
                          from regimeIrisObject in Context.IRISObject
                            .Where(i => i.ID == regime.IRISObjectID)
                          from regimeActivityIrisObject in Context.IRISObject
                            .Where(i => i.ID == regimeActivity.IRISObjectID)
                          from objectTypeREF in Context.ReferenceDataValue
                            .Where(r => r.ID == regimeActivityIrisObject.ObjectTypeID)
                          from subClass1REF in Context.ReferenceDataValue
                            .Where(r => r.ID == regimeActivityIrisObject.SubClass1ID)
                          select new
                          {
                              regimeActivity,
                              regimeActivitySchedule,
                              regime,
                              regimeIrisObject,
                              regimeActivityIrisObject,
                              objectTypeREF,
                              subClass1REF,
                              regimeActivityIrisObject.SecurityContextIRISObject,
                              RegimeActivitySecurityContextObjectTypeREF = regimeActivityIrisObject.SecurityContextIRISObject.ObjectTypeREF
                          };

            return dbquery.AsEnumerable().Select(s => s.regimeActivity).Distinct().ToList();
        }

        #endregion

        #region Fetching Regime Activities

        /// <summary>
        ///    This method will return a list of regime activities for a given regime. Current activities
        ///    are defined as those with a status attribute of "IsCurrentStatus" 
        ///    assigned to them. Historic acitivities are those with any other statuses than the latter 
        ///    that are mentioned.
        /// </summary>
        public List<RegimeActivity> GetRegimeActivities(long regimeId, bool currentFlag)
        {
            var activities = GetRegimeActivitiesWithStatus(regimeId, currentFlag);

            if (currentFlag)
            {
                // Current regimes is a concat of ones with no status, and ones with a given 
                activities = activities.Concat(GetCurrentRegimeActivitiesWithNoStatus(regimeId));
            }

            return activities.ToList();
        }

        public List<RegimeActivity> GetRegimeActivitiesByRegimeId(long regimeId, bool currentFlag)
        {
            var activities = GetRegimeActivitiesWithStatusByRegimeId(regimeId, currentFlag);

            if (currentFlag)
            {
                activities =
                    activities.Concat(
                        GetCurrentRegimeActivitiesWithNoStatusByRegimeId(regimeId));
            }

            return activities.ToList();
        }

        public int GetRegimeActivitiesByRegimeIdCount(long regimeId, bool currentFlag, bool currentActivities)
        {
            var activitiesCount = GetRegimeActivitiesWithStatusByRegimeIdCount(regimeId, currentFlag, currentActivities);
            if (currentFlag) activitiesCount += GetCurrentRegimeActivitiesWithNoStatusByRegimeIdCount(regimeId, currentActivities);
            return activitiesCount;
        }

        public List<RegimeActivity> GetCurrentRegimeActivitiesByManagementSiteId(long managementSiteId)
        {
            var trueString = true.ToString();
            return Context.RegimeActivity
                .SelectMany(
                    activity => Context.RegimeActivityMngtSites.Where(regimeActivityManagementSite =>
                        !regimeActivityManagementSite.IsDeleted &&
                        regimeActivityManagementSite.RegimeActivityMngtID == activity.ID &&
                        regimeActivityManagementSite.ManagementSiteID == managementSiteId),
                    (activity, regimeActivityManagementSite) => new { activity, regimeActivityManagementSite })
                .SelectMany(
                    regimeActivityManagementSite => Context.User.Where(u =>
                        u.ID == regimeActivityManagementSite.activity.OfficerResponsibleID),
                    (activity, user) => new { activity, user })
                .SelectMany(
                    userActivity => Context.IRISObject.Where(i => i.ID == userActivity.activity.activity.IRISObjectID),
                    (userActivity, irisObject) => new { userActivity, irisObject })
                .SelectMany(
                    irisObject => Context.ReferenceDataValue.Where(r => r.ID == irisObject.irisObject.ObjectTypeID),
                    (irisObject, refData1) => new { irisObject, refData1 })
                .SelectMany(
                    referenceDataOne =>
                        Context.ReferenceDataValue.Where(
                            r => r.ID == referenceDataOne.irisObject.irisObject.SubClass1ID),
                    (referenceDataOne, referenceDataTwo) => new { referenceDataOne, referenceDataTwo })
                .SelectMany(
                    referenceDataTwo => Context.ReferenceDataValue.Where(r =>
                        r.ID == referenceDataTwo.referenceDataOne.irisObject.irisObject.SubClass2ID),
                    (referenceDataTwo, referenceDataThree) => new { referenceDataTwo, referenceDataThree })
                .SelectMany(
                    referenceDataThree => Context.RegimeActivitySchedule.Where(regimeActivitySchedule =>
                        regimeActivitySchedule.ID == referenceDataThree.referenceDataTwo.referenceDataOne.irisObject
                            .userActivity.activity.activity.RegimeActivityScheduleID),
                    (referenceDataThree, regimeActivitySchedule) => new { referenceDataThree, regimeActivitySchedule })
                .SelectMany(
                    referenceDataThree => Context.ReferenceDataValue.Where(r =>
                        r.ID == referenceDataThree.regimeActivitySchedule.ScheduleTypeREFID),
                    (regimeActivitySchedule, scheduleType) => new { regimeActivitySchedule, scheduleType })
                .SelectMany(
                    regimeActivitySchedule => (from status in Context.Statuses
                                               where !status.IsDeleted && status.IRISObjectID == regimeActivitySchedule.regimeActivitySchedule
                                                         .referenceDataThree.referenceDataTwo.referenceDataOne.irisObject.irisObject.ID
                                               orderby status.StatusDate descending, status.ID descending
                                               select status).Take(1),
                    (regimeActivitySchedule, latestStatus) => new { regimeActivitySchedule, latestStatus })
                .SelectMany(
                    latestStatus => Context.ReferenceDataValue.Where(referenceDataValue =>
                        referenceDataValue.ID == latestStatus.latestStatus.StatusREFID &&
                        referenceDataValue.ReferenceDataValueAttribute.Any(x =>
                            x.AttrName == ReferenceDataValueAttributeCodes.IsCurrentStatus &&
                            x.AttrValue == trueString)),
                    (latestStatus, referenceDataFour) => new
                    {
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne.irisObject.userActivity.activity.activity,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.regimeActivitySchedule,
                        latestStatus.regimeActivitySchedule.scheduleType,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne.irisObject,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree,
                        referenceDataFour,
                        latestStatus.latestStatus,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne.irisObject.userActivity.user,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne.irisObject.irisObject.SecurityContextIRISObject,
                        RegimeActivitySecurityContextObjectTypeREF = latestStatus.regimeActivitySchedule
                            .regimeActivitySchedule.referenceDataThree.referenceDataTwo.referenceDataOne.irisObject
                            .irisObject.SecurityContextIRISObject.ObjectTypeREF
                    }).AsEnumerable().Select(s => s.activity).Distinct().ToList();
        }

        public int GetCurrentRegimeActivitiesByManagementSiteIdCount(long managementSiteId)
        {
            var trueString = true.ToString();
            return Context.RegimeActivity
                .SelectMany(
                    activity => Context.RegimeActivityMngtSites.Where(regimeActivityManagementSite =>
                        !regimeActivityManagementSite.IsDeleted &&
                        regimeActivityManagementSite.RegimeActivityMngtID == activity.ID &&
                        regimeActivityManagementSite.ManagementSiteID == managementSiteId),
                    (activity, regimeActivityManagementSite) => new { activity, regimeActivityManagementSite })
                .SelectMany(
                    regimeActivityManagementSite => Context.User.Where(u =>
                        u.ID == regimeActivityManagementSite.activity.OfficerResponsibleID),
                    (activity, user) => new { activity, user })
                .SelectMany(
                    userActivity => Context.IRISObject.Where(i => i.ID == userActivity.activity.activity.IRISObjectID),
                    (userActivity, irisObject) => new { userActivity, irisObject })
                .SelectMany(
                    irisObject => Context.ReferenceDataValue.Where(r => r.ID == irisObject.irisObject.ObjectTypeID),
                    (irisObject, refData1) => new { irisObject, refData1 })
                .SelectMany(
                    referenceDataOne =>
                        Context.ReferenceDataValue.Where(
                            r => r.ID == referenceDataOne.irisObject.irisObject.SubClass1ID),
                    (referenceDataOne, referenceDataTwo) => new { referenceDataOne, referenceDataTwo })
                .SelectMany(
                    referenceDataTwo => Context.ReferenceDataValue.Where(r =>
                        r.ID == referenceDataTwo.referenceDataOne.irisObject.irisObject.SubClass2ID),
                    (referenceDataTwo, referenceDataThree) => new { referenceDataTwo, referenceDataThree })
                .SelectMany(
                    referenceDataThree => Context.RegimeActivitySchedule.Where(regimeActivitySchedule =>
                        regimeActivitySchedule.ID == referenceDataThree.referenceDataTwo.referenceDataOne.irisObject
                            .userActivity.activity.activity.RegimeActivityScheduleID),
                    (referenceDataThree, regimeActivitySchedule) => new { referenceDataThree, regimeActivitySchedule })
                .SelectMany(
                    referenceDataThree => Context.ReferenceDataValue.Where(r =>
                        r.ID == referenceDataThree.regimeActivitySchedule.ScheduleTypeREFID),
                    (regimeActivitySchedule, scheduleType) => new { regimeActivitySchedule, scheduleType })
                .SelectMany(
                    regimeActivitySchedule => (from status in Context.Statuses
                                               where !status.IsDeleted && status.IRISObjectID == regimeActivitySchedule.regimeActivitySchedule
                                                         .referenceDataThree.referenceDataTwo.referenceDataOne.irisObject.irisObject.ID
                                               orderby status.StatusDate descending, status.ID descending
                                               select status).Take(1),
                    (regimeActivitySchedule, latestStatus) => new { regimeActivitySchedule, latestStatus })
                .SelectMany(
                    latestStatus => Context.ReferenceDataValue.Where(referenceDataValue =>
                        referenceDataValue.ID == latestStatus.latestStatus.StatusREFID &&
                        referenceDataValue.ReferenceDataValueAttribute.Any(x =>
                            x.AttrName == ReferenceDataValueAttributeCodes.IsCurrentStatus &&
                            x.AttrValue == trueString)),
                    (latestStatus, referenceDataFour) => new
                    {
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne.irisObject.userActivity.activity.activity,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.regimeActivitySchedule,
                        latestStatus.regimeActivitySchedule.scheduleType,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne.irisObject,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree,
                        referenceDataFour,
                        latestStatus.latestStatus,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne.irisObject.userActivity.user,
                        latestStatus.regimeActivitySchedule.regimeActivitySchedule.referenceDataThree.referenceDataTwo
                            .referenceDataOne.irisObject.irisObject.SecurityContextIRISObject,
                        RegimeActivitySecurityContextObjectTypeREF = latestStatus.regimeActivitySchedule
                            .regimeActivitySchedule.referenceDataThree.referenceDataTwo.referenceDataOne.irisObject
                            .irisObject.SecurityContextIRISObject.ObjectTypeREF
                    }).Distinct().Count();
        }

        private IEnumerable<RegimeActivity> GetRegimeActivitiesWithStatusByRegimeId(long regimeId, bool isCurrentStatus)
        {
            var currentStatusString = isCurrentStatus.ToString();
            var dbquery = Context.RegimeActivity

                .SelectMany(activity => Context.User.Where(u => u.ID == activity.OfficerResponsibleID),
                    (activity, user) => new { activity, user })
                .SelectMany(@t => Context.IRISObject.Where(i => i.ID == @t.activity.IRISObjectID),
                    (@t, irisObject) => new { @t, irisObject })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.irisObject.ObjectTypeID),
                    (@t, objTypeREF) => new { @t, objTypeREF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.irisObject.SubClass1ID),
                    (@t, sub1REF) => new { @t, sub1REF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.@t.irisObject.SubClass2ID),
                    (@t, sub2REF) => new { @t, sub2REF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.@t.@t.irisObject.SubClass3ID),
                    (@t, sub3REF) => new { @t, sub3REF })
                .SelectMany(
                    @t => Context.RegimeActivitySchedule.Where(s =>
                        s.RegimeID == regimeId && s.ID == @t.@t.@t.@t.@t.@t.activity.RegimeActivityScheduleID),
                    (@t, schedule) => new { @t, schedule })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.schedule.ScheduleTypeREFID),
                    (@t, scheduleType) => new { @t, scheduleType })
                .SelectMany(
                    @t => Context.RegimeActivityComplianceAuthorisations.Where(a =>
                            a.RegimeActivityComplianceID == @t.@t.@t.@t.@t.@t.@t.@t.activity.ID && !a.IsDeleted)
                        .DefaultIfEmpty(), (@t, raAuth) => new { @t, raAuth })
                .SelectMany(
                    @t => Context.EstimationLabours.Where(l =>
                            l.RegimeActivityID == @t.@t.@t.@t.@t.@t.@t.@t.@t.activity.ID && !l.IsDeleted)
                        .DefaultIfEmpty(), (@t, labour) => new { @t, labour })
                .SelectMany(
                    @t => Context.EstimationLabourAuthorisations
                        .Where(l => l.EstimationLabourID == @t.labour.ID && !l.IsDeleted)
                        .DefaultIfEmpty(), (@t, labourAuth) => new { @t, labourAuth })
                .SelectMany(
                    @t => Context.EquipmentMaterials.Where(e =>
                            e.RegimeActivityID == @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.activity.ID && !e.IsDeleted)
                        .DefaultIfEmpty(), (@t, equipment) => new { @t, equipment })
                .SelectMany(
                    @t => Context.EquipmentMaterialAuthorisations
                        .Where(e => e.EquipmentMaterialID == @t.equipment.ID && !e.IsDeleted)
                        .DefaultIfEmpty(), (@t, equipmentAuth) => new { @t, equipmentAuth })
                .SelectMany(
                    @t => (from status in Context.Statuses
                           where !status.IsDeleted &&
                                 status.IRISObjectID == @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.irisObject.ID
                           orderby status.StatusDate descending, status.ID descending
                           select status).Take(1), (@t, latestStatus) => new { @t, latestStatus })
                .SelectMany(@t => Context.ReferenceDataValue, (@t, statusREF) => new { @t, statusREF })
                .GroupJoin(Context.ReferenceDataValueAttribute, @t => @t.statusREF.ID, attrib => attrib.ValueID,
                    (@t, sa) => new { @t, sa })
                .SelectMany(
                    @t => @t.sa.Where(x => x.AttrName == ReferenceDataValueAttributeCodes.IsCurrentStatus)
                        .DefaultIfEmpty(), (@t, statusAttrib) => new { @t, statusAttrib })
                .Where(@t => @t.@t.@t.statusREF.ID == @t.@t.@t.@t.latestStatus.StatusREFID && (isCurrentStatus
                                 ? @t.statusAttrib.AttrValue == currentStatusString
                                 : (@t.statusAttrib == null || @t.statusAttrib.AttrValue == currentStatusString)))
                .SelectMany(
                    @t => Context.RegimeActivityMngtSites.Where(a =>
                            a.RegimeActivityMngtID == @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.activity.ID && !a.IsDeleted)
                        .DefaultIfEmpty(), (@t, raMngtSites) => new { @t, raMngtSites })
                .Select(@t => new
                {
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.activity,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.schedule,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.scheduleType,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.irisObject,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.objTypeREF,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.sub1REF,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.sub2REF,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.sub3REF,
                    @t.@t.@t.@t.statusREF,
                    @t.@t.@t.@t.@t.latestStatus,
                    @t.@t.statusAttrib,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.user,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.irisObject.SecurityContextIRISObject,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.irisObject.SecurityContextIRISObject
                        .ObjectTypeREF,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.raAuth,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.labour,
                    @t.@t.@t.@t.@t.@t.@t.@t.labourAuth,
                    @t.@t.@t.@t.@t.@t.@t.equipment,
                    @t.@t.@t.@t.@t.@t.equipmentAuth,
                    @t.raMngtSites
                }).AsEnumerable().Select(s => s.activity).Distinct();
            SetHasAssociatedWorkflow(dbquery);
            return dbquery;
        }

        private int GetRegimeActivitiesWithStatusByRegimeIdCount(long regimeId, bool isCurrentStatus, bool currentActivities)
        {
            var currentStatusString = isCurrentStatus.ToString();
            var dbquery = Context.RegimeActivity
                .SelectMany(activity => Context.User.Where(u => u.ID == activity.OfficerResponsibleID),
                    (activity, user) => new { activity, user })
                .SelectMany(@t => Context.IRISObject.Where(i => i.ID == @t.activity.IRISObjectID),
                    (@t, irisObject) => new { @t, irisObject })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.irisObject.ObjectTypeID),
                    (@t, objTypeREF) => new { @t, objTypeREF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.irisObject.SubClass1ID),
                    (@t, sub1REF) => new { @t, sub1REF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.@t.irisObject.SubClass2ID),
                    (@t, sub2REF) => new { @t, sub2REF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.@t.@t.irisObject.SubClass3ID),
                    (@t, sub3REF) => new { @t, sub3REF })
                .SelectMany(
                    @t => Context.RegimeActivitySchedule.Where(s =>
                        s.RegimeID == regimeId && s.ID == @t.@t.@t.@t.@t.@t.activity.RegimeActivityScheduleID),
                    (@t, schedule) => new { @t, schedule })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.schedule.ScheduleTypeREFID),
                    (@t, scheduleType) => new { @t, scheduleType })
                .SelectMany(
                    @t => Context.RegimeActivityComplianceAuthorisations.Where(a =>
                            a.RegimeActivityComplianceID == @t.@t.@t.@t.@t.@t.@t.@t.activity.ID && !a.IsDeleted)
                        .DefaultIfEmpty(), (@t, raAuth) => new { @t, raAuth })
                .SelectMany(
                    @t => Context.EstimationLabours.Where(l =>
                            l.RegimeActivityID == @t.@t.@t.@t.@t.@t.@t.@t.@t.activity.ID && !l.IsDeleted)
                        .DefaultIfEmpty(), (@t, labour) => new { @t, labour })
                .SelectMany(
                    @t => Context.EstimationLabourAuthorisations
                        .Where(l => l.EstimationLabourID == @t.labour.ID && !l.IsDeleted)
                        .DefaultIfEmpty(), (@t, labourAuth) => new { @t, labourAuth })
                .SelectMany(
                    @t => Context.EquipmentMaterials.Where(e =>
                            e.RegimeActivityID == @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.activity.ID && !e.IsDeleted)
                        .DefaultIfEmpty(), (@t, equipment) => new { @t, equipment })
                .SelectMany(
                    @t => Context.EquipmentMaterialAuthorisations
                        .Where(e => e.EquipmentMaterialID == @t.equipment.ID && !e.IsDeleted)
                        .DefaultIfEmpty(), (@t, equipmentAuth) => new { @t, equipmentAuth })
                .SelectMany(
                    @t => (from status in Context.Statuses
                           where !status.IsDeleted &&
                                 status.IRISObjectID == @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.irisObject.ID
                           orderby status.StatusDate descending, status.ID descending
                           select status).Take(1), (@t, latestStatus) => new { @t, latestStatus })
                .SelectMany(@t => Context.ReferenceDataValue, (@t, statusREF) => new { @t, statusREF })
                .GroupJoin(Context.ReferenceDataValueAttribute, @t => @t.statusREF.ID, attrib => attrib.ValueID,
                    (@t, sa) => new { @t, sa })
                .SelectMany(
                    @t => @t.sa.Where(x => x.AttrName == ReferenceDataValueAttributeCodes.IsCurrentStatus)
                        .DefaultIfEmpty(), (@t, statusAttrib) => new { @t, statusAttrib })
                .Where(@t => @t.@t.@t.statusREF.ID == @t.@t.@t.@t.latestStatus.StatusREFID && (isCurrentStatus
                                 ? @t.statusAttrib.AttrValue == currentStatusString
                                 : (@t.statusAttrib == null || @t.statusAttrib.AttrValue == currentStatusString)))
                .Select(@t => new
                {
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.activity,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.schedule,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.scheduleType,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.irisObject,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.objTypeREF,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.sub1REF,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.sub2REF,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.sub3REF,
                    @t.@t.@t.statusREF,
                    @t.@t.@t.@t.latestStatus,
                    @t.statusAttrib,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.user,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.irisObject.SecurityContextIRISObject,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.@t.irisObject.SecurityContextIRISObject
                        .ObjectTypeREF,
                    @t.@t.@t.@t.@t.@t.@t.@t.@t.raAuth,
                    @t.@t.@t.@t.@t.@t.@t.@t.labour,
                    @t.@t.@t.@t.@t.@t.@t.labourAuth,
                    @t.@t.@t.@t.@t.@t.equipment,
                    @t.@t.@t.@t.@t.equipmentAuth
                }).AsEnumerable().Select(s => s.activity).Distinct();

            return currentActivities ? dbquery.GroupBy(s => s.RegimeActivityScheduleID).Count() : dbquery.Count();
        }

        private IEnumerable<RegimeActivity> GetCurrentRegimeActivitiesWithNoStatusByRegimeId(long regimeId)
        {
            var dbquery = Context.RegimeActivity
                .SelectMany(activity => Context.User.Where(u => u.ID == activity.OfficerResponsibleID),
                    (activity, user) => new { activity, user })
                .SelectMany(
                    @t => Context.IRISObject.Where(i => i.ID == @t.activity.IRISObjectID)
                        .Where(i => !Context.Statuses.Any(s => !s.IsDeleted && s.IRISObjectID == i.ID)),
                    (@t, irisObject) => new { @t, irisObject })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.irisObject.ObjectTypeID),
                    (@t, objTypeREF) => new { @t, objTypeREF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.irisObject.SubClass1ID),
                    (@t, sub1REF) => new { @t, sub1REF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.@t.irisObject.SubClass2ID),
                    (@t, sub2REF) => new { @t, sub2REF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.@t.@t.irisObject.SubClass3ID),
                    (@t, sub3REF) => new { @t, sub3REF })
                .SelectMany(
                    @t => Context.RegimeActivitySchedule.Where(s =>
                        s.RegimeID == regimeId && s.ID == @t.@t.@t.@t.@t.@t.activity.RegimeActivityScheduleID),
                    (@t, schedule) => new { @t, schedule })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.schedule.ScheduleTypeREFID),
                    (@t, scheduleType) => new
                    {
                        @t.@t.@t.@t.@t.@t.@t.activity,
                        @t.schedule,
                        scheduleType,
                        @t.@t.@t.@t.@t.@t.irisObject,
                        @t.@t.@t.@t.@t.objTypeREF,
                        @t.@t.@t.@t.sub1REF,
                        @t.@t.@t.sub2REF,
                        @t.@t.sub3REF,
                        @t.@t.@t.@t.@t.@t.@t.user,
                        @t.@t.@t.@t.@t.@t.irisObject.SecurityContextIRISObject,
                        RegimeActivitySecurityContextObjectTypeREF =
                            @t.@t.@t.@t.@t.@t.irisObject.SecurityContextIRISObject.ObjectTypeREF
                    }).AsEnumerable().Select(s => s.activity);

            return dbquery;
        }

        private int GetCurrentRegimeActivitiesWithNoStatusByRegimeIdCount(long regimeId, bool currentActivities)
        {
            var dbQuery = Context.RegimeActivity
                .SelectMany(activity => Context.User.Where(u => u.ID == activity.OfficerResponsibleID),
                    (activity, user) => new { activity, user })
                .SelectMany(
                    @t => Context.IRISObject.Where(i => i.ID == @t.activity.IRISObjectID)
                        .Where(i => !Context.Statuses.Any(s => !s.IsDeleted && s.IRISObjectID == i.ID)),
                    (@t, irisObject) => new { @t, irisObject })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.irisObject.ObjectTypeID),
                    (@t, objTypeREF) => new { @t, objTypeREF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.irisObject.SubClass1ID),
                    (@t, sub1REF) => new { @t, sub1REF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.@t.irisObject.SubClass2ID),
                    (@t, sub2REF) => new { @t, sub2REF })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.@t.@t.@t.irisObject.SubClass3ID),
                    (@t, sub3REF) => new { @t, sub3REF })
                .SelectMany(
                    @t => Context.RegimeActivitySchedule.Where(s =>
                        s.RegimeID == regimeId && s.ID == @t.@t.@t.@t.@t.@t.activity.RegimeActivityScheduleID),
                    (@t, schedule) => new { @t, schedule })
                .SelectMany(@t => Context.ReferenceDataValue.Where(r => r.ID == @t.schedule.ScheduleTypeREFID),
                    (@t, scheduleType) => new
                    {
                        @t.@t.@t.@t.@t.@t.@t.activity,
                        @t.schedule,
                        scheduleType,
                        @t.@t.@t.@t.@t.@t.irisObject,
                        @t.@t.@t.@t.@t.objTypeREF,
                        @t.@t.@t.@t.sub1REF,
                        @t.@t.@t.sub2REF,
                        @t.@t.sub3REF,
                        @t.@t.@t.@t.@t.@t.@t.user,
                        @t.@t.@t.@t.@t.@t.irisObject.SecurityContextIRISObject,
                        RegimeActivitySecurityContextObjectTypeREF =
                            @t.@t.@t.@t.@t.@t.irisObject.SecurityContextIRISObject.ObjectTypeREF
                    }).AsEnumerable().Select(s => s.activity).Distinct();

            return currentActivities ? dbQuery.GroupBy(s => s.RegimeActivityScheduleID).Count() : dbQuery.Count();
        }

        /// <summary>
        ///    Some current regime activities can be created with no statuses. These need to be returned
        ///    in any current view of regime activities
        /// </summary>
        private IEnumerable<RegimeActivity> GetCurrentRegimeActivitiesWithNoStatus(long regimeId)
        {
            var dbquery = from activity in Context.RegimeActivity
                          from user in Context.User.Where(u => u.ID == activity.OfficerResponsibleID)
                          from irisObject in Context.IRISObject
                                .Where(i => i.ID == activity.IRISObjectID)
                                .Where(i => !Context.Statuses.Any(s => !s.IsDeleted && s.IRISObjectID == i.ID)) // IRIS object does not have any statuses
                          from objTypeREF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.ObjectTypeID)
                          from sub1REF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.SubClass1ID)
                          from sub2REF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.SubClass2ID)
                          from sub3REF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.SubClass3ID)
                          from schedule in Context.RegimeActivitySchedule.Where(s => s.RegimeID == regimeId && s.ID == activity.RegimeActivityScheduleID)
                          from scheduleType in Context.ReferenceDataValue.Where(r => r.ID == schedule.ScheduleTypeREFID)

                          select new
                          {
                              activity,
                              schedule,
                              scheduleType,
                              irisObject,
                              objTypeREF,
                              sub1REF,
                              sub2REF,
                              sub3REF,
                              user,
                              irisObject.SecurityContextIRISObject,
                              RegimeActivitySecurityContextObjectTypeREF = irisObject.SecurityContextIRISObject.ObjectTypeREF
                          };

            var result = dbquery.AsEnumerable().Select(s => s.activity).Distinct();
            SetHasAssociatedWorkflow(result);

            return result;
        }

        /// <summary>
        /// This method retrieves the Regime Activities that has the Latest Status's attribute of IsCurrentStatus
        /// </summary>
        /// <param name="regimeId"></param>
        /// <param name="isCurrentStatus"></param>
        /// <returns></returns>
        private IEnumerable<RegimeActivity> GetRegimeActivitiesWithStatus(long regimeId, bool isCurrentStatus)
        {
            var currentStatusString = isCurrentStatus.ToString();
            var dbquery = from activity in Context.RegimeActivity
                          from user in Context.User.Where(u => u.ID == activity.OfficerResponsibleID)
                          from irisObject in Context.IRISObject.Where(i => i.ID == activity.IRISObjectID)
                          from objTypeREF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.ObjectTypeID)
                          from sub1REF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.SubClass1ID)
                          from sub2REF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.SubClass2ID)
                          from sub3REF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.SubClass3ID)

                          from schedule in Context.RegimeActivitySchedule.Where(s =>
                              s.RegimeID == regimeId && s.ID == activity.RegimeActivityScheduleID)
                          from scheduleType in Context.ReferenceDataValue.Where(r => r.ID == schedule.ScheduleTypeREFID)
                          from raAuth in Context.RegimeActivityComplianceAuthorisations
                              .Where(a => a.RegimeActivityComplianceID == activity.ID && !a.IsDeleted).DefaultIfEmpty()
                          from raAuthCon in Context.RegimeActivityComplianceConditions
                              .Where(c => c.RegimeActivityComplianceAuthorisationID == raAuth.ID && !c.IsDeleted).DefaultIfEmpty()
                          from labour in Context.EstimationLabours.Where(l => l.RegimeActivityID == activity.ID && !l.IsDeleted)
                              .DefaultIfEmpty()
                          from labourAuth in Context.EstimationLabourAuthorisations
                              .Where(l => l.EstimationLabourID == labour.ID && !l.IsDeleted).DefaultIfEmpty()
                          from equipment in Context.EquipmentMaterials
                              .Where(e => e.RegimeActivityID == activity.ID && !e.IsDeleted).DefaultIfEmpty()
                          from equipmentAuth in Context.EquipmentMaterialAuthorisations
                              .Where(e => e.EquipmentMaterialID == equipment.ID && !e.IsDeleted).DefaultIfEmpty()

                          from latestStatus in
                          (
                              from status in Context.Statuses
                              where !status.IsDeleted && status.IRISObjectID == irisObject.ID
                              orderby status.StatusDate descending, status.ID descending
                              select status).Take(1)

                          from statusREF in Context.ReferenceDataValue
                          join attrib in Context.ReferenceDataValueAttribute on statusREF.ID equals attrib.ValueID into sa
                          from statusAttrib in sa.Where(x => x.AttrName == ReferenceDataValueAttributeCodes.IsCurrentStatus)
                              .DefaultIfEmpty()
                          where statusREF.ID == latestStatus.StatusREFID

                          select new
                          {
                              activity,
                              schedule,
                              scheduleType,
                              irisObject,
                              objTypeREF,
                              sub1REF,
                              sub2REF,
                              sub3REF,
                              statusREF,
                              latestStatus,
                              statusAttrib,
                              user,
                              irisObject.SecurityContextIRISObject,
                              irisObject.SecurityContextIRISObject.ObjectTypeREF,
                              raAuth,
                              raAuthCon,
                              labour,
                              labourAuth,
                              equipment,
                              equipmentAuth
                          };

            if (isCurrentStatus)
            {
                dbquery = dbquery.Where(x => x.statusAttrib.AttrValue == currentStatusString);
            }
            else
            {
                dbquery = dbquery.Where(x => x.statusAttrib == null || x.statusAttrib.AttrValue == currentStatusString);
            }

            var result = dbquery.AsEnumerable().Select(s => s.activity).Distinct();
            SetHasAssociatedWorkflow(result);
            return result;
        }

        private static void SetHasAssociatedWorkflow(IEnumerable<RegimeActivity> result)
        {
            var irisObjectsIdWithWF =
                RepositoryMap.WorkflowRepository.GeIRISObjectIDsHasWorkflowRunningInstnace(
                    result.Select(x => x.IRISObjectID).ToList());

            foreach (var regimeActivity in result)
            {
                regimeActivity.HasAssociatedWorkflow = irisObjectsIdWithWF.Contains(regimeActivity.IRISObjectID);
            }
        }

        #endregion

        /// <summary>
        ///    A regime activity can only be deleted if it doesn't have any links to an observation
        ///    or a sample results. This methods counts the number of relationships and returns
        ///    a boolean value
        /// </summary>
        /// <param name="regimeActivityId"></param>
        /// <returns></returns>
        public bool HasLinksToObservationOrRemediationOrSampleResult(long regimeActivityId)
        {
            long mobileInspectionCount = 0;
            int observationsCount = Context.Observations.Count(o => o.RegimeActivityID == regimeActivityId);
            int remediationCount = Context.Remediations.Count(r => r.RegimeActivityID == regimeActivityId && !r.IsDeleted);
            int sampleResultsCount = Context.SampleResults.Count(o => o.RegimeActivityID == regimeActivityId);
            bool hasCurrentMobileInspection = Context.RegimeActivityComplianceAuthorisations.Any(
                    x => x.RegimeActivityComplianceID == regimeActivityId && !x.IsDeleted && x.MobileInspectionStatus ==
                    (long)MobileInspectionStatus.SyncedToSphere);
            if (hasCurrentMobileInspection)
            {
                mobileInspectionCount = 1;
            }
            else
            {
                mobileInspectionCount = 0;
            }
            return observationsCount + sampleResultsCount + mobileInspectionCount + remediationCount > 0;
        }

        /// <summary>
        ///    A observation can only be deleted if it doesn't have any links to a sample results. 
        ///    This methods counts the number of relationships and returns a boolean value
        /// </summary>
        /// <param name="regimeActivityId"></param>
        /// <returns></returns>
        public bool HasLinksToSampleResult(long observationId)
        {
            int sampleResultsCount = Context.SampleResults.Count(o => o.ObservationID == observationId);
            return sampleResultsCount > 0;
        }

        public RegimeActivity GetRegimeActivityByIdForDetailsPage(long id)
        {
            var result = Context.RegimeActivity
                .Include(x => x.RegimeActivitySchedule.ScheduleTypeREF)
                .Include(x => x.LabourPositionREF)
                .Include(x => x.NextFinancialYearREF)
                .Include(x => x.FinancialYearREF)
                .Include(x => x.PreviousEstimationActivity.IRISObject)
                .Include(x => x.EstimationLabours)
                .Include("EstimationLabours.EstimationLabourAuthorisations")
                .Include(x => x.EquipmentMaterials)
                .Include("EquipmentMaterials.EquipmentMaterialAuthorisations")
                .Include("RegimeActivitySchedule.Regime")
                .Include("RegimeActivitySchedule.Regime.IRISObject")
                .Include("RegimeActivitySchedule.Regime.FinancialYearREF")
                .Include(x => x.OfficerResponsible).Single(x => x.ID == id);

            result.RegimeActivityResourceNeededs.AddTrackableCollection(GetResourcesNeeded(id));
            result.SampleResults.AddTrackableCollection(Context.SampleResults.Include(x => x.IRISObject).Where(x => x.RegimeActivityID == id).ToList());
            result.IRISObject = GetIRISObject(result.IRISObjectID, Level3);

            // check if the regime activity has workflow.
            var irisObjectIDWithWF = RepositoryMap.WorkflowRepository.GeIRISObjectIDsHasWorkflowRunningInstnace(new List<long> { result.IRISObjectID });
            result.HasAssociatedWorkflow = irisObjectIDWithWF.Contains(result.IRISObjectID);

            GetRegimeActivityTypeSpecificValues(result);

            return result.TrackAll();
        }

        public RegimeActivity GetRegimeActivityByID(long id)
        {
            var result = Context.RegimeActivity
               .Include(x => x.RegimeActivitySchedule.ScheduleTypeREF)
               .Include("RegimeActivitySchedule.Regime")
               .Include("RegimeActivitySchedule.Regime.IRISObject")
               .Include(x => x.OfficerResponsible).Single(x => x.ID == id);

            result.IRISObject = GetIRISObject(result.IRISObjectID, Level3);
            // check if the regime activity has workflow.
            var irisObjectIDWithWF = RepositoryMap.WorkflowRepository.GeIRISObjectIDsHasWorkflowRunningInstnace(new List<long> { result.IRISObjectID });
            result.HasAssociatedWorkflow = irisObjectIDWithWF.Contains(result.IRISObjectID);

            return result.TrackAll();
        }

        public RegimeActivity GetRegimeActivityByIRISObjectID(long id)
        {
            var result = Context.RegimeActivity
                .Include(x => x.RegimeActivitySchedule.ScheduleTypeREF)
                .Include(x => x.NextFinancialYearREF)
               .Include("RegimeActivitySchedule.Regime")
               .Include("RegimeActivitySchedule.Regime.IRISObject")
               .Include(x => x.OfficerResponsible)
               .Include("IRISObject.ActivityObjectRelationships.ActivityObjectRelationshipType")
               .Single(x => x.IRISObjectID == id);

            return result.TrackAll();
        }

        public void SaveRegimeActivityDetails(RegimeActivityMngt mngt, long managementSiteID)
        {
            RegimeActivityMngtSite raMgmtSite = mngt.RegimeActivityMngtSites.SingleOrDefault(x => !x.IsDeleted && x.ManagementSiteID == managementSiteID);
            raMgmtSite.MobileInspectionStatus = (long)MobileInspectionStatus.SyncedBackToIRIS;
            if (!mngt.RegimeActivityMngtSites.Any(x => !x.IsDeleted && x.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedToSphere))
            {
                mngt.IRISObject.MobileInspectionStatus = (long)MobileInspectionStatus.SyncedBackToIRIS;
            }
            ApplyEntityChanges<RegimeActivity>(mngt);
            SaveChanges();
        }


        public SampleResult GetSampleResultById(long id)
        {
            var sampleResult = Context.SampleResults
                          .Include(x => x.IRISObject)
                          .Include(x => x.IRISObject.ObjectTypeREF)
                          .Include(x => x.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                          .Include(x => x.RegimeActivity.RegimeActivitySchedule.ScheduleTypeREF)
                          .Include(x => x.RegimeActivity.RegimeActivitySchedule.Regime)
                          .Include(x => x.RegimeActivity.RegimeActivitySchedule.Regime.IRISObject)
                          .Include(x => x.RegimeActivity)
                          .Include(x => x.RegimeActivity.IRISObject)
                          .Include(x => x.RegimeActivity.IRISObject.SubClass1REF)
                          .Include(x => x.RegimeActivity.IRISObject.SubClass2REF)
                          .Include(x => x.RegimeActivity.IRISObject.SubClass3REF)
                          .Include(x => x.RegimeActivity.OfficerResponsible)
                          .Include(x => x.RegimeActivity.IRISObject.Statuses)
                          .Include(x => x.RegimeActivity.RegimeActivitySchedule.Regime.FinancialYearREF)
                          .Include("RegimeActivity.IRISObject.Statuses.StatusREF")
                          .Include(x => x.Observation)
                          .Include(x => x.Observation.IRISObject.SubClass1REF)
                          .SingleOrDefault(x => x.ID == id);

            if (sampleResult.RegimeActivityID.HasValue)
            {
                return sampleResult.TrackAll();
            }

            Observation observation;

            // whether the observation created from a regime activity
            if (sampleResult.Observation.RegimeActivityID.HasValue)
            {
                observation = Context.Observations
                                .Include(x => x.IRISObject)
                                .Include(x => x.RegimeActivity)
                                .Include(x => x.RegimeActivity.OfficerResponsible)
                                .Include(x => x.RegimeActivity.IRISObject)
                                .Include(x => x.RegimeActivity.IRISObject.SubClass2REF)
                                .Include(x => x.RegimeActivity.IRISObject.SubClass3REF)
                                .Include(x => x.RegimeActivity.RegimeActivitySchedule.Regime.IRISObject)
                                .Include(x => x.RegimeActivity.RegimeActivitySchedule.Regime)
                                .Include("RegimeActivity.IRISObject.Statuses.StatusREF")
                                .SingleOrDefault(x => x.ID == sampleResult.ObservationID);

                if (observation is ObservationMngt)
                {
                    ObservationMngt observationMngt = observation as ObservationMngt;
                    observationMngt.ManagementSite = Context.ManagementSites.SingleOrDefault(x => x.ID == observationMngt.ManagementSiteID);
                }
            }
            else if (sampleResult.Observation.AuthorisationID.HasValue)
            {
                observation = Context.Observations
                                .Include(x => x.IRISObject)
                                .Include(x => x.Authorisation)
                                .Include(x => x.Authorisation.OfficerResponsible)
                                .Include(x => x.Authorisation.IRISObject)
                                .Include(x => x.Authorisation.IRISObject.SubClass2REF)
                                .Include(x => x.Authorisation.IRISObject.SubClass3REF)
                                .Include("Authorisation.IRISObject.Statuses.StatusREF")
                                .SingleOrDefault(x => x.ID == sampleResult.ObservationID);
            }
            else if (sampleResult.Observation.RequestID.HasValue)
            {
                observation = Context.Observations
                                .Include(x => x.IRISObject)
                                .Include(x => x.Request)
                                .Include(x => x.Request.IRISObject)
                                .Include(x => x.Request.IRISObject.SubClass1REF)
                                .Include(x => x.Request.IRISObject.SubClass2REF)
                                .Include(x => x.Request.RequestTypeREF)
                                .Include(x => x.Request.RequestTypeIncident)
                                .Include("Request.IRISObject.Statuses.StatusREF")
                                .SingleOrDefault(x => x.ID == sampleResult.ObservationID);
            }
            else
            {
                observation = Context.Observations.OfType<ObservationMngt>()
                                .Include(x => x.ManagementSite)
                                .Include(x => x.ManagementSite.IRISObject.SubClass1REF)
                                .Include(x => x.ManagementSite.IRISObject.SubClass2REF)
                                .Include("ManagementSite.IRISObject.Statuses.StatusREF")
                                .SingleOrDefault(x => x.ID == sampleResult.ObservationID);
            }

            sampleResult.Observation = observation;

            return sampleResult.TrackAll();
        }

        /// <summary>
        /// Used to fetch neccessary RegimeActivity information for observation detail page usage.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RegimeActivity GetRegimeActivityForObservation(long id)
        {
            //RegimeActivitySchedule.RegimeID is used for add/copy observationCompliance
            var result = Context.RegimeActivity.Include(x => x.RegimeActivitySchedule).SingleOrDefault(x => x.ID == id);

            if (result == null) return null;

            //RegimeActivity.IRISObject.SubClass1REF used in Add/Copy another observation
            //RegimeActivity.IRISObject used in Creating FurtherAction
            result.IRISObject = GetIRISObject(result.IRISObjectID, Level1);

            return result;
        }

        public List<RegimeActivity> GetFutureDatedScheduledRegimeActivitiesByScheduleIdAndRegimeActivityId(long scheduleId, long regimeActivityId, long irisObjectId, DateTime cutoffDate)
        {
            return Context.RegimeActivity
                          .Include(x => x.RegimeActivitySchedule)
                          .Include(x => x.IRISObject)
                          .Include(x => x.IRISObject.Statuses)
                          .Include("IRISObject.Statuses.StatusREF")
                          .Where(x => x.RegimeActivityScheduleID == scheduleId && x.TargetFromDate >= cutoffDate && x.ID != regimeActivityId && x.IRISObjectID != irisObjectId).ToList();
        }

        public List<RegimeActivity> GetRegimeActivitiesForSchedule(long scheduleID)
        {
            return Context.RegimeActivity
                          .Include(x => x.IRISObject)
                          .Include(x => x.IRISObject.SubClass1REF)
                          .Include(x => x.IRISObject.SubClass2REF)
                          .Include(x => x.IRISObject.SubClass3REF)
                          .Include(x => x.IRISObject.Statuses)
                          .Include("IRISObject.Statuses.StatusREF")
                          .Include(x => x.OfficerResponsible)
                          .Where(x => x.RegimeActivityScheduleID == scheduleID).ToList();
        }

        public Observation GetObservationByIdForDetailsPage(long id)
        {
            var result = Context.Observations
                .Include(x => x.AirTemperatureREF)
                .Include(x => x.ObservedByREF)
                .Include(x => x.RegimeActivity.OfficerResponsible)
                .Include(x => x.RegimeActivity.RegimeActivitySchedule)
                .Include(x => x.CloudCoverREF)
                .Include(x => x.RainfallREF)
                .Include(x => x.WindDirectionREF)
                .Include(x => x.WindStrengthREF)
                .Include(x => x.ActualLabours)
                .Include("ActualLabours.ActualLabourAuthorisations")
                .Include(x => x.ActualEquipmentMaterials)
                .Include("ActualEquipmentMaterials.ActualEquipmentMaterialAuthorisations")
                .Single(x => x.ID == id);

            result.ObservationObservingOfficers.AddTrackableCollection(Context.ObservationObservingOfficers.Include(o => o.User).Where(o => o.ObservationID == result.ID && !o.IsDeleted).ToList());

            result.ObservationFurtherActions.AddTrackableCollection(Context.ObservationFurtherActions.Include(o => o.ConductedByREF)
                                                                                                        .Include(o => o.FurtherActionTypeREF)
                                                                                                        .Where(o => o.ObservationID == result.ID && !o.IsDeleted).ToList());


            result.SampleResults.AddTrackableCollection(Context.SampleResults.Include(x => x.IRISObject).Where(x => x.ObservationID == id).ToList());
            result.IRISObject = GetIRISObject(result.IRISObjectID, Level3);

            GetObservationTypeSpecificValues(result);

            return result.TrackAll();
        }

        public Observation GetObservationByIRISObjectID(long id)
        {
            var obs = Context.Observations
                .Include("IRISObject.ActivityObjectRelationships.ActivityObjectRelationshipType")
                .Single(x => x.IRISObjectID == id);
            if (obs is ObservationMngt)
            {
                return Context.Observations.OfType<ObservationMngt>()
                .Include(x => x.IRISObject)
                .Include("IRISObject.ObjectTypeREF")
                .Include("IRISObject.SubClass1REF")
                .Include(x => x.ManagementSite)
                .Include("IRISObject.SecurityContextIRISObject")
                .Include("IRISObject.SecurityContextIRISObject.ObjectTypeREF")
                .Include("ManagementSite.IRISObject")
                .Include("ManagementSite.IRISObject.SubClass1REF")
                .Include("ManagementSite.IRISObject.SubClass2REF")
                .Include("ManagementSite.IRISObject.Statuses")
                .Include("ManagementSite.IRISObject.Statuses.StatusREF")
                .Include(x => x.Survey)
                .Single(x => x.IRISObjectID == id)
                .TrackAll();
            }

            if (obs.AuthorisationID.HasValue)
            {
                obs.Authorisation = Context.Authorisations
                    .Include(x => x.IRISObject)
                    .Include(x => x.IRISObject.SubClass2REF)
                    .Include(x => x.IRISObject.SubClass3REF)
                    .Include(x => x.OfficerResponsible)
                    .Include("IRISObject.Statuses.StatusREF")
                    .Single(x => x.ID == obs.AuthorisationID.Value);
            }

            if (obs.RequestID.HasValue)
            {
                obs.Request = Context.Request
                    .Include(x => x.IRISObject)
                    .Include(x => x.IRISObject.SubClass1REF)
                    .Include(x => x.IRISObject.SubClass2REF)
                    .Include(x => x.RequestTypeREF)
                    .Include(x => x.RequestTypeIncident)
                    .Include("IRISObject.Statuses.StatusREF")
                    .Single(x => x.ID == obs.RequestID.Value);
            }

            var result = GetById<Observation>(obs.ID, "IRISObject.ObjectTypeREF,IRISObject.SubClass1REF,IRISObject.SubClass2REF,Survey,RegimeActivity,RegimeActivity.SubClass2-3,RegimeActivity.OfficerResponsible,RegimeActivity.RegimeActivitySchedule,RegimeActivity.RegimeActivitySchedule.Regime,RegimeActivity.RegimeActivitySchedule.Regime.IRISObject,RegimeActivity.LatestStatus");
            if (result != null && result is ObservationMngt)
            {
                var managementSiteID = (result as ObservationMngt).ManagementSiteID;
                var managementSite = Context.ManagementSites.SingleOrDefault(x => x.ID == managementSiteID);
                (result as ObservationMngt).ManagementSite = managementSite;
            }
            else if (result != null && result is ObservationCompliance)
            {
                (result as ObservationCompliance).ObservationComplianceAuthorisations.AddTrackableCollection(Context.ObservationComplianceAuthorisations.Where(x => x.ObservationComplianceID == result.ID).ToList());
            }
            return result.TrackAll();
        }

        public void AddObservationMngtFromObservationById(long id, long managementSiteId)
        {             
            if(Context.Observations.OfType<ObservationMngt>().SingleOrDefault(x => x.ID == id) == null)
                Context.LinkObservationToManagementSite(id, managementSiteId, SecurityHelper.CurrentUserName);
        }

        public Observation GetObservationById(long id)
        {
            var obs = Context.Observations.Single(x => x.ID == id);

            if (!obs.RegimeActivityID.HasValue && !obs.AuthorisationID.HasValue && !obs.RequestID.HasValue)
            {
                return Context.Observations.OfType<ObservationMngt>()
                .Include(x => x.IRISObject)
                .Include("IRISObject.ObjectTypeREF")
                .Include("IRISObject.SubClass1REF")
                .Include(x => x.ManagementSite)
                .Include("IRISObject.SecurityContextIRISObject")
                .Include("IRISObject.SecurityContextIRISObject.ObjectTypeREF")
                .Include("ManagementSite.IRISObject")
                .Include("ManagementSite.IRISObject.SubClass1REF")
                .Include("ManagementSite.IRISObject.SubClass2REF")
                .Include("ManagementSite.IRISObject.Statuses")
                .Include("ManagementSite.IRISObject.Statuses.StatusREF")
                .Include(x => x.Survey)
                .Single(x => x.ID == id)
                .TrackAll();
            }

            if (obs.AuthorisationID.HasValue)
            {
                obs.Authorisation = Context.Authorisations
                    .Include(x => x.IRISObject)
                    .Include(x => x.IRISObject.SubClass2REF)
                    .Include(x => x.IRISObject.SubClass3REF)
                    .Include(x => x.OfficerResponsible)
                    .Include("IRISObject.Statuses.StatusREF")
                    .Single(x => x.ID == obs.AuthorisationID.Value);
            }

            if (obs.RequestID.HasValue)
            {
                obs.Request = Context.Request
                    .Include(x => x.IRISObject)
                    .Include(x => x.IRISObject.SubClass1REF)
                    .Include(x => x.IRISObject.SubClass2REF)
                    .Include(x => x.RequestTypeREF)
                    .Include(x => x.RequestTypeIncident)
                    .Include("IRISObject.Statuses.StatusREF")
                    .Single(x => x.ID == obs.RequestID.Value);
            }

            var result = GetById<Observation>(id, "IRISObject.ObjectTypeREF,IRISObject.SubClass1REF,IRISObject.SubClass2REF,Survey,RegimeActivity,RegimeActivity.SubClass2-3,RegimeActivity.OfficerResponsible,RegimeActivity.RegimeActivitySchedule,RegimeActivity.RegimeActivitySchedule.Regime,RegimeActivity.RegimeActivitySchedule.Regime.IRISObject,RegimeActivity.LatestStatus");
            if (result != null && result is ObservationMngt)
            {
                var managementSiteID = (result as ObservationMngt).ManagementSiteID;
                var managementSite = Context.ManagementSites.SingleOrDefault(x => x.ID == managementSiteID);
                (result as ObservationMngt).ManagementSite = managementSite;
            }
            else if (result != null && result is ObservationCompliance)
            {
                (result as ObservationCompliance).ObservationComplianceAuthorisations.AddTrackableCollection(Context.ObservationComplianceAuthorisations.Where(x => x.ObservationComplianceID == result.ID).ToList());
            }
            return result.TrackAll();
        }

        public Programme GetProgrammeById(long id)
        {
            var result = Context.Programme
                .Include(x => x.IRISObject)
                .Include(x => x.IRISObject.SubClass1REF)
                .Include(x => x.IRISObject.SubClass2REF)
                .Include(x => x.OfficerResponsible)
                .Include(x => x.FinancialYearREF)
                .Include(x => x.NextFinancialYearREF)
                .Include(x => x.ProgrammeRatingREF)
                .Include(x => x.PriorityREF).SingleOrDefault(x => x.ID == id);

            if (result == null) return null;
            result.IRISObject = GetIRISObject(result.IRISObjectID);

            return result.TrackAll();
        }

        public List<Programme> GetProgrammesByIRISObjectIDs(List<long> irisObjectIDsList)
        {
            var results = Context.Programme
                .Include(x => x.IRISObject)
                .Include(x => x.IRISObject.SubClass1REF)
                .Include(x => x.IRISObject.SubClass2REF)
                .Include(x => x.OfficerResponsible)
                .Include(x => x.FinancialYearREF)
                .Include(x => x.NextFinancialYearREF)
                .Include(x => x.ProgrammeRatingREF)
                .Include(x => x.PriorityREF)
                .Where(x => irisObjectIDsList.Contains(x.IRISObjectID))
                .ToList();
            return results;
        }

        #region Methods to reteive type specific values for hierarchical types.

        private void GetRegimeTypeSpecificValues(Regime result)
        {
            var regimeSelectedLandUseSite = result as RegimeSelectedLandUseSite;
            if (regimeSelectedLandUseSite != null)
            {
                regimeSelectedLandUseSite.FundingREF = GetReferenceDataValue(regimeSelectedLandUseSite.FundingREFID);
            }
        }

        private void GetObservationTypeSpecificValues(Observation result)
        {
            if (result.RegimeActivityID.HasValue)
            {
                result.RegimeActivity.IRISObject = Context.IRISObject
                    .Include(x => x.SubClass2REF)
                    .Include(x => x.SubClass3REF)
                    .Include(x => x.Statuses)
                    .Include("Statuses.StatusREF")
                    .Single(x => x.ID == result.RegimeActivity.IRISObjectID);

                result.RegimeActivity.RegimeActivitySchedule.Regime = Context.Regime
                    .Include(x => x.IRISObject)
                    .Single(x => x.ID == result.RegimeActivity.RegimeActivitySchedule.RegimeID);
            }

            if (result.AuthorisationID.HasValue)
            {
                result.Authorisation = Context.Authorisations
                    .Include(x => x.IRISObject)
                    .Include(x => x.IRISObject.SubClass2REF)
                    .Include(x => x.IRISObject.SubClass3REF)
                    .Include(x => x.OfficerResponsible)
                    .Include("IRISObject.Statuses.StatusREF")
                    .Single(x => x.ID == result.AuthorisationID.Value);
            }

            if (result.RequestID.HasValue)
            {
                result.Request = Context.Request
                    .Include(x => x.IRISObject)
                    .Include(x => x.IRISObject.SubClass1REF)
                    .Include(x => x.IRISObject.SubClass2REF)
                    .Include(x => x.RequestTypeREF)
                    .Include(x => x.RequestTypeIncident)
                    .Include("IRISObject.Statuses.StatusREF")
                    .Single(x => x.ID == result.RequestID.Value);
                
                var dbquery = from count in Context.ObservationRequestSpeciesCounts.Where(c => c.ObservationID == result.ID && !c.IsDeleted)
                              from species in Context.Species.Where(s => s.ID == count.SpeciesID)
                              from speciesType in Context.SpeciesTypes.Where(s => s.SpeciesID == species.ID)
                              from refData1 in Context.ReferenceDataValue.Where(rdv => rdv.ID == count.ObservationMethodREFID)
                              from countItem in Context.ObservationRequestSpeciesCountItems.Where(i => i.ObservationRequestSpeciesCountID == count.ID && !i.IsDeleted).DefaultIfEmpty()
                              select new { count, species, refData1, countItem, speciesType };

                result.ObservationRequestSpeciesCounts.AddTrackableCollection(dbquery.AsEnumerable().Distinct().Select(x => x.count).ToList());
            }

            var observationMngt = result as ObservationMngt;
            if (observationMngt != null)
            {

                observationMngt.ObservationMngtLineMonitoring = Context.ObservationMngtLineMonitorings.Include(x => x.MethodREF).Include(x => x.PhaseREF).Include(x => x.TargetSpeciesREF)
                    .SingleOrDefault(x => x.ID == observationMngt.ObservationMngtLineMonitoringID);  //0..1 so may be null.
                observationMngt.ObservationMngtLines.AddTrackableCollection(GetObservationLinesByObservationMngtId(result.ID));
                foreach (var line in observationMngt.ObservationMngtLines)
                {
                    line.ObservationMngtLineResults.AddTrackableCollection(GetObservationLineResultsByObservationMngtLineId(line.ID));
                    foreach (var lineResult in line.ObservationMngtLineResults)
                    {
                        lineResult.ObservationMngtLineResultItems.AddTrackableCollection(GetObservationLineResultItemsByObservationMngtLineResultId(lineResult.ID));
                    }
                }

                observationMngt.ManagementSite = Context.ManagementSites.SingleOrDefault(x => x.ID == observationMngt.ManagementSiteID);
                if (observationMngt.ManagementSite != null)
                {
                    observationMngt.ManagementSite.IRISObject = GetIRISObject(observationMngt.ManagementSite.IRISObjectID, Level1);
                }

                var dbquery = from count in Context.ObservationMngtSpeciesCounts.Where(c => c.ObservationMngtID == observationMngt.ID && !c.IsDeleted)
                              from species in Context.Species.Where(s => s.ID == count.SpeciesID)
                              from speciesType in Context.SpeciesTypes.Where(s => s.SpeciesID == species.ID)
                              from refData1 in Context.ReferenceDataValue.Where(rdv => rdv.ID == count.ObservationMethodREFID)
                              from countItem in Context.ObservationMngtSpeciesCountItems.Where(i => i.ObservationMngtSpeciesCountID == count.ID && !i.IsDeleted).DefaultIfEmpty()
                              select new { count, species, refData1, countItem, speciesType };

                observationMngt.ObservationMngtSpeciesCounts.AddTrackableCollection(dbquery.AsEnumerable().Distinct().Select(x => x.count).ToList());

                var dbquery2 = from observationMngtSitePlan in Context.ObservationMngtSitePlans.Where(c => c.ObservationMngtID == observationMngt.ID && !c.IsDeleted)
                               from sitePlan in Context.SitePlans.Where(s => s.ID == observationMngtSitePlan.SitePlanID && !s.IsDeleted)
                               from ref1 in Context.ReferenceDataValue.Where(r => r.ID == sitePlan.TypeREFID)
                               from ref2 in Context.ReferenceDataValue.Where(r => r.ID == sitePlan.StatusREFID)
                               select new { observationMngtSitePlan, sitePlan, ref1, ref2 };
                observationMngt.ObservationMngtSitePlans.AddTrackableCollection(dbquery2.AsEnumerable().Distinct().Select(x => x.observationMngtSitePlan).ToList());

                return;
            }

            var observationCompliance = result as ObservationCompliance;
            if (observationCompliance != null)
            {
                if (observationCompliance.ComplianceStatusREFID != null)
                {
                    observationCompliance.ComplianceStatusREF = Context.ReferenceDataValue.Single(r => r.ID == observationCompliance.ComplianceStatusREFID);
                }

                if (observationCompliance.RiskOfNonComplianceREFID != null)
                {
                    observationCompliance.RiskOfNonComplianceREF = Context.ReferenceDataValue.Single(r => r.ID == observationCompliance.RiskOfNonComplianceREFID);
                }

                var dbquery = from observationComplianceAuthorisation in Context.ObservationComplianceAuthorisations.Where(a => a.ObservationComplianceID == observationCompliance.ID && !a.IsDeleted)
                              from authorisation in Context.Authorisations.Where(a => a.ID == observationComplianceAuthorisation.AuthorisationID)
                              from authorisationObj in Context.IRISObject.Where(o => o.ID == authorisation.IRISObjectID)
                              from observationComplianceCondition in Context.ObservationComplianceConditions.Where(c => c.ObservationComplianceAuthorisationID == observationComplianceAuthorisation.ID && !c.IsDeleted).DefaultIfEmpty()
                              from observationComplianceConditionParam in Context.ObservationComplianceParameters.Where(p => p.ObservationComplianceConditionID == observationComplianceCondition.ID).DefaultIfEmpty()
                              select new
                              {
                                  observationComplianceAuthorisation,
                                  authorisation,
                                  authorisationObj,
                                  observationComplianceCondition,
                                  observationComplianceConditionParam
                              };

                var observationComplianceAuthorisations = dbquery.AsEnumerable().Select(x => x.observationComplianceAuthorisation).Distinct().ToList();
                observationCompliance.ObservationComplianceAuthorisations.AddTrackableCollection(observationComplianceAuthorisations);
                return;
            }

            var observationSelectedLandUse = result as ObservationSelectedLandUse;
            if (observationSelectedLandUse != null)
            {
                List<ObservationSelectedLandUseIndicator> indicators = Context.ObservationSelectedLandUseIndicators
                                                                                .Include(x => x.IndicatorREF)
                                                                                .Where(x => x.ObservationSelectedLandUseID == observationSelectedLandUse.ID)
                                                                                .ToList();
                observationSelectedLandUse.ObservationSelectedLandUseIndicators.AddTrackableCollection(indicators);
                return;
            }
        }

        private void GetRegimeActivityTypeSpecificValues(RegimeActivity result)
        {
            var regimeActivityDam = result as RegimeActivityDam;
            if (regimeActivityDam == null)
                result.SampleResults.AddTrackableCollection(Context.SampleResults.Include(x => x.IRISObject).Where(x => x.RegimeActivityID == result.ID).ToList());

            var regimeActivityCompliance = result as RegimeActivityCompliance;
            if (regimeActivityCompliance != null)
            {
                regimeActivityCompliance.OverallComplianceStatusREF = GetReferenceDataValue(regimeActivityCompliance.OverallComplianceStatusREFID);

                regimeActivityCompliance.RegimeActivityComplianceAuthorisations.AddTrackableCollection(
                    Context.RegimeActivityComplianceAuthorisations
                              .Include(x => x.Authorisation.IRISObject)
                              //.Include(x => x.Authorisation.IRISObject.ObjectTypeREF)
                              .Include(x => x.RegimeActivityComplianceConditions)
                              .Include("RegimeActivityComplianceConditions.Condition")
                              .Include("RegimeActivityComplianceConditions.Condition.Parameters")
                              //.Include("RegimeActivityComplianceConditions.Condition.Parameters.ParameterTypeREF")
                              //.Include("RegimeActivityComplianceConditions.Condition.Parameters.UnitREF")
                              .Where(c => c.RegimeActivityComplianceID == result.ID && !c.IsDeleted).ToList());
                return;

            }

            var regimeActivityMngt = result as RegimeActivityMngt;
            if (regimeActivityMngt != null)
            {
                regimeActivityMngt.RegimeActivityMngtSites.AddTrackableCollection(GetRegimeActivityMngtSitesByRegimeActivityMngtId(result.ID));
                return;
            }

            if (regimeActivityDam == null) return;

            regimeActivityDam.RegimeActivityDamFurtherActions.AddTrackableCollection(Context.RegimeActivityDamFurtherActions.Include(o => o.ConductedByREF)
                                                                                            .Include(o => o.FurtherActionTypeREF)
                                                                                            .Where(o => o.RegimeActivityDamID == result.ID && !o.IsDeleted).ToList());
            regimeActivityDam.RegimeActivityDamReports.AddTrackableCollection(Context.RegimeActivityDamReports.Include(o => o.DamRegister)
                                                                                     .Include(o => o.ReportTypeREF)
                                                                                     .Where(o => o.RegimeActivityDamID == result.ID && !o.IsDeleted).ToList());
        }

        /// <summary>
        ///    Given a regime object, this method will populate the regime with its plan and
        ///    policy information. This data is stored in different tables depending on the
        ///    type of regime. Hence why it is not a very straight forward query.
        /// </summary>
        /// <param name="regime"></param>
        private void LoadPlanAndRuleInformation(Regime regime)
        {
            // Include Plans and Policies for relevant regimes
            RegimeMngt regimeMngt = regime as RegimeMngt;
            if (regimeMngt != null)
            {
                RepositoryHelpers.LoadPlansRulesPoliciesForEntity(Context, regimeMngt);
            }

            RegimeEnvironment regimeEnvironment = regime as RegimeEnvironment;
            if (regimeEnvironment != null)
            {
                RepositoryHelpers.LoadPlansRulesPoliciesForEntity(Context, regimeEnvironment);
            }
        }

        #endregion

        private ReferenceDataValue GetReferenceDataValue(long? id)
        {
            if (id == null) return null;
            return (from n in Context.ReferenceDataValue
                    where
                        n.ID == id
                    select n).SingleOrDefault().TrackAll();
        }

        private List<RegimeActivityResourceNeeded> GetResourcesNeeded(long id)
        {
            return Context.RegimeActivityResourceNeededs
                .Include(x => x.ResourceNeededREF)
                .Where(x => x.RegimeActivityID == id && !x.IsDeleted)
                .ToList();
        }

        private IRISObject GetIRISObject(long id, int subClassLevel = 0)
        {
            if (subClassLevel == 1)
            {
                return Context.IRISObject
                    .Include(i => i.OtherIdentifiers)
                    .Include("OtherIdentifiers.IdentifierContextREF")
                    .Include(i => i.ObjectTypeREF)
                    .Include(i => i.SubClass1REF)
                    .Include(i => i.SubClass2REF)
                    .Include(i => i.SecurityContextIRISObject.ObjectTypeREF)
                    .Include(i => i.Statuses)
                    .Include("Statuses.StatusREF")
                    .Where(i => i.ID == id)
                    .Single();
            }

            if (subClassLevel == 2)
            {
                return Context.IRISObject
                    .Include(i => i.OtherIdentifiers)
                    .Include("OtherIdentifiers.IdentifierContextREF")
                    .Include(i => i.ObjectTypeREF)
                    .Include(i => i.SubClass1REF)
                    .Include(i => i.SubClass2REF)
                    .Include(i => i.SecurityContextIRISObject.ObjectTypeREF)
                    .Include(i => i.Statuses)
                    .Include("Statuses.StatusREF")
                    .Where(i => i.ID == id)
                    .Single();
            }

            if (subClassLevel == 3)
            {
                return Context.IRISObject
                    .Include(i => i.OtherIdentifiers)
                    .Include("OtherIdentifiers.IdentifierContextREF")
                    .Include(i => i.ObjectTypeREF)
                    .Include(i => i.SubClass1REF)
                    .Include(i => i.SubClass2REF)
                    .Include(i => i.SubClass3REF)
                    .Include(i => i.SecurityContextIRISObject.ObjectTypeREF)
                    .Include(i => i.Statuses)
                    .Include("Statuses.StatusREF")
                    .Where(i => i.ID == id)
                    .Single();
            }

            return Context.IRISObject
                .Include(i => i.OtherIdentifiers)
                .Include("OtherIdentifiers.IdentifierContextREF")
                .Include(i => i.ObjectTypeREF)
                .Include(i => i.SecurityContextIRISObject.ObjectTypeREF)
                .Include(i => i.Statuses)
                .Include("Statuses.StatusREF")
                .Where(i => i.ID == id)
                .Single();
        }

        public List<Observation> GetObservations(long regimeActivityId)
        {
            var results = Context.Observations
                            .Include(x => x.IRISObject.SubClass2REF)
                            .Include(x => x.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                            .Include(x => x.ObservationFurtherActions)
                            .Where(x => x.RegimeActivityID == regimeActivityId).ToList();

            results.OfType<ObservationSelectedLandUse>().ForEach(x => x.ObservationSelectedLandUseIndicators
                .AddTrackableCollection(Context.ObservationSelectedLandUseIndicators.Include(i => i.IndicatorREF).Where(i => i.ObservationSelectedLandUseID == x.ID).ToList()));

            return results;
        }

        public List<Observation> GetObservationsCreatedByManagementSite(long ManagementSiteID)
        {
            return Context.Observations.OfType<ObservationMngt>()
                            .Include(x => x.IRISObject.SubClass2REF)
                            .Include(x => x.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                            .Where(x => !x.RegimeActivityID.HasValue && x.ManagementSiteID == ManagementSiteID).ToList<Observation>();
        }

        public List<Observation> GetObservationsByMngtSitID(long mngtSitID)
        {
            return Context.Observations.OfType<ObservationMngt>()
                            .Include(x => x.IRISObject.SubClass2REF)
                            .Include(x => x.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                            .Where(x => x.ManagementSiteID == mngtSitID).ToList<Observation>();
        }

        public List<ObservationCompliance> GetObservationCompliances(long regimeActivityComplianceId)
        {
            return Context.Observations.OfType<ObservationCompliance>()
                                       .Include(x => x.IRISObject)
                                       .Include(x => x.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                                       .Where(o => o.RegimeActivityID == regimeActivityComplianceId).ToList();
        }

        public List<ObservationCompliance> GetObservationCompliancesForConditionAndRegime(long regimeID)
        {
            var dbquery = from observation in Context.Observations.OfType<ObservationCompliance>()
                          from observationIRISObject in Context.IRISObject.Where(i => i.ID == observation.IRISObjectID)
                          from observationSubClass1REF in Context.ReferenceDataValue.Where(r => r.ID == observationIRISObject.SubClass1ID)
                          from observationComplianceStatus in Context.ReferenceDataValue.Where(r => r.ID == observation.ComplianceStatusREFID).DefaultIfEmpty()
                          from regimeActivity in Context.RegimeActivity.Where(r => r.ID == observation.RegimeActivityID)
                          from regimeActivitySchedule in Context.RegimeActivitySchedule.Where(r => r.ID == regimeActivity.RegimeActivityScheduleID)
                          from regime in Context.Regime.Where(r => r.ID == regimeActivitySchedule.RegimeID)
                          select new
                          {
                              observation,
                              observationIRISObject,
                              observationSubClass1REF,
                              observationComplianceStatus,
                              observationIRISObject.SecurityContextIRISObject,
                              observationIRISObject.SecurityContextIRISObject.ObjectTypeREF
                          };
            return dbquery.AsEnumerable().Select(x => x.observation).Distinct().ToList();
        }


        public List<RegimeActivityObservationComplianceRow> GetRegimeActivityObservationComplianceRow(long regimeActivityID, long regimeActivityIRISObjectID)
        {
            int AuthsLinkToRA = Context.RegimeActivityComplianceAuthorisations.Where(a => a.RegimeActivityComplianceID == regimeActivityID && !a.IsDeleted).Count();

            var list = Context.Observations.OfType<ObservationCompliance>()
                .Include(i => i.IRISObject)
                .Include(i => i.IRISObject.SubClass2REF)
                .Include(i => i.IRISObject.SubClass3REF)
                .Include(i => i.ComplianceStatusREF)
                .Include(i => i.ObservationComplianceAuthorisations)
                .Where(o => o.RegimeActivityID == regimeActivityID)
                .ToList();

            List<RegimeActivityObservationComplianceRow> regimeActivityObservationComplianceRows = new List<RegimeActivityObservationComplianceRow>();


            foreach (ObservationCompliance ob in list)
            {
                var AuthorisationNames = Context.ObservationComplianceAuthorisations
                                            .Include(i => i.Evidence)
                                            .Include(i => i.ComplianceStatusReasoning)
                                            .Include(i => i.ActionRequired)
                                            .Include(i => i.RiskReasoning)
                                            .Where(o =>
                                            o.ObservationComplianceID == ob.ID &&
                                            (o.Evidence.Length > 0 ||
                                            o.ComplianceStatusREFID != null ||
                                            o.ComplianceStatusReasoning.Length > 0 ||
                                            o.ActionRequired.Length > 0 ||
                                            o.RiskOfNonComplianceREFID != null ||
                                            o.RiskReasoning.Length > 0)).
                                            OrderBy(a => (a.Authorisation.IRISObject.BusinessID + " " + a.Authorisation.AuthorisationName))
                                            .Select(a => (a.Authorisation.IRISObject.BusinessID + " " + a.Authorisation.AuthorisationName))
                                            .Take(3).ToList();

                regimeActivityObservationComplianceRows.Add(new RegimeActivityObservationComplianceRow
                {
                    ID = ob.ID,
                    ObservationDate = ob.ObservationDate,
                    ObservationTime = ob.ObservationTime,
                    ObservationIRISID = ob.IRISObject.BusinessID,
                    Type = ob.IRISObject.SubClass2REF.DisplayValue,
                    TypeSubtype = ob.IRISObject.SubClass2REF.DisplayValue + (ob.IRISObject.SubClass3REF != null ? " | " + ob.IRISObject.SubClass3REF.DisplayValue : string.Empty),
                    NoOfAuthAssessed = Context.ObservationComplianceAuthorisations
                                            .Include(i => i.Evidence)
                                            .Include(i => i.ComplianceStatusReasoning)
                                            .Include(i => i.ActionRequired)
                                            .Include(i => i.RiskReasoning)
                                            .Where(o =>
                                            o.ObservationComplianceID == ob.ID &&
                                            (o.Evidence.Length > 0 ||
                                            o.ComplianceStatusREFID != null ||
                                            o.ComplianceStatusReasoning.Length > 0 ||
                                            o.ActionRequired.Length > 0 ||
                                            o.RiskOfNonComplianceREFID != null ||
                                            o.RiskReasoning.Length > 0)
                                       ).Count(),
                    NoOfAuthLinkToRA = AuthsLinkToRA,
                    ComplianceStatus = ob.ComplianceStatusREF != null ? ob.ComplianceStatusREF.DisplayValue : string.Empty,
                    Authorisations = string.Join(" | ", AuthorisationNames)
                });
            }

            return regimeActivityObservationComplianceRows;
        }

        public List<AuthorisationAssessedRow> GetAuthorisationAssessedRows(long observationID, long? regimeActivityID, long observationIRISObjectID)
        {
            var ids = Context.ObservationComplianceAuthorisations.Where(o => o.ObservationComplianceID == observationID && !o.IsDeleted)
                .Select(oca => oca.ID).ToList();

            return ids.Select(i => GetObservationComplianceAuthorisation(i, observationIRISObjectID)).Select(oca => new AuthorisationAssessedRow
            {
                ObservationComplianceAuthorisationID = oca.ID,
                AuthorisationID = oca.AuthorisationID,
                AuthorisationName = oca.Authorisation.AuthorisationName,
                AuthorisationIRISObjectID = oca.Authorisation.IRISObjectID,
                AuthorisatinoIRISID = oca.AuthorisationBusinessID,
                NoOfCondition = regimeActivityID.HasValue ? GetRegimeActivityComplianceConditions(regimeActivityID.Value, oca.AuthorisationID, observationIRISObjectID).Count
                                                          : GetAuthorisationComplianceConditions(oca.ID, observationIRISObjectID).Count,
                NoOfConditionAssessed = oca.ObservationComplianceConditions.Count,
                AuthorisationCompliance = oca.ComplianceStatusREFID != null ? GetReferenceDataValue(oca.ComplianceStatusREFID).DisplayValue : "",
                ConditionCompliance = LowestLevelConditionComplianceStatus(oca.ObservationComplianceConditions)
            }).ToList();
        }

        private string LowestLevelConditionComplianceStatus(TrackableCollection<ObservationComplianceCondition> observationComplianceConditions)
        {
            HashSet<ReferenceDataValue> compliaceREFs = new HashSet<ReferenceDataValue>();
            observationComplianceConditions.ForEach(c =>
            {
                if (c.ComplianceStatusREFID != null)
                {
                    compliaceREFs.Add(GetReferenceDataValue(c.ComplianceStatusREFID));
                }
            });
            return compliaceREFs.Count > 0 ? compliaceREFs.OrderBy(r => r.DisplayOrder).FirstOrDefault().DisplayValue : null;
        }

        public ObservationComplianceAuthorisation GetObservationComplianceAuthorisation(long observationComplianceAuthorisationID, long observationIRISObjectID)
        {
            // Get ObservationComplianceAuthorisation and their ObservationComplianceCondition with Parameters
            var dbquery = from observationComplianceAuthorisation in Context.ObservationComplianceAuthorisations.Where(a => a.ID == observationComplianceAuthorisationID && !a.IsDeleted)
                          from observationComplianceCondition in Context.ObservationComplianceConditions.Where(c => c.ObservationComplianceAuthorisationID == observationComplianceAuthorisation.ID && !c.IsDeleted).DefaultIfEmpty()
                          from observationComplianceConditionParam in Context.ObservationComplianceParameters.Where(p => p.ObservationComplianceConditionID == observationComplianceCondition.ID).DefaultIfEmpty()
                          from param in Context.Parameters.Where(p => p.ID == observationComplianceConditionParam.ParameterID).DefaultIfEmpty()
                          from paramTypeRef in Context.ReferenceDataValue.Where(r => r.ID == param.ParameterTypeID).DefaultIfEmpty()
                          from unitTypeRef in Context.ReferenceDataValue.Where(r => r.ID == param.UnitID).DefaultIfEmpty()
                          from authorisation in Context.Authorisations.Where(a => a.ID == observationComplianceAuthorisation.AuthorisationID)
                          from irisObj in Context.IRISObject.Where(o => o.ID == authorisation.IRISObjectID)
                          from irisObjTypeREF in Context.ReferenceDataValue.Where(r => r.ID == irisObj.ObjectTypeID)
                          from irisObjSubclass1REF in Context.ReferenceDataValue.Where(r => r.ID == irisObj.SubClass1ID)
                          from irisObjSubclass2REF in Context.ReferenceDataValue.Where(r => r.ID == irisObj.SubClass2ID)
                          from irisObjSubclass3REF in Context.ReferenceDataValue.Where(r => r.ID == irisObj.SubClass3ID)
                          from latestStatus in
                              (
                                from status in Context.Statuses
                                where !status.IsDeleted && status.IRISObjectID == irisObj.ID
                                orderby status.StatusDate descending, status.ID descending
                                select status).Take(1)
                              .DefaultIfEmpty()
                          from latestStatusREF in Context.ReferenceDataValue.Where(r => r.ID == latestStatus.StatusREFID).DefaultIfEmpty()
                          select new
                          {
                              observationComplianceAuthorisation,
                              observationComplianceCondition,
                              observationComplianceConditionParam,
                              param,
                              paramTypeRef,
                              unitTypeRef,
                              authorisation,
                              irisObj,
                              irisObjTypeREF,
                              irisObjSubclass1REF,
                              irisObjSubclass2REF,
                              irisObjSubclass3REF,
                              latestStatus,
                              latestStatusREF
                          };

            return dbquery.AsEnumerable().Select(x => x.observationComplianceAuthorisation).Distinct().Single();
        }

        public List<AuthorisationComplianceCondition> GetAuthorisationComplianceConditions(long observationComplianceAuthorisationID, long observationIRISObjectID)
        {
            return Context.AuthorisationComplianceConditions
                          .Include(a => a.Condition)
                          .Include(a => a.ConditionSchedule)
                          .Include(a => a.Condition.Parameters)
                          .Include("Condition.Parameters.ParameterTypeREF")
                          .Include("Condition.Parameters.UnitREF")
                          .Where(c => c.ObservationComplianceAuthorisationID == observationComplianceAuthorisationID && !c.IsDeleted)
                          .ToList();
        }

        public List<RegimeActivityComplianceCondition> GetRegimeActivityComplianceConditions(long regimeActivityID, long authorisationID, long regimeActivityIRISObjectID)
        {
            var dbquery = from regimeActivityAuthorisation in Context.RegimeActivityComplianceAuthorisations.Where(a => a.RegimeActivityComplianceID == regimeActivityID && a.AuthorisationID == authorisationID && !a.IsDeleted)
                          from regimeActivityComplianceCondition in Context.RegimeActivityComplianceConditions.Where(c => c.RegimeActivityComplianceAuthorisationID == regimeActivityAuthorisation.ID && !c.IsDeleted)
                          from cond in Context.Conditions.Where(c => c.ID == regimeActivityComplianceCondition.ConditionID && !c.IsDeleted)
                          from condSchd in Context.ConditionSchedules.Where(s => s.ID == regimeActivityComplianceCondition.ConditionScheduleID).DefaultIfEmpty()
                          from param in Context.Parameters.Where(p => p.ConditionID == cond.ID && !p.IsDeleted).DefaultIfEmpty()
                          from paramTypeRef in Context.ReferenceDataValue.Where(r => r.ID == param.ParameterTypeID).DefaultIfEmpty()
                          from unitTypeRef in Context.ReferenceDataValue.Where(r => r.ID == param.UnitID).DefaultIfEmpty()
                          select new { regimeActivityComplianceCondition, cond, condSchd, param, paramTypeRef, unitTypeRef };

            return dbquery.AsEnumerable().Select(x => x.regimeActivityComplianceCondition).Distinct().ToList();
        }

        private List<RegimeActivityMngtSite> GetRegimeActivityMngtSitesByRegimeActivityMngtId(long id)
        {
            return Context.RegimeActivityMngtSites
                   .Include(x => x.ManagementSite)
                   .Where(x => x.RegimeActivityMngtID == id && !x.IsDeleted).ToList();
        }

        private List<ObservationMngtLine> GetObservationLinesByObservationMngtId(long id)
        {
            return Context.ObservationMngtLines
                   .Include(x => x.HabitatREF)
                   .Include(x => x.StratumREF)
                   .Where(x => x.ObservationMngtID == id && !x.IsDeleted)
                   .ToList();
        }

        private List<ObservationMngtLineResult> GetObservationLineResultsByObservationMngtLineId(long id)
        {
            return Context.ObservationMngtLineResults
                   .Where(x => x.ObservationMngtLineID == id && !x.IsDeleted)
                   .ToList();
        }

        public List<ObservationMngtLineResultItem> GetObservationLineResultItemsByObservationMngtLineResultId(long id)
        {
            return Context.ObservationMngtLineResultItems
                   .Where(x => x.ObservationMngtLineResultID == id && !x.IsDeleted)
                   .ToList();
        }

        public IRISObject GetIRISObjectForRegimeByRegimeID(long regimeId)
        {
            var dbquery = from regime in Context.Regime.Where(s => s.ID == regimeId)
                          from irisObject in Context.IRISObject.Where(i => i.ID == regime.IRISObjectID)
                          from objectTypeREF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.ObjectTypeID)
                          select new { irisObject, objectTypeREF };

            return dbquery.AsEnumerable().Select(i => i.irisObject).Single();
        }

        public List<Authorisation> GetParentRegimeLinkedAuthorisation(long regimeActivityComplianceIRISObjectId, bool getActiveRelationShip)
        {
            var leftDBQuery = from ra in Context.RegimeActivity.Where(x => x.IRISObjectID == regimeActivityComplianceIRISObjectId)
                              from raSchedule in Context.RegimeActivitySchedule.Where(x => x.ID == ra.RegimeActivityScheduleID)
                              from regime in Context.Regime.Where(x => x.ID == raSchedule.RegimeID)

                              from leftRelationship in Context.ActivityObjectRelationship.Where(x => x.IRISObjectID == regime.IRISObjectID).DefaultIfEmpty()
                              from leftRelationshipType in Context.ActivityObjectRelationshipType.Where(x => x.Code == "RegimeSubjectAuthorisation" && x.ID == leftRelationship.ActivityObjectRelationshipTypeID)
                              from leftAuth in Context.Authorisations.Where(x => x.IRISObjectID == leftRelationship.RelatedIRISObjectID)
                              from leftAuthIRISObject in Context.IRISObject.Where(i => i.ID == leftAuth.IRISObjectID)
                              from leftAuthIRISObjectTypeREF in Context.ReferenceDataValue.Where(r => r.ID == leftAuthIRISObject.ObjectTypeID)
                              from leftAuthIRISObjectSubclass1REF in Context.ReferenceDataValue.Where(r => r.ID == leftAuthIRISObject.SubClass1ID)
                              from leftAuthIRISObjectSubclass2REF in Context.ReferenceDataValue.Where(r => r.ID == leftAuthIRISObject.SubClass2ID)
                              from leftAuthIRISObjectSubclass3REF in Context.ReferenceDataValue.Where(r => r.ID == leftAuthIRISObject.SubClass3ID)

                              select new { leftAuth, leftAuthIRISObject, leftAuthIRISObjectTypeREF, leftAuthIRISObjectSubclass1REF, leftAuthIRISObjectSubclass2REF, leftAuthIRISObjectSubclass3REF, leftRelationship.CurrentTo };

            var rightDBQuery = from ra in Context.RegimeActivity.Where(x => x.IRISObjectID == regimeActivityComplianceIRISObjectId)
                               from raSchedule in Context.RegimeActivitySchedule.Where(x => x.ID == ra.RegimeActivityScheduleID)
                               from regime in Context.Regime.Where(x => x.ID == raSchedule.RegimeID)

                               from rightRelationship in Context.ActivityObjectRelationship.Where(x => x.RelatedIRISObjectID == regime.IRISObjectID).DefaultIfEmpty()
                               from rightRelationshipType in Context.ActivityObjectRelationshipType.Where(x => x.Code == "RegimeSubjectAuthorisation" && x.ID == rightRelationship.ActivityObjectRelationshipTypeID)
                               from rightAuth in Context.Authorisations.Where(x => x.IRISObjectID == rightRelationship.IRISObjectID)
                               from rightAuthIRISObject in Context.IRISObject.Where(i => i.ID == rightAuth.IRISObjectID)
                               from rightAuthIRISObjectTypeREF in Context.ReferenceDataValue.Where(r => r.ID == rightAuthIRISObject.ObjectTypeID)
                               from rightAuthIRISObjectSubclass1REF in Context.ReferenceDataValue.Where(r => r.ID == rightAuthIRISObject.SubClass1ID)
                               from rightAuthIRISObjectSubclass2REF in Context.ReferenceDataValue.Where(r => r.ID == rightAuthIRISObject.SubClass2ID)
                               from rightAuthIRISObjectSubclass3REF in Context.ReferenceDataValue.Where(r => r.ID == rightAuthIRISObject.SubClass3ID)

                               select new { rightAuth, rightAuthIRISObject, rightAuthIRISObjectTypeREF, rightAuthIRISObjectSubclass1REF, rightAuthIRISObjectSubclass2REF, rightAuthIRISObjectSubclass3REF, rightRelationship.CurrentTo };

            var authorisations = leftDBQuery.AsEnumerable().Where(x => getActiveRelationShip == false || x.CurrentTo == null).Select(s => s.leftAuth)
                                                         .Concat(rightDBQuery.AsEnumerable().Where(x => getActiveRelationShip == false || x.CurrentTo == null).Select(s => s.rightAuth)).ToList();

            return authorisations;
        }

        public List<Authorisation> GetRegimeLinkedAuthorisation(long regimeId, long regimeIRISObjectId)
        {
            var leftDBQuery = from regime in Context.Regime.Where(x => x.ID == regimeId)
                              from leftRelationship in Context.ActivityObjectRelationship.Where(x => x.IRISObjectID == regime.IRISObjectID && x.CurrentTo == null).DefaultIfEmpty()
                              from leftRelationshipType in Context.ActivityObjectRelationshipType.Where(x => x.Code == "RegimeSubjectAuthorisation")
                              from leftAuth in Context.Authorisations.Where(x => x.IRISObjectID == leftRelationship.RelatedIRISObjectID)
                              from leftAuthIRISObject in Context.IRISObject.Where(i => i.ID == leftAuth.IRISObjectID)
                              from leftAuthIRISObjectTypeREF in Context.ReferenceDataValue.Where(r => r.ID == leftAuthIRISObject.ObjectTypeID)
                              from leftAuthIRISObjectSubclass1REF in Context.ReferenceDataValue.Where(r => r.ID == leftAuthIRISObject.SubClass1ID)
                              from leftAuthIRISObjectSubclass2REF in Context.ReferenceDataValue.Where(r => r.ID == leftAuthIRISObject.SubClass2ID)
                              from leftAuthIRISObjectSubclass3REF in Context.ReferenceDataValue.Where(r => r.ID == leftAuthIRISObject.SubClass3ID)

                              select new { leftAuth, leftAuthIRISObject, leftAuthIRISObjectTypeREF, leftAuthIRISObjectSubclass1REF, leftAuthIRISObjectSubclass2REF, leftAuthIRISObjectSubclass3REF };

            var rightDBQuery = from regime in Context.Regime.Where(x => x.ID == regimeId)
                               from rightRelationship in Context.ActivityObjectRelationship.Where(x => x.RelatedIRISObjectID == regime.IRISObjectID && x.CurrentTo == null).DefaultIfEmpty()
                               from rightRelationshipType in Context.ActivityObjectRelationshipType.Where(x => x.Code == "RegimeSubjectAuthorisation")
                               from rightAuth in Context.Authorisations.Where(x => x.IRISObjectID == rightRelationship.IRISObjectID)
                               from rightAuthIRISObject in Context.IRISObject.Where(i => i.ID == rightAuth.IRISObjectID)
                               from rightAuthIRISObjectTypeREF in Context.ReferenceDataValue.Where(r => r.ID == rightAuthIRISObject.ObjectTypeID)
                               from rightAuthIRISObjectSubclass1REF in Context.ReferenceDataValue.Where(r => r.ID == rightAuthIRISObject.SubClass1ID)
                               from rightAuthIRISObjectSubclass2REF in Context.ReferenceDataValue.Where(r => r.ID == rightAuthIRISObject.SubClass2ID)
                               from rightAuthIRISObjectSubclass3REF in Context.ReferenceDataValue.Where(r => r.ID == rightAuthIRISObject.SubClass3ID)

                               select new { rightAuth, rightAuthIRISObject, rightAuthIRISObjectTypeREF, rightAuthIRISObjectSubclass1REF, rightAuthIRISObjectSubclass2REF, rightAuthIRISObjectSubclass3REF };

            var authorisations = leftDBQuery.AsEnumerable().Select(s => s.leftAuth).Concat(rightDBQuery.AsEnumerable().Select(s => s.rightAuth)).ToList();

            return authorisations;
        }

        public Remediation GetRemediationById(long id)
        {
            var result = Context.Remediations
                .Include(r => r.ActionByREF)
                .Include(r => r.FundingREF)
                .Include(r => r.ContractorREF)
                .Include(r => r.StatusREF)
                .Include(r => r.OfficeResponsible)
                .Include(r => r.AirTemperatureREF)
                .Include(r => r.CloudCoverREF)
                .Include(r => r.RainfallREF)
                .Include(r => r.WindDirectionREF)
                .Include(r => r.WindStrengthREF)
                .Include(x => x.ActualLabours)
                .Include(x => x.ActualEquipmentMaterials)
                .Include(r => r.ManagementSite)
                .Include(r => r.ManagementSite.IRISObject.SubClass1REF)
                .Include(r => r.ManagementSite.IRISObject.SubClass2REF)
                .Include("ManagementSite.IRISObject.Statuses.StatusREF")
                .Include(r => r.RegimeActivity.IRISObject.SubClass1REF)
                .Include(r => r.RegimeActivity.IRISObject.SubClass2REF)

                .SingleOrDefault(x => x.ID == id);

            result.IRISObject = GetIRISObject(result.IRISObjectID);
            result.RemediationSitePlans.AddTrackableCollection(Context.RemediationSitePlans
                                                                        .Include(x => x.SitePlan.StatusREF)
                                                                        .Include(x => x.SitePlan.TypeREF)
                                                                        .Where(sp => sp.RemediationID == result.ID && !sp.IsDeleted).ToList());

            if (result.RegimeActivityID.HasValue)
            {
                result.RegimeActivity = Context.RegimeActivity.OfType<RegimeActivityMngt>().Include("IRISObject.Statuses.StatusREF")
                                                              .Include(x => x.IRISObject.SubClass2REF)
                                                              .Include(x => x.IRISObject.SubClass3REF)
                                                              .Include("RegimeActivityMngtSites.ManagementSite")
                                                              .Include(x => x.RegimeActivitySchedule.Regime.IRISObject)
                                                              .Include(x => x.RegimeActivitySchedule.Regime.OfficerResponsible)
                                                              .SingleOrDefault(r => result.RegimeActivityID == r.ID);
            }

            GetRemediationTypeSpecificValues(result);
            return result.TrackAll();
        }

        public List<Remediation> GetRemediationsByManagementSiteId(long MangementSiteID)
        {
            var dbquery = from remediations in Context.Remediations
                          //.Include(x => x.IRISObject)
                          //.Include("IRISObject.SecurityContextIRISObject")
                          //.Include("IRISObject.SecurityContextIRISObject.ObjectTypeREF")
                          .Where(r => r.ManagementSiteID == MangementSiteID)
                          select new
                          {
                              remediations
                          };
            var result = dbquery.AsEnumerable().Select(r => r.remediations).Distinct().ToList();

            foreach (Remediation r in result)
                r.IRISObject = GetIRISObject(r.IRISObjectID);
            return result;
        }
        /// <summary>
        /// Get the conditions linked to the authorisation directly,
        /// construct it into a RegimeActivityComplianceConditionRow
        /// </summary>
        /// <param name="authorisationID"></param>
        /// <returns></returns>
        public List<RegimeActivityComplianceConditionRow> GetConditionsLinkedDirectToAuthorisation(long authorisationID, long regimeActivityID)
        {
            List<RegimeActivityComplianceConditionRow> authorisationConditions = (from condition in Context.Conditions
                                                                                  from authorisationCondition in Context.AuthorisationConditions.Where(ac => ac.ConditionID == condition.ID
                                                                                                                                                        && ac.AuthorisationID == authorisationID
                                                                                                                                                        && !condition.IsDeleted)
                                                                                  select new RegimeActivityComplianceConditionRow { AuthorisationID = authorisationID, Condition = condition }).ToList();

            return authorisationConditions;

        }

        /// <summary>
        /// Get the conditions linked to the authorisation via the condition schedule,
        /// construct both the condition and the conditionSchedule into a 
        /// RegimeActivityComplianceConditionRow
        /// </summary>
        public List<RegimeActivityComplianceConditionRow> GetConditionsLinkedToAuthorisationViaConditionSchedule(long authorisationID, long regimeActivityID)
        {
            ILinkingRepository linkingRepository = RepositoryMap.LinkingRepository;

            long authorisationIRISObjectID = Context.Authorisations.Single(a => a.ID == authorisationID).IRISObjectID;

            RelationshipLinkCriteria conditionScheduleCriteria = new RelationshipLinkCriteria
            {
                IRISObjectID = authorisationIRISObjectID,
                IncludedRelationshipTypeCodeList = new List<string> { "ActAuthConditionSchedule" },
                IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.ConditionSchedule }
            };

            List<RelationshipEntry> conditionScheduleRelationshipEntries = linkingRepository.GetRelationshipLinks(conditionScheduleCriteria);

            IEnumerable<long> conditionScheduleIRISObjectIDs = conditionScheduleRelationshipEntries.Select(r => r.LinkedIRISObject.ID);

            var dbquery = from condition in Context.Conditions
                          from conditionScheduleCondition in Context.ConditionScheduleConditions.Where(csc => csc.ConditionID == condition.ID && !condition.IsDeleted)
                          from conditionSchedule in Context.ConditionSchedules.Where(cs => cs.ID == conditionScheduleCondition.ConditionScheduleID
                                                                                           && conditionScheduleIRISObjectIDs.Contains(cs.IRISObjectID))
                          select new RegimeActivityComplianceConditionRow
                          {
                              AuthorisationID = authorisationID,
                              Condition = condition,
                              ConditionSchedule = conditionSchedule,
                              SecurityContext = new SecurityContext
                              {
                                  IRISObjectID = conditionSchedule.IRISObjectID,
                                  ForIRISObjectID = conditionSchedule.IRISObjectID,
                                  ObjectTypeCode = conditionSchedule.IRISObject.ObjectTypeREF.Code,
                                  ObjectTypeID = conditionSchedule.IRISObject.ObjectTypeID,
                                  SubClass1ID = conditionSchedule.IRISObject.SubClass1ID,
                                  SubClass2ID = conditionSchedule.IRISObject.SubClass2ID

                              }
                          };
            return dbquery.ToList();
        }

        /// <summary>
        ///  Business rule: A regimeActivityComplianceAuthorisation cannot be deleted if
        ///  its authorsation links to an observation of that regimeActivity.
        /// </summary>
        /// <param name="regimeActivityComplianceConditionID"></param>
        /// <returns></returns>
        public bool IsRegimeActivityComplianceAuthorisationDeletable(long regimeActivityComplianceAuthorisationID, long regimeActivityIRISObjectId)
        {
            var query = from raAuth in Context.RegimeActivityComplianceAuthorisations.Where(x => x.ID == regimeActivityComplianceAuthorisationID)
                        from ra in Context.RegimeActivity.OfType<RegimeActivityCompliance>().Where(x => x.ID == raAuth.RegimeActivityComplianceID)
                        from observation in Context.Observations.OfType<ObservationCompliance>().Where(x => x.RegimeActivityID == ra.ID)
                        from observationAuth in Context.ObservationComplianceAuthorisations.Where(x => x.AuthorisationID == raAuth.AuthorisationID && observation.ID == x.ObservationComplianceID && !x.IsDeleted)
                        select raAuth;

            return !query.Any();
        }

        public Regime GetSimpleRegimeForBanner(long regimeActivityID)
        {
            var dbquery = from regime in Context.Regime
                          from regimeActivitySchedule in Context.RegimeActivitySchedule.Where(r => r.RegimeID == regime.ID)
                          from regimeActivity in Context.RegimeActivity.Where(r => r.ID == regimeActivityID && r.RegimeActivityScheduleID == regimeActivitySchedule.ID)
                          from irisObject in Context.IRISObject.Where(i => i.ID == regime.IRISObjectID)
                          from refData1 in Context.ReferenceDataValue.Where(r => r.ID == irisObject.ObjectTypeID)
                          from refData2 in Context.ReferenceDataValue.Where(r => r.ID == irisObject.SubClass1ID)
                          from officerResponible in Context.User.Where(r => r.ID == regime.OfficerResponsibleID)
                          select new { regime, irisObject, refData1, refData2, officerResponible };

            return dbquery.AsEnumerable().Select(x => x.regime).Single();
        }

        public string ObservationMngtMonitoring_TargetSpeciesResultPercent(long observationMngtID)
        {
            var result = new ObjectParameter("LineMonitoringTargetSpeciesResultPercent", typeof(string));
            Context.ObservationMngtMonitoring_TargetSpeciesResultPercent(observationMngtID, result);
            return result.Value.ToString();
        }

        public Observation GetObservationByObservationMngtLineId(long id)
        {
            var query = from mngtline in Context.ObservationMngtLines
                        join observationMngt in Context.Observations.OfType<ObservationMngt>() on mngtline.ObservationMngtID equals observationMngt.ID
                        join obsOfficer in Context.ObservationObservingOfficers on observationMngt.ID equals obsOfficer.ObservationID
                        join result in Context.ObservationMngtLineResults on new { ID = mngtline.ID, IsDeleted = false } equals new { ID = result.ObservationMngtLineID, IsDeleted = result.IsDeleted } into MngtLineResult
                        from mngtlineResult in MngtLineResult.DefaultIfEmpty()
                        join resultItem in Context.ObservationMngtLineResultItems on new { ID = mngtlineResult.ID, IsDeleted = false } equals new { ID = resultItem.ObservationMngtLineResultID, IsDeleted = resultItem.IsDeleted } into MngtLineResultItem
                        from mngtlineResultItem in MngtLineResultItem.DefaultIfEmpty()
                        where mngtline.ID == id && !mngtline.IsDeleted
                        select new
                        {
                            bioline = mngtline,
                            mngtline.HabitatREF,
                            mngtline.StratumREF,

                            mngtlineResult,
                            mngtlineResultItem,
                            mngtlineResultItem.SpeciesREF,

                            observationBio = observationMngt,

                            observationMngt.ObservationMngtLineMonitoring,
                            observationMngt.ObservationMngtLineMonitoring.MethodREF,
                            observationMngt.ObservationMngtLineMonitoring.PhaseREF,
                            observationMngt.ObservationMngtLineMonitoring.TargetSpeciesREF,

                            observationMngt.ObservationObservingOfficers,

                            observationMngt.IRISObject,
                            observationMngt.IRISObject.ObjectTypeREF,
                            observationMngt.IRISObject.SubClass1REF,
                            observationMngt.IRISObject.SubClass2REF,
                            observationMngt.IRISObject.SubClass3REF,
                            observationMngt.IRISObject.SecurityContextIRISObject,

                            SecurityContextIRISObjectTypeREF = observationMngt.IRISObject.SecurityContextIRISObject.ObjectTypeREF
                        };

            var observation = query.AsEnumerable().Select(x => x.observationBio).Distinct().Single();
            observation.IRISObject.Statuses.AddTrackableCollection(Context.Statuses.Include(x => x.StatusREF).Where(x => x.IRISObjectID == observation.IRISObjectID).ToList());
            return observation.TrackAll();
        }

        public Remediation GetLastestRemediation(long irisObjectID)
        {
            var result = Context.Remediations
                .Include(x => x.RegimeActivity)
                .Include(x => x.ManagementSite)
                .Include(x => x.AirTemperatureREF)
                .Include(x => x.ActionByREF)
                .Include(x => x.CloudCoverREF)
                .Include(x => x.RainfallREF)
                .Include(x => x.WindDirectionREF)
                .Include(x => x.WindStrengthREF)
                .Include(x => x.OfficeResponsible)
                .Where(x => !x.IsDeleted && x.IsActual && (x.RegimeActivity.IRISObjectID == irisObjectID || x.ManagementSite.IRISObjectID == irisObjectID))
                .OrderByDescending(x => x.DateCreated)
                .FirstOrDefault();

            return result;
        }

        private void GetRemediationTypeSpecificValues(Remediation remediation)
        {
            remediation.RemediationItems.AddTrackableCollection(Context.RemediationItems
                                                                        .Include(x => x.TypeREF)
                                                                        .Include(x => x.SubtypeREF)
                                                                        .Where(i => i.RemediationID == remediation.ID && !i.IsDeleted).ToList());
            // type specific informaiton
            remediation.RemediationItems.ForEach(i =>
            {
                if (i is RemediationTrapping)
                {
                    var remediationTrapping = i as RemediationTrapping;
                    remediationTrapping.TrapTypeREF = GetReferenceDataValue(remediationTrapping.TrapTypeREFID);
                    remediationTrapping.RemediationTrappingPests.AddTrackableCollection(Context.RemediationTrappingPests
                                                                                            .Include(p => p.CatchPestREF)
                                                                                            .Where(p => p.RemediationTrappingID == remediationTrapping.ID && !p.IsDeleted).ToList());
                }
                else if (i is RemediationPhysicalControl)
                {
                    var remediationPhysicalControl = i as RemediationPhysicalControl;
                    remediationPhysicalControl.Species = Context.Species.Include(s => s.SpeciesTypes).SingleOrDefault(s => s.ID == remediationPhysicalControl.SpeciesID);
                    remediationPhysicalControl.MethodREF = GetReferenceDataValue(remediationPhysicalControl.MethodREFID);
                }
                else if (i is RemediationPlanting)
                {
                    var remediationPlanting = i as RemediationPlanting;
                    remediationPlanting.PurposeREF = GetReferenceDataValue(remediationPlanting.PurposeREFID);
                    remediationPlanting.RemediationPlantingSpecies.AddTrackableCollection(Context.RemediationPlantingSpecies
                                                                                            .Include(s => s.SpeciesREF)
                                                                                            .Where(s => s.RemediationPlantingID == remediationPlanting.ID && !s.IsDeleted).ToList());
                }
                else if (i is RemediationStructureOther)
                {
                    var remediationStructure = i as RemediationStructureOther;
                    remediationStructure.PurposeREF = GetReferenceDataValue(remediationStructure.PurposeREFID);
                    remediationStructure.StructureOtherTypeREF = GetReferenceDataValue(remediationStructure.StructureOtherTypeREFID);
                }
                else if (i is RemediationTreatmentSpecies)
                {
                    var remediationTreatmentSpecies = i as RemediationTreatmentSpecies;
                    remediationTreatmentSpecies.RemediationTreatments.AddTrackableCollection(Context.RemediationTreatments
                                                                                               .Include(t => t.AgentREF)
                                                                                               .Include(t => t.Species.SpeciesTypes)
                                                                                               .Where(t => t.TreatmentSpeciesID == remediationTreatmentSpecies.ID && !t.IsDeleted).ToList());
                }
                else if (i is RemediationShooting)
                {
                    var remediationShooting = i as RemediationShooting;
                    remediationShooting.RemediationShootingPests.AddTrackableCollection(Context.RemediationShootingPests
                                                                                               .Include(p => p.PestREF)
                                                                                               .Where(p => p.RemediationShootingID == remediationShooting.ID && !p.IsDeleted).ToList());
                }
            });
        }

        public List<ObservationRemediationSummary> GetObservationRemediationSummaries(User user, long irisObjectID)
        {
            var summaries = Context.GetListObservationRemediationSummary(irisObjectID, user.AccountName).ToList();

            summaries.Where(x => x.ManagementSiteID.HasValue).ForEach(x =>
            {
                if (x.ItemName == "Observation")
                {
                    var obsId = (long.Parse(x.RowID.Substring(0, x.RowID.Length - 2)));
                    var bioObs = Context.Observations.OfType<ObservationMngt>()
                        .Include(i => i.IRISObject)
                        .Include(i => i.IRISObject.SubClass1REF)
                        .Include(i => i.ObservationMngtSpeciesCounts)
                        .Include("ObservationMngtSpeciesCounts.Species")
                        .SingleOrDefault(i => i.ID == obsId &&
                            (i.IRISObject.SubClass1REF.Code == ReferenceDataValueCodes.ObservationTypes.Biodiversity
                             ||
                             i.IRISObject.SubClass1REF.Code == ReferenceDataValueCodes.ObservationTypes.Biosecurity));

                    if (bioObs != null)
                    {
                        //.Include(i => i.IRISObject).Include(i => i.IRISObject.SubClass1REF)
                        //For Biodiversity or Biosecurity Observation:
                        // •	 Pipe (‘ | ‘) separated list of up to 3 Species from the Species Count values for this Observation (from first 3 based upon order of entry).
                        //o	Show <Species Common Name> (<Scientific name>).
                        //o	If there 4 or more Indicators then append ‘…’ to the displayed text.
                        var species = bioObs.ObservationMngtSpeciesCounts.Where(s => !s.IsDeleted).OrderBy(o => o.ID).Select(s => s.Species);
                        species.Take(3).ForEach(s => x.Details += (string.IsNullOrEmpty(s.CommonName) ? "" : s.CommonName + " ") + "(" + s.ScientificName + ") | ");
                        if (species.Any()) x.Details = x.Details.Substring(0, x.Details.Length - 3);
                        if (species.Count() > 3)
                            x.Details += " ...";
                    }
                    else
                    {
                        //For Land Management Observation:
                        //•	If Observed By is Council Officer or Joint then show:
                        //o	‘Observed by ‘
                        //o	+ <Display Name of 1st Observing Officer  (based upon order of entry)>
                        //•	If Observed By is External then show:
                        //o	‘Observed by External‘
                        //•	If Comments are not <blank> then concatenate:
                        //o	‘ | ‘
                        //o	+ <Comments>
                        //o	Text is truncated if 3 lines on screen exceeded and ‘…’ is added to the end of the display text.

                        var landMngtObs = Context.Observations.OfType<ObservationMngt>()
                        .Include(i => i.IRISObject).Include(i => i.IRISObject.SubClass1REF).Include(i => i.ObservedByREF)
                        .Include(i => i.ObservationObservingOfficers).Include("ObservationObservingOfficers.User")
                        .SingleOrDefault(i => i.ID == obsId && i.IRISObject.SubClass1REF.Code == ReferenceDataValueCodes.ObservationTypes.LandManagement);
                        if (landMngtObs != null)
                        {
                            if (landMngtObs.ObservedByREF.Code == ReferenceDataValueCodes.Observer.External)
                                x.Details += "Observed by External";
                            else
                            {
                                var officer = landMngtObs.ObservationObservingOfficers.OrderBy(i => i.ID).FirstOrDefault(i => !i.IsDeleted && i.ObservationID == landMngtObs.ID);
                                x.Details += "Observed by " + (officer == null ? "" : officer.User.DisplayName);
                            }
                            if (!string.IsNullOrEmpty(landMngtObs.Comments))
                                x.Details += " | " + landMngtObs.Comments.Trim();
                        }
                    }

                    var sitePlans = from sitePlan in Context.SitePlans
                                    from observationMngtSitePlan in Context.ObservationMngtSitePlans.Where(r => r.SitePlanID == sitePlan.ID && r.ObservationMngtID == obsId && !r.IsDeleted)
                                    where !sitePlan.IsDeleted
                                    select sitePlan;
                    if (sitePlans.Any())
                    {
                        string externalSitePlanIDs = "";
                        sitePlans.ForEach(y => externalSitePlanIDs += y.ExternalSitePlanID + ", ");
                        externalSitePlanIDs = externalSitePlanIDs.Substring(0, externalSitePlanIDs.Length - 2);
                        x.Details += string.IsNullOrEmpty(x.Details) ? externalSitePlanIDs : " | " + externalSitePlanIDs;
                    }

                }
                else if (x.ItemName == "Remediation")
                {
                    long remediationID = x.RowID.Replace("_r", "").ParseAsLong();
                    var sitePlans = from sitePlan in Context.SitePlans
                                    from remediationSitePlan in Context.RemediationSitePlans.Where(r => r.SitePlanID == sitePlan.ID && r.RemediationID == remediationID && !r.IsDeleted)
                                    where !sitePlan.IsDeleted
                                    select sitePlan;
                    if (sitePlans.Any())
                    {
                        string externalSitePlanIDs = "";
                        sitePlans.ForEach(y => externalSitePlanIDs += y.ExternalSitePlanID + ", ");
                        externalSitePlanIDs = externalSitePlanIDs.Substring(0, externalSitePlanIDs.Length - 2);
                        x.Details += string.IsNullOrEmpty(x.Details) ? externalSitePlanIDs : " | " + externalSitePlanIDs;
                    }
                }
            });

            return summaries;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parentIrisObjectID">Just for security checking</param>
        /// <param name="managementSiteIDs"></param>
        /// <returns></returns>
        public List<SitePlan> GetSitePlans(long managementSiteIDs)
        {
            return Context.SitePlans
                          .Include(x => x.TypeREF)
                          .Include(x => x.StatusREF)
                          .Where(x => x.ManagementSiteID == managementSiteIDs && !x.IsDeleted).ToList();
        }

        public List<RegimeActivityComplianceAuthorisationSummary> GetComplianceAuthorisationSummaries(long regimeActivityID)
        {
            var result = from ra in Context.RegimeActivity.OfType<RegimeActivityCompliance>().Where(x => x.ID == regimeActivityID)
                         from raAuth in Context.RegimeActivityComplianceAuthorisations.Where(x => x.RegimeActivityComplianceID == ra.ID && !x.IsDeleted)

                             // authorisations & directly linked conditions
                         from auth in Context.Authorisations.Where(x => x.ID == raAuth.AuthorisationID)
                         from authConditionsGroup in
                            (
                                from authConditions in Context.AuthorisationConditions.Where(x => x.AuthorisationID == auth.ID).DefaultIfEmpty()
                                from conditions in Context.Conditions.Where(c => c.ID == authConditions.ConditionID && !c.IsDeleted)
                                select new { authConditions.AuthorisationID, conditions.ID }).GroupBy(x => x.AuthorisationID).DefaultIfEmpty()
                         from authIRISObject in Context.IRISObject.Where(x => x.ID == auth.IRISObjectID)
                             // latest authorisation status
                             // todo: need first define the latest status logic
                         from latestStatus in
                             (
                               from status in Context.Statuses
                               where !status.IsDeleted && status.IRISObjectID == authIRISObject.ID
                               orderby status.StatusDate descending, status.ID descending
                               select status).Take(1)
                              .DefaultIfEmpty()
                         from latestStatusREF in Context.ReferenceDataValue.Where(r => r.ID == latestStatus.StatusREFID).DefaultIfEmpty()

                             // conditions linked to the regime activity
                         from raConditions in Context.RegimeActivityComplianceConditions.Where(x => x.RegimeActivityComplianceAuthorisationID == raAuth.ID && !x.IsDeleted).GroupBy(x => x.RegimeActivityComplianceAuthorisationID).DefaultIfEmpty()

                             // observation linked to these authorisations under the regime activity
                         from observations in Context.Observations.OfType<ObservationCompliance>().Where(x => x.RegimeActivityID == ra.ID).DefaultIfEmpty()
                         from observationAuth in Context.ObservationComplianceAuthorisations.Where(x => x.AuthorisationID == auth.ID && x.ObservationComplianceID == observations.ID).DefaultIfEmpty()
                         from observationAuthREF in Context.ReferenceDataValue.Where(x => x.ID == observationAuth.ComplianceStatusREFID).DefaultIfEmpty()
                         select new RegimeActivityComplianceAuthorisationSummary
                         {
                             RegimeActivityComplianceAuthorisationID = raAuth.ID,
                             AuthorisationID = auth.ID,
                             AuthorisationIRISObjectID = authIRISObject.ID,
                             AuthorisationIRISID = authIRISObject.BusinessID,
                             AuthorisationName = auth.AuthorisationName,
                             LatestAuthorisationStatus = latestStatus == null ? null : latestStatusREF.DisplayValue,
                             NumberOfConditionLinkToAuthorisation = authConditionsGroup.Count(),
                             NumberOfConditionLinkToCurrentRegimeActivity = raConditions.Count(),
                             //NumberOfLinkedObservation = observations.Count(),
                             LatestAuthorisationComplianceStatus = observationAuth == null ? null : observationAuthREF.DisplayValue,
                             ObservationDate = observations.ObservationDate == null ? DateTime.MinValue : observations.ObservationDate,
                             ObservationTime = observations.ObservationTime,
                             MobileStatus = raAuth.MobileInspectionStatus
                         };

            // Find the row in the group with the latest (not null) compliance status
            var summaryList = result.AsEnumerable().GroupBy(x => x.RegimeActivityComplianceAuthorisationID).Select(g => g.OrderBy(r => r.LatestAuthorisationComplianceStatus == null ? "1" : "0")
                .ThenByDescending(t => t.ObservationDate).ThenByDescending(t => t.ObservationTime).FirstOrDefault()).ToList();

            List<long> authIRISObjectIDs = summaryList.Select(x => x.AuthorisationIRISObjectID).ToList();
            var leftQuery = from leftConditionScheduleRelationship in Context.ActivityObjectRelationship.Where(x => authIRISObjectIDs.Contains(x.RelatedIRISObjectID) && x.CurrentTo == null).DefaultIfEmpty()
                            from leftConditionSchedule in Context.ConditionSchedules.Where(x => x.IRISObjectID == leftConditionScheduleRelationship.IRISObjectID).DefaultIfEmpty()
                            from leftConditionScheduleConditions in Context.ConditionScheduleConditions.Where(x => x.ConditionScheduleID == leftConditionSchedule.ID).DefaultIfEmpty()
                            from leftConditions in Context.Conditions.Where(c => c.ID == leftConditionScheduleConditions.ConditionID && !c.IsDeleted)
                            select new { leftConditionScheduleRelationship.RelatedIRISObjectID, leftConditions.ID };

            var rightQuery = from rightConditionScheduleRelationship in Context.ActivityObjectRelationship.Where(x => authIRISObjectIDs.Contains(x.IRISObjectID) && x.CurrentTo == null).DefaultIfEmpty()
                             from rightConditionSchedule in Context.ConditionSchedules.Where(x => x.IRISObjectID == rightConditionScheduleRelationship.RelatedIRISObjectID).DefaultIfEmpty()
                             from rightConditionScheduleConditions in Context.ConditionScheduleConditions.Where(x => x.ConditionScheduleID == rightConditionSchedule.ID).DefaultIfEmpty()
                             from rightConditions in Context.Conditions.Where(c => c.ID == rightConditionScheduleConditions.ConditionID && !c.IsDeleted)
                             select new { rightConditionScheduleRelationship.IRISObjectID, rightConditions.ID };

            var leftQueryCount = leftQuery.GroupBy(x => x.RelatedIRISObjectID).Select(x => new { x.Key, Count = x.Count() }).ToList();
            var rightQueryCount = rightQuery.GroupBy(x => x.IRISObjectID).Select(x => new { x.Key, Count = x.Count() }).ToList();

            summaryList.ForEach(s =>
            {
                var leftCount = leftQueryCount.Where(x => x.Key == s.AuthorisationIRISObjectID).Select(x => x.Count).SingleOrDefault();
                var rightCount = rightQueryCount.Where(x => x.Key == s.AuthorisationIRISObjectID).Select(x => x.Count).SingleOrDefault();
                s.NumberOfConditionLinkToAuthorisation += leftCount + rightCount;
            });

            return summaryList;
        }

        public List<EstimationLabourRow> GetRegimeActivityLabour(long regimeActivityID)
        {
            return Context.EstimationLabours
                .Include(x => x.Officer)
                .Include(x => x.EstimationLabourAuthorisations)
                .Where(x => x.RegimeActivityID == regimeActivityID && !x.IsDeleted).Select(x => new EstimationLabourRow
                {
                    ID = x.ID,
                    Officer = x.Officer.DisplayName,
                    EstimatedEffort = x.EstimationEffort,
                    TravelRequired = x.TravelRequired,
                    TravelTime = x.TravelTime,
                    Provisional = x.Provisional,
                    ProvisionalEffort = x.ProvisionalEffort,
                    showWarning = x.EstimationLabourAuthorisations.All(a => a.IsDeleted)
                }).ToList();
        }

        public List<EquipmentMaterialRow> GetRegimeActivityEquipment(long regimeActivityID)
        {
            return Context.EquipmentMaterials
                .Include(x => x.TypeREF)
                .Include(x => x.UnitTypeREF)
                .Include(x => x.EquipmentMaterialAuthorisations)
                .Where(x => x.RegimeActivityID == regimeActivityID && !x.IsDeleted).Select(x => new EquipmentMaterialRow
                {
                    ID = x.ID,
                    EquipmentType = x.TypeREF.DisplayValue,
                    UnitType = x.UnitTypeREF.DisplayValue,
                    Quantity = x.Quantity,
                    Provisional = x.Provisional,
                    ProvisionalQuantity = x.ProvisionalQuantity,
                    ActualQuantity = x.ActualQuantity,
                    NotUsed = x.NotUsed,
                    Reason = x.Reason,
                    showWarning = x.EquipmentMaterialAuthorisations.All(a => a.IsDeleted)
                }).ToList();
        }

        public List<ActualLabourRow> GetObservationLabour(long observationID)
        {
            return Context.ActualLabours
                .Include(x => x.Officer)
                .Include(x => x.ActualLabourAuthorisations)
                .Include("ActualLabourAuthorisations.IRISObject")
                .Where(x => x.ObservationID == observationID && !x.IsDeleted).Select(x => new ActualLabourRow
                {
                    ID = x.ID,
                    ObservationID = x.ObservationID,
                    Officer = x.Officer.DisplayName,
                    Effort = x.Effort,
                    TimeCode = x.TimeCode,
                    Authorisations = x.ActualLabourAuthorisations.Select(a => a.Authorisation.IRISObject.BusinessID)
                }).ToList();
        }

        public List<ActualLabourRow> GetRemediationLabour(long remediationID)
        {
            var list = Context.ActualLabours
                 .Include(x => x.Officer)
                 .Where(x => x.RemediationID == remediationID && !x.IsDeleted)
                 .Select(x => new ActualLabourRow
                 {
                     ID = x.ID,
                     Officer = x.Officer.DisplayName,
                     Effort = x.Effort,
                     TimeCode = x.TimeCode,
                 }).ToList();
            list = list.Select(x => { x.Authorisations = new List<string>(); return x; }).ToList();
            return list;
        }

        public List<ActualEquipmentMaterialRow> GetObservationEquipment(long observationID)
        {
            return Context.ActualEquipmentMaterials
                .Include(x => x.TypeREF)
                .Include(x => x.UnitTypeREF)
                .Include(x => x.ActualEquipmentMaterialAuthorisations)
                .Include("ActualEquipmentMaterialAuthorisations.IRISObject")
                .Where(x => x.ObservationID == observationID && !x.IsDeleted).Select(x => new ActualEquipmentMaterialRow
                {
                    ID = x.ID,
                    EquipmentType = x.TypeREF.DisplayValue,
                    UnitType = x.UnitTypeREF.DisplayValue,
                    Quantity = x.Quantity,
                    Authorisations = x.ActualEquipmentMaterialAuthorisations.Select(a => a.Authorisation.IRISObject.BusinessID)
                }).ToList();
        }

        public List<ActualEquipmentMaterialRow> GetRemediationEquipment(long remediationID)
        {
            return Context.ActualEquipmentMaterials
                .Include(x => x.TypeREF)
                .Include(x => x.UnitTypeREF)
                .Include(x => x.ActualEquipmentMaterialAuthorisations)
                .Where(x => x.RemediationID == remediationID && !x.IsDeleted)
                .Select(x => new ActualEquipmentMaterialRow
                {
                    ID = x.ID,
                    EquipmentType = x.TypeREF.DisplayValue,
                    UnitType = x.UnitTypeREF.DisplayValue,
                    Quantity = x.Quantity,
                    Authorisations = x.ActualEquipmentMaterialAuthorisations.Select(a => a.Authorisation.IRISObject.BusinessID)
                }).ToList();
        }

        public List<ObservationComplianceCondition> GetObservedConditionsForRegimeActivity(long regimeActivityID)
        {
            var result = from o in Context.Observations.OfType<ObservationCompliance>().Where(x => x.RegimeActivityID == regimeActivityID)
                         from oca in Context.ObservationComplianceAuthorisations.Where(x => x.ObservationComplianceID == o.ID && !x.IsDeleted)
                         from occ in Context.ObservationComplianceConditions.Where(x => x.ObservationComplianceAuthorisationID == oca.ID && !x.IsDeleted)
                         select occ;
            return result.ToList();
        }

        public List<RegimeActivity> UpdateRegimeActivitiesOfficerResponsibleForRegime(long regimeId, long officerResponsiblePreviousId, long officerResponsibleCurrentId, string currentUserAccountName)
        {
            //SearchResultsIndices results = new SearchResultsIndices(searchHeaderID);

            ObjectParameter searchCount = new ObjectParameter("searchCount", typeof(int));
            List<RegimeActivity> updatedRegimeActivities = Context.UpdateRegimeActivitiesOfficerResponsibleForRegime(regimeId, officerResponsiblePreviousId, officerResponsibleCurrentId, currentUserAccountName).ToList();

            return updatedRegimeActivities;
        }

        public bool HasOutstandingMobileInspectionForAuth(long authorisationID)
        {
            return Context.RegimeActivityComplianceAuthorisations.Any(a => a.AuthorisationID == authorisationID && !a.IsDeleted && a.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedToSphere);
        }

        public bool HasRegimeActivityAuthMobileInspectionCompleted(long regimeActivityComplianceAuthorisationID)
        {
            return Context.RegimeActivityComplianceAuthorisations.Any(a => a.ID == regimeActivityComplianceAuthorisationID && a.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedBackToIRIS);
        }

        public bool HasRegimeActivityAuthMobileInspectionOut(long regimeActivityComplianceAuthorisationID)
        {
            return Context.RegimeActivityComplianceAuthorisations.Any(a => a.ID == regimeActivityComplianceAuthorisationID && a.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedToSphere);
        }

        public bool HasRegimeActivityMngtSiteMobileInspectionOut(long regimeActivityMngtSiteID)
        {
            return Context.RegimeActivityMngtSites.Any(a => a.ID == regimeActivityMngtSiteID && a.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedToSphere);
        }

        public bool HasRegimeActivityMngtSiteMobileInspectionCompleted(long regimeActivityMngtSiteID)
        {
            return Context.RegimeActivityMngtSites.Any(a => a.ID == regimeActivityMngtSiteID && a.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedBackToIRIS);
        }

        public bool HasOutstandingMobileInspectionForRegimeActivity(long regimeActivityID)
        {
            return Context.RegimeActivityComplianceAuthorisations.Any(a => a.RegimeActivityComplianceID == regimeActivityID && !a.IsDeleted && a.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedToSphere);
        }

        public bool HasOutstandingMobileInspectionForRegimeActivityMngt(long regimeActivityID)
        {
            return Context.RegimeActivityMngtSites.Any(a => a.RegimeActivityMngtID == regimeActivityID && !a.IsDeleted && a.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedToSphere);
        }

        public bool HasOutstandingMobileInspectionForManagementSite(long managementSiteID)
        {
            return Context.ManagementSites.Any(a => a.ID == managementSiteID && a.IRISObject.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedToSphere)
                || Context.RegimeActivityMngtSites.Any(a => !a.IsDeleted && a.ManagementSiteID == managementSiteID && a.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedToSphere);
        }

        public bool HasAllMobileInspectionsCompleted(long regimeActivityID)
        {
            var regimeActivityComplianceAuthorisations = Context.RegimeActivityComplianceAuthorisations.Where(a => a.RegimeActivityComplianceID == regimeActivityID && !a.IsDeleted);
            return regimeActivityComplianceAuthorisations.Any() && regimeActivityComplianceAuthorisations.All(x => x.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedBackToIRIS);
        }

        public bool HasAllMngtMobileInspectionsCompleted(long regimeActivityID)
        {
            var regimeActivityMngtSites = Context.RegimeActivityMngtSites.Where(a => a.RegimeActivityMngtID == regimeActivityID && !a.IsDeleted);
            return regimeActivityMngtSites.Any() && regimeActivityMngtSites.All(x => x.MobileInspectionStatus == (long)MobileInspectionStatus.SyncedBackToIRIS);
        }

        public bool IsAuthorisationObservedWithinRegimeActivity(long authorisationIRISObjectID, long regimeActivityIRISObjectID)
        {
            var observedAuth = from regimeActivity in Context.RegimeActivity.Where(r => r.IRISObjectID == regimeActivityIRISObjectID)
                               from observation in Context.Observations.Where(o => o.RegimeActivityID == regimeActivity.ID)
                               from obsAuth in Context.ObservationComplianceAuthorisations.Where(oa => oa.ObservationComplianceID == observation.ID && !oa.IsDeleted)
                               from auth in Context.Authorisations.Where(a => a.ID == obsAuth.AuthorisationID)
                               select new { auth };
            return observedAuth.Any(a => a.auth.IRISObjectID == authorisationIRISObjectID);
        }

        public Observation FindObservationByDateTimeForRegimeActivity(long regimeActivityIRISObjectID, string observationTypeCode, DateTime observationDate, string inspectionTimeStamp)
        {
            var result = from regimeActivity in Context.RegimeActivity.Where(x => x.IRISObjectID == regimeActivityIRISObjectID)
                         from observation in Context.Observations.Where(x => x.RegimeActivityID == regimeActivity.ID && x.ObservationDate == observationDate && x.MobileInspectionCreatedTimeStamp == inspectionTimeStamp)
                         from irisObject in Context.IRISObject.Where(x => x.ID == observation.IRISObjectID)
                         from observationREF in Context.ReferenceDataValue.Where(x => x.Code == observationTypeCode && x.ID == irisObject.SubClass2ID)
                         from observingOfficer in Context.ObservationObservingOfficers.Where(x => x.ObservationID == observation.ID)
                         select new { observation, irisObject, observingOfficer };

            return result.AsEnumerable().Select(s => s.observation).FirstOrDefault();
        }

        public void ScheduleRegimeActivityBatch(long regimeActivityIRISObjectID)
        {
            Context.BatchJob_RegimeActivity_ActivityScheduling(null, null, regimeActivityIRISObjectID);
        }

        public Dictionary<long, List<RegimeActivity>> GetOrderedRegimeActivitiesWithinRegime(long regimeID)
        {
            return Context.RegimeActivitySchedule.Where(s => s.RegimeID == regimeID)
                .ToDictionary(k => k.ID,
                    v => Context.RegimeActivity.Where(a => a.RegimeActivityScheduleID == v.ID)
                            .OrderBy(x => x.ActualFromDate.HasValue ? x.ActualFromDate : x.TargetFromDate).ToList());
        }

        public long GetParentRegimeID(long regimeActivityIRISObjectID)
        {
            return Context.RegimeActivity.Where(x => x.IRISObjectID == regimeActivityIRISObjectID).Select(x => x.RegimeActivitySchedule.RegimeID).Single();
        }

        public Observation GetObservationWithParentIDByIRISID(long observationIRISObjectID)
        {
            return Context.Observations
                .Include(o => o.RegimeActivity.RegimeActivitySchedule)
                .SingleOrDefault(x => x.IRISObjectID == observationIRISObjectID);
        }

    }
}
