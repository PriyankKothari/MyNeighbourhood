using System;
using MyNeighbourhood.Persistent.DbEnums;
using MyNeighbourhood.Persistent.DbObjects.Geographic;
using MyNeighbourhood.Persistent.DbObjects.Members;

namespace MyNeighbourhood.Persistent.DbObjects.General
{
    public class Address
    {
        #region Primitive Properties

        public int Id { get; set; }

        public AddressTypes AddressType { get; set; }

        public bool? IsPreferredAddress { get; set; }

        public string AddressLineOne { get; set; }

        public string AddressLineTwo { get; set; }

        public string AddressLineThree { get; set; }

        public string PostCode { get; set; }

        public long? Latitude { get; set; }

        public long? Longitude { get; set; }

        public Neighbourhood Neighbourhood { get; set; }

        public Suburb Suburb { get; set; }

        public City City { get; set; }

        public District District { get; set; }

        public State State { get; set; }

        public Country Country { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        #endregion

        #region Navigatino Properties

        public int MemberId { get; set; }

        public Member Member { get; set; }

        #endregion
    }
}