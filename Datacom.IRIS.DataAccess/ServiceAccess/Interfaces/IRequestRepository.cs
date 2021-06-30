using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.Common;
using Datacom.IRIS.DomainModel.DTO;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IRequestRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Request GetRequestByID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        Request GetRequestByIRISObjectID(long irisObjectID);

        /// <summary>
        ///  NOTE: This method is only used in getting a list of Cause And Effects for displaying and 
        ///  editting the RequestIncidentType.
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName="requestIRISObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RequestTypeIncidentCauseEffect> GetRequestCausesAndEffectsById(long requestTypeIncidentId, long requestIRISObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "requestIRISObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        RequestTypeIncidentCauseEffect GetRequestCauseAndEffectById(RequestTypeIncident requestTypeIncident, long id, long requestIRISObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RequestOfficerResponsible> GetRequestOfficerResponsibleById(long requestId, long irisObjectId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        RequestOfficerResponsible GetLatestOfficerResponsible(long requestId, long irisObjectId);

        /// <summary>
        ///  NOTE: This method is only used in getting a list of statuses for displaying Request
        ///  maptips. To use it in other places, the security checking need be reviewed.
        ///  We skip the security checking since to get the linked request IRISObjectID the user
        ///  must have request view permission already.
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<long, KeyValuePair<string, string>> GetRequestStatusPairs(List<long> irisObjectIDs);  //used by DatacomService

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "requestIRISID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<RequestObservationRow> GetRequestLinkedObservations(long requestID, long requestIRISID);
    }
}