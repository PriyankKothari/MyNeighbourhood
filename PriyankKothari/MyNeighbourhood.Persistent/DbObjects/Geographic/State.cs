using System;
using System.Collections.Generic;

namespace MyNeighbourhood.Persistent.DbObjects.Geographic
{
    public class State
    {
        #region Primitive Properties

        public int Id { get; set; }

        public string Name { get; set; }

        public List<District> Districts { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        #endregion

        #region Navigation Properties

        public int CountryId { get; set; }

        public Country Country { get; set; }

        #endregion
    }
}