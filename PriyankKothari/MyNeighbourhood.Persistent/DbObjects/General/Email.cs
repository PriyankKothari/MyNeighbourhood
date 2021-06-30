using System;
using MyNeighbourhood.Persistent.DbEnums;

namespace MyNeighbourhood.Persistent.DbObjects.General
{
    public class Email
    {
        #region Primitive Properties

        public int Id { get; set; }

        public EmailTypes EmailType { get; set; }

        public bool? IsPreferredEmail { get; set; }

        public string EmailAddress { get; set; }

        public string EmailDescription { get; set; }

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