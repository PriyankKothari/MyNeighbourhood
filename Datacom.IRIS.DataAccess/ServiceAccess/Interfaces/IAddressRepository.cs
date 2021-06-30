using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.Attributes;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IAddressRepository : IRepositoryBase
    {
        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Address GetAddressByID(long addressID);
    }
}