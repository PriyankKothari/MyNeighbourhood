using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DomainModel.DTO;
using System.Data;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IAuthorisationRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Application GetApplicationByID(long id);
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Application GetApplicationByBusinessID(string businessId);
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Application GetApplicationByIRISObjectID(long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]  //set precheck to be none because we want to check the user permission to an Application, not to an Activity
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Activity GetActivityByIrisObjectID(long irisObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]  //set precheck to be none because we want to check the user permission to an Application, not to an Activity
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Activity GetActivityByIDForDetailsPage(long activityActivityID);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]  //set precheck to be none because we want to check the user permission to an Application, not to an Activity
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Activity GetActivityByID(long activityActivityID);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Authorisation GetAuthorisationByID(long authorisationID);
        
        /// <summary>
        ///    This method is used to return a simple authorisation detail for "Add Condition" 
        ///    Overlay in Compliance RegimeActivity detail page.
        /// </summary>
        /// <param name="authorisationID"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Authorisation GetSimpleAuthorisationByID(long authorisationID);
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Authorisation GetAuthorisationByBusinessID(string businessId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Authorisation GetAuthorisationByIrisObjectId(long irisObjectId);

        /// <summary>
        ///    This method is used in two places: checking if an activity has an authorisation
        ///    for the sake of setting menu items in the action menu, retrieving the authorisation
        ///    name to compile a URL in the activity details page.
        ///    Pre Check would verify user has permisison to view the activity IRISObject 
        ///    Only the Name and Authorisation ID is required.  Thus there is no post check on Authorisation View permission.
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName="irisObjectId")]  
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        AuthorisationData GetAuthorisationByActivityID(long activityID, long irisObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<long, KeyValuePair<long, string>> GetAuthorisationIDsIRISIDsByActivitiesIDs(List<long> activityIDs, List<long> irisObjectIDs);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<long, string> GetAuthorisationStatusPairs(List<long> irisObjectIDs);         //used by datacom service (map component)

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RegimeActivityComplianceAuthorisation> GetRegimeActivityComplianceByAuthorisationId(long authorisationId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<long, string> GetApplicationStatusPairsForActivities(List<long> irisObjectIDs);   //used by datacom service (map component)

        [DoNotGenerateBusinessWrapperAttribute]
        IRISObject GetIRISObjectByAuthorisationID(long authorisationID);

        [DoNotGenerateBusinessWrapperAttribute]
        IRISObject GetIRISObjectForApplicationByActivityID(long activityID);

        [DoNotGenerateBusinessWrapperAttribute]
        IRISObject GetIRISObjectForApplicationByApplicationID(long applicationID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        AuthorisationGroup GetAuthorisationGroupByID(long authorisationGroupId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Authorisation> GetAuthorisationGroupAuthorisationsByGroupIRISID(long authGroupObjectID);
        void BulkReplaceEndDateAuthorisation(BulkReplaceAuthorisationOptions bulkReplaceAuthorisationOptions, User user);
        [DoNotGenerateBusinessWrapper]
        void CopyActivityCDFAnswersToAuthorisation(long authorisationID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Authorisation> GetRegimeActivityLinkedAuthorisation(long regimeActivityID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<Authorisation> GetRegimeActivityObservedAuthorisation(long regimeActivityID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "authIRISID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<AuthorisationObservationRow> GetAuthorisationLinkedObservations(long authorisationID, long authIRISID);

        // Only used in PostEntityChangeHooks
        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "activityIRISObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        long GetParentApplicationID(long activityIRISObjectID);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        DataTable GetContactAuthorisations(long contactIrisObjectId);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        DataTable GetConsentsForRenewalChange(long contactIrisObjectId);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string ValidateConsentsForRenewalChange(string selectedAuthorisations);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        bool IsAuthorisationIDReserved(long activityId, string businessId);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Activity GetActivityByAuthorisationIRISID(string authorisationIRISID);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Programme> GetProgrammesLinkedToAuthorisation(long authoristaionIrisObjectID);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Regime> GetRegimesLinkedToAuthorisation(long authoristaionIrisObjectID);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Authorisation> GetAuthorisationsByPreviousAuthorisationID(long authoristaionIrisObjectID);
    }
}