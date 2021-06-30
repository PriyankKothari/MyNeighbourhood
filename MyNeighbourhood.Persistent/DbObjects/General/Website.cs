using System;
using MyNeighbourhood.Persistent.DbEnums;

namespace MyNeighbourhood.Persistent.DbObjects.General
{
    public class Website
    {
        #region Primitive Properties

        public int Id { get; set; }

        public WebsiteTypes WebsiteType { get; set; }

        public bool? IsPreferredWebsite { get; set; }

        public string WebsiteAddress { get; set; }

        public string WebsiteDescription { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        #endregion

        #region Navigation Properties

        public int ContactId { get; set; }

        public Contact Contact { get; set; }

        #endregion
    }
}