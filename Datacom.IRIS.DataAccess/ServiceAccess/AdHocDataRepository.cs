using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common.Utils;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class AdHocDataRepository : RepositoryStore, IAdHocDataRepository
    {

        public AdHocData GetAdHocDataById(long adHocDataId)
        {
            var adHocDataQuery = from adHoc in Context.AdHocData
                                     .Include(a => a.IRISObject)
                                     .Include(a => a.IRISObject.ObjectTypeREF)
                                     .Include(a => a.OfficerResponsible)
                                     .Where(adHoc => adHoc.ID == adHocDataId)
                                 select adHoc;
            return adHocDataQuery.First().TrackAll();
        }

        public bool IsDataSetNameUnique(long currentId, string dataSetName)
        {
            return (from i in Context.AdHocData where i.DatasetName.ToLower() == dataSetName.ToLower() && i.ID != currentId select i).Count() == 0;
        }
    }

}
