using System;
using Datacom.IRIS.Common.Utils;
using System.ComponentModel;

namespace Datacom.IRIS.Common
{
    public struct Context
    {
        public const string IRIS_LOGGING_CONTEXT = "IRISLoggingContext";
        public const string IRIS_MESSAGE_LIST = "IRISMessageList";
    }

    public struct CacheConstants
    {
        public const string IrisErrorMessages = "IrisErrorMessages";
        public const string AppSettings = "AppSettings";
        public const string Permissions = "Permissions";
        public const string HelpLinks = "HelpLinks";
        public const string UserGroups = "UserGroups";
        public const string DataSecurity = "DataSecurity";
        public const string SpeciesSpeciesType = "SpeciesSpeciesType";

        public const string FunctionList_Functions = "FunctionList_Functions";
        public const string FunctionList_AdminFunctions = "FunctionList_AdminFunctions";
        public const string FunctionList_ObjectSubClass1and2s = "FunctionList_ObjectSubClass1and2s";

        public const string UsersWithFunctionsPermissions_Users = "UsersWithFunctionsPermissions_Users";
        public const string UsersWithFunctionsPermissions_UserGroups = "UsersWithFunctionsPermissions_UserGroups";
        public const string UsersWithFunctionsPermissions_ObjectSubClass1and2s = "UsersWithFunctionsPermissions_ObjectSubClass1and2s";

        public const string UsersWithFunctionsPermissions_Users_Admin = "UsersWithFunctionsPermissions_Users_Admin";
        public const string UsersWithFunctionsPermissions_UserGroups_Admin = "UsersWithFunctionsPermissions_UserGroups_Admin";

        public const string ActivityObjectRelationshipTypes_ObjectTypes = "ActivityObjectRelationshipTypes_ObjectTypes";
        public const string ActivityObjectRelationshipTypes_RelationshipTypes = "ActivityObjectRelationshipTypes_RelationshipTypes";

        public const string ReferenceDataCollections_Collections = "ReferenceDataCollections_Collections";
        public const string ReferenceDataCollections_Values = "ReferenceDataCollections_Values";

        public const string ObjectTypes_ReferenceDataValues = "ObjectTypes_ReferenceDataValues";
        public const string ObjectTypes_Attributes = "ObjectTypes_Attributes";

        public const string SubClasses_ReferenceDataValues = "SubClasses_ReferenceDataValues";
        public const string SubClasses_Attributes = "SubClasses_Attributes";

    }

    public struct ConfigSettings
    {
        public const string EnableReferenceDataCaching = "EnableReferenceDataCaching";
        public const string EnableCaching = "EnableCaching";
        public const string DisableUniqueWindowIdCheck = "DisableUniqueWindowIdCheck";

        public const string MiniProfilerInstrument = "MiniProfilerInstrument";
        public const string MiniProfilerLogToSql = "MiniProfilerLogToSql";
        public const string MiniProfilerDisplayOnScreen = "MiniProfilerDisplayOnScreen";

        public const string PassLoggedInUserCredentialToSSRS = "PassLoggedInUserCredentialToSSRS";
        public const string PassLoggedInUserCredentialToEDRMS = "PassLoggedInUserCredentialToEDRMS";
        public const string PassLoggedInUserCredentialToFinancials = "PassLoggedInUserCredentialToFinancials";
        public const string PassLoggedInUserCredentialToContactsIntegration = "PassLoggedInUserCredentialToContactsIntegration";

        public const string IsAdminSite = "IsAdminSite";

        public const string DatabaseCommandTimeout = "DatabaseCommandTimeout";
        public const string AsyncPostBackTimeout = "AsyncPostBackTimeout";


        public const string SphereIdentityProviderUrl = "SphereIdentityProviderUrl";
        public const string SphereIdentityProviderAppliesToUrl = "SphereIdentityProviderAppliesToUrl";    
        public const string SphereIdentityProviderUserName = "SphereIdentityProviderUserName";
        public const string SphereIdentityProviderPassword = "SphereIdentityProviderPassword";
        public const string SphereDownloadPdfUrl = "SphereDownloadPdfUrl";
        public const string SphereDownloadDocumentUrl = "SphereDownloadDocumentUrl";
        public const string SphereSubmitJobUrl = "SphereSubmitJobUrl";
        public const string SphereCancelJobUrl = "SphereCancelJobUrl";
        public const string SphereMoblieSummaryUrl = "SphereMoblieSummaryUrl";

        public const string OnlineServicesStatusUpdateUrl = "OnlineServicesStatusUpdateUrl";
    }

    public struct LoggingCategory
    {
        public const string ErrorCategory = "Error";
        public const string DebugCategory = "Debug";
        public const string ReleaseCategory = "Release";
        public const string TraceCategory = "Trace";
        public const string SqlCategory = "Sql";
        public const string InstrumentationCategory = "Instrumentation";
        public const string WorkflowCategory = "Workflow";
        public const string MiniProfilerCategory = "MiniProfiler";
    }

    public struct ErrorMessageCodes
    {
        public const string ACCESS_DENIED = "EM_AccessDenied";
        public const string ACCOUNT_DOES_NOT_EXISTS_IN_AD = "EM_AccountDoesNotExistInAD";
    }

    public struct ClientSideErrorMessages
    {
        public const string KEYWORD_REQUIRED = "Please enter one or more keywords.";
        public const string AT_LEAST_ONE_REQUIRED = "At least one item must be selected to link.";
        public const string NO_AUTH_FOUND = "No Authorisation records found that could be linked.";
        public const string NO_AVAILABLE_RELATIONSHIP_TYPE = "There are no available link relationship types.";
        public const string EXISTING_LINK_RELATIONSHIP_EXIST = "One or more items selected to be linked already has an existing current link relationship of the same type and new links have not been created for these.";
        public const string ACTIVITYRESOURCECONSENT_REQUESTED_ACTIVITYCLASS = "Activity Class is configured to be mandatory but is missing from the original Authorisation you have selected.  This needs to be entered for both the original Authorisation plus the new Application Activity just created.";

        public const string DocGenPreviewGenerationFailure = "An error occurred during document preview generation.";
        public const string DocGenGenerationFailure = "An error occurred during document generation.";
        public const string SessionTimeOutWarning = "Your session has timed out. The page has been refreshed. Please try again.";
        public const string InfoDialogFailure = "An error has occurred. The page will be refreshed. Please try again.";
        public const string SpatialSearchSessionTimeOutWarning = "Your session has timed out. You have been redirected to the Home page. Please try again.";
        public const string SpatialSearchExpiryWarning = "Your spatial search has expired. You have been redirected to the Home page. Please try again.";

        public const string SelectedContactsAreAlreadyAssociatedWithJFC = "One or more selected Contacts are already an Associated Contact.";

        public const string ManagementSiteDeleteValidationFailureObservations = "Cannot delete – Management Site contains one or more Observation/Remediation records or is associated with one or more Regime Activities.";
        public const string EnforcementDeleteValidationFailureOnActions = "Cannot delete Enforcement with one or more Enforcement Actions";
        public const string AuthorisationDeleteValidationFailureOnObservations = "Cannot delete – Authorisation contains one or more Observation or is associated with one or more Regime Activities.";
        public const string AuthorisationDeleteValidationFailureOnApplications = "Cannot delete Authorisation with one or more Applications created from the Authorisation.";
        public const string AuthorisationDeleteValidationFailureOnReplacementAuthorisations = "Cannot delete Authorisation that has been replaced by one or more DPA/PA Authorisations";
        public const string EnforcementActionDeleteValidationFailureOnMoj = "Cannot delete Enforcement Action that has been extracted as part of MOJ Infringement Export.";
    }

    public static class IrisUrl
    {
        private const string Path = "/";

        public readonly static string AdminFolder = "/admin/";
        public readonly static string AdminPath = GlobalUtils.GetAppSettingsValue("AdminURL") + AdminFolder;

        // Common Pages
        public const string Home = Path + "Default.aspx";
        public const string Notes = Path + "Notes.aspx";
        public const string Events = Path + "Events.aspx";
        public const string Search = Path + "SearchResults.aspx";
        public const string AdvancedSearch = Path + "AdvancedSearch.aspx";
        public const string SearchResultReport = Path + "SearchResultReport.aspx";
        public const string OtherLinks = Path + "OtherLinks.aspx";
        public const string ContactOtherLinks = Path + "ContactOtherLinks.aspx";
        public const string Documents = Path + "Documents.aspx";
        public const string Workflow = Path + "Workflow.aspx";
        public const string Reports = Path + "Reports.aspx";
        public const string Dashboard = Path + "Dashboard.aspx";
        public const string GlobalReports = Path + "GlobalReports.aspx";
        public const string ErrorRecordNotFound = Path + "ErrorRecordNotFound.aspx";
        public const string ErrorSecurity = Path + "ErrorSecurity.aspx";
        public const string ErrorDangerousRequest = Path + "ErrorDangerousRequest.aspx";


        //Admin Pages
        public readonly static string AdminDefault = AdminPath + "Default.aspx";
        public readonly static string AdminLogin = AdminPath + "Login.aspx";
        public readonly static string AdminViewCDFLists = AdminPath + "cdf/ViewLists.aspx";
        public readonly static string AdminViewCDFTaskLists = AdminPath + "cdf/ViewTaskCDFs.aspx";
        public readonly static string AdminRelationshipTypes = AdminPath + "relationshiptypes/RelationshipType.aspx";
        public readonly static string AdminApplicationSettings = AdminPath + "appsettings/AppSettings.aspx";

        public readonly static string AdminSecurityFunction = AdminPath + "security/Function.aspx";
        public readonly static string AdminSecurityFunctions = AdminPath + "security/Functions.aspx";

        public readonly static string AdminSecurityGroup = AdminPath + "security/Group.aspx";
        public readonly static string AdminSecurityGroups = AdminPath + "security/Groups.aspx";

        public readonly static string AdminUserProfile = AdminPath + "UserProfile.aspx";
        public readonly static string AdminSecurityUsers = AdminPath + "security/Users.aspx";

        public readonly static string AdminReferenceDataCollections = AdminPath + "referencedata/Collections.aspx";
        public readonly static string AdminReferenceDataCollection = AdminPath + "referencedata/Collection.aspx";

        public readonly static string AdminReports = AdminPath + "reports/Reports.aspx";
        public readonly static string AdminTemplates = AdminPath + "reports/Templates.aspx";

        public readonly static string AdminHelpLinks = AdminPath + "help/HelpLinks.aspx";
        public readonly static string AdminHelpContent = AdminPath + "help/HelpContent.aspx";

        public readonly static string AdminCalendars = AdminPath + "calendars/Calendars.aspx";
        public readonly static string AdminCalendarObjects = AdminPath + "calendars/CalendarObjects.aspx";
        public readonly static string AdminLibraryConditions = AdminPath + "LibraryConditions/LibraryConditions.aspx";
        public readonly static string AdminSpeciesData = AdminPath + "Species/Species.aspx";

