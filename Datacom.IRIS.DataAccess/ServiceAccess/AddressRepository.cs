using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common.Utils;


namespace Datacom.IRIS.DataAccess.ServiceAccess
{
 
    public class AddressRepository : RepositoryStore, IAddressRepository
    {
        public Address GetAddressByID(long addressID)
        {
            //TODO: Consider whether we should read the Address and then get the relevant ReferenceData items in an additional query and manually hook them up.

            // Read the address to determine it's type. Is there a better way of doing this?
            Address address = Context.Address.FirstOrDefault(a=>a.ID==addressID);

            // Reread as oer the type together with the Type specific reference data
            if (address is DeliveryAddress)
            {
                return
                    (Context.Address.OfType<DeliveryAddress>().Where(a => a.ID == addressID) as ObjectQuery<DeliveryAddress>)
                    .IncludeTypeSpecificReferenceData()
                    .SingleOrDefault();
            }
            if (address is OverseasAddress)
            {
                return
                    (Context.Address.OfType<OverseasAddress>().Where(a => a.ID == addressID) as ObjectQuery<OverseasAddress>)
                    .IncludeTypeSpecificReferenceData()
                    .SingleOrDefault();
            }
            if (address is UrbanRuralAddress)
            {
                return
                    (Context.Address.OfType<UrbanRuralAddress>().Where(a => a.ID == addressID) as ObjectQuery<UrbanRuralAddress>)
                    .IncludeTypeSpecificReferenceData()
                    .SingleOrDefault();
            }

            return null;
        }

    }

}
