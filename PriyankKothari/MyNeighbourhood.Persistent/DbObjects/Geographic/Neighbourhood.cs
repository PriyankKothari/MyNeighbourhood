using System;

namespace MyNeighbourhood.Persistent.DbObjects.Geographic
{
    public class Neighbourhood
    {
        #region Primitive Properties

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        #endregion

        #region Navigation Properties

        public int SuburbId { get; set; }

        public Suburb Suburb { get; set; }

        #endregion
    }
}