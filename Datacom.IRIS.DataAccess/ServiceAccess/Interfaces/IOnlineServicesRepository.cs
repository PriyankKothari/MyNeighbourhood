using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Attributes;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IOnlineServicesRepository : IRepositoryBase
    {
        //Use RegistrationEmail to serach Online Services Accounts table to find ContactID
        long GetContactIDFromOnlineServicesAccountsByUserID(string userID);
    }
}