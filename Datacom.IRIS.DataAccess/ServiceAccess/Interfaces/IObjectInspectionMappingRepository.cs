using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IObjectInspectionMappingRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ObjectInspectionTypeMapping GetInspectionMappingByID(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<ObjectInspectionTypeMapping> GetAllInspectionTypeMappings(bool includeInActive = false);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        ObjectInspectionTypeMapping GetInspectionMappingByPrimaryAndSecondaryID(long primaryID, long? secondaryID);
    }
}
