using System;
using MyNeighbourhood.Persistent.DbEnums;

namespace MyNeighbourhood.Persistent.DbObjects.General
{
    public class Name
    {
        #region Primitive Properties

        public int Id { get; set; }

        public NameTypes NameType { get; set; }

        public bool? IsPreferredName { get; set; }

        public TitleTypes TitleType { get; set; }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

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