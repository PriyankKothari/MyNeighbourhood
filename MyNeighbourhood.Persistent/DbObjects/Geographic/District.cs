using System;
using System.Collections.Generic;

namespace MyNeighbourhood.Persistent.DbObjects.Geographic
{
    public class District
    {
        #region Primitive Properties
        
        public int Id { get; set; }

        public string Name { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime ModifiedOn { get; set; }

        public List<City> Cities { get; set; }

        #endregion

        #region Navigation Properties

        public int StateId { get; set; }

        public State State { get; set; }

        #endregion
    }
}