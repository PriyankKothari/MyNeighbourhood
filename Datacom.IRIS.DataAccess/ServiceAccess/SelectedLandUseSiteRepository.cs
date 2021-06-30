using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Utils;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class SelectedLandUseSiteRepository : RepositoryStore, ISelectedLandUseSiteRepository
    {
        public SelectedLandUseSite GetSiteById(long id)
        {
            //Minimal data retrieval 
            //   i.e. Basic object info including the iris object, object type, status.
            //
            
            var site = Context.SelectedLandUseSites
                .Include(x => x.IRISObject)
                .Include(x => x.IRISObject.ObjectTypeREF)
                .Include(x => x.IRISObject.Statuses)
                .Include("IRISObject.Statuses.StatusREF")
                .Include(x => x.OfficerResponsibleREF)
                .SingleOrDefault(x => x.ID == id);

            //Note: SelectedLandUseSiteClassifications info is required by the banner.
            if (site != null)
            {
                var classifications =Context.SelectedLandUseSiteClassifications
                        .Include(x => x.ClassificationREF)
                        .Include(x => x.ContextREF)
                        .Where(c => c.SelectedLandUseSiteID == site.ID && !c.IsDeleted);

                site.SelectedLandUseSiteClassifications.AddTrackableCollection(classifications.ToList());
            }

            return site.TrackAll();
        }


        public List<SelectedLandUseSite> GetSiteByIRISObjectIDs(List<long> IRISObjectIDs)
        {
            var result = from slus in Context.SelectedLandUseSites
                                  .Include(x => x.SelectedLandUseSiteClassifications)
                                  .Include("SelectedLandUseSiteClassifications.ClassificationREF")
                                  .Include(a => a.IRISObject)
                                  .Include(a => a.IRISObject.ObjectTypeREF)
                                  .Where(slu => IRISObjectIDs.Contains(slu.IRISObjectID))
                         select slus;

            return result.ToList();
        }

        public SelectedLandUseSite GetSiteByIdForDetailsPage(long id)
        {
            var result = GetSiteById(id);

            //Extra details page info here.

            // Getting friendly user name for the created by field
            var users = RepositoryMap.SecurityRepository.GetUserList();
            var createdByUser = users.FirstOrDefault(u => u.AccountName.ToLower() == result.CreatedBy.ToLower());
            
            // Getting Site Contaminants
            var siteContaminants = from contaminants in Context.SelectedLandUseSiteContaminants
                                                   .Include(s => s.ContaminantREF)
                                                   .Include(s => s.PresenceREF)
                                                   .Include(s => s.TypeREF)
                                                   .Where(s => s.SelectedLandUseSiteID == result.ID && !s.IsDeleted)
                                   select contaminants;

            foreach (SelectedLandUseSiteContaminant selectedLandUseSiteContaminant in siteContaminants)
            {
                var updatedBy = users.FirstOrDefault(u => u.AccountName.ToLower() == selectedLandUseSiteContaminant.ModifiedBy.ToLower());
                selectedLandUseSiteContaminant.LastUpdatedByUserDisplayValue = updatedBy != null ? updatedBy.DisplayName : "System";
            }

            // Getting Site HAIL
            var siteHAILs = from hails in Context.SelectedLandUseSiteHAILs
                                                   .Include(s => s.HAILCategoryREF)
                                                   .Include(s => s.HAILGroupREF)
                                                   .Where(s => s.SelectedLandUseSiteID == result.ID && !s.IsDeleted)
                                   select hails;
            
            foreach (SelectedLandUseSiteHAIL selectedLandUseSiteHail in siteHAILs)
            {
                var updatedBy = users.FirstOrDefault(u => u.AccountName.ToLower() == selectedLandUseSiteHail.ModifiedBy.ToLower());
                selectedLandUseSiteHail.LastUpdatedByUserDisplayValue = updatedBy != null ? updatedBy.DisplayName : "System";
            }


            // Getting Rapid Risk Screening
            var siteRapidRiskScreening = from riskScreenings in Context.SelectedLandUseSiteRapidRiskScreenings
                                                   .Include(s => s.AssessedByREF)
                                                   .Include(s => s.SurfaceWaterRiskRatingREF)
                                                   .Include(s => s.GroundWaterRiskRatingREF)
                                                   .Include(s => s.DirectContactRiskRatingREF)
                                                   .Include(s => s.OverallSiteRiskRatingREF)
                                                   .Where(s => s.SelectedLandUseSiteID == result.ID && !s.IsDeleted)
                                         select riskScreenings;

           
            // Getting Other Identifiers
            var otherIdentifiers = from otherIdentifier in Context.OtherIdentifiers
                                         .Include(o => o.IdentifierContextREF)
                                         .Where(o => o.IRISObjectID == result.IRISObjectID && !o.IsDeleted)
                                   select otherIdentifier;

            result.CreatedByUserDisplayValue = createdByUser != null ? createdByUser.DisplayName : "System";
            result.SelectedLandUseSiteHAILs.AddTrackableCollection(siteHAILs.ToList());
            result.SelectedLandUseSiteContaminants.AddTrackableCollection(siteContaminants.ToList());
            result.SelectedLandUseSiteRapidRiskScreenings.AddTrackableCollection(siteRapidRiskScreening.ToList());
            result.IRISObject.OtherIdentifiers.AddTrackableCollection(otherIdentifiers.ToList());

            return result;
        }

    }
}