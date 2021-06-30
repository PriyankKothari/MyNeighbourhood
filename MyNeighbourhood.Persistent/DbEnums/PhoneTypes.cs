using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System.Collections.Specialized;
using MyNeighbourhood.Domain.Attributes;

namespace MyNeighbourhood.Persistent.DbEnums
{
    [JsonConverter(typeof(StringEnumerator))]
    public enum PhoneTypes
    {
        NotSpecified = 0,

        [Display(Name = "PersonalPhone", Description = "Personal Phone Number")]
        [StringValue("Personal Phone")]
        PersonalPhone = 1,

        [Display(Name = "ProfessionalPhone", Description = "Professional Phone Number")]
        [StringValue("Professional Phone")]
        ProfessionalPhone = 2,

        [Display(Name = "PersonalMobile", Description = "Personal Mobile Number")]
        [StringValue("Personal Mobile")]
        PersonalMobile = 3,

        [Display(Name = "ProfessionalMobile", Description = "Professional Mobile Number")]
        [StringValue("Professional Mobile")]
        ProfessionalMobile = 4,

        [Display(Name = "TollFreePhone", Description = "Toll-Free Phone Number")]
        [StringValue("Toll-Free Phone")]
        TollFreePhone = 5,

        [Display(Name = "TollFreeMobile", Description = "Toll-Free Mobile Number")]
        [StringValue("Toll-Free Mobile")]
        TollFreeMobile = 6
    }
}