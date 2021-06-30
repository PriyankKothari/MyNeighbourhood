
using System;
using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.DTO;
using Datacom.IRIS.Common;
using System.Web.UI.WebControls;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IMonitoringRepository : IRepositoryBase
    {
        // -----------------------------------

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Estimation, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValueRate GetReferenceDataValueRate(long resourceLabourPositionId, long financialYearId, long resourceunitTypeId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Estimation, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ReferenceDataValueRate GetReferenceDataValueRateById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Estimation, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ReferenceDataValueRate> GetReferenceDataValueRateByFinancialYear(long financialYearId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Regime GetRegime(long id);

        bool HasLinksToObservationOrRemediationOrSampleResult(long regimeActivityId);
        bool HasLinksToSampleResult(long observationId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Regime> GetRegimesByIRISObjectIDs(List<long> irisObjectIDsList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Observation> GetObservationsByIRISObjectIDs(List<long> irisObjectIDsList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<RegimeActivity> GetRegimeActivitiesByIRISObjectIDs(List<long> irisObjectIDsList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<RegimeActivity> GetRegimeActivities(long regimeId, bool currentFlag);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<RegimeActivity> GetRegimeActivitiesByRegimeId(long regimeId, bool currentFlag);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        int GetRegimeActivitiesByRegimeIdCount(long regimeId, bool currentFlag, bool currentActivities);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<RegimeActivity> GetCurrentRegimeActivitiesByManagementSiteId(long managementSiteId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        int GetCurrentRegimeActivitiesByManagementSiteIdCount(long managementSiteId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        RegimeActivity GetRegimeActivityByIdForDetailsPage(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        RegimeActivity GetRegimeActivityByID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        RegimeActivity GetRegimeActivityByIRISObjectID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        void SaveRegimeActivityDetails(RegimeActivityMngt mngt, long managementSiteID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        SampleResult GetSampleResultById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Observation GetObservationByObservationMngtLineId(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RegimeActivity> GetRegimeActivitiesForSchedule(long scheduleID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Observation GetObservationByIdForDetailsPage(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Observation GetObservationById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        void  AddObservationMngtFromObservationById(long id, long managementSiteId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Observation GetObservationByIRISObjectID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Programme GetProgrammeById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Programme> GetProgrammesByIRISObjectIDs(List<long> irisObjectIDsList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Observation> GetObservations(long regimeActivityId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Observation> GetObservationsCreatedByManagementSite(long ManagementSiteID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Observation> GetObservationsByMngtSitID(long mngtSitID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName="regimeActivityComplianceIRISObjectId" )]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Authorisation> GetParentRegimeLinkedAuthorisation(long regimeActivityComplianceIRISObjectId,bool getActiveRelationShip);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "regimeIRISObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Authorisation> GetRegimeLinkedAuthorisation(long regimeId, long regimeIRISObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Remediation GetRemediationById(long id);

        /// <summary>
        ///  NOTE: This method is only used in getting a list of Condition abstracts for editting 
        ///  the RegimeActivityComplianceCondition. 
        /// </summary>
        /// <param name="authorisationID"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "authorisationID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.Authorisation)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RegimeActivityComplianceConditionRow> GetConditionsLinkedDirectToAuthorisation(long authorisationID, long regimeActivityID);

        /// <summary>
        ///  NOTE: This method is only used in getting a list of Condition abstracts for editting 
        ///  the RegimeActivityComplianceCondition. 
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "authorisationID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.Authorisation)] //check permission to Authorisation
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]  //check permission to ConditonSchedule
        List<RegimeActivityComplianceConditionRow> GetConditionsLinkedToAuthorisationViaConditionSchedule(long authorisationID, long regimeActivityID);
        
        //[SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "regimeActivityComplianceIRISObjectId")] //check view permission to RegimeActivity
        //[SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]  //Assume if user has access to regime activity, it's ok to allow it to access the related authorisation and conditions.
        //List<RegimeActivityComplianceCondition> GetConditionsByRegimeActivityComplianceId(long regimeActivityComplianceId, long regimeActivityComplianceIRISObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Regime GetSimpleRegimeForBanner(long regimeActivityID);

        [DoNotGenerateBusinessWrapper]
        IRISObject GetIRISObjectForRegimeByRegimeID(long regimeActivityScheduleId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "regimeActivityIRISObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool IsRegimeActivityComplianceAuthorisationDeletable(long regimeActivityComplianceAuthorisationID, long regimeActivityIRISObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<ObservationCompliance> GetObservationCompliances(long regimeActivityComplianceId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<ObservationCompliance> GetObservationCompliancesForConditionAndRegime(long regimeID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "observationIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ObservationComplianceAuthorisation GetObservationComplianceAuthorisation(long observationComplianceAuthorisationID, long observationIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "observationIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<AuthorisationComplianceCondition> GetAuthorisationComplianceConditions(long observationComplianceAuthorisationID, long observationIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "regimeActivityIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RegimeActivityComplianceCondition> GetRegimeActivityComplianceConditions(long regimeActivityID, long authorisationID, long regimeActivityIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "observationIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<AuthorisationAssessedRow> GetAuthorisationAssessedRows(long observationID, long? regimeActivityID, long observationIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "regimeActivityIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RegimeActivityObservationComplianceRow> GetRegimeActivityObservationComplianceRow(long regimeActivityID, long regimeActivityIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        RegimeActivity GetRegimeActivityForObservation(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId", PermissionName = PermissionNames.Edit)] //edit permission is required 
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RegimeActivity> GetFutureDatedScheduledRegimeActivitiesByScheduleIdAndRegimeActivityId(long scheduleId, long regimeActivityId, long irisObjectId, DateTime cutoffDate);

        /// <summary>
        ///  NOTE: This method is only used in getting a list of Remediation abstracts for displaying 
        ///  the Remediation. 
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ObservationRemediationSummary> GetObservationRemediationSummaries([UseCurrentUserInBusinessWrapper] User user, long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Remediation GetLastestRemediation(long irisObjectID);

        /// <summary>
        ///  parentIrisObjectID will be either the IRISObjectID of the parent RegimeActivity
        ///  or the parent ManagementSite which is depending on where the observation/remediation
        ///  has been created.
        /// </summary>
        /// <param name="parentIrisObjectID">Just for security checking</param>
        /// <param name="managementSiteIDs"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "managementSiteID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.ManagementSite)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<SitePlan> GetSitePlans(long managementSiteID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "regimeActivityID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.RegimeActivity)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RegimeActivityComplianceAuthorisationSummary> GetComplianceAuthorisationSummaries(long regimeActivityID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "regimeActivityID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.RegimeActivity)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EstimationLabourRow> GetRegimeActivityLabour(long regimeActivityID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "regimeActivityID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.RegimeActivity)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EquipmentMaterialRow> GetRegimeActivityEquipment(long regimeActivityID);

        //[SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "observationID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.Observation)]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ActualLabourRow> GetObservationLabour(long observationID);

        //[SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "remediationID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.Remediation )]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ActualLabourRow> GetRemediationLabour(long remediationID);

        //[SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "observationID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.Observation)]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ActualEquipmentMaterialRow> GetObservationEquipment(long observationID);

        //[SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "remediationID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.Remediation)]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ActualEquipmentMaterialRow> GetRemediationEquipment(long remediationID);

        /// <summary>
        ///  NOTE: This method is only used in getting a list of Conditions which had been
        ///  observed under a regime activity
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "regimeActivityID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.RegimeActivity)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ObservationComplianceCondition> GetObservedConditionsForRegimeActivity(long regimeActivityID);
		
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<RegimeActivity> UpdateRegimeActivitiesOfficerResponsibleForRegime(long regimeId, long officerResponsiblePreviousId, long officerResponsibleCurrentId, string currentUserAccountName);

        // whether to show the synced icon for Authorisation
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "authorisationID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.Authorisation)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasOutstandingMobileInspectionForAuth(long authorisationID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "managementSiteID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.ManagementSite)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasOutstandingMobileInspectionForManagementSite(long managementSiteID);

        // whether to show the synced icon for Authorisation
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasRegimeActivityAuthMobileInspectionCompleted(long regimeActivityComplianceAuthorisationID);

        // whether to show the synced icon for Authorisation
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasRegimeActivityAuthMobileInspectionOut(long regimeActivityComplianceAuthorisationID);
        
        //whether to show the synced icon for Management Site
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasRegimeActivityMngtSiteMobileInspectionOut(long regimeActivityMngtSiteID);

        //whether to show the synced icon for Management Site
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasRegimeActivityMngtSiteMobileInspectionCompleted(long regimeActivityMngtSiteID);

        // whether to show the synced icon for RegimeActivity
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "regimeActivityID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.RegimeActivity)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasOutstandingMobileInspectionForRegimeActivity(long regimeActivityID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "regimeActivityID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.RegimeActivity)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasOutstandingMobileInspectionForRegimeActivityMngt(long regimeActivityID);

        // whether to show the synced back icon for RegimeActivity
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "regimeActivityID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.RegimeActivity)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasAllMobileInspectionsCompleted(long regimeActivityID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObjectViaLinkID, LinkIDParameterName = "regimeActivityID", ObjectTypeCode = ReferenceDataValueCodes.ObjectType.RegimeActivity)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasAllMngtMobileInspectionsCompleted(long regimeActivityID);

        // whether the authorisation is observed within the regimeactivity
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "regimeActivityIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool IsAuthorisationObservedWithinRegimeActivity(long authorisationIRISObjectID, long regimeActivityIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "regimeActivityIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Observation FindObservationByDateTimeForRegimeActivity(long regimeActivityIRISObjectID, string observationTypeCode, DateTime observationDate, string inspectionTimeStamp);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        void ScheduleRegimeActivityBatch(long regimeActivityIRISObjectID);

        // return value: <scheduleID, ordered RA ids within the schedule>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<long, List<RegimeActivity>> GetOrderedRegimeActivitiesWithinRegime(long regimeID);

        // Only used in PostEntityChangeHooks
        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "regimeActivityIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        long GetParentRegimeID(long regimeActivityIRISObjectID);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "observationIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Observation GetObservationWithParentIDByIRISID(long observationIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Remediation> GetRemediationsByManagementSiteId(long MangementSiteID);
    }
}
