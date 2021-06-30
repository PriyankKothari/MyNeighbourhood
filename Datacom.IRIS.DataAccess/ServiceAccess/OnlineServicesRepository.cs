using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class OnlineServicesRepository : RepositoryStore, IOnlineServicesRepository
    {
        public long GetContactIDFromOnlineServicesAccountsByUserID(string userID)
        {
            return Context.OnlineServicesAccount.Where(x => x.Value == userID && x.IsActive).Select(x => x.ContactID).FirstOrDefault();
        }
    }
}