        public readonly static string AdminWorkflowDefinitions = AdminPath + "Workflows/WorkflowDefinitions.aspx";
        public readonly static string AdminWorkflowDefinition = AdminPath + "Workflows/WorkflowDefinition.aspx";
        public readonly static string AdminWorkflowCallMappings = AdminPath + "Workflows/WorkflowCallMappings.aspx";
        public readonly static string AdminWorkflowInstances = AdminPath + "Workflows/WorkflowInstances.aspx";

        public readonly static string AdminHelp = AdminPath + "Help.aspx";

        public readonly static string AdminAddressValidationDataStreets = AdminPath + "AddressValidationData/Streets.aspx";
        public readonly static string AdminAddressValidationDataSuburbs = AdminPath + "AddressValidationData/Suburbs.aspx";
        public readonly static string AdminAddressValidationDataTownsCities = AdminPath + "AddressValidationData/TownsCities.aspx";

        public readonly static string AdminDVRDataImport = AdminPath + "PropertyData/ImportDVR.aspx";
        public readonly static string AdminDVRDataImportLog = AdminPath + "PropertyData/ImportDVRLog.aspx";
        public readonly static string AdminPropertyDataDetails = AdminPath + "PropertyData/PropertyDataDetails.aspx";

        public readonly static string AdminSurveys = AdminPath + "Surveys/Surveys.aspx";
        public readonly static string AdminSurveyCategories = AdminPath + "Surveys/SurveyCategories.aspx";
        public readonly static string AdminSurveyCategoryQuestions = AdminPath + "Surveys/SurveyCategoryQuestions.aspx";

        public readonly static string CustomLinks = AdminPath + "CustomLinks/CustomLinks.aspx";
        public readonly static string AdminEstimationRates = AdminPath + "EstimationRates/EstimationRates.aspx";
        public readonly static string ObjectInspectionTypeMapping = AdminPath + "ObjectInspectionTypeMapping/ObjectInspectionTypeMapping.aspx";

        public readonly static string AdminErrorRecordNotFound = AdminPath + "ErrorRecordNotFound.aspx";
        public readonly static string AdminErrorDangerousRequest = AdminPath + "ErrorDangerousRequest.aspx";
        public readonly static string AdminErrorSecurity = AdminPath + "ErrorSecurity.aspx";

        // User Pages
        public const string UserProfile = Path + "UserProfile.aspx";

        // Contact Pages
        public const string ContactDetails = Path + "ContactDetails.aspx";
        public const string ContactGroupDetails = Path + "ContactGroupDetails.aspx";
        public const string ContactGroupCustomFields = Path + "ContactGroupCustomFields.aspx";
        public const string ContactGroupMemberDetails = Path + "ContactGroupMemberDetails.aspx";
        public const string JFCContactDetails = Path + "JointFinancialCustomerDetails.aspx";
        
        // Security Pages
        public const string Security = Path + "Security.aspx";

        // Location Pages
        public const string LocationGroupDetails = Path + "LocationGroupDetails.aspx";
        public const string LocationMap = Path + "Map.aspx";
        public const string LocationDetails = Path + "LocationDetails.aspx";

        // Applications Pages
        public const string ApplicationDetails = Path + "ApplicationDetails.aspx";
        public const string ApplicationOtherInformation = Path + "ApplicationOtherInformation.aspx";
        public const string ApplicationContacts = Path + "ApplicationContacts.aspx";
        public const string Contacts = Path + "Contacts.aspx";

        // Acivity Pages
        public const string ActivityDetails = Path + "ActivityDetails.aspx";
        public const string ActivityOtherInformation = Path + "ActivityOtherInformation.aspx";
        public const string ActivityConditions = Path + "ActivityConditions.aspx";

        // Authorisation Pages
        public const string AuthorisationDetails = Path + "AuthorisationDetails.aspx";
        public const string AuthorisationOtherInformation = Path + "AuthorisationOtherInformation.aspx";
        public const string AuthorisationConditions = Path + "AuthorisationConditions.aspx";
        public const string AuthorisationContacts = Path + "AuthorisationContacts.aspx";
        public const string AuthorisationObservations = Path + "AuthorisationObservations.aspx";

        // Authorisation Group Pages
        public const string AuthorisationGroupDetails = Path + "AuthorisationGroupDetails.aspx";

        // Regime Pages
        public const string ProgrammeDetails = Path + "ProgrammeDetails.aspx";
        public const string RegimeDetails = Path + "RegimeDetails.aspx";
        public const string RegimeActivities = Path + "RegimeActivities.aspx";
        public const string RegimeActivityDetails = Path + "RegimeActivityDetails.aspx";
        public const string ObservationMngtLines = Path + "ObservationMngtLines.aspx";
        public const string RegimeActivityOtherInformation = Path + "RegimeActivityOtherInformation.aspx";
        public const string RegimeActivityObservations = Path + "RegimeActivityObservations.aspx";
        public const string RegimeActivityObservationsRemediation = Path + "RegimeActivityObservationsRemediation.aspx";
        public const string RegimeActivitySampleResults = Path + "RegimeActivitySampleResults.aspx";
        public const string ObservationDetails = Path + "ObservationDetails.aspx";
        public const string ObservationSurvey = Path + "ObservationSurvey.aspx";
        public const string SampleResultDetails = Path + "SampleResultDetails.aspx";

        // Requests Pages
        public const string RequestDetails = Path + "RequestDetails.aspx";
        public const string RequestOtherInformation = Path + "RequestOtherInformation.aspx";
        public const string RequestObservations = Path + "RequestObservations.aspx";

        // Ad Hoc Data Pages
        public const string AdHocDataDetails = Path + "AdHocDataDetails.aspx";


        //Register Pages
        public const string DamRegisterDetails = Path + "DamRegisterDetails.aspx";
        public const string GeneralRegisterDetails = Path + "GeneralRegisterDetails.aspx";

        //Management Site Pages
        public const string ManagementSiteDetails = Path + "ManagementSiteDetails.aspx";
        public const string ManagementSiteObservationsRemediations = Path + "ManagementSiteObservationsRemediations.aspx";
        public const string ManagementSiteOtherInformation = Path + "ManagementSiteOtherInformation.aspx";

        //Select Land Use Pages
        public const string SelectedLandUseSiteDetails = Path + "SelectedLandUseSiteDetails.aspx";

        //Condition pages
        public const string ConditionDetails = Path + "ConditionDetails.aspx";
        public const string ConditionScheduleDetails = Path + "ConditionScheduleDetails.aspx";

        // Document Preview Page
        public const string DocumentPreview = Path + "DocumentPreview.aspx";

        // Help Pages
        public const string Help = Path + "Help.aspx";

        // Financial Pages
        public const string FinancialsPage = "Financials.aspx";
        public const string Financials = Path + FinancialsPage;

        // Other Information Pages
        public const string OtherInformation = Path + "OtherInformation.aspx";

        // Enforcement Pages
        public const string EnforcementDetails = Path + "EnforcementDetails.aspx";
        public const string EnforcementActions = Path + "EnforcementActions.aspx";
        public const string EnforcementActionDetails = Path + "EnforcementActionDetails.aspx";
        public const string EnforcementAllegedOffenceDetails = Path + "EnforcementAllegedOffenceDetails.aspx";
        public const string EnforcementActionProsecutionDefendantDetails = Path + "EnforcementActionProsecutionDefendantDetails.aspx";
        public const string EnforcementMOJDownload = Path + "EnforcementMOJDownload.aspx";

        // Remediation Page
        public const string RemediationDetails = Path + "RemediationDetails.aspx";

    }


    public struct DisplayConstants
    {
        public const string NonCurrentReferenceData = "inactive";
        public const string NonCurrent = "(NON-CURRENT) ";
        public const string PersonIsDeceased = "(DECEASED) ";

    }

    public struct QuestionTypes
    {
        /// <summary>
        ///    Either a dropdown or a radio button list
        /// </summary>
        public const string OptionField = "OPTION";
        public const string Checkbox = "CHECKBOX";
        public const string TextField = "TEXT";
        public const string MultiLine = "TEXT";
        public const string TwoChoiceRadioButton = "BOOL";
        public const string DateField = "DATE";
        public const string TimeField = "TIME";
        public const string Number = "NUMBER";
        public const string Amount = "AMOUNT";

        public struct SubTypes
        {
            public const string DropDown = "DDL";
            public const string RadioButtonListVertical = "VRB";
            public const string RadioButtonListHorizontal = "HRB";
        }
    }

    public struct FucntionExpression
    {
        public const string Today = "#TODAY#";
    }

    public struct SearchConstants
    {
        public const int DEFAULT_RESULTS_PERPAGE = 10;

        public const string SEARCH_MODE_DEFAULT = "Default";
        public const string SEARCH_MODE_LINK = "SearchAndLink";
        public const string SEARCH_MODE_DUPLICATES = "DuplicateSearch";
        public const string SEARCH_MODE_ADVANCE = "AdvancedSearch";
        public const string SEARCH_MODE_LINK_ADVANCE = "SearchAndLinkAdvancedSearch";

        public const string PHONETIC_SEARCH_CONNECTION_STRING = "PhoneticSearchDB";
    }

