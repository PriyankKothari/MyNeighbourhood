using System;
using System.Collections.Generic;

namespace MyNeighbourhood.Persistent.DbObjects.Geographic
{
    public class Country
    {
        #region Primitive Properties

        public int Id { get; set; }

        public string Name { get; set; }

        public string Code { get; set; }

        public string Capital { get; set; }

        public string Currency { get; set; }

        public string PhoneCode { get; set; }

        public long? Latitude { get; set; }

        public long? Longitude { get; set; }

        public string WikipediaSource { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public List<State> States { get; set; }

        public List<TimeZone> TimeZones { get; set; }

        #endregion
    }
}