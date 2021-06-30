using System;
using System.Collections.Generic;
using MyNeighbourhood.Persistent.DbEnums;
using MyNeighbourhood.Persistent.DbObjects.Members;

namespace MyNeighbourhood.Persistent.DbObjects.General
{
    public class Contact
    {
        #region Primitive Properties

        public int Id { get; set; }

        public ContactTypes ContactType { get; set; }

        public string FacebookProfileLink { get; set; }

        public string TwitterHandle { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public List<Email> Emails { get; set; }

        public List<Phone> Phones { get; set; }

        public List<Website> Websites { get; set; }

        #endregion

        #region Navigation Properties

        public int MemberId { get; set; }

        public Member Member { get; set; }

        #endregion
    }
}