using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using MyNeighbourhood.Domain.Attributes;
using Newtonsoft.Json;

namespace MyNeighbourhood.Persistent.DbEnums
{
    [JsonConverter(typeof(StringEnumerator))]
    public enum TitleTypes
    {
        NotSpecified = 0,

        [Display(Name = "Doctor", Description = "Doctor")]
        [StringValue("Dr")]
        Doctor = 1,

        [Display(Name = "Honourable", Description = "Honourable")]
        [StringValue("Honourable")]
        Honourable = 2,

        [Display(Name = "HonourableMr", Description = "Honourable Mr")]
        [StringValue("Honourable Mr")]
        HonourableMr = 3,

        [Display(Name = "HonourableMrs", Description = "Honourable Mrs")]
        [StringValue("Honourable Mrs")]
        HonourableMrs = 4,

        [Display(Name = "HonourableMs", Description = "Honourable Ms")]
        [StringValue("Honourable Ms")]
        HonourableMs = 5,

        [Display(Name = "Lord", Description = "Lord")]
        [StringValue("Lord")]
        Lord = 6,

        [Display(Name = "Lieutenant", Description = "Lieutenant")]
        [StringValue("Lieutenant")]
        Lieutenant = 7,

        [Display(Name = "Major", Description = "Major")]
        [StringValue("Major")]
        Major = 8,

        [Display(Name = "Master", Description = "Master")]
        [StringValue("Master")]
        Master = 9,

        [Display(Name = "Minister", Description = "Minister")]
        [StringValue("Minister")]
        Minister = 10,

        [Display(Name = "Miss", Description = "Miss")]
        [StringValue("Miss")]
        Miss = 11,

        [Display(Name = "Mr", Description = "Mr")]
        [StringValue("Mr")]
        Mr = 12,

        [Display(Name = "Mrs", Description = "Mrs")]
        [StringValue("Mrs")]
        Mrs = 13,

        [Display(Name = "Ms", Description = "Ms")]
        [StringValue("Ms")]
        Ms = 14,

        [Display(Name = "Professor", Description = "Professor")]
        [StringValue("Professor")]
        Professor = 15,

        [Display(Name = "RightHonourable", Description = "Right Honourable")]
        [StringValue("Right Honourable")]
        RightHonourable = 16,

        [Display(Name = "SeniorConstable", Description = "Senior Constable")]
        [StringValue("Senior Constable")]
        SeniorConstable = 17,

        [Display(Name = "SeniorSergeant", Description = "Senior Sergeant")]
        [StringValue("Senior Sergeant")]
        SeniorSergeant = 18,

        [Display(Name = "Sergeant", Description = "Sergeant")]
        [StringValue("Sergeant")]
        Sergeant = 19,

        [Display(Name = "Sir", Description = "Sir")]
        [StringValue("Sir")]
        Sir = 20,
    }
}