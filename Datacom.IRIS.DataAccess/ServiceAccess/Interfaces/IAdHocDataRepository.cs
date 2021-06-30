using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IAdHocDataRepository : IRepositoryBase
    {

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        AdHocData GetAdHocDataById(long adHocDataId);

        bool IsDataSetNameUnique(long currentId, string dataSetName);
    }
}
