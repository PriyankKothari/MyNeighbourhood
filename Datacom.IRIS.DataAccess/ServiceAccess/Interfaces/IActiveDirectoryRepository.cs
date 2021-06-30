using System;
namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface IActiveDirectoryRepository
    {
        void PopulateUserInfoFromAD(Datacom.IRIS.DomainModel.Domain.User user);
        bool UserExistsInAD(string accountName);
    }
}