    public struct ReferenceDataCollectionCode
    {
        public const string IrisObjects = "ObjectType";
        public const string IrisObjectsSubClass1 = "SubClassification1";
        public const string IrisObjectsSubClass2 = "SubClassification2";
        public const string IrisObjectsSubClass3 = "SubClassification3";
        public const string Titles = "Titles";
        public const string UnitTypes = "UnitTypes";
        public const string FloorTypes = "FloorTypes";
        public const string StreetTypes = "StreetTypes";
        public const string CountryCodes = "CountryCodes";
        public const string Genders = "Genders";
        public const string NameTypes = "NameTypes";
        public const string StreetDirections = "StreetDirections";
        public const string EmailTypes = "EmailTypes";
        public const string WebsiteTypes = "WebsiteTypes";
        public const string PhoneTypes = "PhoneTypes";
        public const string OrganisationTypes = "OrganisationTypes";
        public const string OrganisationStatuses = "OrganisationStatuses";
        public const string LocationGroupStatuses = "LocationGroupStatuses";
        public const string ContactAddressTypes = "ContactAddressTypes";
        public const string DeliveryServiceTypes = "DeliveryServiceTypes";
        public const string DelegationActs = "DelegationActs";
        public const string DelegationSections = "DelegationSections";
        public const string Reliability = "Reliability";
        public const string IdentifierContexts = "IdentifierContexts";
        public const string IdentifierContextBySubClassification1 = "IdentifierContextBySubClassification1";
        public const string IdentifierContextBySubClassification2 = "IdentifierContextBySubClassification2";
        public const string IdentifierContextBySubClassification3 = "IdentifierContextBySubClassification3";
        public const string CaptureMethods = "CaptureMethods";
        public const string Datum = "Datum";
        public const string DocumentTypes = "DocumentTypes";
        public const string DocumentSubtypes = "DocumentSubtypes";
        public const string GlobalFolders = "GlobalFolders";
        public const string InstanceFolders = "InstanceFolders";
        public const string InteractionTypes = "InteractionTypes";
        public const string TaskTypes = "TaskTypes";
        public const string IndustryPurposes = "IndustryPurposes";
        public const string ApplicationIndustry = "ApplicationIndustry";
        public const string ApplicationPurpose = "ApplicationPurpose";
        public const string ExpectedLifetimes = "ExpectedLifetimes";
        public const string DurationTypes = "DurationTypes";
        public const string ApplicationDecisions = "ApplicationDecisions";
        public const string ApplicationTypes = "ApplicationTypes";
        public const string ApplicationStatuses = "ApplicationStatuses";
        public const string ApplicationPurposes = "ApplicationPurposes";
        public const string ApplicationOnHoldReasons = "ApplicationOnHoldReasons";
        public const string ThirdPartyInvolvementResponse = "ThirdPartyInvolvementResponse";
        public const string ThirdPartyInvolvementWithToBeHeardType = "ThirdPartyInvolvementWithToBeHeardType";
        public const string ThirdPartyInvolvementStatus = "ThirdPartyInvolvementStatus";
        public const string DecisionMakers = "DecisionMakers";
        public const string ActivityClasses = "ActivityClasses";
        public const string S124Statuses = "S124Statuses";
        public const string NotificationResponsibility = "NotificationResponsibility";
        public const string SpecialEventTypes = "SpecialEventType";
        public const string AuthorisationStatuses = "AuthorisationStatuses";
        public const string AuthorisationAnnualChargeTypes = "AuthorisationAnnualChargeType";
        public const string AuthorisationChargeFinancialYear = "AuthorisationChargeFinancialYear";
        public const string AuthorisationChargeFinancialYearPeriod = "AuthorisationChargeFinancialYearPeriod";
        public const string WorkflowCallContexts = "WorkflowCallContext";
        public const string Plans = "Plans";
        public const string Rules = "Rules";
        public const string Policies = "Policies";
        public const string ConditionTypes = "ConditionTypes";
        public const string ActivityOutcomeStatuses = "ActivityOutcomeStatuses";
        public const string ActivityOutcomes = "ActivityOutcomes";
        public const string TimeframeTypes = "TimeFrameTypes";
        public const string TimeframeCategories = "TimeFrameCategory";
        public const string ParameterUnitTypes = "ParameterUnitTypes";
        public const string ParameterTypes = "ParameterTypes";
        public const string CallInInitiators = "CallInInitiators";
        public const string DirectReferralRequestOutcomes = "DirectReferralRequestOutcomes";
        public const string NotificationDecisionOutcomes = "NotificationDecisionOutcomes";
        public const string Objectors = "Objectors";
        public const string ObjectingTos = "ObjectingTos";
        public const string ObjectionDecisions = "ObjectionDecisions";
        public const string ObjectionDecisionMakers = "ObjectionDecisionMakers";
        public const string GrantPrimaryClasses = "GrantPrimaryClasses";
        public const string GrantSecondaryClasses = "GrantSecondaryClasses";
        public const string GrantTerritorialAuthority = "GrantTerritorialAuthority";
        public const string Appellants = "Appellants";
        public const string AppealDecisions = "AppealDecisions";
        public const string AppealDecisionProcesses = "AppealDecisionProcesses";
        public const string TimeCodes = "TimeCodes";
        public const string DomicileRegionalCouncils = "DomicileRegionalCouncils";
        public const string RegimeStatus = "RegimeStatus";
        public const string RegimeActivityStatus = "RegimeActivityStatus";
        public const string RegimeActivityScheduleType = "RegimeActivityScheduleType";
        public const string ObservationComplianceStatus = "ObservationComplianceStatus";
        public const string ObservationNonComplianceRisk = "ObservationNonComplianceRisk";
        public const string SelectedLandUseSiteMonitoringFunding = "SelectedLandUseSiteMonitoringFunding";
        public const string SchedulePeriods = "SchedulePeriods";
        public const string Days = "Days";
        public const string LabourPosition = "LabourPosition";
        public const string Resources = "Resources";
        public const string Timezones = "Timezone";
        public const string Observer = "Observer";
        public const string Rainfall = "Rainfall";
        public const string WindDirection = "WindDirection";
        public const string WindStrength = "WindStrength";
        public const string CloudCover = "CloudCover";
        public const string AirTemperature = "AirTemperature";
        public const string LineMonitoringTargetSpecies = "LineMonitoringTargetSpecies";
        public const string LineMonitoringMethods = "LineMonitoringMethods";
        public const string LineMonitoringPhases = "LineMonitoringPhases";
        public const string Habitats = "Habitats";
        public const string Stratum = "Stratum";
        public const string ObservationSelectedLandUseIndicators = "ObservationSelectedLandUseIndicators";
        public const string SpeciesTypes = "SpeciesTypes";
        public const string ObservationMethods = "ObservationMethods";
        public const string ObservationClassifiers = "ObservationClassifiers";
        public const string ObservationAbundances = "ObservationAbundances";
        public const string ProgrammePriorities = "ProgrammePriorities";
        public const string ProgrammeStatuses = "ProgrammeStatuses";
        public const string FurtherActionConductors = "FurtherActionConductors";
        public const string DamReportTypes = "DamReportTypes";
        public const string MonitoringFurtherActionType = "MonitoringFurtherActionType";
        public const string RequestStatus = "RequestStatus";
        public const string RequestType = "RequestType";
        public const string RequestContactMethod = "RequestContactMethod";
        public const string RequestPriority = "RequestPriority";
        public const string RequestorIncidentCause = "RequestorIncidentCause";
        public const string RequestorIncidentEffect = "RequestorIncidentEffect";
        public const string Resolution = "Resolution";
        public const string ReferredTo = "ReferredTo";
        public const string ResponseMethod = "ResponseMethod";
        public const string RequestorSatisfactionLevel = "RequestorSatisfactionLevel";
        public const string ResponseIncidentCause = "ResponseIncidentCause";
        public const string ResponseIncidentEffect = "ResponseIncidentEffect";
        public const string IncidentSource = "IncidentSource";
        public const string AllegedSeverity = "AllegedSeverity";
        public const string AssessedSeverity = "AssessedSeverity";
        public const string RequestSource = "RequestSource";
        public const string RequestFurtherActionConductors = "RequestFurtherActionConductors";
        public const string RequestFurtherActionTypes = "RequestFurtherActionTypes";
        public const string RequestTransportRoute = "RequestTransportRoute";
        public const string RequestTypeIncidentHazardousSubstanceType = "RequestIncidentHazardousSubstanceType";
        public const string ResourceType = "ResourceType";
        public const string RequestBreach = "RequestBreach";
        public const string DamRegisterExempt = "Exempt";
        public const string DamRegisterHeritage = "Heritage";
        public const string DamRegisterDamUse = "DamUse";
        public const string DamRegisterPotentialImpactClassification = "PotentialImpactClassification";
        public const string DamRegisterDamMaterial = "DamMaterial";
        public const string DamRegisterSpillwayMaterial = "SpillwayMaterial";
        public const string DamRegisterStatuses = "DamRegisterStatus";
        public const string GeneralRegisterStatuses = "GeneralRegisterStatus";
        public const string ContactGroupStatuses = "ContactGroupStatuses";
        public const string ContactGroupTypes = "ContactGroupTypes";
        public const string PropertyDataDVRImportStatuses = "PropertyDataDVRImportStatuses";
        public const string OriginatedFrom = "OriginatedFrom";
	    public const string OffenceDateType = "OffenceDateType";
	    public const string OffenceSection = "OffenceSection";
        public const string NatureOfOffence = "NatureOfOffence";
        public const string ActionSection = "ActionSection";
        public const string EnforcementActionToBeTaken = "ActionToBeTaken";
        public const string EnforcementNoActionReason = "NoActionReason";
        public const string EnforcementActionDocumentServed = "DocumentServed";
        public const string ServedMethod = "ServedMethod";
        public const string CourtOfHearing = "CourtOfHearing";
        public const string EnforcementOrderDecision = "EnforcementOrderDecision";
        public const string EnforcementActionStatus = "EnforcementActionStatus";
        public const string EnforcementActionProsecutionPlea = "Plea";
        public const string EnforcementActionProsecutionChargeOutcome = "ChargeOutcome";
        public const string EnforcementActionProsecutionChargeOutcomeStatus = "ChargeOutcomeStatus";
        public const string EnforcementActionProsecutionFinalSentencingJudge = "FinalSentencingJudge";
        public const string EnforcementActionProsecutionSentence = "Sentence";

		public const string ManagementSiteHabitatType = "HabitatType";
        public const string ManagementSiteLegalProtection = "LegalProtection";
        public const string ManagementSiteManagementIssueType = "ManagementIssueType";
        public const string ManagementSiteLandTenure = "LandTenure";
        public const string ManagementSiteEcologicalFeatureType = "EcologicalFeatureType";
        public const string ManagementSiteIndustryPurpose = "Industry/Purpose";
        public const string ManagementSiteSituation = "Situation";
        public const string ManagementSiteStatus = "ManagementSiteStatus";
        public const string ManagementSiteClassificationType = "ManagementSiteClassificationType";
        public const string ManagementSiteClassification = "ManagementSiteClassification";
        public const string SitePlanType = "SitePlanType";
        public const string SitePlanStatus = "SitePlanStatus";
        public const string SelectedLandUseSiteClassifications = "SelectedLandUseSiteClassificationTypes";
        public const string SelectedLandUseSiteClassificationContexts = "SelectedLandUseSiteClassificationContexts";
        public const string SelectedLandUseSiteHAILGroups = "SelectedLandUseSiteHAILGroups";
        public const string SelectedLandUseSiteHAILCategories = "SelectedLandUseSiteHAILCategories";
        public const string SelectedLandUseSiteRiskRatings = "SelectedLandUseSiteRiskRatings";
        public const string SelectedLandUseSiteContaminantTypes = "SelectedLandUseSiteContaminantTypes";
        public const string SelectedLandUseSiteContaminants = "SelectedLandUseSiteContaminants";
        public const string SelectedLandUseSitePresenceTypes = "SelectedLandUseSitePresenceTypes";
        public const string SelectedLandUseSiteStatuses = "SelectedLandUseSiteStatuses";
        public const string ThreatSpeciesStatus = "ThreatSpeciesStatus";

