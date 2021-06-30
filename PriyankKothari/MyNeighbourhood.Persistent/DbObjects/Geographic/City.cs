using System;
using System.Collections.Generic;

namespace MyNeighbourhood.Persistent.DbObjects.Geographic
{
    public class City
    {
        #region Primitive Properties

        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public List<Suburb> Suburbs { get; set; }

        #endregion

        #region Navigation Properties

        public int DistrictId { get; set; }

        public District District { get; set; }

        #endregion
    }
}