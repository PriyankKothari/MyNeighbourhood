using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.Utils
{
    static class AddressQueryExtensions
    {

         public static ObjectQuery<UrbanRuralAddress> IncludeTypeSpecificReferenceData(this ObjectQuery<UrbanRuralAddress> query)
        {
            return query
                .Include(q => q.FloorTypeREF)
                .Include(q => q.StreetTypeREF)
                .Include(q => q.StreetDirectionREF)
                .Include(q => q.UnitTypeREF);
        }

        public static ObjectQuery<DeliveryAddress> IncludeTypeSpecificReferenceData(this ObjectQuery<DeliveryAddress> query)
        {
            return query
                .Include(q => q.DeliveryAddressTypeREF);
        }

        public static ObjectQuery<OverseasAddress> IncludeTypeSpecificReferenceData(this ObjectQuery<OverseasAddress> query)
        {
            return query
                .Include(q => q.CountryREF);
        }

    }
}
