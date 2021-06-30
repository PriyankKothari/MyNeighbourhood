using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using MyNeighbourhood.Domain.Attributes;
using Newtonsoft.Json;

namespace MyNeighbourhood.Persistent.DbEnums
{
    [JsonConverter(typeof(StringEnumerator))]
    public enum NameTypes
    {
        NotSpecified = 0,

        [Display(Name = "FullName", Description = "Full Name")]
        [StringValue("Full Name")]
        FullName = 1,

        [Display(Name = "KnownAs", Description = "Known As")]
        [StringValue("Known As")]
        KnownAs = 2
    }
}