using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Specialized;
using MyNeighbourhood.Domain.Attributes;

namespace MyNeighbourhood.Persistent.DbEnums
{
    [JsonConverter(typeof(StringEnumerator))]
    public enum EmailTypes
    {
        NotSpecified = 0,

        [Display(Name = "Personal", Description = "Personal Email Address")]
        [StringValue("Personal Email Address")]
        Personal = 1,

        [Display(Name = "Professional", Description = "Professional Email Address")]
        [StringValue("Professional Email Address")]
        Professional = 2
    }
}