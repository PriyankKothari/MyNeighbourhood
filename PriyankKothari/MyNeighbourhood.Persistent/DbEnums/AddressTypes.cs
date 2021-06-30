using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using MyNeighbourhood.Domain.Attributes;
using Newtonsoft.Json;

namespace MyNeighbourhood.Persistent.DbEnums
{
    [JsonConverter(typeof(StringEnumerator))]
    public enum AddressTypes
    {
        NotSpecified = 0,

        [Display(Name = "Home", Description = "Home Address")]
        [StringValue("Home Address")]
        Home = 1,

        [Display(Name = "Business", Description = "Business Address")]
        [StringValue("Business Address")]
        Business = 2,

        [Display(Name = "Work", Description = "Work Address")]
        [StringValue("Work Address")]
        Work = 3,
    }
}