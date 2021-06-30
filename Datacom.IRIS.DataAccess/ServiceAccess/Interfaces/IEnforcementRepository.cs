using System.Collections.Generic;
using System.Xml.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.DTO;
using Datacom.IRIS.DomainModel.DTO.Enforcement;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Attributes;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IEnforcementRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementActionIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        long GetactivityRelationshipObjectIdByEnforcementActionId(long enforcementActionIRISObjectID);


        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Enforcement GetEnforcementByID(long enforcementId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Enforcement GetEnforcementByIDForDetailsPage(long enforcementId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Enforcement GetEnforcementByIRISObjectID(long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Enforcement> GetEnforcementsByIRISObjectIDs(List<long> irisObjectIDsList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<EnforcementAllegedOffence> GetEnforcementAllegedOffencesByIRISObjectIDs(List<long> irisObjectIDsList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        EnforcementAllegedOffence GetEnforcementAllegedOffenceById(long EnforcementAllegedOffenceId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        EnforcementAllegedOffence GetEnforcementAllegedOffenceByIdForDetailsPage(long EnforcementAllegedOffenceId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.MOJInfringements, PermissionName = PermissionNames.Export)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementUnpaidInfringementDTO> GetUnpaidInfringements();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.MOJInfringements, PermissionName = PermissionNames.Export)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<MOJExportReportLineDTO> GetMOJExportReportByInfringementIDs(List<long> infringementIds);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.MOJInfringements, PermissionName = PermissionNames.Export)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string GetMOJExportReportSequenceNumber();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.MOJInfringements, PermissionName = PermissionNames.Export)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string MOJExportUpdateInfringementNoticeActionsAndIncrementBatchSequenceOnCompletion(List<long> infringementNoticeIds);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementAllegedOffence> GetAllegedOffencesAndOffendersByEnforcementID(long enforcementID, long enforcementIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementAllegedOffenceIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementAllegedOffenceOffender> GetAllegedOffendersList(long enforcementAllegedOffenceIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RelationshipEntry> GetAllegedOffendersListByEnforcementIRISObjectID(long enforcementIRISObjectID, long enforcementID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<EnforcementAllegedOffence> GetAllegedOffencesList(long enforcementId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementAllegedOffenceIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementAllegedOffenceOffender> GetAllegedOffendersListWithContactSublinks(long enforcementAllegedOffenceIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<EnforcementAction> GetEnforcementActions(long enforcementId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        EnforcementAction GetEnforcementActionById(long EnforcementActionId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasOutstandingInspectionForEnforcement(long enforcementId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool HasSyncedMobileEnforcementAction(long enforcementId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        EnforcementAction GetEnforcementActionByDefendantId(long enforcementActionProsecutionDefendantId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        EnforcementAction GetEnforcementActionByIdForDetailsPage(long EnforcementActionId); 

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "allegedOffenceIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RelationshipEntry> GetUnassociatedAllegedOffendersForOffenceByEnforcementIDAndOffenceIDForDropdown(
            long allegedOffenceIRISObjectID, long enforcementId, long? relationshipIDToInclude = null);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementAllegedOffenceOffender> GetOffendersWhereActionIsToBeTakenWithNamesByEnforcementActionIDForDropdown(long enforcementID, long enforcementIRISObjectID, long enforcementActionID, long? offenderIDToInclude = null);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementAllegedOffence> GetChargeOffencesByDefendantIDForDropdown(long enforcementID, long enforcementIRISObjectID, long defendantID, long? offenceIDToInclude = null);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RelationshipEntry> GetDefenseCouncillorsByEnforcementIRISObjectIDForDropdown(long enforcementID, long enforcementIRISObjectID, long? activityRelationshipObjectIDToInclude = null);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ContactAddress> GetContactAddressesByActivityRelationshipObjectId(long enforcementIRISObjectID, long activityRelationshipObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ActivityObjectRelationship GetActivityObjectRelationshipByIRISObjectIDAndContactID(long irisObjectID, long contactID, string linkType);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Enforcement GetEnforcementWithOffendersWhereActionToBeTakenYes(long enforcementId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforementIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementActionRecipientOrRespondent> GetEnforcementActionRecipientsByOffenderID(long offenderID, long enforementIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementActionOfficerResponsible> GetEnforcementActionOfficerResponsibleById(long irisObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementActionIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementAllegedOffence> GetAllegedOffencesListByEnforcementAction(long enforcementActionIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementActionIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementActionRecipientOrRespondent> GetRecipientsAndRespondentsByEnforcementActionIRISObjectId(long enforcementActionIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementActionIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RelationshipEntry> GetUnassignedOffenderRelationshipsWhereActionIsToBeTakenByEnforcementActionIRISObjectIDForRespondentsRecipientsDropdown(
            long enforcementActionID, long enforcementActionIRISObjectID, long enforcementID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementActionIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<EnforcementAllegedOffence> GetAllegedOffencesFilteredByActAndOffenders(long enforcementActionIRISObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementIrisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<long> GetCountOfAllegedOffenceAssociated(long enforcementIrisObjectID, long allegedOffenceID);

        // Only used in PostEntityChangeHooks
        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "enforcementActionIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        long GetParentEnforcementID(long enforcementActionIRISObjectID);
    }
}
