using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain.Interfaces;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class LookupRepository : RepositoryStore, ILookupRepository
    {
        public List<LookupStreet> LookupStreets()
        {
            return Context.LookupStreet
                          .Where(street => !street.IsDeleted)
                          .ToList();
        }
        public List<string> LookupStreetsStartingWith(string streetTerm)
        {
            return Context.LookupStreet
                .Where(street => street.Name.StartsWith(streetTerm) && !street.IsDeleted)
                .Select(street => street.Name)
                .ToList();
        }

        public List<string> LookupStreetsContains(string streetTerm)
        {
            return Context.LookupStreet
                .Where(street => street.Name.Contains(streetTerm) && !street.IsDeleted)
                .Select(street => street.Name)
                .ToList();
        }

        public LookupStreet GetLookupStreetByID(long lookupStreetID)
        {
            return Context.LookupStreet.Single(x => x.ID == lookupStreetID).TrackAll();
        }
        public bool IsLookupStreetUnique(LookupStreet street)
        {
            return !Context.LookupStreet.Any(x => x.Name == street.Name &&
                                                  x.ID != street.ID &&
                                                  !x.IsDeleted);
        }

        public List<LookupSuburb> LookupSuburbs()
        {
            return Context.LookupSuburb
                          .Where(suburb => !suburb.IsDeleted)
                          .ToList();
        }

        public List<string> LookupSuburbsStartingWith(string streetTerm)
        {
            return Context.LookupSuburb
                .Where(suburb => suburb.Name.StartsWith(streetTerm) && !suburb.IsDeleted)
                .Select(suburb => suburb.Name)
                .ToList();
        }

        public List<string> LookupSuburbsContains(string streetTerm)
        {
            return Context.LookupSuburb
                .Where(suburb => suburb.Name.Contains(streetTerm) && !suburb.IsDeleted)
                .Select(suburb => suburb.Name)
                .ToList();
        }

        public LookupSuburb GetLookupSuburbByID(long lookupSuburbID)
        {
            return Context.LookupSuburb.Single(x => x.ID == lookupSuburbID).TrackAll();
        }

        public bool IsLookupSuburbUnique(LookupSuburb suburb)
        {
            return !Context.LookupSuburb.Any(x => x.Name == suburb.Name &&
                                                  x.ID != suburb.ID &&
                                                  !x.IsDeleted);
        }

        public List<LookupTownCity> LookupTownsAndCities()
        {
            return Context.LookupTownCity
                          .Where(townCity => !townCity.IsDeleted)
                          .ToList();
        }

        public List<string> LookupTownsAndCitiesStartingWith(string streetTerm)
        {
            return Context.LookupTownCity
                .Where(townCity => townCity.Name.StartsWith(streetTerm) && !townCity.IsDeleted)
                .Select(townCity => townCity.Name)
                .ToList();
        }

        public List<string> LookupTownsAndCitiesContains(string streetTerm)
        {
            return Context.LookupTownCity
                .Where(townCity => townCity.Name.Contains(streetTerm) && !townCity.IsDeleted)
                .Select(townCity => townCity.Name)
                .ToList();
        }

        public LookupTownCity GetLookupTownCityByID(long lookupTownCityID)
        {
            return Context.LookupTownCity.Single(x => x.ID == lookupTownCityID).TrackAll();
        }

        public bool IsLookupTownCityUnique(LookupTownCity townCity)
        {
            return !Context.LookupTownCity.Any(x => x.Name == townCity.Name &&
                                                   x.ID != townCity.ID && 
                                                   !x.IsDeleted);
        }

    }
}