        public const string RemediationType = "RemediationType";
        public const string RemediationSubtype = "RemediationSubtype";
        public const string RemediationActionBy = "RemediationActionBy";
        public const string RemediationContractor = "RemediationContractor";
        public const string RemediationStatus = "RemediationStatus";
        public const string RemediationFunding = "RemediationFunding";
        public const string RemediationTreatmentToxicityUnit = "RemediationTreatmentToxicityUnit";
        public const string RemediationTrapType = "RemediationTrapType";
        public const string RemediationPlantingPurpose = "RemediationPlantingPurpose";
        public const string RemediationPlantingSpecies = "RemediationPlantingSpecies";
        public const string RemediationStructurePurpose = "RemediationStructurePurpose";
        public const string RemediationShootingPest = "RemediationShootingPest";
        public const string RemediationTrappingPest = "RemediationTrappingPest";
        public const string RemediationPhysicalControlMethod = "RemediationPhysicalControlMethod";
        public const string RemediationPhysicalControlMethodQuantityUnit = "RemediationPhysicalControlMethodQuantityUnit";
        public const string RemediationTreatmentAgent = "RemediationTreatmentAgent";
        public const string RemediationTreatmentAgentUnit = "RemediationTreatmentAgentUnit";
        public const string RemediationTreatmentApplicationMethod = "RemediationTreatmentApplicationMethod";
        public const string RemediationTreatmentApplicationMethodUnit = "RemediationTreatmentApplicationMethodUnit";
        public const string RemediationStructureType = "RemediationStructureType";

        public const string FinancialYear = "FinancialYear";
        public const string EquipmentMaterialType = "EquipmentMaterialType";
        public const string EquipmentUnitType = "EquipmentUnitType";
        public const string FieldCategory = "FieldCategory";
        public const string EstimationRateCategoryTypes = "EstimationRateCategoryTypes";
        public const string EstimationRateForLabourCategory = "EstimationRateForLabourCategory";
        public const string ProgrammeRating = "ProgrammeRating";

        public const string ResourceUnitType = "ResourceUnitType";
        public const string LabourPositionType = "LabourPositionType";
        public const string RelatedAuthorisationRelationship = "RelatedAuthorisationRelationship";
    }

    public struct ReferenceDataValueCodes
    {
        public struct ObjectType
        {
            public const string Application = "Application";
            public const string Activity = "Activity";
            public const string LocationGroup = "LocationGroup";
            public const string User = "User";
            public const string Location = "Location";
            public const string Contact = "Contact";
            public const string Authorisation = "Authorisation";
            public const string ConditionSchedule = "ConditionSchedule";
            public const string AuthorisationGroup = "AuthorisationGroup";
            public const string Regime = "Regime";
            [Description("Regime Activity")]
            public const string RegimeActivity = "RegimeActivity";
            public const string Observation = "Observation";
            public const string DamRegister = "DamRegister";
            public const string GeneralRegister = "GeneralRegister";
            public const string SelectedLandUseSite = "SelectedLandUseSite";
            public const string Programme = "Programme";
            public const string SampleResult = "SampleResult";
            public const string MapContext = "MapContext";
            public const string Request = "Request";
            public const string ContactGroup = "ContactGroup";
            public const string ContactGroupMember = "ContactGroupMember";
            public const string AdHocData = "AdHocData";
            public const string Enforcement = "Enforcement";
            public const string EnforcementAllegedOffence = "EnforcementAllegedOffence";
            public const string EnforcementAction = "EnforcementAction";
            public const string EnforcementAllegedOffenceOffender = "EnforcementAllegedOffenceOffender";
            public const string EnforcementActionProsecutionDefendant = "EnforcementActionProsecutionDefendant";

            public const string PropertyDataValuation = "PropertyDataValuation";
            [Description("Management Site")]
            public const string ManagementSite = "ManagementSite";

            public const string JointFinancialCustomer = "JointFinancialCustomer";
            public const string Remediation = "Remediation";
            public const string ObjectInspectionTypeMapping = "ObjectInspectionTypeMapping";
        }

	    public struct FieldCategory
	    {
		    public const string MappedField = "MappedField";
		    public const string CDFField = "CDFField";
	    }

        public struct EstimationRateCategoryTypes
        {
            public const string EquipmentAndMaterials = "EquipmentMaterialType";
            public const string Labour = "LabourPositionType";
        }

        public struct EstimationRateForLabourCategory
        {
            public const string Hour = "Hour";
        }

        public struct IndustryPurpose
        {
            public const string NotYetDetermined = "NOTYETDETERMINED";
        }

        public struct ActivityClass
        {
            public const string NotYetDetermined = "NOTYETDETERMINED";
        }

        public struct ContactType
        {
            public const string Person = "Person";
            public const string Organisation = "Organisation";
        }

        public struct JointFinancialCustomerSubClass1Code
        {
            public const string JointFinancialCustomer = "JointFinancialCustomer";
        }

        public struct EnforcementSubClass1Code
        {
            public const string Enforcement = "Enforcement";
        }

        public struct ParameterUnitTypeREFCode
        {
            public const string Date = "Date";
        }

        public struct ProgrammeSubClass1Code
        {
            public const string Programme = "Programme";
        }

        public struct LocationType
        {
            public const string Spatial = "Spatial";
            public const string Textual = "Textual";
        }

        public struct NameType
        {
            public const string FullName  = "FullName";
            public const string LegalName = "LegalName";
        }

        public struct ApplicationType
        {
            public const string ResourceConsent = "ResourceConsent";
            public const string BuildingConsent = "BuildingConsent";
            public const string CertificateOfCompliance = "CertificateOfCompliance";
            public const string SpecialEvent = "SpecialEvent";
            public const string Grant = "Grant";
            public const string General = "General";
        }

        public struct ActivityType
        {
            public const string ResourceConsent = "ResourceConsent";
            public const string BuildingConsent = "BuildingConsent";
            public const string CertificateOfCompliance = "CertificateOfCompliance";
            public const string SpecialEvent = "SpecialEvent";
            public const string Grant = "Grant";
            public const string General = "General";
        }

        public struct RegimeType
        {
            public const string EnvironmentalMonitoring = "EnvironmentalMonitoring";
            public const string Biodiversity = "Biodiversity";
            public const string Biosecurity = "Biosecurity";
            public const string ComplianceMonitoring = "ComplianceMonitoring";
            public const string DamMonitoring = "DamMonitoring";
            public const string LandManagement = "LandManagement";
            public const string SelectedLandUseSiteMonitoring = "SelectedLandUseSiteMonitoring";
        }

        public struct RegimeActivityType
        {
            public const string EnvironmentalMonitoring = RegimeType.EnvironmentalMonitoring;
            public const string Biodiversity = RegimeType.Biodiversity;
            public const string Biosecurity = RegimeType.Biosecurity;
            public const string ComplianceMonitoring = RegimeType.ComplianceMonitoring;
            public const string DamMonitoring = RegimeType.DamMonitoring;
            public const string LandManagement = RegimeType.LandManagement;
            public const string SelectedLandUseSiteMonitoring = RegimeType.SelectedLandUseSiteMonitoring;
        }

        public struct RegimeActivityScheduleType
        {
            public const string OneOff = "OneOff";
            public const string Recurring = "Recurring";
            public const string Unscheduled = "Unscheduled";
        }

        public struct TimeframeType
        {
            public const string Extension = "Extension";
            public const string Processing = "Processing";
        }

        /// <summary>
        /// Filtered by Application Type
        /// </summary>
        public struct ApplicationDecision
        {
            public struct ResourceConsent
            {
                public const string NotProceeding = "NotProceeding";
                public const string ReturnedIncomplete = "ReturnedIncomplete";
                public const string OneOrMoreActivitiesGranted = "OneOrMoreActivitiesGranted";
                public const string AllActivitiesDeclined = "AllActivitiesDeclined";
                public const string AllActivitiesWithdrawn = "AllActivitiesWithdrawn";
                public const string ReturnedS91A = "ReturnedS91A";
                public const string Issued = "Issued";
                public const string Declined = "Declined";
                public const string Withdrawn = "Withdrawn";
            }

            public struct BuildingConsent
            {
                public const string Granted = "Granted";
                public const string NotProceeding = "NotProceeding";
                public const string Refused = "Refused";
                public const string Withdrawn = "Withdrawn";
            }

            public struct SpecialEvent
            {
                public const string Approved = "Approved";
                public const string Declined = "Declined";
                public const string Withdrawn = "Withdrawn";
            }

            public struct Grant
            {
                public const string Approved = "Approved";
                public const string Declined = "Declined";
                public const string Withdrawn = "Withdrawn";
            }

            public struct General
            {
                public const string Approved = "Approved";
                public const string Declined = "Declined";
                public const string Withdrawn = "Withdrawn";
            }
        }

        public struct ApplicationPurpose
        {
            public const string Change = "Change";
            public const string ExtensionOfLapse = "ExtensionOfLapse";
            public const string Replace = "Replace";
            public const string Review = "Review";
            public const string Grant = "Grant";
            public const string New = "New";
            public const string Amendment = "Amendment";
            public const string CertificateOfAcceptance = "CertificateOfAcceptance";
            public const string CodeComplianceCertificate = "CodeComplianceCertificate";
            public const string Certificate = "Certificate";
        }

        #region Status constants

        public struct ApplicationStatus
        {
            public const string PreApplication = "PreApplication";
            public const string Lodged = "Lodged";
            public const string Active = "Active";
            public const string ActiveSubmissions = "ActiveSubmissions";
            public const string ActiveAwaitingHearing = "ActiveAwaitingHearing";
            public const string ActiveAwaitingDecision = "ActiveAwaitingDecision";
            public const string DecisionMade = "DecisionMade";
            public const string DecisionServed = "DecisionServed";

            public const string OnHold = "OnHold";
        }

        public struct RequestStatus
        {
            public const string Open = "Open";
            public const string InitialResponse = "InitialResponse";
            public const string Closed = "Closed";
            public const string Received = "Received";
            public const string Reopened = "Reopened";
            public const string Cancelled = "Cancelled";
        }

        public struct RegimeActivityStatus
        {
            public const string Scheduled = "Scheduled";
            public const string InProgress = "InProgress";
            public const string Complete = "Complete";
            public const string NotUndertaken = "NotUndertaken";
            public const string Cancelled = "Cancelled";
            public const string Intermittent = "Intermittent";
            public const string Provisional = "Provisional";
        }

        public struct RegimeStatus
        {
            public const string Active = "Active";
            public const string Intermittent = "Intermittent";
            public const string Provisional = "Provisional";
            public const string Draft = "Draft";
        }

        public struct EnforcementActionStatus
        {
            public const string Closed = "Closed";
        }

        public struct EnforcementActionInfringementNoticeStatus
        {
            public const string Initiated = "Initiated";
            public const string Active = "Active";
            public const string Closed = "Closed";
        }

