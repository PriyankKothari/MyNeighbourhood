using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Specialized;
using MyNeighbourhood.Domain.Attributes;

namespace MyNeighbourhood.Persistent.DbEnums
{
    [JsonConverter(typeof(StringEnumerator))]
    public enum ContactTypes
    {
        NotSpecified = 0,

        [Display(Name = "Regular", Description = "Regular Contact Type")]
        [StringValue("Regular Contact Type")]
        Regular = 1,

        [Display(Name = "Emergency", Description = "Emergency Contact Type")]
        [StringValue("Emergency Contact Type")]
        Emergency = 2
    }
}