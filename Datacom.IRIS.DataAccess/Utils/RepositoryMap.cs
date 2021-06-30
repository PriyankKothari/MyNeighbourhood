using Datacom.IRIS.Common.DependencyInjection;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Unity;

namespace Datacom.IRIS.DataAccess.Utils
{
    public static class RepositoryMap
    {
        public static ILookupRepository LookupRepository
        {
            get { return DIContainer.Container.Resolve<ILookupRepository>(); }
        }

        public static IAuthorisationRepository AuthorisationRepository
        {
            get { return DIContainer.Container.Resolve<IAuthorisationRepository>(); }
        }

        public static IAddressRepository AddressRepository
        {
            get { return DIContainer.Container.Resolve<IAddressRepository>(); }
        }

        public static ICDFRepository CDFRepository
        {
            get { return DIContainer.Container.Resolve<ICDFRepository>(); }
        }

        public static ICommonRepository CommonRepository
        {
            get { return DIContainer.Container.Resolve<ICommonRepository>(); }
        }

        public static IContactRepository ContactRepository
        {
            get { return DIContainer.Container.Resolve<IContactRepository>(); }
        }

        public static IDocumentRepository DocumentRepository
        {
            get{ return DIContainer.Container.Resolve<IDocumentRepository>(); }
        }

        public static ILinkingRepository LinkingRepository
        {
            get { return DIContainer.Container.Resolve<ILinkingRepository>(); }
        }

        public static ILocationRepository LocationRepository
        {
            get { return DIContainer.Container.Resolve<ILocationRepository>(); }
        }

        public static ISearchRepository SearchRepository
        {
            get { return DIContainer.Container.Resolve<ISearchRepository>(); }
        }

        public static ISecurityRepository SecurityRepository
        {
            get { return DIContainer.Container.Resolve<ISecurityRepository>(); }
        }

        public static IWorkflowRepository WorkflowRepository
        {
            get { return DIContainer.Container.Resolve<IWorkflowRepository>(); }
        }

        public static IHelpRepository HelpRepository
        {
            get { return DIContainer.Container.Resolve<IHelpRepository>(); }
        }

        public static IActiveDirectoryRepository ActiveDirectoryRepository
        {
            get { return DIContainer.Container.Resolve<IActiveDirectoryRepository>(); }
        }

        public static ICalendarRepository CalendarRepository
        {
            get { return DIContainer.Container.Resolve<ICalendarRepository>(); }
        }

        public static IConditionRepository ConditionRepository
        {
            get { return DIContainer.Container.Resolve<IConditionRepository>(); }
        }

        public static ITimeRecordingRepository TimeRecordingRepository
        {
            get { return DIContainer.Container.Resolve<ITimeRecordingRepository>(); }
        }

        public static IMonitoringRepository MonitoringRepository
        {
            get { return DIContainer.Container.Resolve<IMonitoringRepository>(); }
        }

        public static ISpeciesRepository SpeciesRepository
        {
            get { return DIContainer.Container.Resolve<ISpeciesRepository>(); }
        }
        public static IRequestRepository RequestRepository
        {
            get { return DIContainer.Container.Resolve<IRequestRepository>(); }
        }
        public static IRegisterRepository RegisterRepository
        {
            get { return DIContainer.Container.Resolve<IRegisterRepository>(); }
        }
        public static IPropertyDataRepository PropertyDataRepository
        {
            get { return DIContainer.Container.Resolve<IPropertyDataRepository>(); }
        }
        public static IAdHocDataRepository AdHocDataRepository
        {
            get { return DIContainer.Container.Resolve<IAdHocDataRepository>(); }
        }
        public static ISelectedLandUseSiteRepository SelectedLandUseSiteRepository
        {                                              
            get { return DIContainer.Container.Resolve<ISelectedLandUseSiteRepository>(); }
        }
        public static IManagementSiteRepository ManagementSiteRepository
        {
            get { return DIContainer.Container.Resolve<IManagementSiteRepository>(); }
        }		
        public static IEnforcementRepository EnforcementRepository
        {
            get { return DIContainer.Container.Resolve<IEnforcementRepository>(); }
        }

        public static IReferenceDataRepository ReferenceDataRepository
        {
            get { return DIContainer.Container.Resolve<IReferenceDataRepository>(); }
        }

        public static IOnlineServicesRepository OnlineServicesRepository
        {
            get { return DIContainer.Container.Resolve<IOnlineServicesRepository>(); }
        }

        public static IObjectInspectionMappingRepository ObjectInspectionMappingRepository
        {
            get { return DIContainer.Container.Resolve<IObjectInspectionMappingRepository>(); }
        }
    }
}
