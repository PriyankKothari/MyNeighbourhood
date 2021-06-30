using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using MyNeighbourhood.Domain.Attributes;
using Newtonsoft.Json;

namespace MyNeighbourhood.Persistent.DbEnums
{
    [JsonConverter(typeof(StringEnumerator))]
    public enum WebsiteTypes
    {
        NotSpecified = 0,

        [Display(Name = "Personal", Description = "Personal Website Address")]
        [StringValue("Personal Website Address")]
        Personal = 1,

        [Display(Name = "Professional", Description = "Professional Website Address")]
        [StringValue("Professional Website Address")]
        Professional = 2
    }
}