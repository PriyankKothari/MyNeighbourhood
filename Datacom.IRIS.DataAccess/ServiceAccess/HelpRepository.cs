using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.DTO;
using LinqToCache;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class HelpRepository : RepositoryStore, IHelpRepository
    {
        public HelpLink GetHelpLinkByID(long id)
        {
            return Context.HelpLink.Where(helpLink => helpLink.ID == id).Single().TrackAll(); //Do not make use of cache as this is for the overlay from the Admin screen (ie. for edit)
        }

        public List<HelpContentDTO> GetAllHelpContentDTOs()
        {       
           var query =  from helpContent in Context.HelpContent
                        where !helpContent.IsDeleted
                        select new HelpContentDTO
                        {
                            ID = helpContent.ID,
                            Name = helpContent.Name,
                            Title = helpContent.Title
                        };

           return query.OrderBy(x => x.Name).ToList();
        }

        public HelpContent GetHelpContentByNameWithDifferentID(string name, long id)
        {
            return Context.HelpContent.Where(helpContent => helpContent.Name == name && helpContent.ID != id).SingleOrDefault().TrackAll();
        }

        public List<HelpLink> GetExistingHelpLinksWithContentId(long contentId)
        {
            return Context.HelpLink.Where(hl => hl.HelpContentID == contentId).ToList().TrackAll();
        }

        public List<HelpLinkDTO> GetAllHelpLinkDTOs()
        {
            var query = from helpLink in Context.HelpLink
                        select new HelpLinkDTO
                        {
                            ID = helpLink.ID,
                            HelpLinkName = helpLink.Name,
                            HelpContentName = (helpLink.HelpContentID.HasValue && string.IsNullOrEmpty(helpLink.URL)) ? helpLink.HelpContent.Name : helpLink.URL
                        };                        

            return query.OrderBy(x=> x.HelpLinkName).ToList();
        }

        public HelpContent GetHelpContentByID(long id)
        {
            return Context.HelpContent.Where(helpContent => helpContent.ID == id && !helpContent.IsDeleted).Single().TrackAll();
        }

        public List<HelpLink> GetHelpLinkByName(params string[] namesList)
        {
            var namesListLowerCase = namesList.Select(x => x.ToLower()).ToArray();  //convert to lower case
            return GetCachedHelpLinks().Where(helpLink => namesListLowerCase.Contains(helpLink.Name.ToLower())).ToList(); //Make use of cached helplinks 
        }


        private List<HelpLink> GetCachedHelpLinks()
        {
            return Context.HelpLink.FromCached(CacheConstants.HelpLinks).ToList();
        }




        
    }



}