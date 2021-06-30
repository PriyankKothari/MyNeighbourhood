using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{

    public class SpeciesRepository : RepositoryStore, ISpeciesRepository
    {
        public Species GetSpeciesByID(long speciesID)
        {
            return (from species in Context.Species
                    join speciesType in Context.SpeciesTypes on species.ID equals speciesType.SpeciesID
                    where species.ID == speciesID && !speciesType.IsDeleted
                    select new
                    {
                        species,
                        speciesType
                    }).AsEnumerable().Select(x => x.species).Distinct().Single().TrackAll();

        }

        public List<Species> GetAllSpeciesForSpeciesType(bool includeInactive, long speciesTypeID)
        {
            return GetAllSpeciesType()
                        .Where(x => (x.SpeciesTypeREFID == speciesTypeID) &&
                                    (includeInactive || x.Species.IsActive))
                        .Select(x => x.Species)
                        .Distinct()
                        .ToList();            
        }


        public List<Species> GetSpeciesListBySpeciesTypeID(long speciesTypeREFID)
        {
            return GetAllSpeciesType()
                        .Where(x => x.SpeciesTypeREFID == speciesTypeREFID && x.Species.IsActive)
                        .Select(x => x.Species)
                        .Distinct()
                        .ToList();
        }

        /// <summary>
        ///    A species must always have a unique scientific name. Hence this method performs check
        /// </summary>
        public bool IsSpeciesScientificNameUnique(Species species)
        {
            return Context.Species.Where(s => string.Compare(s.ScientificName, species.ScientificName, true) == 0 && s.ID != species.ID).Count() == 0;
        }

        public bool IsSpeciesCommonNameUnique(Species species)
        {
            return Context.Species.Where(s => string.Compare(s.CommonName, species.CommonName, true) == 0 && s.ID != species.ID).Count() == 0;
        }

        public List<SpeciesType> GetAllSpeciesType()
        {
            return (from species in Context.Species
                    join speciesType in Context.SpeciesTypes on species.ID equals speciesType.SpeciesID
                    where !speciesType.IsDeleted
                    select new SpeciesSpeciesType
                    {
                        Species = species,
                        SpeciesType = speciesType
                    })
                    .FromCached<SpeciesSpeciesType>(CacheConstants.SpeciesSpeciesType)
                    .AsEnumerable()
                    .Select(x => x.SpeciesType)
                    .ToList();
        }

        private class SpeciesSpeciesType
        {
            public SpeciesType SpeciesType { get; set; }
            public Species Species { get; set; }
        }


    }
}
