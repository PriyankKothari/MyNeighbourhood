using System;
using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;


namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class ManagementSiteRepository : RepositoryStore, IManagementSiteRepository
    {
        public ManagementSite GetManagementSiteByID(long ManagementSiteID)
        {
            var result = Context.ManagementSites
                            .Include(r => r.HabitatTypeREF)
                            .Include(r => r.OfficerResponsible)
                            .Include(a => a.IRISObject.Statuses)
                            .Include("IRISObject.Statuses.StatusREF")
                            .SingleOrDefault(r => r.ID == ManagementSiteID);

            if (result == null) return null;

            result.MngtSiteSiteProtections.AddTrackableCollection(Context.MngtSiteSiteProtections.Include(o => o.LegalProtectionREF)
                                                                                                        .Where(o => o.ManagementSiteID == result.ID && !o.IsDeleted).ToList());

            result.MngtSiteClassifications.AddTrackableCollection(Context.MngtSiteClassifications.Include(o => o.ClassificationREF)
                                                                                                        .Include(o => o.TypeREF)
                                                                                                        .Where(o => o.ManagementSiteID == result.ID && !o.IsDeleted).ToList());

            var mngtSiteThreatSpecieses = from mngtSTS in Context.MngtSiteThreatSpecies.Where(o => o.ManagementSiteID == result.ID && !o.IsDeleted)
                                          from species in Context.Species.Where(x => x.ID == mngtSTS.SpeciesID)
                                          from speciesType in Context.SpeciesTypes.Where(x => x.SpeciesID == species.ID && !x.IsDeleted)
                                          from speciesStatus in Context.SpeciesStatus.Where(x => x.MngtSiteThreatSpeciesID == mngtSTS.ID & !x.IsDeleted).DefaultIfEmpty()//.OrderByDescending(x=>x.StatusDate).ThenByDescending(x=>x.StatusTime)    
                                          from referenceDataValue in Context.ReferenceDataValue.Where(x => x.ID == speciesStatus.StatusREFID).DefaultIfEmpty()
                                          orderby speciesStatus.StatusDate descending, speciesStatus.StatusTime descending
                                          select new { mngtSTS, species, speciesType, speciesStatus, referenceDataValue };
                                          
            result.MngtSiteThreatSpecies.AddTrackableCollection(mngtSiteThreatSpecieses.AsEnumerable().Select(x => x.mngtSTS).Distinct().ToList());

            result.MngtSiteOtherManagementIssues.AddTrackableCollection(Context.MngtSiteOtherManagementIssues.Include(o => o.TypeREF)
                                                                                            .Where(o => o.ManagementSiteID == result.ID && !o.IsDeleted).ToList());

            result.MngtSiteIndustryPurposes.AddTrackableCollection(Context.MngtSiteIndustryPurposes.Include(o => o.TypeREF)
                                                                                                        .Include(o => o.SituationREF)
                                                                                                        .Where(o => o.ManagementSiteID == result.ID && !o.IsDeleted).ToList());

            result.MngtSiteLandTenures.AddTrackableCollection(Context.MngtSiteLandTenures.Include(o => o.LandTenureREF)
                                                                                .Where(o => o.ManagementSiteID == result.ID && !o.IsDeleted).ToList());

            result.RegimeActivityMngtSites.AddTrackableCollection(Context.RegimeActivityMngtSites.Where(o => o.ManagementSiteID == result.ID && !o.IsDeleted).ToList());

            result.SitePlans.AddTrackableCollection(Context.SitePlans.Include(o => o.TypeREF)
                                                                     .Include(o => o.StatusREF)
                                                                     .Include(o => o.OfficerResponsible)
                                                                     .Where(o => o.ManagementSiteID == result.ID && !o.IsDeleted).ToList());

            result.IRISObject = Context.IRISObject.Include(i => i.OtherIdentifiers)
                                                .Include("OtherIdentifiers.IdentifierContextREF")
                                                .Include(i => i.ObjectTypeREF)
                                                .Include(i => i.SubClass1REF)
                                                .Include(i => i.SubClass2REF)
                                                .Where(i => i.ID == result.IRISObjectID)
                                                .Single();

            GetManagementSiteTypeSpecificValues(result);

            return result.TrackAll();
        }

        public ManagementSite GetSimpleManagementSiteForBanner(long ManagementSiteID)
        {
            var result = Context.ManagementSites
                            .SingleOrDefault(r => r.ID == ManagementSiteID);

            if (result == null) return null;

            result.IRISObject = Context.IRISObject.Include(i => i.ObjectTypeREF)
                                                .Include(i => i.SubClass1REF)
                                                .Include(i => i.SubClass2REF)
                                                .Where(i => i.ID == result.IRISObjectID)
                                                .Single();

            return result.TrackAll();
        }

        private void GetManagementSiteTypeSpecificValues(ManagementSite result)
        {
            MngtSiteBiodiversity mngtSiteBiodiversity = result as MngtSiteBiodiversity;
            if (mngtSiteBiodiversity != null)
            {
                var mngtSiteBiodiversityConsvSpecies = from mngtSTS in Context.MngtSiteBiodiversityConsvSpecies.Where(o => o.MngtSiteBiodiversityID == result.ID && !o.IsDeleted)
                                                          from species in Context.Species.Where(x => x.ID == mngtSTS.SpeciesID)
                                                          from speciesType in Context.SpeciesTypes.Where(x => x.SpeciesID == species.ID && !x.IsDeleted)
                                                          select new { mngtSTS, species, speciesType };
                mngtSiteBiodiversity.MngtSiteBiodiversityConsvSpecies.AddTrackableCollection(mngtSiteBiodiversityConsvSpecies.AsEnumerable().Select(x => x.mngtSTS).Distinct().ToList());

                mngtSiteBiodiversity.MngtSiteBiodiversityEcoFeatures.AddTrackableCollection(Context.MngtSiteBiodiversityEcoFeatures.Include(o => o.TypeREF)
                                                                                                            .Where(o => o.MngtSiteBiodiversityID == result.ID && !o.IsDeleted).ToList());
            }

            var mngtSiteLandManagement = result as MngtSiteLandManagement;
            if (mngtSiteLandManagement != null)
            {
                // Plans and Rules
                RepositoryHelpers.LoadPlansRulesPoliciesForEntity(Context, result);
            }
        }

        public List<ManagementSite> GetManagementSitesByIRISObjectIDs(List<long> irisObjectIDList)
        {
            var result = from mngt in Context.ManagementSites
                                   .Include(a => a.IRISObject)
                                   .Include(a => a.IRISObject.ObjectTypeREF)
                                   .Include(a => a.IRISObject.SubClass1REF)
                                   .Include(a => a.IRISObject.SubClass2REF)
                                   .Include(a => a.HabitatTypeREF)
                                   .Where(req => irisObjectIDList.Contains(req.IRISObjectID))
                              select mngt;

            foreach (var ManagementSite in result)
            {
                MngtSiteThreatSpecies firstMngtSiteThreatSpecies = Context.MngtSiteThreatSpecies.Include(o => o.Species).FirstOrDefault(o => o.ManagementSiteID == ManagementSite.ID && !o.IsDeleted);
                if (firstMngtSiteThreatSpecies != null)
                {
                    ManagementSite.MngtSiteThreatSpecies.Add(firstMngtSiteThreatSpecies);
                }
            }

            return result.ToList();
        }
        
        public List<ManagementSite> GetManagementSitesByRegimeActivityId(long regimeActivityId)
        {
            var dbquery = from regimeActivityMngtSite in Context.RegimeActivityMngtSites.Where(s => s.RegimeActivityMngtID == regimeActivityId && !s.IsDeleted )
                          from managementSite in Context.ManagementSites.Where(m => m.ID == regimeActivityMngtSite.ManagementSiteID)
                          from irisObject in Context.IRISObject.Where(i => i.ID == managementSite.IRISObjectID)
                          from objectTypeREF in Context.ReferenceDataValue.Where(r => r.ID == irisObject.ObjectTypeID)
                          select new { managementSite, irisObject, objectTypeREF };

            return dbquery.AsEnumerable().Select(i => i.managementSite).ToList().TrackAll();
        }

        /// <summary>
        ///  Check if the externalSitePlanID is unique with in the ManagementSite.
        /// </summary>
        /// <param name="managementSiteID"></param>
        /// <param name="sitePlanID"></param>
        /// <param name="externalSitePlanID"></param>
        /// <returns></returns>
        public bool HasDuplicatedExternalSitePlanID(long managementSiteID, long sitePlanID, string externalSitePlanID)
        {
            return Context.SitePlans.Any(
                x =>
                !x.IsDeleted &&
                x.ManagementSiteID == managementSiteID && x.ExternalSitePlanID == externalSitePlanID &&
                (sitePlanID == 0 || x.ID != sitePlanID));
        }

        public bool SitePlanHasObservationOrRemediationAttached(long id)
        {
            return Context.ObservationMngtSitePlans.Where(s => s.SitePlanID == id)
                .Join(Context.Observations, s => s.ObservationMngtID, o => o.ID, (s, o) => o)
                .Any() || Context.RemediationSitePlans.Where(x => x.SitePlanID == id)
                    .Join(Context.Remediations, s => s.RemediationID, r => r.ID, (s, r) => r)
                    .Any(z => !z.IsDeleted);
        }

        public ManagementSite GetManagementSiteByIRISObjectID(long irisObjectId)
        {
            return
                Context.ManagementSites.Include("IRISObject.SubClass1REF").Include("IRISObject.ObjectTypeREF").Include("IRISObject.Statuses.StatusREF").Include("IRISObject.SecurityContextIRISObject.ObjectTypeREF")
                .Include("MngtSiteThreatSpecies.SpeciesStatus")
                    .SingleOrDefault(x => x.IRISObjectID == irisObjectId).TrackAll();
        }
    }
}
