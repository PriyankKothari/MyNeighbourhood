using System;

namespace MyNeighbourhood.Persistent.DbObjects.Geographic
{
    public class TimeZone
    {
        #region Primitive Properties

        public int Id { get; set; }

        public string DisplayName { get; set; }

        public string StandardName { get; set; }

        public bool? HasDayLightSavingTime { get; set; }

        public int? UtcOffSet { get; set; }    

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