        public struct OrganisationStatus
        {
            public const string Active = "Active";
        }

        public struct ManagementSiteStatus
        {
            public const string Active = "Active";
        }

        #endregion

        public struct TaskType
        {
            public const string WorkflowTask = "WorkflowTask";
            public const string UserTask = "UserTask";
        }

        public struct IdentifierContexts
        {
            public const string OnlineServicesReference = "OnlineServicesReference";
            public const string MobileCaptureReference = "MobileCaptureReference";
            public const string DuplicateContactID = "DuplicateContactID";
        }

        public struct ActivityOutcomeStatuses
        {
            public const string Stands = "Stands";
            public const string UnderAppeal = "UnderAppeal";
            public const string Overturned = "Overturned";
            public const string Upheld = "Upheld";
            public const string NotUpheld = "NotUpheld";
        }

        public struct ActivityOutcomes
        {
            public const string Granted = "Granted";
            public const string Declined = "Declined";
            public const string Discontinued = "Discontinued";
            public const string Issued = "Issued";
            public const string Refused = "Refused";
            public const string Withdrawn = "Withdrawn";
            public const string DeemedPermittedActivity = "DeemedPermittedActivity";
        }

        public struct AuthorisationTypes
        {
            public const string PermittedActivity = "PermittedActivity";
            public const string DeemedPermittedActivity = "DeemedPermittedActivity";
        }

        public struct AuthorisationTypesDisplay
        {
            public const string PermittedActivity = "Permitted Activity";
            public const string DeemedPermittedActivity = "Deemed Permitted Activity";
        }

        public struct AuthorisationStatuses
        {
            public const string Current = "Current";
            public const string NotYetCommence = "NotYetCommence";
            public const string NotYetCommencedUnderObjection = "NotYetCommencedUnderObjection";
            public const string CurrentUnderObjection = "CurrentUnderObjection";
            public const string Lapsed = "Lapsed";
            public const string Cancelled = "Cancelled";
            public const string CancelledUnderObjection = "CancelledUnderObjection";
            public const string Expired = "Expired";
            public const string ExpiredReplacedDeemedPermittedActivity = "ExpiredReplacedDeemedPermittedActivity";
            public const string ExpiredReplacedPermittedActivity = "ExpiredReplacedPermittedActivity";
            public const string ExpiredS124Protection = "ExpiredS124Protection";
            public const string Superseded = "Superseded";
            public const string Surrendered = "Surrendered";
        }

        public struct RelatedAuthorisationRelation
        {
            public const string Previous = "Previous";
            public const string Replace = "Replace";
        }

        public struct ExpectedLifeTimes
        {
            public const string Limited = "Limited";
        }

        public struct DurationTypes
        {
            public const string Years = "Years";
        }

        public struct SpecialEventTypes
        {
            public const string Granted = "Granted";
            public const string Requested = "Requested";
        }

        public struct WorkflowDisplayNames
        {
            public const string S37ExtendDecisionTimeframe = "S.37 Extend Decision Timeframe";
            public const string S91ASuspension = "S.91A Suspension";
            public const string S91DNonNotifiedSuspension = "S.91D Non Notfied Suspension";
            public const string S91FurtherConsentsRequired = "S.91 Further Consents Required";
            public const string S921InformationRequested = "S.92(1) Information Requested";
            public const string S922CommissionReport = "S.92(2) Commission Report Required";
            public const string S95EAffectedPersonsApproval = "S.95E Affected Persons Approval";
            public const string S95EAffectedPersonsApprovalSideline = "S.95E Affected Persons Approval - Sideline";
            public const string Cancellation = "Cancellation";
            public const string Surrender = "Surrender";
            public const string TransferToAuthorisationHolder = "Transfer to Authorisation Holder";
            public const string ProsecutionOutcome = "Prosecution Outcome";
            public const string TankPull = "Tank Pull";
            public const string RegimeActivity = "Regime Activity";
            public const string WithdrawnActionsMenu = "Withdrawn (from Actions Menu)";
        }

        public struct WorkflowCallContexts
        {
            public const string LodgedApplicationCreated = "LodgedApplicationCreated";
            public const string BuildingConsent = "BuildingConsent";
            public const string ResourceConsent = "ResourceConsent";
            public const string PreApplication = "PreApplication";
            public const string Surrender = "Surrender";
            public const string Cancellation = "Cancellation";
            public const string TransferToNewAuthorisationHolder = "TransferToNewAuthorisationHolder";
            public const string AuthorisationCreated = "AuthorisationCreated"; 
            public const string BuildingConsentLapse = "BuildingConsentLapse";
            public const string S37ExtendDecisionTimeframe = "S37ExtendDecisionTimeframe";
            public const string S91FurtherConsentsRequired = "S91FurtherConsentsRequired";
            public const string S91ASuspension = "S91ASuspension";
            public const string S91DNonNotifiedSuspension = "S91DNonNotifiedSuspension";
            public const string S921InformationRequested = "S92(1)InformationRequested";
            public const string S922CommissionReport = "S92(2)CommissionReport";
            public const string S95EAffectedPersonsApproval = "S95EAffectedPersonsApproval";
            public const string S95EAffectedPersonsApprovalSideline = "S95EAffectedPersonsApprovalSideline";
            public const string BuildingConsentFurtherInformationRequired = "BuildingConsentFurtherInformationRequired";
            public const string Objections = "Objections";
            public const string Appeals = "Appeals";
            public const string AuthorisationRenewalAdvice = "AuthorisationRenewalAdvice";
            public const string RegimeActivity = "RegimeActivity";
            public const string RequestLogged = "RequestLogged";
            public const string RequestForService = "RequestForService";
            public const string ServiceComplaint = "ServiceComplaint";
            public const string Incident = "Incident";
            public const string AssignOfficerResponsible = "AssignOfficerResponsible";
            public const string EnforcementActionCreated = "EnforcementActionCreated";
            public const string RequestLoggedLandManagement = "RequestLoggedLandManagement";
            public const string RFSLandManagement = "RFSLandManagement";
            public const string ProsecutionOutcome = "ProsecutionOutcome";
            public const string TankPull = "TankPull";
            public const string NotYetDetermined = "NotYetDetermined";
            public const string WithdrawnActionsMenu = "WithdrawnActionsMenu";
            public const string CostsReview = "CostsReview";
            public const string OnlineServicesNewJetskiRegistration = "OnlineServicesNewJetskiRegistration";
            public const string CheckOfficerResponsibleforJetski = "CheckOfficerResponsibleforJetski";
            public const string OnlineServicesJetskiDeregistration = "OnlineServicesJetskiDeregistration";
            public const string OnlineServicesJetskiTransfer = "OnlineServicesJetskiTransfer";
            public const string NewGeneralRegister = "NewGeneralRegister";
            public const string SetOutcomeToDeemedPermittedActivitySideline = "SetOutcomeToDeemedPermittedActivitySideline";
            public const string AssignOfficerResponsibleforManagementSite = "AssignOfficerResponsibleforManagementSite";
        }

        public struct ObservationTypes
        {
            public const string ComplianceMonitoring = "ComplianceMonitoring";
            public const string Request = "Request";
            public const string EnvironmentalMonitoring = "EnvironmentalMonitoring";
            public const string Biodiversity = "Biodiversity";
            public const string Biosecurity = "Biosecurity";
            public const string LandManagement = "LandManagement";
            public const string SelectedLandUseSiteMonitoring = "SelectedLandUseSiteMonitoring";
        }

        public struct ProgrammeTypes
        {
            public const string ComplianceMonitoring = "ComplianceMonitoring";
            public const string Biodiversity = "Biodiversity";
            public const string Biosecurity = "Biosecurity";
            public const string LandManagement = "LandManagement";
        }

        public struct ProgrammeStatuses
        {
            public const string Provisional = "Provisional";
            public const string Intermittent = "Intermittent";
            public const string Draft = "Draft";
        }

        public struct RemediationTypes
        {
            public const string Planned = "Planned";
            public const string Actual = "Actual";
        }

        public struct RemediationSubTypes
        {
            public const string Biocontrol = "Biocontrol";
            public const string PhysicalControl = "PhysicalControl";
            public const string Poisoning = "Poisoning";
            public const string Spraying = "Spraying";
            public const string Shooting = "Shooting";
            public const string Trapping = "Trapping";
            public const string Planting = "Planting";
            public const string StructureOther = "Structure/Other";
        }

        public struct RemediationTreatmentToxicityUnits
        {
            public const string MLPerLitre = "mlperlitre";
            public const string Percent = "percent";
        }

        public struct RequestType
        {
            public const string Incident = "Incident";
            public const string RequestForService = "RequestForService";
            public const string ServiceComplaint = "ServiceComplaint";
            public const string ServiceCompliment = "ServiceCompliment";
        }

        public struct RequestTypeIncidentHazardousSubstanceType
        {
            public const string NotAssessed = "NotAssessed";
            public const string Unknown = "Unknown";
            public const string No = "No";
            public const string Yes = "Yes";
        }

        public struct RequestSubject
        {
            public const string Biosecurity = "Biosecurity";
            public const string Transport = "Transport";
            public const string LandManagement = "LandManagement";
            public const string NotYetDetermined = "NotYetDetermined";
        }

        public struct Observer
        {
            public const string CouncilOfficer = "CouncilOfficer";
            public const string External = "External";
            public const string Joint = "Joint";
        }

        public struct RemediationActionBy
        {
            public const string CouncilOfficer = "CouncilOfficer";
            public const string External = "External";
            public const string Joint = "Joint";
        }

        public struct RemediationStatus
        {
            public const string Completed = "Completed";
        }

        public struct RequestResolution
        {
            public const string Referred = "Referred";
        }

        public struct RequestSource
        {
            public const string External = "External";
            public const string Internal = "Internal";
        }

        public struct OffenceDateType
        {
            public const string OnOrAbout = "OnOrAbout";
            public const string Between = "Between";
        }

        public struct EnforcementAction
        {
            public const string Letter = "Letter";
            public const string Notice = "Notice";
            public const string EnforcementOrder = "EnforcementOrder";
            public const string FormalWarning = "FormalWarning";
            public const string InfringementNotice = "InfringementNotice";
            public const string Prosecution = "Prosecution";
        }

        public struct EnforcementActionInfringementOutcome
        {
            public const string Unpaid = "Unpaid";
        }

        public struct EnforcementActionToBeTaken
        {
            public const string Yes = "Yes";
            public const string No = "No";
        }

        public struct EnforcementNoActionReason
        {
            public const string Other = "Other";
        }

        public struct EnforcementActionDocumentServed
        {
            public const string Original = "Original";
        }

        public struct OriginatedFrom
        {
            public const string Incident = "Incident";
        }

