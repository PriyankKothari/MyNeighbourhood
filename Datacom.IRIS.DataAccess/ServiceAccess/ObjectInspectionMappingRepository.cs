using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class ObjectInspectionMappingRepository : RepositoryStore, IObjectInspectionMappingRepository
    {
        /// <summary>
        /// Used in Admin screen so returns Deleted/InActive row as well
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ObjectInspectionTypeMapping GetInspectionMappingByID(long id)
        {
            return Context.ObjectInspectionTypeMappings.SingleOrDefault(x => x.ID == id);
        }

        public List<ObjectInspectionTypeMapping> GetAllInspectionTypeMappings(bool includeInActive = false)
        {
            var query = Context.ObjectInspectionTypeMappings.AsQueryable();
            if (!includeInActive)
            {
                 query = query.Where(x => !x.IsDeleted);
            }
            var results = query.ToList();
            return results;
        }

        public ObjectInspectionTypeMapping GetInspectionMappingByPrimaryAndSecondaryID(long primaryID, long? secondaryID)
        {
            var results = Context.ObjectInspectionTypeMappings.ToList();
            var inspection = results.SingleOrDefault(x => x.PrimaryObjectAndSubClassesREFID == primaryID && x.SecondaryObjectAndSubClassesREFID == secondaryID && !x.IsDeleted);
            return inspection;
        }
    }
}
