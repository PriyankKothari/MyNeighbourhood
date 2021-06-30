using System;
using MyNeighbourhood.Persistent.DbEnums;

namespace MyNeighbourhood.Persistent.DbObjects.General
{
    public class Phone
    {
        #region Primitive Properties

        public int Id { get; set; }

        public PhoneTypes PhoneType { get; set; }

        public bool? IsPreferredPhone { get; set; }

        public int? CountryCode { get; set; }

        public int? PhoneAreaCode { get; set; }

        public int? PhoneNumber { get; set; }

        public int? ExtensionNumber { get; set; }

        public string PhoneDescription { get; set; }

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