using System;
using System.Collections.Generic;

namespace MyNeighbourhood.Persistent.DbObjects.Geographic
{
    public class Suburb
    {
        #region Primitive Properties

        public int Id { get; set; }

        public int Name { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public List<Neighbourhood> Neighbourhoods { get; set; }

        #endregion

        #region Navigation Properties

        public int CityId { get; set; }

        public City City { get; set; }

        #endregion
    }
}