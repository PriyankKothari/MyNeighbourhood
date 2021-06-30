
namespace Datacom.IRIS.Common
{
    public enum OverlayMode
    {
        /// <summary>
        ///    A mode that can be used when an overlay is opened for no particular transaction
        ///    type.
        /// </summary>
        Default,

        /// <summary>
        ///    A mode that can be used when an overlay is opened for a create transaction.
        /// </summary>
        Create,

        /// <summary>
        ///    A mode that can be used when an overlay is opened for an add transaction.
        /// </summary>
        Add,

        /// <summary>
        ///    A mode that can be used when an overlay is opened for an edit transaction.
        /// </summary>
        Edit,

        /// <summary>
        ///    A mode that can be used when an overlay is opened for a view transaction.
        /// </summary>
        View
    }

    public enum RequestContactType
    {
        Requestor,
        AllegedOffender
    }

    public enum IRISGridViewRowSize
    {
        Medium,
        Small,
        Large
    }

    public enum SubClassLevel
    {
        None = -1,
        Object = 0,
        One = 1,
        Two = 2,
        Three = 3
    }
    
    public enum SearchScope
    {
        [StringValue("Location Group")]
        LocationGroup,

        [StringValue("Location")]
        Location,

        [StringValue("Contact")]
        Contact,

        [StringValue("ContactGroup")]
        ContactGroup,

        [StringValue("_All")]
        All,

        [StringValue("Map Context")]
        MapContext,

        [StringValue("User")]
        User,

        [StringValue("Authorisation")]
        Authorisation,

        [StringValue("AuthorisationGroup")]
        AuthorisationGroup,

        [StringValue("Application")]
        Application,

        [StringValue("ConditionSchedule")]
        ConditionSchedule,

        [StringValue("Regime")]
        Regime,

        [StringValue("Programme")]
        Programme,

        [StringValue("DamRegister")]
        DamRegister,

        [StringValue("SelectedLandUseSite")]
        SelectedLandUseSite,

        [StringValue("Request")]
        Request,

        [StringValue("Property Data Valuation")]
        PropertyDataValuation
    }

    public enum DocumentGenerationContactOption
    {
        [StringValue("")]
        NotSelected,

        [StringValue("Not required")]
        NotRequired,

        [StringValue("For profiling only")]
        ForProfilingOnly,

        [StringValue("For profiling and addressing")]
        ForProfilingAndAddressing,

        [StringValue("For addressing")]
        ForAddressingOnly
    }

    public enum StringOperation
    {
        Contains,
        StartsWith,
        EndsWith
    }

    public enum ContactVerificationStatus
    {
        Verified,
        VerificationExpired,
        NotVerified
    }

    public enum AutoCompleteModule
    {
        Address,
        Species
    }
}