        public struct ManagementSiteType
        {
            public const string Biodiversity = "Biodiversity";
            public const string Biosecurity = "Biosecurity";
            public const string LandManagement = "LandManagement";
        }

        public struct RequestContactMethod
        {
            public const string OnlineServices = "OnlineServices";
        }

        public struct IncidentSource
        {
            public const string OnlineServices = "OnlineServices";
        }

        public struct InteractionTypes
        {
            public const string EMail = "EMail";
            public const string Meeting = "Meeting";
            public const string PhoneCall = "PhoneCall";
            public const string MobileInspectionNote = "MobileInspectionNote";
            public const string SphereUpdate = "SphereUpdate";
        }

        public const string PersonalWatercraft = "PersonalWatercraft";
    }

    public struct ReferenceDataValueAttributeCodes
    {
        public const string DisallowCDF = "DisallowCDF";
        public const string IsSecurable = "IsSecurable";
        public const string AllowBasicSearch = "AllowBasicSearch";
        public const string AllowAdvancedSearch = "AllowAdvancedSearch";
        public const string AllowAdminSearch = "AllowAdminSearch";
        public const string HasDataSecurity = "HasDataSecurity";
        public const string AllowDocGeneration = "AllowDocGeneration";
        public const string AllowViewReport = "AllowViewReport";
        public const string AllowCustomLinks = "AllowCustomLinks";
        public const string HasCalendar = "HasCalendar";
        public const string CanRelate = "CanRelate";
        public const string EDRMSReferenceRequired = "EDRMSReferenceRequired";
        public const string HasCreateProjectCodeMenu = "HasCreateProjectCodeMenu";
        public const string HasManageWorkflow = "HasManageWorkflow";
        public const string ShowInOnlineServices = "ShowInOnlineServices";
        public const string IsCompletedStatus = "IsCompletedStatus";
        public const string IsOpenStatus = "IsOpenStatus";
        public const string IsCurrentStatus = "IsCurrentStatus";
        public const string IsDPAPAReplaceable = "IsDPAPAReplaceable";
        public const string IsForMobile = "IsForMobile";
        public const string IsForOnlineService = "IsForOnlineService";
    }

    //TODO - merge this into Datacom.IRIS.DomainModel.Domain.Constants.AppSttingConstants as they are the same thing!
    public struct AppSettingCodes
    {
        public const string DomainName = "DomainName";
        public const string ReportServerURL = "ReportServerURL";
        public const string SearchResultReportPath = "SearchResultReportPath";
        public const string SSRSReportsPath = "SSRSReportsPath";
        public const string SSRSDocumentTemplatesPath = "SSRSDocumentTemplatesPath";
        public const string SSRSRoot = "SSRSRoot";
        public const string SSRSGlobalReportsFolder = "SSRSGlobalReportsFolder";
        public const string SSRSInstanceReportsFolder = "SSRSInstanceReportsFolder";
        public const string DashBoardReportPath = "DashBoardReportPath";
        public const string RegimeActivitySampleResultReportPath = "RegimeActivitySampleResultReportPath";
        public const string DVRImportLogReportFullPathReportPath = "PropertyData_DVRImportLogReportPath";
        public const string FinancialsReportFullPathReportPath = "Financials_ReportPath";
        public const string FinancialsContactReportFullPathReportPath = "Financials_ContactReportPath";
        public const string BuildingConsent = "BuildingConsent";
        public const string ExpiryValueInYears = "ExpiryValueInYears";
        public const string AutocompleteMinLengthSpecies = "AutocompleteMinLength_Species";
        public const string AutocompleteMinLengthAddress = "AutocompleteMinLength_Address";
        public const string SystemAdministratorEmailAddr = "SystemAdministratorEmailAddr";
        public const string FailToUploadDocumentsBackupFolderPath = "FailToUploadDocumentsBackupFolderPath";
        public const string AutoRefreshMilliSeconds = "AutoRefreshMilliSeconds";
        public const string TempOutputFolderPath = "TempOutputFolderPath";
        public const string DocumentIdMaxLength = "DocumentIdMaxLength";

    }

    public struct BuildingConsentCode
    {
        public const string BCA = "BCA";
    }

    public struct SequencesCodes
    {
        public const string NextDocumentReferenceNumber = "NextDocumentReferenceNumber";
        public const string NextMOJExportSequenceNumber = "NextMOJExportSequenceNumber";
        public const string NextFinancialCustomerCode = "NextFinancialCustomerCode";
    }

    public struct BaseFunctionNames
    {
        public const string AdministrationAccess = "Administration";
        public const string Security = "Security";
        public const string CustomFields = "Custom Fields";
        public const string CustomTaskFields = "Custom Task Fields";
        public const string ReferenceData = "Reference Data";
        public const string UserProfile = "User Profile";
        public const string ReportsAndTemplates = "Reports/Templates";
        public const string Help = "Help";
        public const string LibraryConditions = "Library Conditions";
        public const string SpeciesData = "Species Data";
        public const string AddressValidationData = "Address Validation Data";
        public const string PropertyData = "Property Data";
        public const string Surveys = "Surveys";
        public const string Workflows = "Workflows";
        public const string Calendars = "Calendars";
        public const string Configuration = "Configuration";
        public const string Contact = "Contact";
        public const string RelationshipTypes = "Relationship Types";
        public const string MOJInfringements = "MOJ Infringements";
        public const string Authorisation = "Authorisation";
        public const string Application = "Application";
        public const string ContactGroup = "Contact Group";
        public const string DamRegister = "Dam Register";
        public const string GeneralRegister = "General Register";
        public const string Location = "Location";
        public const string Regime = "Regime";
        public const string Programme = "Programme";
        public const string Enforcement = "Enforcement";
        public const string Request = "Request";
        public const string ManagementSite = "Management Site";
        public const string SelectedLandUseSite = "Selected Land Use Site";
        public const string BulkReassign = "Bulk Reassign";
        public const string RollForwardAnnualCharges = "Roll Forward Annual Charges";
        public const string Estimation = "Estimation Data";
        public const string ObjectInspectionTypeMapping = "Object Inspection Type Mapping";
    }

    public struct PermissionNames
    {
        public const string View = "View";
        public const string Edit = "Edit";
        public const string Delete = "Delete";
        public const string Maintain = "Maintain";
        public const string Access = "Access";
        public const string EditContactAddress = "Edit Address";
        public const string EditStatus = "Edit Status";
        public const string SecureData = "Secure Data";
        public const string ConfigureCustomField = "Configure Custom Fields";
        public const string Export = "Export";
        public const string EditStatusHoldAndTimeFrame = "Edit Status, Hold, and Timeframe";
        public const string EditBilling = "Edit Billing";
        public const string CreateFinancialCustomer = "Create Financial Customer";
        public const string CreateFinancialProject = "Create Financial Project";
        public const string SelectFinancialProject = "Select Financial Project";
        public const string EditFlaggedAuthorisationCharge = "Edit Flagged Authorisation Charge";
        public const string Event = "Event";
        public const string ManageWorkflow = "Manage Workflow";
        public const string CreateIRISID = "Create IRIS ID";
        public const string EditIRISID = "Edit IRIS ID";
        public const string CloneConditionSchedule = "Clone Condition Schedule";
        public const string EditAuthorisationObservation = "Edit Authorisation-Observation";
        public const string CancelMobileInspection = "Cancel Mobile Inspection";
        public const string CreateWorkflow = "Create Workflow";
        public const string FindMergeDuplicateContacts = "Find & Merge Duplicate Contacts";
        public const string DeleteSitePlan = "Delete Site Plan";
        public const string EditRequestType = "Edit Request Type";
        public const string SplitApplicationActivities = "Split Application Activities";
        public const string BulkReplaceAuthorisation = "Bulk Replace Authorisation";
        public const string DeleteAllConditions = "Delete All Conditions";
        public const string ChangeContactType = "Change Contact Type";
        public const string ChangeActivityTypeOrSubtype = "Change Activity Type/Subtype";
        public const string ChangeRequestTypeOrSubtype = "Change Request Type/Subtype";
    }

    public struct SearchQueryStringKeys
    {
        public const string Mode = "mode";
        public const string Scope = "scope";
        public const string Keyword = "keyword";
        public const string MapView = "map";
        public const string SpatialQueryGUID = "spatialguid";
        public const string SearchResultsID = "sid";

        public const string LocationCommonName = "commonname";
        public const string LocationDescription = "description";
        public const string FeatureTypeIDs = "featuretypes";
        public const string LocationCreatedTo = "to";
        public const string LocationCreatedFrom = "from";
        public const string LocationRestricted = "restricted";
        public const string LocationRestrictedCommnents = "restrictedcomments";
        public const string LocationLegalDescription = "locationlegaldescription";
        public const string LocationGroupIRISObjectID = "locationgroupirisobjectid";

        public const string ContactFirstName = "contactfirstname";
        public const string ContactLastName = "contactlastname";
        public const string ContactOrganisationName = "contactorgname";
        public const string ContactCompanyNumber = "contactcompanyno";
        public const string ContactContactID = "contactid";
        public const string ContactStreetNumber = "contactstreetnumber";
        public const string ContactStreetAlpha = "contactstreetalpha";
        public const string ContactStreetName = "contactstreetname";
        public const string ContactSuburb = "contactsuburb";
        public const string ContactPhoneNumber = "contactphonenumber";
        public const string ContactTownCityUrban = "contacttowncityurban";
        public const string ContactTownCityDelivery = "contacttowncitydelivery";
        public const string ContactDeliveryServiceIdentifier = "contactdsi";
        public const string ContactBoxLobby = "contactboxlobby";
        public const string ContactAddress = "contactaddress";
        public const string ContactCountryID = "contactcountryid";
        public const string ContactExcludeDeceased = "contactexcdeceased";
        public const string ContactIncludeDuplicates = "contactincduplicate";
        public const string ContactAddressType = "contactaddresstype";

        public const string ApplicationTypeID = "applicationtypeid";
        public const string ApplicationPurposeID = "applicationpurposeid";
        public const string ApplicationActivityTypeID = "applicationactivitytypeid";
        public const string ApplicationActivitySubtypeID = "applicationactivitysubtypeid";
        public const string ApplicationStatusID = "applicationstatusid";
        public const string ApplicationLodgedFrom = "applicationlodgedfrom";
        public const string ApplicationLodgedTo = "applicationlodgedto";
        public const string ApplicationOfficerResponsibleID = "applicationofficerresponsibleid";
        public const string ApplicationDescription = "applicationdescription";

        public const string AuthorisationTypeID = "authorisationtypeid";
        public const string AuthorisationActivityTypeID = "authorisationactivitytypeid";
        public const string AuthorisationActivitySubtypeID = "authorisationactivitysubtypeid";
        public const string AuthorisationStatusID = "authorisationstatusid";
        public const string AuthorisationCommencedFrom = "authorisationcommencedfrom";
        public const string AuthorisationCommencedTo = "authorisationcommencedto";
        public const string AuthorisationOfficerResponsibleID = "authorisationofficerresponsibleid";
        public const string AuthorisationDescription = "authorisationdescription";        

