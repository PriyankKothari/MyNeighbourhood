using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Attributes;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface ILocationRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        LocationGroup GetLocationGroupByID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None )]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<LocationGroup> GetAllLocationGroups();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]  
        Location GetLocationByID(long id);

        //cannot set to SecurityCheckMethod.Default or EnsureValidIRISUser because the is used by DatacomService 
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Location> GetSpatialLocationsByIrisObjectIds(params long[] irisObjectIdValues); //everyone has access to location.  Used by DatacomService 

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)] //because we are returning TextualLocation not Location. by checking permission to secAttParam.TypeToCheck.Name (TextualLocation), the call will fail if we set SecurityCheckMethod to Default
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        TextualLocation GetFirstTextualLocationByIrisObjectId(List<long> irisObjectIDs);  //irisObjectIDs are Locations' IRISObjectID

        //Due to performance consideration, use EnsureValidIRISUser instead of setting CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectIdList" 
        //because everyone has read access to location anyway
        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<long, string> GetLocationWarnings(List<long> irisObjectIdList);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName="irisObjectIdList")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Dictionary<long, string> GetLocationGroupWarnings(List<long> irisObjectIdList);  

        bool IsLocationGroupCommonNameUnique(LocationGroup locationGroup);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectID")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string HasAssociateGeometry(long irisObjectID);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "applicationIRISObjectID")] 
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        TextualLocation GetFirstTextualLocationLinkedtoApplicationActivities(long applicationIRISObjectID);
    }
}