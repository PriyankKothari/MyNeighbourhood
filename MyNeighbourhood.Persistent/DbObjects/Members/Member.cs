using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using MyNeighbourhood.Persistent.DbObjects.General;
using MyNeighbourhood.Persistent.DbObjects.Geographic;

namespace MyNeighbourhood.Persistent.DbObjects.Members
{
    public class Member : IdentityUser
    {
        #region Primitive Properties

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Gender { get; set; }

        public DateTime MemberSince { get; set; }

        public string Biography { get; set; }

        public string FavoriteThingAboutNeighbourhood { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public List<Address> MemberAddresses { get; set; }

        public List<Contact> MemberContacts { get; set; }

        #endregion

        #region Navigation Properties

        public int CurrentNeighbourhoodId { get; set; }

        public Neighbourhood CurrentNeighbourhood { get; set; }

        #endregion
    }
}