        public const string RegimeTypeID = "regimetypeid";
        public const string RegimeActivityTypeID = "regimeactivityid";
        public const string RegimeClassificationID = "regimeclassificationid";
        public const string RegimeOfficerResponsibleID = "regimeofficerresponsibleid";
        public const string RegimeActivityName = "regimeactivityname";
        public const string RegimeDescription = "regimedescription";
        public const string RegimeStatusID = "regimestatusid";
        public const string RegimeFinancialYearID = "regimefinancialyearid";

        public const string ProgrammeStartDateFrom = "programmestartdatefrom";
        public const string ProgrammeStartDateTo = "programmestartdateto";
        public const string ProgrammeEndDateFrom = "programmeenddatefrom";
        public const string ProgrammeEndDateTo = "programmeenddateto";
        public const string ProgrammeType = "programmetype";
        public const string ProgrammePriority = "programmepriority";
        public const string ProgrammeOfficerResponsible = "programmeofficerresponsible";
        public const string ProgrammeDescription = "programmedescription";

        public const string RequestTypeREFID = "requesttypeid";
        public const string RequestSubjectTypeID = "requestsubjecttypeid";
        public const string RequestSubjectID = "requestsubjectid";
        public const string RequestPriorityID = "requestpriorityid";
        public const string RequestStatusID = "requeststatusid";
        public const string RequestOrganisationPersonName = "requestorganisationpersonname";
        public const string RequestContactRelationshipTypeID = "requestcontactrelationshipTypeid";
        public const string RequestDetail = "requestdetail";
        public const string RequestDateFrom = "requestdatefrom";
        public const string RequestDateTo = "requestdateto";
        public const string RequestOfficerResponsibleID = "requestofficerresponsibleid";
        public const string RequestThreatSpeciesID = "requestthreatspeciesid";
        public const string RequestThreatSpeciesTypeID = "requestthreatspeciestypeid";

        public const string LinkedOrganisationPersonName = "linkedorganisationpersonname";
        public const string LinkedContactRelationshipTypeIDs = "linkedcontactrelationshipTypeids";

        public const string SelectedLandUseSiteID = "selectedlandusesiteid";

        public const string MngtSiteTypeID = "mngtsitetypeid";
        public const string MngtSiteSubtypeIDs = "mngtsitesubtypeids";
        public const string MngtSiteOfficerResponsibleID = "mngtsiteofficerresponsibleid";
        public const string MngtSiteHabitatID = "mngtsitehabitatid";
        public const string MngtSiteStatusID = "mngtsitestatusid";
        public const string MngtSiteClassificationTypeID = "mngtsiteclassificationtypeid";
        public const string MngtSiteClassificationIDs = "mngtsiteclassificationids";
        public const string MngtSiteConsSpeciesTypeID = "mngtsiteconsvtypeid";
        public const string MngtSiteConsCommonNameID = "mngtsiteconsvcomnameid";
        public const string MngtSiteConsScientificNameID = "mngtsiteconsvscnameid";
        public const string MngtSiteThreatSpeciesTypeID = "mngtsitethrtypeid";
        public const string MngtSiteThreatCommonNameID = "mngtsitethrcomnameid";
        public const string MngtSiteThreatScientificNameID = "mngtsitethrscnameid";
        public const string MngtSiteIndustryPurposeID = "mngtsiteindustrypurposeid";
        public const string MngtSiteSituationID = "mngtsitesituationid";
        public const string MngtSiteDescription = "mngtsitedescription";

        public const string EnforcementBriefDescription = "enforcebriefdesc";
        public const string EnforcementActionREFID = "enforceactionid";
        public const string EnforcementActionTypeREFID = "enforceactiontypeid";
        public const string EnforcementActREFID = "enforceactid";
        public const string EnforcementOffenceSectionREFID = "enforceoffencesectid";
        public const string EnforcementNatureOfOffenceREFID = "enforcenatureid";
        public const string EnforcementDateStart = "enforcementdatefrom";
        public const string EnforcementDateEnd = "enforcementdateto";
        public const string EnforcementOfficerResponsibleID = "enforceofficerid";
        public const string EnforcementStatusREFID = "enforcestatusid";

        public const string SLUSClassificationREFID = "slusclassificationid";
        public const string SLUSContextREFID = "slusclassificationcontextid";
        public const string SLUSHAILGroupREFID = "slushailgroupid";
        public const string SLUSHAILCategoryREFID = "slushailcategoryid";
        public const string SLUSContaminantTypeREFID = "sluscontaminanttypeid";
        public const string SLUSContaminantREFID = "sluscontaminantid";
        public const string SLUSDescription = "slusdescription";
        public const string SLUSStatusID = "slusstatusid";

        public const string GeneralRegisterTypeID = "generalregistertypeid";
        public const string GeneralRegisterStatusID = "generalregisterstatusid";

        public const string CDFIDPrefix = "cdf_";
    }

    public struct SearchQueryMode
    {
        public const string Contact = "advcontact";
        public const string Location = "advlocation";
        public const string LocationGroup = "advlocationgrp";
        public const string MapContext = "advmapcxt";
        public const string MapUser = "advuser";
        public const string Application = "advapplication";
        public const string Authorisation = "advauthorisation";
        public const string Regime = "advregime";
        public const string Programme = "advprogramme";
        public const string Request = "advrequest";
        public const string ManagementSite = "advmngtsite";
        public const string Enforcement = "advenforcement";
        public const string SelectedLansUseSite = "advselectedlandusesite";
        public const string GeneralRegister = "generalregister";
    }

    public struct NotificationDecisionCodes
    {
        public const string PubliclyNotified = "publiclynotified";
        public const string LimitedNotified = "limitednotified";
    }

    public struct ObjectRelationshipTypesCodes
    {
        public const string Partner = "Partner";
        public const string Colleague = "Colleague";
        public const string Spouse = "Spouse";
        public const string Trustee = "Trustee";
        public const string Employee = "Employee";
        public const string Subsidiary = "Subsidiary";
        public const string NotDuplicateContact = "NotDuplicateContact";
        public const string IWIContact = "IWIContact";
        public const string FarmManager = "FarmManager";
        public const string Sharemilker = "Sharemilker";
        public const string Applicant = "Applicant";
        public const string ApplicationAgent = "ApplicationAgent";
        public const string ApplicantsConsultant = "ApplicantsConsultant";
        public const string ApplicationConsultant = "ApplicationConsultant";
        public const string ChildLocation = "ChildLocation";

        public const string AuthorisationHolder = "AuthorisationHolder";
        public const string AuthorisationAgent = "AuthorisationAgent";

        public const string PreviousAuthorisation = "PreviousAuthorisation";
        public const string ApplicationActivity = "ApplicationActivity";
        public const string ResultingAuthorisation = "ResultingAuthorisation";

        public const string RegimeSubjectAuthorisation = "RegimeSubjectAuthorisation";
        public const string RegimeSubjectDamRegister = "RegimeSubjectDamRegister";
        public const string RegimeSubjectSelectedLandUseSite = "RegimeSubjectSelectedLandUseSite";
        public const string RegimeSubjectManagementSite = "RegimeSubjectManagementSite";
        public const string LinkedProgrammeRegime = "LinkedProgrammeRegime";
        public const string ProgrammeRegime = "ProgrammeRegime";
        public const string ProgrammeSubject = "ProgrammeSubject";
        public const string RelatedProgramme = "RelatedProgramme";
        public const string PreviousEstimationRegimeActivity = "PreviousEstimationRegimeActivity";

        // These constants need to have the same values for cloning to work. The issue of relying on equal codes and making
        // the assumption they are available for cloning has been discussed with the BAs many times, with the conclusion that this is sufficient.
        public const string ActivityConditionSchedule = "ActAuthConditionSchedule";
        public const string AuthorisationConditionSchedule = "ActAuthConditionSchedule";

        public const string AuthorisationLocation = "ActAuthLocation";
        public const string ActivityLocation = "ActAuthLocation";

        public const string Requestor = "Requestor";

        public const string EnforcementAllegedOffender = "AllegedOffender";
        public const string MonitoringEnforcement = "MonitoringEnforcement";
        public const string ObservationEnforcement = "ObservationEnforcement";
        public const string IncidentEnforcement = "IncidentEnforcement";
        public const string IncidentObservationEnforcement = "IncidentObservationEnforcement";
        public const string EnforcementSubjectAuthorisation = "EnforcementSubjectAuthorisation";
        public const string EnforcementSubjectMngtSite = "EnforcementSubjectMngtSite";
        public const string ManagementSiteEnforcement = "ManagementSiteEnforcement";
        public const string ManagementSiteRequest = "ManagementSiteRequest";
        public const string AuthorisationEnforcement = "AuthorisationEnforcement";
        public const string EnforcementSubjectDamRegister = "EnforcementSubjectDamRegister";
        public const string EnforcementSubjectSelectedLandUseSite = "EnforcementSubjectSelectedLandUseSite";
        public const string Prosecutor = "Prosecutor";
        public const string DefenseCounsel = "DefenseCounsel";
        public const string SameEvent = "SameEvent";
        public const string GroupMember = "GroupMember";
        public const string NotificationRequest = "NotificationRequest";

        public const string Other = "Other";
    }

    public struct EDRMSReferenceValidationType
    {
        public const string Write = "write";
        public const string Read = "read";
    }

    public struct SublinkType
    {
        public const string Type = "SublinkType";
        public const string Names = "Names";
        public const string Addresses = "Addresses";
        public const string PhoneNumbers = "Phone Numbers";
        public const string Emails = "Emails";
        public const string Websites = "Websites";
    }

    public struct EmailTemplates
    {
        public const string ReassignedOfficerResponsibleFrom = "ReassignedOfficerResponsibleFrom";
        public const string ReassignedOfficerResponsibleTo = "ReassignedOfficerResponsibleTo";
        public const string ReassignedOfficerResponsibleBulkFrom = "ReassignedOfficerResponsibleBulkFrom";
        public const string ReassignedOfficerResponsibleBulkTo = "ReassignedOfficerResponsibleBulkTo";
    }

    public struct SpatialReference
    {
        public const int IRISSpatialRefernece = 2193;
        public const int SphereSpatialRefernece = 4326;
    }

