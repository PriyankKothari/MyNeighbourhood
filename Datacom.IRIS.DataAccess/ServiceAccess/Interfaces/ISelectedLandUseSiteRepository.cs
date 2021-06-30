using System.Collections.Generic;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface ISelectedLandUseSiteRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        SelectedLandUseSite GetSiteById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        List<SelectedLandUseSite> GetSiteByIRISObjectIDs(List<long> IRISObjectIDs);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        SelectedLandUseSite GetSiteByIdForDetailsPage(long id);
    }
}