    public enum HelpLinksPage
    {
        // Admin pages unique help links
        Admin_Default,
        Admin_Login,
        Admin_ViewLists,
        Admin_RelationshipType,
        Admin_AppSettings,
        Admin_Function,
        Admin_Functions,
        Admin_Group,
        Admin_Groups,
        Admin_UserProfile,
        Admin_Users,
        Admin_Collections,
        Admin_Collection,
        Admin_Reports,
        Admin_Templates,
        Admin_HelpLinks,
        Admin_HelpContent,
        Admin_Calendars,
        Admin_CalendarObjects,
        Admin_Streets,
        Admin_Suburbs,
        Admin_TownsCities,
        Admin_WorkflowDefinitions,
        Admin_WorkflowDefinition,
        Admin_WorkflowCallMappings,
        Admin_WorkflowInstances,
        Admin_Surveys,
        Admin_SurveyCategories,        
        Admin_SurveyCategoryQuestions,
        Admin_CustomLinks,

        // Help pages unique help links
        SystembarHelp,
        Support,
        KnowledgeBase
    }

    public enum HelpLinksOverlay
    {
        Overlay_DefaultHelpLink,
        Overlay_AppSettings,
        Overlay_CreateApplication,
        Overlay_WithdrawApplication,
        Overlay_SplitApplication,
        Overlay_CreateRequest,
        Overlay_ReorderFields,
        Overlay_ReorderCDFLists,
        Overlay_CDF,
        Overlay_CDFList,
        Overlay_CDFListEdit,
        Overlay_AddEditObjectLink,
        Overlay_NotesAddEdit,
        Overlay_Search,
        Overlay_SearchAndLink,
        Overlay_DataSecurity,
        Overlay_CreateContact,
        Overlay_AddEditEmailAddress,
        Overlay_AddEditName,
        Overlay_AddEditWebSite,
        Overlay_AddEditPhoneNumber,
        Overlay_CloneContact,
        Overlay_ChangeContactType,
        Overlay_ModifyAddress,
        Overlay_ModifyOrganisationDetails,
        Overlay_ModifyPersonDetails,
        Overlay_SelectContactType,
        Overlay_GenerateDocumentWizard,
        Overlay_UploadDocumentWizard,
        Overlay_EmailLinkedContactsWizard,
        Overlay_AddToFavourites,
        Overlay_EditFavourites,
        Overlay_HelpContent,
        Overlay_HelpLink,
        Overlay_AddModifyOtherIdentifiers,
        Overlay_CreateLocation,
        Overlay_CreateLocationGroup,
        Overlay_ModifyLocationGeneralInformation,
        Overlay_LocationGroup,
        Overlay_ModifyLocationSpatialAttributes,
        Overlay_ModifyLocationTextualAttributes,
        Overlay_Login,
        Overlay_RefDataValue,
        Overlay_ObjectInspectionTypeMapping,
        Overlay_RelationshipType,
        Overlay_AddEditDocumentTemplate,
        Overlay_AddEditDocumentID,
        Overlay_AddEditReport,
        Overlay_MapSearch,
        Overlay_Users,
        Overlay_GroupsInFunction,
        Overlay_EditFavorites,
        Overlay_Functions,
        Overlay_FunctionsInGroup,
        Overlay_Groups,
        Overlay_UserInGroup,
        Overlay_Note,
        Overlay_UserTask,
        Overlay_Reschedule,
        Overlay_WorkflowTask,
        Overlay_AddDelegation,
        Overlay_Calendars,
        Overlay_CalendarNonWorkPeriods,
        Overlay_ObjectTypeCalendars,
        Overlay_ApplicationType,
        Overlay_CreateAuthorisation,
        Overlay_CreateAuthorisationGroup,
        Overlay_CreateAuthorisationMobileInspection,
        Overlay_EditGeneralInformation,
        Overlay_EditAnnualChargesApply,
        Overlay_EditAuthorisationCharge,
        Overlay_EditHolds,
        Overlay_EditTimeFrame,
        Overlay_EditAuthorisationDetails,
        Overlay_EditAuthorisationAddressForService,
        Overlay_EditApplicationAddressForService,
        Overlay_EditRegimeAddressForService,
        Overlay_EditResourceConsentInformation,
        Overlay_EditNotification,
        Overlay_EditObjections,
        Overlay_EditAppeals,
        Overlay_EditThirdPartyInvolvement,
        Overlay_EditGrantInformation,
        Overlay_EditApplicationActivity,
        Overlay_EditAuthorisationOtherInformation,
        Overlay_AuthorisationSpecialEventDate,
        Overlay_AddEditLibraryCondition,
        Overlay_AddEditCondition,
        Overlay_ReorderConditions,
        Overlay_ImportConditions,
        Overlay_ActivityGeneralInformation,
        Overlay_ActivityStatusHistory,
        Overlay_ActivityOutcomeDeemedPermitted,
        Overlay_AddEditParameter,
        Overlay_RecordTime,
        Overlay_CreateFinancialCustomer,
        Overlay_CreateFinancialProject,
        Overlay_SelectFinancialProject,
        Overlay_CreateProgramme,
        Overlay_CreateRegime,
        Overlay_EditSpecies,
        Overlay_CreateRegimeActivity,
        Overlay_EditRegimeActivityDetails,
        Overlay_EditRegimeActivitySchedule,
        Overlay_ContactLinkRelationship,
        Overlay_ConditionSchedule,
        Overlay_StatusHistory,
        Overlay_PlanRules,
        Overlay_CreateObservation,
        Overlay_EditObservation,
        Overlay_AddObservingOfficer,
        Overlay_AddManagementSite,
        Overlay_ObservationEditManagementSite,
        Overlay_ObservationAddEditSpeciesCount,
        Overlay_ObservationEditComplianceInformation,
        Overlay_ObservationEditParameter,
        Overlay_ObservationAddEditFurtherAction,
        Overlay_ObservationEditEnvironmentalInformation,
        Overlay_ObservationAddEditIndicator,
        Overlay_ObservationLabour,
        Overlay_ObserationEquipment,
        Overlay_CreateObservationSampleResult,
        Overlay_AddEditRegimeActivityMngtLineOverlay,
        Overlay_AddEditRegimeActivityMngtLineResultOverlay,
        Overlay_EditRegimeActivityMngtLineMonitoringOverlay,
        Overlay_RegimeActivityComplianceEditComplianceInformation,
        Overlay_RegimeActivitySampling,
        Overlay_AddEditRegimeActivityDamReport,
        Overlay_AddEditRegimeActivityResourceNeeded
        ,Overlay_RegimeActivityComplianceEditCondition
        ,Overlay_RegimeActivityLabour
        ,Overlay_RegimeActivityEquipment
        ,Overlay_RegimeActivityTimeline
        ,Overlay_RegimeActivitySampleResult
        ,Overlay_AddEditRegimeActivityDamFurtherAction
        ,Overlay_RegimeActivitySampleResultInformation
        ,Overlay_ProgrammeInformation
        ,Overlay_ModifyWorkflowDefinition
        ,Overlay_ModifyTaskDefinition
        ,Overlay_RestartWorkflow
        ,Overlay_CloneProgramme
        ,Overlay_RolloverProgramme
        ,Overlay_AddEditSpeciesData
        ,Overlay_EditAllegedOffender
        ,Overlay_EditIncidentClassification
        ,Overlay_EditRequestor
        ,Overlay_EditIncidentInformation
        ,Overlay_AddEditFurtherActionOverlay
        ,Overlay_RequestsCausesAndEffects
        ,Overlay_EditRFSInformation
        ,Overlay_EditRoutesInformation
        ,Overlay_AddRequestOfficerReponsible
        ,Overlay_RequestBiosecuritySpecies
        ,Overlay_EditRequestResolution
        ,Overlay_AddEditStreet
        ,Overlay_AddEditSuburb
        ,Overlay_AddEditTownCity
        ,Overlay_AddEditDamRegister
        ,Overlay_AddEditGeneralRegister
        ,Overlay_AddEditContactGroup        
        ,Overlay_AddEditWorkflowCallMapping
        ,Overlay_CreateAdHocData
        ,Overlay_EditDocumentGeneralInformation
        ,Overlay_EditPropertyDataPrivacy
        ,Overlay_AddRegimeSubjectsProgramme
        /* Enforcement Module */
        ,Overlay_CreateEnforcement
        ,Overlay_CreateEnforcementSelectSubjects
        ,Overlay_CreateEnforcementAction
        ,Overlay_AddEnforcementAllegedOffence
        ,Overlay_AddEnforcementActionOfficerReponsible
        ,Overlay_AddEditEnforcementActionProsecutionDefendant
        ,Overlay_AddEditEnforcementAllegedOffender
        ,Overlay_AddEditEnforcementActionAllegedOffender
        ,Overlay_AddEditEnforcementActionProsecutionDefendantCharges
        ,Overlay_EditEnforcementGeneralInfo
        ,Overlay_EditEnforcementAllegedOffenceGeneralInfo
        ,Overlay_EditEnforcementActionGeneralInfo
        ,Overlay_EditEnforcementActionAllegedOffences
        ,Overlay_EditEnforcementActionInfringementDetails
        ,Overlay_EditEnforcementActionNoticeDetails
        ,Overlay_EditEnforcementActionOrderDetails
        ,Overlay_EditEnforcementActionProsecutionDetails
        ,Overlay_EditEnforcementActionFormalWarningDetails
        ,Overlay_EditEnforcementActionLetterDetails

        ,Overlay_CreateSelectedLandUseSite
        ,Overlay_AddEditSelectedLandUseSite
        ,Overlay_AddObservationSurvey
        ,Overlay_AddEditManagementSite
        ,Overlay_AddEditManagementSiteSiteProtection
        ,Overlay_AddEditManagementSiteClassification
        ,Overlay_AddEditManagementSiteConservationSpecies
        ,Overlay_AddEditManagementSiteThreatSpecies
        ,Overlay_AddEditManagementSiteOtherManagementIssue
        ,Overlay_AddEditManagementSiteEcologicalFeature
        ,Overlay_AddEditManagementSiteIndustryPurpose
        ,Overlay_AddEditManagementSiteAdvancedSearch
        ,Overlay_AddEditSitePlan
        ,Overlay_EditSitePlanObserved
        ,Overlay_AddEditDVRUploads
  		,Overlay_ExportInfringements
        ,Overlay_SelectedLandUseSiteClassification
        ,Overlay_SelectedLandUseSiteHAIL
        ,Overlay_SelectedLandUseSiteContaminant
        ,Overlay_SelectedLandUseSiteRapidRiskScreening
        ,Overlay_EditSelectedLandUseSiteRiskAssessment
        ,Overlay_TankPullSelectedLandUseSite
        ,Overlay_AddEditRemediation
        ,Overlay_AddEditRemediatonWorkControl
        ,Overlay_CreateJFC
        ,Overlay_ChangeSublinks
        ,Overlay_ReassignOfficerResponsible
        ,Overlay_BulkReassignOfficerResponsible
        ,Overlay_RollForwardAnnualCharges
        ,Overlay_CustomLinks
        ,Overlay_BulkReplaceAuthorisationWizard
    }

}
