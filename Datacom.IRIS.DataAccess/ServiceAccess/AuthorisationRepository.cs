using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common.Utils;
using Datacom.IRIS.DomainModel.Domain.Constants;
using Datacom.IRIS.DomainModel.DTO;
using System.Data;
using System.Data.SqlClient;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{

    public class AuthorisationRepository : RepositoryStore, IAuthorisationRepository
    {
        public Application GetApplicationByID(long applicationId)
        {
            var applicationQuery = from app in Context.Applications
                                       .Include(a => a.OfficerResponsible)
                                       .Include(a => a.ApplicationDecision)
                                       .Include(a => a.DecisionMaker)
                                       .Where(app => app.ID == applicationId)
                                   select app;

            return GetApplication(applicationQuery);
        }

        public Application GetApplicationByBusinessID(string businessId)
        {
            var applicationQuery = from app in Context.Applications
                                       .Include(a => a.OfficerResponsible)
                                       .Include(a => a.ApplicationDecision)
                                       .Include(a => a.DecisionMaker)
                                       .Where(app => app.IRISObject.BusinessID == businessId)
                                   select app;

            return GetApplication(applicationQuery);
        }

        public Application GetApplicationByIRISObjectID(long irisObjectId)
        {
            var applicationQuery = from app in Context.Applications
                                       .Include(a => a.OfficerResponsible)
                                       .Include(a => a.ApplicationDecision)
                                       .Include(a => a.DecisionMaker)
                                       .Where(app => app.IRISObjectID == irisObjectId)
                                   select app;

            return GetApplication(applicationQuery);
        }
        
        private Application GetApplication(IQueryable<Application> applicationQuery)
        {
            var application = applicationQuery.FirstOrDefault();
            if (application == null)
            {
                return null;
            }
            long applicationId = application.ID;

            application.IRISObject = (from n in Context.IRISObject
                                        .Include(o => o.ObjectTypeREF)
                                        .Include(o => o.SubClass1REF)
                                        .Include(o => o.SubClass2REF)
                                        .Include(o => o.SubClass3REF)
                                        .Include(o => o.OtherIdentifiers)
                                        .Include("OtherIdentifiers.IdentifierContextREF")
                                        .Include(o => o.Clock)
                                        .Include("Clock.Holds")
                                        .Include("Clock.Holds.Reason")
                                        .Include("Clock.TimeFrames")
                                        .Include("Clock.TimeFrames.TimeFrameType")
                                        .Include("Clock.TimeFrames.TimeFrameCategory")
                                      where n.ID == application.IRISObjectID
                                      select n).Single();

            var activityQuery = from activity in Context.Activities
                                .Include(a=>a.IRISObject)
                                .Include(a=>a.IRISObject.ObjectTypeREF)
                                .Include(a=>a.IRISObject.SubClass1REF)
                                .Include(a=>a.IRISObject.SubClass2REF)
                                .Include(a=>a.IRISObject.SubClass3REF)
                                .Include(a=>a.PrimaryIndustryPurpose)
                                .Include(a=>a.SecondaryIndustryPurpose)
                                .Include(a=>a.DurationType)
                                .Include(a=>a.ApplicationPurpose)
                                .Include(a=>a.ActivityOutcomes)
                                .Include("ActivityOutcomes.ActivityOutcomeREF") 
                                .Include(a=>a.ExpectedLifeTime)
                                .Where(activity => activity.ApplicationID == applicationId)
                                select activity;

            application.Activities.AddTrackableCollection(activityQuery.ToList());
            
            // Add to application's linked contact or locations   
            //TODO - review - is this correct? Because application can be in IRISObjectID or RelatedIRISObjectID?
            var activityObjectRelationQuery = (from a in Context.ActivityObjectRelationship
                                                   .Include(a => a.IRISObject)
                                                   .Include(a => a.IRISObject.ObjectTypeREF)
                                                   .Include(a => a.ContactSubLink)                                                
                                               where a.IRISObjectID == application.IRISObjectID
                                               select a);

            List<ActivityObjectRelationship> activityObjectRelationshipList = activityObjectRelationQuery.ToList();
            application.IRISObject.ActivityObjectRelationships.AddTrackableCollection(activityObjectRelationshipList);
 
            //When Application creation is triggered by Online Self Service, the application record may have
            //no contact and no addressForService
            if (application.ServiceAddressID.HasValue)
            {
                application.ServiceAddress = (from servAddr in Context.ServiceAddresses
                                                    .Include(a => a.Email)
                                                    .Include(a => a.ContactAddress)
                                              where servAddr.ID == application.ServiceAddressID.Value
                                              select servAddr).Single();

                if (application.ServiceAddress.ContactAddressID != null)
                {
                    application.ServiceAddress.ContactAddress.Address = RepositoryMap.AddressRepository.GetAddressByID(application.ServiceAddress.ContactAddress.AddressID);
                }
            }
            
            //Get statuses and plans/rules/policies separately to make the resulting SQL query much easier
            var appStatusesQuery = from appStatus in Context.Statuses
                                                    .Include(s => s.StatusREF)
                                                    .Where(s => s.IRISObjectID == application.IRISObject.ID && !s.IsDeleted)
                                   select appStatus;

            application.IRISObject.Statuses.AddRange(appStatusesQuery.ToList());

            PopulateHoldsAndTimeFrameCreatedByDisplayName(application);

            // If our application is a resource consent type then we pull all the objection and appeal records as well
            ApplicationResourceConsent applicationResourceConsentObject = application as ApplicationResourceConsent;
            if (applicationResourceConsentObject != null)
            {
                applicationResourceConsentObject.NotificationDecisionOutcome = (from n in Context.ReferenceDataValue
                                                                                where n.ID == applicationResourceConsentObject.NotificationDecisionOutcomeID
                                                                                select n).SingleOrDefault().TrackAll();

                applicationResourceConsentObject.CallInInitiatedBy = (from n in Context.ReferenceDataValue
                                                                      where n.ID == applicationResourceConsentObject.CallInInitiatedByID
                                                                      select n).SingleOrDefault().TrackAll();

                applicationResourceConsentObject.DirectReferralRequestOutcome = (from n in Context.ReferenceDataValue
                                                                                 where n.ID == applicationResourceConsentObject.DirectReferralRequestOutcomeID
                                                                                 select n).SingleOrDefault().TrackAll();

                var objectionsQuery = (from o in Context.Objections
                                           .Include(o => o.Objector)
                                           .Include(o => o.ObjectingTo)
                                           .Include(o => o.Decision)
                                           .Include(o => o.DecisionMaker)
                                       where o.ResourceConsentID == applicationId && !o.IsDeleted
                                       select o);

                List<Objection> objections = objectionsQuery.AsEnumerable().Select(o => o).ToList();

                var appealsQuery = from appeal in Context.Appeals.Where(a => a.ResourceConsentID == applicationId && !a.IsDeleted)
                                   from appellantREF in Context.ReferenceDataValue.Where(i => appeal.AppellantID == i.ID).DefaultIfEmpty()
                                   from decision in Context.ReferenceDataValue.Where(i => i.ID == appeal.DecisionID).DefaultIfEmpty()
                                   from decisionProcess in Context.ReferenceDataValue.Where(i => i.ID == appeal.DecisionProcessID).DefaultIfEmpty()
                                   from appealAppellant in Context.AppealAppellants.Where(i => i.AppealID == appeal.ID && !i.IsDeleted).DefaultIfEmpty()
                                   from relationship in Context.ActivityObjectRelationship.Where(x => x.ID == appealAppellant.ActivityObjectRelationshipID).DefaultIfEmpty()
                                   from contactSubLink in Context.ContactSubLink.Where(x => relationship.ID == x.ActivityObjectRelationshipID).DefaultIfEmpty()
                                   from contactName in Context.Name.Where(x => !x.IsDeleted && contactSubLink.NameID == x.ID).DefaultIfEmpty()
                                   from contact in Context.Contact.Where(x => contactSubLink.ContactID == x.ID).DefaultIfEmpty()
                                   from organisationStatusREF in Context.ReferenceDataValue.Where(x => contact.OrganisationStatusID == x.ID).DefaultIfEmpty()

                                   select new { appeal, appellantREF, decision, decisionProcess, appealAppellant, relationship, contactSubLink, contactName, contact, organisationStatusREF };

                List<Appeal> appeals = appealsQuery.AsEnumerable().Distinct().Select(x => x.appeal).ToList();

                PopulateObjectionsCreatedByDisplayName(applicationResourceConsentObject);
                PopulateAppealsCreatedByDisplayName(applicationResourceConsentObject);

                applicationResourceConsentObject.Objections.AddTrackableCollection(objections);
                applicationResourceConsentObject.Appeals.AddTrackableCollection(appeals);
            }

            // get RMA2 effective date
            DateTime result = DateTime.MaxValue;
            var appSetting = Context.AppSetting.SingleOrDefault(a => a.Key == AppSettingConstants.Application_RMA2EffectiveDate);
            if (appSetting != null && DateTime.TryParse(appSetting.Value, out result))
            {
                application.RMA2EffectiveDate = result;
            }

            result = DateTime.MaxValue;
            var appSettingRMA2020 = Context.AppSetting.SingleOrDefault(a => a.Key == AppSettingConstants.RMA2020EffectiveDate_Consents);
            if (appSettingRMA2020 != null && DateTime.TryParse(appSettingRMA2020.Value, out result))
            {
                application.RMA2020EffectiveDate = result;
            }
            return application.TrackAll();
        }



       public Activity GetActivityByIrisObjectID(long irisObjectId)
        {
            var query = from act in Context.Activities
                                   .Include(a => a.PrimaryIndustryPurpose)
                                   .Include(a => a.SecondaryIndustryPurpose)
                                   .Include(a => a.DurationType)
                                   .Include(a => a.ExpectedLifeTime)
                                   .Include(a => a.ApplicationPurpose)
                                   .Include(a => a.ActivityOutcomes)
                                   .Include(a => a.Authorisation)
                                   .Include(a => a.Authorisation.IRISObject)
                                   .Where(a => a.IRISObjectID == irisObjectId)
                        select act;


            var activity = query.AsEnumerable().First();

            activity.IRISObject = (from n in Context.IRISObject
                                            .Include(o => o.ObjectTypeREF)
                                            .Include(o => o.SubClass1REF)
                                            .Include(o => o.SubClass2REF)
                                            .Include(o => o.SubClass3REF)
                                            .Include(o => o.OtherIdentifiers)
                                            .Include(o => o.SecurityContextIRISObject.ObjectTypeREF)                                        
                                            .Include("OtherIdentifiers.IdentifierContextREF")
                                   where n.ID == irisObjectId
                                   select n).Single();

            var actConditionsQuery = from cond in Context.ActivityConditions
                                          .Include(c => c.Condition.Parameters)
                                          .Include(c => c.Condition.ConditionType)
                                          .Where(c => c.ActivityID == activity.ID)
                                     select cond;

            var actConditions = actConditionsQuery.Where(ac => !ac.Condition.IsDeleted);
            activity.ActivityConditions.AddTrackableCollection(actConditions.ToList());


            //Get activity outcomes
            var actOutcomes = from actOutcome in Context.ActivityOutcomes
                                                    .Include(o => o.ActivityOutcomeREF)
                                                    .Include(o => o.ActivityStatusREF)
                                                    .Where(activityOutcome => activityOutcome.ActivityID == activity.ID && !activityOutcome.IsDeleted)
                              select actOutcome;
            activity.ActivityOutcomes.AddTrackableCollection(actOutcomes.ToList());



            var coc = activity as ActivityCertificateOfCompliance;
            if (coc != null && coc.ActivityClassID.HasValue)
            {
                coc.ActivityClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == coc.ActivityClassID);

            }

            var resource = activity as ActivityResourceConsent;
            if (resource != null && resource.ActivityClassID.HasValue)
            {
                resource.ActivityClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == resource.ActivityClassID);
            }


            var specialEvent = activity as ActivitySpecialEvent;
            if (specialEvent != null)
            {
                specialEvent.ActivityClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == specialEvent.ActivityClassID);
                specialEvent.NotificationResponsibility = Context.ReferenceDataValue.Single(rdv => rdv.ID == specialEvent.NotificationResponsibilityID);
                List<ActivitySpecialEventDate> activitySpecialEventDates = Context.ActivitySpecialEventDates.Where(d => d.ActivitySpecialEventID == specialEvent.ID).ToList();

                foreach (ActivitySpecialEventDate activitySpecialEventDate in activitySpecialEventDates)
                {
                    activitySpecialEventDate.SpecialEventType = Context.ReferenceDataValue.Single(rdv => rdv.ID == activitySpecialEventDate.SpecialEventTypeID);
                }

                specialEvent.ActivitySpecialEventDates.AddTrackableCollection(activitySpecialEventDates);
            }

            var grant = activity as ActivityGrant;

            if (grant != null)
            {
                if (grant.PrimaryClassID.HasValue)
                {
                    grant.PrimaryClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == grant.PrimaryClassID);
                }

                if (grant.SecondaryClassID.HasValue)
                {
                    grant.SecondaryClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == grant.SecondaryClassID);
                }

                if (grant.TerritorialAuthorityID.HasValue)
                {
                    grant.TerritorialAuthority = Context.ReferenceDataValue.Single(rdv => rdv.ID == grant.TerritorialAuthorityID);
                }
            }

            return activity.TrackAll();
        }

        public Activity GetActivityByAuthorisationIRISID(string authorisationIRISID)
        {
            var query = from act in Context.Activities.Where(a => a.AuthorisationIrisID == authorisationIRISID)
                        select act;
            var activity = query.AsEnumerable().FirstOrDefault();
            return activity.TrackAll();
        }
        
        public Activity GetActivityByIDForDetailsPage(long activityID)
        {
            var query = from act in Context.Activities
                                    .Include(a => a.PrimaryIndustryPurpose)
                                    .Include(a => a.PrimaryIndustry)
                                    .Include(a => a.PrimaryPurpose)
                                    .Include(a => a.SecondaryIndustryPurpose)
                                    .Include(a => a.SecondaryIndustry)
                                    .Include(a => a.SecondaryPurpose)
                                    .Include(a => a.DurationType)
                                    .Include(a => a.ExpectedLifeTime)
                                    .Include(a => a.ApplicationPurpose)
                                    .Include(a => a.ActivityOutcomes)
                                    .Include("ActivityOutcomes.ActivityOutcomeREF")
                                    .Include(a => a.Application)
                                    .Include("Application.IRISObject")
                                    .Include("Application.IRISObject.SubClass1REF")
                                    .Include("Application.IRISObject.Statuses")                                    
                                    .Include("Application.IRISObject.Statuses.StatusREF")
                                    .Include("Application.IRISObject.Clock")
                                    .Include("Application.IRISObject.Clock.Holds")
                                    .Include("Application.IRISObject.Clock.Holds.Reason")
                                    .Include("Application.IRISObject.Clock.Timeframes")
                                    .Include("Application.IRISObject.Clock.Timeframes.TimeframeType")
                                    .Include(a => a.Authorisation)
                                    .Include(a => a.AuthorisationOfficerResponsible)
                                    .Include(a => a.Authorisation.IRISObject)
                                    .Where(a => a.ID == activityID)
                        select act;


            var activity = query.AsEnumerable().First();

            activity.IRISObject = (from n in Context.IRISObject
                                            .Include(o => o.ObjectTypeREF)
                                            .Include(o => o.SubClass1REF)
                                            .Include(o => o.SubClass2REF)
                                            .Include(o => o.SubClass3REF)
                                            .Include(o => o.OtherIdentifiers)
                                            .Include(o => o.SecurityContextIRISObject.ObjectTypeREF)
                                            .Include("OtherIdentifiers.IdentifierContextREF")
                                   where n.ID == activity.IRISObjectID
                                   select n).Single();


            /* 
             * The following set of queries will get  the collections for Plans, statuses and conditions.
             * 
             */
            
            RepositoryHelpers.LoadPlansRulesPoliciesForEntity(Context, activity);

            //Getting Activity Conditions
            var actConditionsQuery = from cond in Context.ActivityConditions
                                          .Include(c => c.Condition.Parameters)
                                          .Include(c => c.Condition.ConditionType)
                                          .Where(c => c.ActivityID == activityID)
                                      select cond;

            var actConditions = actConditionsQuery.Where(ac => !ac.Condition.IsDeleted);
            activity.ActivityConditions.AddTrackableCollection(actConditions.ToList());


            //Get activity Outcomes/Statuses
            var actOutcomes = from actOutcome in Context.ActivityOutcomes
                                                    .Include(o => o.ActivityOutcomeREF)
                                                    .Include(o => o.ActivityStatusREF)
                                                    .Where(activityOutcome => activityOutcome.ActivityID == activityID && !activityOutcome.IsDeleted)
                              select actOutcome;
            activity.ActivityOutcomes.AddTrackableCollection(actOutcomes.ToList());            
            /* 
             * The following set of queries will get the type specific fields for the appropriate Activity type.
             * 
             */

            //Getting the Activity Certificate of Compliance type specific fields.
            var coc = activity as ActivityCertificateOfCompliance;
            if (coc != null && coc.ActivityClassID.HasValue)
            {
                coc.ActivityClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == coc.ActivityClassID);

            }

            //Getting the Activity Resource Consent type specific fields.
            var resource = activity as ActivityResourceConsent;
            if (resource != null && resource.ActivityClassID.HasValue)
            {
                resource.ActivityClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == resource.ActivityClassID);
            }

            //Getting the Activity Special Event type specific fields.
            var specialEvent = activity as ActivitySpecialEvent;
            if (specialEvent != null)
            {
                specialEvent.ActivityClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == specialEvent.ActivityClassID);
                specialEvent.NotificationResponsibility = Context.ReferenceDataValue.Single(rdv => rdv.ID == specialEvent.NotificationResponsibilityID);

                //TODO - review LINQ
                List<ActivitySpecialEventDate> activitySpecialEventDates = Context.ActivitySpecialEventDates.Where(d => d.ActivitySpecialEventID == specialEvent.ID && !d.IsDeleted).ToList();

                foreach (ActivitySpecialEventDate activitySpecialEventDate in activitySpecialEventDates)
                {
                    activitySpecialEventDate.SpecialEventType = Context.ReferenceDataValue.Single(rdv => rdv.ID == activitySpecialEventDate.SpecialEventTypeID);
                }

                specialEvent.ActivitySpecialEventDates.AddTrackableCollection(activitySpecialEventDates);
            }

            //Getting the Activity Grant type specific fields.
            var grant = activity as ActivityGrant;
            if (grant != null)
            {
                if (grant.PrimaryClassID.HasValue)
                {
                    grant.PrimaryClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == grant.PrimaryClassID);
                }

                if (grant.SecondaryClassID.HasValue)
                {
                    grant.SecondaryClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == grant.SecondaryClassID);
                }

                if (grant.TerritorialAuthorityID.HasValue)
                {
                    grant.TerritorialAuthority = Context.ReferenceDataValue.Single(rdv => rdv.ID == grant.TerritorialAuthorityID);
                }
            }
            return activity.TrackAll();
        }

        public Activity GetActivityByID(long activityActivityID)
        {
            return GetById<Activity>(activityActivityID,"SubClass1-3,Application.SubClass1,Application.LatestStatus,Application.Holds,Application.Timeframes,PrimaryIndustry,SecondaryIndustry,PrimaryPurpose,SecondaryPurpose");
        }

        /// <summary>
        ///  By given an activity IRISObjectID, we get the application it linked. 
        ///  Then we return the latest status of application with the IRISObjectID 
        ///  of the activity
        /// </summary>
        /// <param name="irisObjectIDs"></param>
        /// <returns></returns>
        public Dictionary<long, string> GetApplicationStatusPairsForActivities(List<long> irisObjectIDs)
        {
            // get the pairs of (relatedAppplication.IRISObjectID, activity.IRISObjectID)
            Dictionary<long, long> applicationIRISObjectIDs = Context.Activities.Include(a => a.Application)
                                                                                .WhereIn(a => a.IRISObjectID, irisObjectIDs)
                                                                                .Select(c => new { ActivityIRISObjectID = c.IRISObjectID, ApplicationIRISObjectID = c.Application.IRISObjectID })
                                                                                .ToDictionary( c => c.ApplicationIRISObjectID, c => c.ActivityIRISObjectID);

            // get the latest status for the application, and return the pair (activity.IRISObjectID, status)
            return Context.SearchIndex.Where(si => si.IRISObjectID.HasValue && applicationIRISObjectIDs.Keys.Contains(si.IRISObjectID.Value))
                                        .ToDictionary(si => applicationIRISObjectIDs[si.IRISObjectID.Value], si => si.Status);
        }

        public IRISObject GetIRISObjectForApplicationByActivityID(long activityID)
        {
            var activity = Context.Activities
                .Include(a => a.Application.IRISObject)
                .Include(a => a.Application.IRISObject.ObjectTypeREF)
                .Single(a => a.ID == activityID);
            return activity.Application.IRISObject;
        }

        public IRISObject GetIRISObjectForApplicationByApplicationID(long applicationID)
        {
            var application = Context.Applications
                .Include(a => a.IRISObject.ObjectTypeREF)
                .Single(a => a.ID == applicationID);
            return application.IRISObject;
        }

        #region Authorisations

        /// <summary>
        /// Gets a fully populated authorisation object given its ID
        /// </summary>
        /// <param name="authorisationID"></param>
        /// <returns></returns>
        public Authorisation GetAuthorisationByID(long authorisationID)
        {

            var query = from auth in Context.Authorisations

                                    .Include(a => a.OfficerResponsible)
                                    .Include(a => a.PrimaryIndustryPurpose)
                                    .Include(a => a.PrimaryIndustry)
                                    .Include(a => a.PrimaryPurpose)
                                    .Include(a => a.SecondaryIndustryPurpose)
                                    .Include(a => a.SecondaryIndustry)
                                    .Include(a => a.SecondaryPurpose)
                                    .Include(a => a.DurationType)
                                    .Include(a => a.ExpectedLifetime)
                                    .Include(a => a.PreviousAuthorisation)
                                    .Include("PreviousAuthorisation.IRISObject")
                                    .Include(a => a.RelatedAuthorisations)
                                    .Include(a=>a.RelatedAuthorisations1)
                                    .Include("RelatedAuthorisations.RelationAuthorisation.IRISObject.Statuses.StatusREF")
                                    .Include("Activity")
                                    .Include("Activity.IRISObject")
                                    .Include("Activity.IRISObject.SubClass2REF")
                                    .Include("Activity.IRISObject.SubClass3REF")

                                    .Where(a => a.ID == authorisationID)
                        select auth;

            var authorisation = query.AsEnumerable().First();

            //Get single objects: IRIS object and Service address
            authorisation.IRISObject = (from n in Context.IRISObject
                                            .Include(o => o.ObjectTypeREF)
                                            .Include(o => o.SubClass1REF)
                                            .Include(o => o.SubClass2REF)
                                            .Include(o => o.SubClass3REF)
                                            .Include(o => o.OtherIdentifiers)
                                            .Include("OtherIdentifiers.IdentifierContextREF")

                                        where n.ID == authorisation.IRISObjectID
                                        select n).Single();

            //Get other objects/collections seprately to make the query faster
            var authStatusesQuery = from authStatus in Context.Statuses
                                                    .Include(s => s.StatusREF.ReferenceDataValueAttribute)
                                                    .Where(s => s.IRISObjectID == authorisation.IRISObjectID && !s.IsDeleted)
                                    select authStatus;

            authorisation.IRISObject.Statuses.AddTrackableCollection(authStatusesQuery.ToList());


            authorisation.ServiceAddress = (from servAddr in Context.ServiceAddresses
                                                .Include(a => a.Email)
                                                .Include(a => a.ContactAddress)
                                            where servAddr.ID == authorisation.ServiceAddressID
                                            select servAddr).Single();
            if (authorisation.ServiceAddress.ContactAddressID != null)
            {
                authorisation.ServiceAddress.ContactAddress.Address = RepositoryMap.AddressRepository.GetAddressByID(authorisation.ServiceAddress.ContactAddress.AddressID);
            }
            //Get collections: Plans, statuses and conditions
            RepositoryHelpers.LoadPlansRulesPoliciesForEntity(Context, authorisation);

            var authConditionsQuery = from cond in Context.AuthorisationConditions
                                                      .Include(c => c.Condition.Parameters)
                                                      .Include(c => c.Condition.ConditionType)
                                                      .Where(c => c.AuthorisationID == authorisationID)
                                      select cond;

            var authConditions = authConditionsQuery.Where(ac => !ac.Condition.IsDeleted);
            authorisation.AuthorisationConditions.AddTrackableCollection(authConditions.ToList());

            //Get related AuthorisationCharges
            var authChargesQuery = from charge in Context.AuthorisationCharge
                                          .Include(c => c.AuthorisationChargeTypeREF)
                                          .Include(c => c.FinancialYearREF)
                                          .Include(c => c.FinancialYearPeriodREF)
                                          .Where(c => c.AuthorisationID == authorisationID)
                                   select charge;

            var authCharges = authChargesQuery.Where(ac => !ac.IsDeleted);
            authorisation.AuthorisationCharges.AddTrackableCollection(authCharges.ToList());

            GetTypeSpecificDataForAuthorisation(authorisation);
            PopulatePreviousActivityAndPreviousAuthorisation(authorisation);
            PopulateRelatedApplicationsInformation(authorisation);

            return authorisation.TrackAll();
        }

        public Authorisation GetSimpleAuthorisationByID(long authorisationID)
        {
            return Context.Authorisations
                .Include(a => a.IRISObject.ObjectTypeREF)
                .Include(a => a.IRISObject.SubClass1REF)
                .Include(a => a.IRISObject.SubClass2REF)
                .Include(a => a.IRISObject.SubClass3REF)
                .Where(o => o.ID == authorisationID).Single();
        }

        public Authorisation GetAuthorisationByBusinessID(string businessId)
        {
            return Context.Authorisations
                .Include(a => a.IRISObject.ObjectTypeREF)
                .Include(a => a.PrimaryIndustry)
                .Include(a => a.PrimaryPurpose)
                .Include(a => a.SecondaryIndustry)
                .Include(a => a.SecondaryPurpose)
                .Include(a => a.IRISObject.SubClass1REF)
                .Include(a => a.IRISObject.SubClass2REF)
                .Include(a => a.IRISObject.SubClass3REF)
                .Where(o => o.IRISObject.BusinessID == businessId).SingleOrDefault();
        }

        public AuthorisationData GetAuthorisationByActivityID(long activityID, long irisObjectID)
        {
            //The reason for linking through IRISobject table instead of the more obvious Activity table is because
            //Activity has multiple subclasses and EF4 is not performing well when querying subclass using TPT (table per type) inheritance
            //var query = from auth in Context.Authorisations
            //            join activity in Context.Activities on auth.ActivityID equals activity.ID
            //            where activity.ID == activityID && activity.IRISObjectID == irisObjectID
            var query =  from irisObject in Context.IRISObject
                         join auth in Context.Authorisations on irisObject.LinkID equals auth.ActivityID
                         where irisObject.ID == irisObjectID && irisObject.LinkID == activityID
                         select new AuthorisationData
                         {
                            ID = auth.ID,
                            AuthorisationName = auth.AuthorisationName
                         };

            return query.SingleOrDefault();
        }

        public Dictionary<long, KeyValuePair<long, string>> GetAuthorisationIDsIRISIDsByActivitiesIDs(List<long> activityIDs, List<long> irisObjectIDs)
        {
            var query = from irisObject in Context.IRISObject
                        join auth in Context.Authorisations on irisObject.LinkID equals auth.ActivityID into authorisations
                        from a in authorisations.DefaultIfEmpty()
                        where irisObjectIDs.Contains(irisObject.ID) && activityIDs.Contains(irisObject.LinkID.Value)
                        select new { actID = irisObject.LinkID.Value, authID = (a==null ? 0 : a.ID) , irisID = (a==null ? "" : a.IRISObject.BusinessID) };
            return query.ToDictionary(x=> x.actID, x=> new KeyValuePair<long, string>(x.authID, x.irisID));
        }

        /// <summary>
        /// Gets a fully populated authorisation object given its IRIS Object ID
        /// </summary>
        /// <param name="authorisationID"></param>
        /// <returns></returns>
        public Authorisation GetAuthorisationByIrisObjectId(long irisObjectId)
        {
            long authorisationID = Context.Authorisations.FirstOrDefault(a => a.IRISObjectID == irisObjectId).ID;
            return GetAuthorisationByID(authorisationID);
        }


        public List<AuthorisationCondition> GetAuthorisationCondtionsByAuthorisationId(long authorisationID)
        {
            var query =
                from condition in
                    Context.AuthorisationConditions.Include(c=>c.Condition).Where(a => a.AuthorisationID == authorisationID)
                select condition;

            return query.ToList().TrackAll();
        }

        public IRISObject GetIRISObjectByAuthorisationID(long authorisationID)
        {
            var auth = Context.Authorisations
                .Include(a => a.IRISObject)
                .Include(a => a.IRISObject.ObjectTypeREF)
                .Single(a => a.ID == authorisationID);
            return auth.IRISObject;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="irisObjectIDs"></param>
        /// <returns></returns>
        public Dictionary<long, string> GetAuthorisationStatusPairs(List<long> irisObjectIDs)
        {
            return Context.SearchIndex.Where(si => si.IRISObjectID.HasValue && irisObjectIDs.Contains(si.IRISObjectID.Value)).ToDictionary(si=>si.IRISObjectID.Value, si=>si.Status);
        }

        public List<RegimeActivityComplianceAuthorisation> GetRegimeActivityComplianceByAuthorisationId(long authorisationId)
        {
            return Context.RegimeActivityComplianceAuthorisations.Include(x => x.RegimeActivityComplianceConditions).Where(r => r.AuthorisationID == authorisationId).ToList();
        }

        #endregion

        private void PopulateHoldsAndTimeFrameCreatedByDisplayName(Application application)
        {
            var users = RepositoryMap.SecurityRepository.GetUserList();

            List<Clock> clocks = application.IRISObject.Clock.ToList();
            clocks.ForEach(clock =>
            {
                clock.Holds.ForEach(hold =>
                {
                    var user = users.FirstOrDefault(u => u.AccountName.ToLower() == hold.CreatedBy.ToLower());
                    hold.HoldCreatedByUserDisplayValue = user != null ? user.DisplayName : "System";
                });

                clock.TimeFrames.ForEach(timeframe =>
                {
                    var user = users.FirstOrDefault(u => u.AccountName.ToLower() == timeframe.CreatedBy.ToLower());
                    timeframe.TimeFrameCreatedByUserDisplayValue = user != null ? user.DisplayName : "System";
                    if (timeframe.ModifiedBy != null)
                    {
                        var userUpdated = users.FirstOrDefault(u => u.AccountName.ToLower() == timeframe.ModifiedBy.ToLower());
                        timeframe.TimeFrameUpdatedByUserDisplayValue = userUpdated != null ? userUpdated.DisplayName : "System";
                    }
                    else
                    {
                        timeframe.TimeFrameUpdatedByUserDisplayValue = user != null ? user.DisplayName : "System";
                    }
                });
            });
        }

        private void PopulateObjectionsCreatedByDisplayName(ApplicationResourceConsent applicationResourceConsent)
        {
            var users = RepositoryMap.SecurityRepository.GetUserList();

            List<Objection> objections = applicationResourceConsent.Objections.ToList();
            objections.ForEach(objection =>
            {
                var user = users.FirstOrDefault(u => u.AccountName.ToLower() == objection.CreatedBy.ToLower());
                objection.CreatedByUserDisplayValue = user != null ? user.DisplayName : "System";
            });
        }

        private void PopulateAppealsCreatedByDisplayName(ApplicationResourceConsent applicationResourceConsentObject)
        {
            var users = RepositoryMap.SecurityRepository.GetUserList();

            List<Appeal> appeals = applicationResourceConsentObject.Appeals.ToList();
            appeals.ForEach(appeal =>
            {
                var user = users.FirstOrDefault(u => u.AccountName.ToLower() == appeal.CreatedBy.ToLower());
                appeal.CreatedByUserDisplayValue = user != null ? user.DisplayName : "System";
            });
        }

        /// <summary>
        /// Populates the data (mainly ref data) that is authorisation-type specific
        /// </summary>
        /// <param name="authorisation"></param>
        private void GetTypeSpecificDataForAuthorisation(Authorisation authorisation)
        {
            var coc = authorisation as AuthorisationCertificateOfCompliance;
            if (coc != null && coc.ActivityClassID.HasValue)
            {
                coc.ActivityClass = Context.ReferenceDataValue.Single(rdv => rdv.ID == coc.ActivityClassID);
            }

            var resConsent = authorisation as AuthorisationResourceConsent;
            if (resConsent != null)
            {
                resConsent.ActivityClass = !resConsent.ActivityClassID.HasValue ? null :
                                                Context.ReferenceDataValue.Single(rdv => rdv.ID == resConsent.ActivityClassID);
                resConsent.S124Status = !resConsent.S124StatusID.HasValue ? null :
                                             Context.ReferenceDataValue.Single(rdv => rdv.ID == resConsent.S124StatusID);
            }

            var specialEvt = authorisation as AuthorisationSpecialEvent;
            if (specialEvt != null)
            {
                specialEvt.NotificationResponsibility = !specialEvt.NotificationResponsibilityID.HasValue ? null :
                                            Context.ReferenceDataValue.Single(rdv => rdv.ID == specialEvt.NotificationResponsibilityID);

                specialEvt.ActivityClass = !specialEvt.ActivityClassID.HasValue ? null :
                                                Context.ReferenceDataValue.Single(rdv => rdv.ID == specialEvt.ActivityClassID);

                var specialEventDatesQuery = from sda in Context.AuthorisationSpecialEventDates
                                             .Include(ev => ev.SpecialEventType)
                                             .Where(ev => ev.AuthorisationSpecialEventID == specialEvt.ID && !ev.IsDeleted)
                                             select sda;
                specialEvt.AuthorisationSpecialEventDates.AddTrackableCollection(specialEventDatesQuery.ToList());
            }
        }

        public List<Authorisation> GetAuthorisationsByPreviousAuthorisationID(long PreviousAuthorisationID)
        {
            var activityIDs = Context.Activities.Where(a => a.PreviousAuthorisationID == PreviousAuthorisationID).Select(a=>a.ID).ToList();
            return Context.Authorisations.Include(a => a.IRISObject).Where(a => activityIDs.Contains(a.ActivityID.Value) ).ToList();
        }

        private void PopulatePreviousActivityAndPreviousAuthorisation(Authorisation authorisation)
        {
            if (authorisation.ActivityID != null)
            {
                var activity = Context.Activities
                                .Include(a => a.Application)
                                .Single(a => a.ID == authorisation.ActivityID.Value);

                authorisation.Activity = activity;

                if (activity.PreviousAuthorisationID.HasValue)
                {
                    var previousAuth = Context.Authorisations
                                                .Include(a => a.IRISObject)
                                                .Include(a => a.IRISObject.ObjectTypeREF)
                                                .Single(a => a.ID == activity.PreviousAuthorisationID.Value);

                    authorisation.ActivityPreviousAuthorisationID = activity.PreviousAuthorisationID;
                    authorisation.ActivityPreviousAuthorisationIRISID = previousAuth.IRISObject.BusinessID;
                }

            }
        }

        /// <summary>
        ///  1. Populate all the related applications which are initiated from the current Authorisation
        ///  2. Check if any related applications of the current authorisation have status rather than DecisionMade or DecisionServed
        /// </summary>
        /// <param name="authorisation"></param>
        private void PopulateRelatedApplicationsInformation(Authorisation authorisation)
        {
            var dbQuery = Context.Activities
                .Include(x => x.Application)
                .Include(x => x.Application.IRISObject)
                .Include(x => x.Application.IRISObject.Statuses)
                .Include("Application.IRISObject.Statuses.StatusREF")
                .Where(x => x.PreviousAuthorisationID == authorisation.ID);

            authorisation.RelatedApplications = dbQuery.AsEnumerable().Select(x => x.Application).ToList();

            // Check if any related applications of the current authorisation have status rather than DecisionMade or DecisionServed
            Expression<Func<ReferenceDataValue, bool>> expression = r => r.ReferenceDataCollection.Code == ReferenceDataCollectionCode.ApplicationStatuses
                                                                        && (r.Code == ReferenceDataValueCodes.ApplicationStatus.DecisionMade
                                                                            || r.Code == ReferenceDataValueCodes.ApplicationStatus.DecisionServed);

            var decisionIDs = Context.ReferenceDataValue.Where(expression);

            authorisation.HasActiveRelatedApplication = authorisation.RelatedApplications.Any(application =>
            {
                var currentStatus = application.IRISObject.LatestStatus;                
                return currentStatus!=null && decisionIDs.All(d => d.ID != currentStatus.StatusREFID);
            });

            if (authorisation.HasActiveRelatedApplication) return;

            var linkedApplications = new LinkingRepository()
                .GetRelatedIrisObjects(authorisation.IRISObjectID, new[] { ReferenceDataValueCodes.ObjectType.Application });

            if (linkedApplications.ContainsKey(ReferenceDataValueCodes.ObjectType.Application))
            {
                authorisation.HasActiveRelatedApplication = linkedApplications[ReferenceDataValueCodes.ObjectType.Application]
                .Any(irisObject =>
                {
                    irisObject.Statuses.AddTrackableCollection(Context.Statuses.Include(x => x.StatusREF).Where(x => x.IRISObjectID == irisObject.ID).ToList());
                    var currentStatus = irisObject.LatestStatus;
                    return currentStatus != null && decisionIDs.All(d => d.ID != currentStatus.StatusREFID);
                });
            }
        }

        /// <summary>
        ///    Call to the CalculateMilestoneAndDate Stored Procedure 
        /// </summary>
        public string CalculateMilestoneAndDate(long? applicationID)
        {
            ObjectParameter milestone = new ObjectParameter("Milestone", typeof(string));
            ObjectParameter milestoneDate = new ObjectParameter("MilestoneDate", typeof(string));
            Context.CalculateMilestoneAndMilestoneDate(applicationID, milestone, milestoneDate);

            string mileStoneStr = (milestone.Value != null && !Convert.IsDBNull(milestone.Value)) ? (string)milestone.Value : string.Empty;
            string mileStoneDateStr = (milestoneDate.Value != null && !Convert.IsDBNull(milestoneDate.Value)) ? (string)milestoneDate.Value : string.Empty;

            return mileStoneStr + ":" + mileStoneDateStr;
        }
        
        public AuthorisationGroup GetAuthorisationGroupByID(long authorisationGroupId)
        {
            return (from authGroup in Context.AuthorisationGroups
                        .Include(a => a.IRISObject)
                        .Include(a => a.IRISObject.ObjectTypeREF)
                        .Include(a => a.IRISObject.SubClass1REF)
                        .Include(a => a.AuthorisationTypeREF)
                        .Include(a => a.Owner)
                        .Where(authGroup => authGroup.ID == authorisationGroupId)
                    select authGroup).Single();
        }

        public List<Authorisation> GetAuthorisationGroupAuthorisationsByGroupIRISID(long authGroupObjectID)
        {
            var dbquery = from link in Context.ActivityObjectRelationship.Where(l => l.IRISObjectID == authGroupObjectID || l.RelatedIRISObjectID == authGroupObjectID)
                          from relatedObject in Context.IRISObject.Where(o => o.ID == link.IRISObjectID && link.RelatedIRISObjectID == authGroupObjectID || o.ID == link.RelatedIRISObjectID && link.IRISObjectID == authGroupObjectID )
                          from objectType in Context.ReferenceDataValue.Where(t => t.ID == relatedObject.ObjectTypeID && t.Code == ReferenceDataValueCodes.ObjectType.Authorisation)
                          from auth in Context.Authorisations.Where(a => a.IRISObjectID == relatedObject.ID)
                          select new {auth};

            return dbquery.AsEnumerable().Select(x => x.auth).Distinct().ToList();
        }

        public void CopyActivityCDFAnswersToAuthorisation(long authorisationID)
        {
             Context.CopyActivityCDFAnswersToAuthorisation(authorisationID);
        }

        public List<Authorisation> GetRegimeActivityLinkedAuthorisation(long regimeActivityID)
        {
            var dbquery = from regAuth in Context.RegimeActivityComplianceAuthorisations.Where(a => a.RegimeActivityComplianceID == regimeActivityID && !a.IsDeleted)
                          from auth in Context.Authorisations.Where(a => a.ID == regAuth.AuthorisationID)
                          from authIRISObject in Context.IRISObject.Where(i => i.ID == auth.IRISObjectID)
                          from irisObjectTypeREF in Context.ReferenceDataValue.Where(x => x.ID == authIRISObject.ObjectTypeID)
                          select new { auth, authIRISObject, irisObjectTypeREF };

            return dbquery.AsEnumerable().Select(x => x.auth).Distinct().ToList();
        }

        public List<Authorisation> GetRegimeActivityObservedAuthorisation(long regimeActivityID)
        {
            var dbquery = from obs in Context.Observations.Where(o => o.RegimeActivityID == regimeActivityID)
                          from obsAuth in Context.ObservationComplianceAuthorisations.Where(a => a.ObservationComplianceID == obs.ID && !a.IsDeleted)
                          from auth in Context.Authorisations.Where(a => a.ID == obsAuth.AuthorisationID)
                          from authIRISObject in Context.IRISObject.Where(i => i.ID == auth.IRISObjectID)
                          from irisObjectTypeREF in Context.ReferenceDataValue.Where(x => x.ID == authIRISObject.ObjectTypeID)
                          select new { auth, authIRISObject, irisObjectTypeREF };

            return dbquery.AsEnumerable().Select(x => x.auth).Distinct().ToList();
        }

        public List<AuthorisationObservationRow> GetAuthorisationLinkedObservations(long authorisationID, long authIRISID)
        {


            // for observation created with authorisation 
            var dbquery = from obsAuth in Context.ObservationComplianceAuthorisations.Where(a => a.AuthorisationID == authorisationID && !a.IsDeleted)
                          from obs in Context.Observations.Where(o => o.ID == obsAuth.ObservationComplianceID)
                          select new { obs, obsAuth };

            var result = dbquery.Select(q => new AuthorisationObservationRow
            {
                ID = q.obs.ID,
                IRISObjectID = q.obs.IRISObjectID,
                ObservationDate = q.obs.ObservationDate,
                ObservationTime = q.obs.ObservationTime,
                ObservationIRISID = q.obs.IRISObject.BusinessID,
                TypeSubtype = q.obs.IRISObject.SubClass2REF.DisplayValue + ( q.obs.IRISObject.SubClass3REF != null ? " | " + q.obs.IRISObject.SubClass3REF.DisplayValue : string.Empty),
                OfficerResponsible = q.obs.ObservationObservingOfficers.Where(o=>!o.IsDeleted).OrderBy(o=>o.ID).FirstOrDefault().User.DisplayName,
                NoOfConditionsAssessed = q.obsAuth.ObservationComplianceConditions.Count(c => !c.IsDeleted),

                NoOfConditionsLinked = q.obs.AuthorisationID != null ? q.obsAuth.AuthorisationComplianceConditions.Count :
                                       (q.obs.RegimeActivity as RegimeActivityCompliance).RegimeActivityComplianceAuthorisations.FirstOrDefault(a => a.AuthorisationID == authorisationID && !a.IsDeleted).RegimeActivityComplianceConditions.Count,
                AuthorisationCompliance = q.obsAuth.ComplianceStatusREF.DisplayValue,
                ConditionCompliance = q.obsAuth.ObservationComplianceConditions.Where(c => c.ComplianceStatusREFID != null && !c.IsDeleted).Select(s => s.ComplianceStatusREF).OrderBy(r => r.DisplayOrder).ThenBy(r => r.DisplayValue).FirstOrDefault().DisplayValue,
                IsDirectlyLinkedToAuth = q.obs.AuthorisationID != null,
                IsFromMobileInspection = !string.IsNullOrEmpty(q.obs.IRISObject.MobileInspector)
            }).ToList();

            // for observation created without authorisation (NRC configuration)
            var dbquery2 = Context.Observations.Where(o => o.AuthorisationID == authorisationID && !dbquery.Select(i => i.obs.ID).Contains(o.ID));
            var obsWithoutAuth = dbquery2.Select(
                            q => new AuthorisationObservationRow
                            {
                                ID = q.ID,
                                IRISObjectID = q.IRISObjectID,
                                ObservationDate = q.ObservationDate,
                                ObservationTime = q.ObservationTime,
                                ObservationIRISID = q.IRISObject.BusinessID,
                                Type = q.IRISObject.SubClass2REF.DisplayValue,
                                IsDirectlyLinkedToAuth = q.AuthorisationID != null,
                                IsFromMobileInspection = !string.IsNullOrEmpty(q.IRISObject.MobileInspector)
                            });

            result.AddRange(obsWithoutAuth);
            return result;
        }


        public long GetParentApplicationID(long activityIRISObjectID)
        {
            return Context.Activities.Where(x => x.IRISObjectID == activityIRISObjectID).Select(x => x.ApplicationID).Single();
        }

        public DataTable GetContactAuthorisations(long contactIrisObjectId)
        {
            var contactParam = new SqlParameter("@ContactIRISObjectId", contactIrisObjectId);
            return Context.ExecuteSPReturnDataTable("CallGetLinkedAuthorisationList", contactParam);
        }

        public DataTable GetConsentsForRenewalChange(long contactIrisObjectId)
        {
            var contactParam = new SqlParameter("@ContactIRISObjectId", contactIrisObjectId);
            return Context.ExecuteSPReturnDataTable("CallGetConsentsForRenewalChange", contactParam);
        }

        public string ValidateConsentsForRenewalChange(string selectedAuthorisations)
        {
            var authParam = new SqlParameter("@SelectedAuthorisations", selectedAuthorisations);
            return Context.ExecuteSPReturnScalar("CallValidateSelectedConsentsForRenewalChange", authParam);
        }

        public bool IsAuthorisationIDReserved(long activityId, string businessId)
        {
            return Context.Activities.Any(x => x.ID != activityId && x.AuthorisationIrisID == businessId);
        }

        public List<Programme> GetProgrammesLinkedToAuthorisation(long authoristaionIrisObjectID)
        {            
            RelationshipLinkCriteria programmeCriteria = new RelationshipLinkCriteria
            {
                IRISObjectID = authoristaionIrisObjectID,
                IncludedRelationshipTypeCodeList = new List<string> { ObjectRelationshipTypesCodes.ProgrammeSubject },
                IncludeExpiredLinks = false
            };
            var relationshipEntries = RepositoryMap.LinkingRepository.GetRelationshipLinks(programmeCriteria);
            var programmeIrisObjectIds = relationshipEntries.Select(r => r.LinkedIRISObjectID).ToList();
            var programmes = RepositoryMap.MonitoringRepository.GetProgrammesByIRISObjectIDs(programmeIrisObjectIds);
            return programmes;
        }

        public List<Regime> GetRegimesLinkedToAuthorisation(long authoristaionIrisObjectID)
        {            
            RelationshipLinkCriteria regimeCriteria = new RelationshipLinkCriteria
            {
                IRISObjectID = authoristaionIrisObjectID,
                IncludedRelationshipTypeCodeList = new List<string> { ObjectRelationshipTypesCodes.RegimeSubjectAuthorisation },
                IncludeExpiredLinks = false
            };
            var relationshipEntries = RepositoryMap.LinkingRepository.GetRelationshipLinks(regimeCriteria);
            var regimeIrisObjectIds = relationshipEntries.Select(r => r.LinkedIRISObjectID).ToList();
            var regimes = RepositoryMap.MonitoringRepository.GetRegimesByIRISObjectIDs(regimeIrisObjectIds);
            return regimes;
        }

        public void BulkReplaceEndDateAuthorisation(BulkReplaceAuthorisationOptions options, User user)
        {
            if (options == null)
                return;
            ILinkingRepository linkingRepository = RepositoryMap.LinkingRepository;
            IMonitoringRepository monitoringRepository = RepositoryMap.MonitoringRepository;
            var origAuth = Context.Authorisations.Include(a => a.IRISObject.ActivityObjectRelationships)
                                                  .Include(a => a.IRISObject.RelatedActivityObjectRelationships)
                                                  .Single(a => a.ID == options.AuthorisationID);
            //Programmes
            
            if (options.SelectedProgrammeIDs!= null && options.SelectedProgrammeIDs.Count > 0)
            {
                foreach(long ProgrammeID in options.SelectedProgrammeIDs)
                {
                    var programme = Context.Programme.Include("IRISObject.ActivityObjectRelationships")
                                                    .Include("IRISObject.RelatedActivityObjectRelationships")
                                                    .Single(p => p.ID == ProgrammeID);
                    var replaceAuth = Context.Authorisations.Include(a => a.IRISObject.ActivityObjectRelationships)
                                                            .Include(a => a.IRISObject.RelatedActivityObjectRelationships)
                                                            .SingleOrDefault(a => a.ID == options.ReplacementAuthorisationID);
                    if (options.ReplaceAuthorisation)
                    {                            
                        ActivityObjectRelationshipType programmeAuthRelationshipType = linkingRepository.GetActivityObjectRelationshipWithCode(ReferenceDataValueCodes.ObjectType.Authorisation,
                                                            ReferenceDataValueCodes.ObjectType.Programme,
                                                            ObjectRelationshipTypesCodes.ProgrammeSubject);
                        ActivityObjectRelationship programmeAuthRelationship = new ActivityObjectRelationship
                        {
                            RelatedIRISObjectID = replaceAuth.IRISObjectID,
                            ActivityObjectRelationshipTypeID = programmeAuthRelationshipType.ID,
                            CurrentFrom = options.ActionDate,
                            CreatedBy = user.AccountName,
                            DateCreated = DateTime.Now
                        };

                        RelationshipLinkCriteria programmeAuthLink = new RelationshipLinkCriteria
                        {
                            IRISObjectID = programme.IRISObjectID,
                            IncludedRelationshipTypeCodeList = new List<string> { ObjectRelationshipTypesCodes.ProgrammeSubject },
                            IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Authorisation }
                        };
                        var link = linkingRepository.GetRelationshipLinks(programmeAuthLink).Where(r => r.LinkedIRISObjectID == replaceAuth.IRISObjectID).ToList();

                        if (link.Count == 0 && options.ReplaceAuthorisation)
                        {
                            programme.IRISObject.ActivityObjectRelationships.Add(programmeAuthRelationship);
                        }

                        LinkContactsAndLocationsToProgramme(options, programme, replaceAuth, user);
                            
                    }

                    //End date contacts and locations
                    EndDateProgrammeContactsAndLocations(options, origAuth, programme, options.ActionDate);

                    if (options.CurrentMonitoringStructure == MonitoringStructure.ProgrammeLed)
                    {
                        RelationshipLinkCriteria programmeRegimeLink = new RelationshipLinkCriteria
                        {
                            IRISObjectID = programme.IRISObjectID,
                            IncludedRelationshipTypeCodeList = new List<string> { ObjectRelationshipTypesCodes.ProgrammeRegime, ObjectRelationshipTypesCodes.LinkedProgrammeRegime },
                            IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Regime }
                        };
                        var regimeLinks = linkingRepository.GetRelationshipLinks(programmeRegimeLink);
                        if (regimeLinks.Count > 0)
                        {
                            foreach (RelationshipEntry r in regimeLinks)
                            {
                                var regime = Context.Regime.Include(x=>x.IRISObject.ActivityObjectRelationships)
                                    .Include(x => x.IRISObject.RelatedActivityObjectRelationships)
                                    .Include(p => p.RegimeActivitySchedules)
                                    .Where(x => x.IRISObjectID == r.LinkedIRISObjectID).SingleOrDefault();

                                var relationToOrigAuth = regime.IRISObject.ActivityObjectRelationships.
                                        Where(a => (a.IRISObjectID == origAuth.IRISObjectID || a.RelatedIRISObjectID == origAuth.IRISObjectID) && a.CurrentTo == null).SingleOrDefault() ??
                                        regime.IRISObject.RelatedActivityObjectRelationships.
                                        Where(a => (a.IRISObjectID == origAuth.IRISObjectID || a.RelatedIRISObjectID == origAuth.IRISObjectID) && a.CurrentTo == null).SingleOrDefault();
                                if (relationToOrigAuth == null)
                                    continue;
                                var linkIDs = Context.ActivityObjectRelationshipType
                                    .Where(at => at.Code == ObjectRelationshipTypesCodes.ProgrammeRegime || at.Code == ObjectRelationshipTypesCodes.LinkedProgrammeRegime).Select(a=>a.ID).ToList();
                                var programmes = regime.IRISObject.ActivityObjectRelationships
                                    .Where(a => linkIDs.Contains(a.ActivityObjectRelationshipTypeID) 
                                    && !(a.IRISObjectID == programme.IRISObjectID || a.RelatedIRISObjectID == programme.IRISObjectID) && a.CurrentTo == null).ToList();
                                programmes.AddRange(regime.IRISObject.RelatedActivityObjectRelationships
                                    .Where(a => linkIDs.Contains(a.ActivityObjectRelationshipTypeID)
                                    && !(a.IRISObjectID == programme.IRISObjectID || a.RelatedIRISObjectID == programme.IRISObjectID) && a.CurrentTo == null).ToList());
                                bool consentAddOnly = false;
                                foreach (ActivityObjectRelationship a in programmes)
                                {
                                    if (options.ProgrammesLinkedToAuth.Contains(a.IRISObject.LinkID.Value) && !options.SelectedProgrammeIDs.Contains(a.IRISObject.LinkID.Value))
                                    {
                                        consentAddOnly = true;
                                        break;
                                    }
                                }
                                bool HasLinkedObservations = false;
                                bool HasOutstandingMobileInspection = false;
                                if (options.ReplaceAuthorisation)
                                {
                                    ActivityObjectRelationshipType regimeAuthRelationshipType = linkingRepository.GetActivityObjectRelationshipWithCode(ReferenceDataValueCodes.ObjectType.Authorisation,
                                                                ReferenceDataValueCodes.ObjectType.Regime,
                                                                ObjectRelationshipTypesCodes.RegimeSubjectAuthorisation);
                                    ActivityObjectRelationship regimeAuthRelationship = new ActivityObjectRelationship
                                    {
                                        RelatedIRISObjectID = replaceAuth.IRISObjectID,
                                        ActivityObjectRelationshipTypeID = regimeAuthRelationshipType.ID,
                                        CurrentFrom = options.ActionDate,
                                        CreatedBy = user.AccountName,
                                        DateCreated = DateTime.Now
                                    };

                                    var regimeReplaceAuthRelation = regime.IRISObject.ActivityObjectRelationships.Where(a => a.IRISObjectID == replaceAuth.IRISObjectID || a.RelatedIRISObjectID == replaceAuth.IRISObjectID).SingleOrDefault()
                                        ?? regime.IRISObject.RelatedActivityObjectRelationships.Where(a => a.IRISObjectID == replaceAuth.IRISObjectID || a.RelatedIRISObjectID == replaceAuth.IRISObjectID).SingleOrDefault();
                                    if (regimeReplaceAuthRelation == null)                                        
                                        regime.IRISObject.ActivityObjectRelationships.Add(regimeAuthRelationship);

                                    LinkContactsAndLocationsToRegime(options, regime, replaceAuth, user);
                                }

                                EndDateRegimeContactsAndLocations(options, origAuth, regime, options.ActionDate);

                                var regimeActivitySchedules = regime.RegimeActivitySchedules.Where(rs => rs.ScheduleTypeREFID != Context.ReferenceDataValue.Where(ras => ras.Code == ReferenceDataValueCodes.RegimeActivityScheduleType.Unscheduled).Single().ID).Select(rs => rs.ID).ToList();
                                var regimeActivities = Context.RegimeActivity
                                    .Include(p => p.IRISObject.ActivityObjectRelationships)
                                    .Include(p => p.IRISObject.RelatedActivityObjectRelationships)
                                    .Include(x => x.EstimationLabours)
                                    .Include("EstimationLabours.EstimationLabourAuthorisations")
                                    .Include(x => x.EquipmentMaterials)
                                    .Include("EquipmentMaterials.EquipmentMaterialAuthorisations")
                                    .Where(ra => regimeActivitySchedules.Contains(ra.RegimeActivityScheduleID)).ToList();

                                foreach (RegimeActivity ra in regimeActivities)
                                {
                                    var latestStatus = RepositoryMap.CommonRepository.GetLatestStatusByIrisId(ra.IRISObjectID);
                                    if (latestStatus != null)
                                    {
                                        var refAttrib = Context.ReferenceDataValueAttribute.Where(a => a.ValueID == latestStatus.StatusREFID && a.AttrName == ReferenceDataValueAttributeCodes.IsOpenStatus && a.AttrValue.ToLower() == "true").SingleOrDefault();
                                        bool isOpenStatus = (refAttrib == null) ? false : bool.Parse(refAttrib.AttrValue) == true;
                                        if (!isOpenStatus)
                                            continue;
                                    }
                                    else
                                        continue;
                                    var regimeActivityCompliance = ra as RegimeActivityCompliance;
                                    regimeActivityCompliance.RegimeActivityComplianceAuthorisations.AddTrackableCollection(
                                    Context.RegimeActivityComplianceAuthorisations
                                        .Include(x => x.Authorisation.IRISObject)
                                        .Include(x => x.Authorisation.IRISObject.ObjectTypeREF)
                                        .Include(x => x.RegimeActivityComplianceConditions)
                                        .Include("RegimeActivityComplianceConditions.Condition")
                                          
                                        .Where(c => c.RegimeActivityComplianceID == regimeActivityCompliance.ID && !c.IsDeleted).ToList());

                                    if (options.ReplaceAuthorisation)
                                    {
                                        var origAuthRegimeActivityLink = regimeActivityCompliance.RegimeActivityComplianceAuthorisations.Where(rc => rc.AuthorisationID == origAuth.ID).SingleOrDefault();
                                        if (origAuthRegimeActivityLink == null)
                                            continue;
                                        var authRegimeActivityLink = regimeActivityCompliance.RegimeActivityComplianceAuthorisations.Where(rc => rc.AuthorisationID == replaceAuth.ID).SingleOrDefault();                                        
                                        if (authRegimeActivityLink == null)
                                        {
                                            var newCompAuth = new RegimeActivityComplianceAuthorisation
                                            {
                                                AuthorisationID = replaceAuth.ID,
                                                CreatedBy = user.AccountName,
                                                DateCreated = DateTime.Now
                                            };
                                            if (options.CurrentSelectConditions == ConditionsEnum.SelectConditions || options.CurrentSelectConditions == ConditionsEnum.AddAllConditions)
                                                options.SelectedConditions.ForEach(x => newCompAuth.RegimeActivityComplianceConditions.Add(new RegimeActivityComplianceCondition
                                                {
                                                    ConditionID = x.ConditionID,
                                                    ConditionScheduleID = x.ConditionScheduleID,
                                                    CreatedBy = user.AccountName,
                                                    DateCreated = DateTime.Now
                                                }));

                                            regimeActivityCompliance.RegimeActivityComplianceAuthorisations.Add(newCompAuth);
                                            
                                        }

                                        LinkContactsAndLocationsToRegimeActivity(options, ra, replaceAuth, user);
                                    }

                                    EndDateRegimeActivityContactsAndLocations(options, origAuth, ra, options.ActionDate);

                                    if (ra.IsForEstimation.HasValue && ra.IsForEstimation.Value)
                                    {
                                        foreach (EquipmentMaterial em in ra.EquipmentMaterials)
                                        {
                                            var equipmentMaterialAuthorisation = em.EquipmentMaterialAuthorisations.Where(x => x.AuthorisationID == origAuth.ID).SingleOrDefault();
                                            if (equipmentMaterialAuthorisation == null)
                                                continue;
                                            if (options.ReplaceAuthorisation)
                                                equipmentMaterialAuthorisation.AuthorisationID = replaceAuth.ID;
                                            else
                                                equipmentMaterialAuthorisation.IsDeleted = true;
                                        }
                                        foreach (EstimationLabour el in ra.EstimationLabours)
                                        {
                                            var estimationLabourAuthorisation = el.EstimationLabourAuthorisations.Where(x => x.AuthorisationID == origAuth.ID).SingleOrDefault();
                                            if (estimationLabourAuthorisation == null)
                                                continue;
                                            if (options.ReplaceAuthorisation)
                                                estimationLabourAuthorisation.AuthorisationID = replaceAuth.ID;
                                            else
                                                estimationLabourAuthorisation.IsDeleted = true;
                                        }
                                    }
                                    var existingAuthorisation = regimeActivityCompliance.RegimeActivityComplianceAuthorisations.Where(x => x.AuthorisationID == origAuth.ID).SingleOrDefault();
                                    HasLinkedObservations = RepositoryMap.MonitoringRepository.IsAuthorisationObservedWithinRegimeActivity(origAuth.IRISObjectID, ra.IRISObjectID);
                                    HasOutstandingMobileInspection = RepositoryMap.MonitoringRepository.HasOutstandingMobileInspectionForRegimeActivity(ra.ID);
                                    if (!HasLinkedObservations && !HasOutstandingMobileInspection)
                                        if (existingAuthorisation != null)
                                            existingAuthorisation.IsDeleted = true;
                                    
                                }
                                if (!consentAddOnly)
                                {
                                    var regimeOrigAuthRelation = regime.IRISObject.ActivityObjectRelationships.
                                        Where(a => (a.IRISObjectID == origAuth.IRISObjectID || a.RelatedIRISObjectID == origAuth.IRISObjectID) && a.CurrentTo == null).SingleOrDefault()
                                        ?? regime.IRISObject.RelatedActivityObjectRelationships.
                                        Where(a => (a.IRISObjectID == origAuth.IRISObjectID || a.RelatedIRISObjectID == origAuth.IRISObjectID) && a.CurrentTo == null).SingleOrDefault();
                                    regimeOrigAuthRelation.CurrentTo = options.ActionDate;
                                }
                                ApplyEntityChanges(regime);
                            }
                        }
                    }

                    var currentProgramAuthRelationship = programme.IRISObject.ActivityObjectRelationships.Where(r => (r.RelatedIRISObjectID == options.AuthorisationIrisObjectID
                    || r.IRISObjectID == options.AuthorisationIrisObjectID) && r.CurrentTo == null).SingleOrDefault() ??  programme.IRISObject.RelatedActivityObjectRelationships.Where(r => (r.RelatedIRISObjectID == options.AuthorisationIrisObjectID
                        || r.IRISObjectID == options.AuthorisationIrisObjectID) && r.CurrentTo == null).SingleOrDefault();                    

                    if(currentProgramAuthRelationship != null)
                        currentProgramAuthRelationship.CurrentTo = options.ActionDate;                        
                        
                    ApplyEntityChanges(programme);
                }                    
                
            }
            //Regimes
            
            if (options.SelectedRegimeIDs != null && options.SelectedRegimeIDs.Count > 0)
            {
                foreach (long RegimeID in options.SelectedRegimeIDs)
                {
                    var regime = Context.Regime.Include(p => p.IRISObject.ActivityObjectRelationships)
                                                .Include(p => p.IRISObject.RelatedActivityObjectRelationships)
                                                .Include(p => p.RegimeActivitySchedules)
                                                .Single(p => p.ID == RegimeID);
                   
                    var replaceAuth = Context.Authorisations.Include(a => a.IRISObject.ActivityObjectRelationships)
                                                            .Include(a => a.IRISObject.RelatedActivityObjectRelationships)
                                                            .SingleOrDefault(a => a.ID == options.ReplacementAuthorisationID);
                    if (options.ReplaceAuthorisation)
                    {
                        ActivityObjectRelationshipType regimeAuthRelationshipType = linkingRepository.GetActivityObjectRelationshipWithCode(ReferenceDataValueCodes.ObjectType.Authorisation,
                                                        ReferenceDataValueCodes.ObjectType.Regime,
                                                        ObjectRelationshipTypesCodes.RegimeSubjectAuthorisation);
                        ActivityObjectRelationship regimeAuthRelationship = new ActivityObjectRelationship
                        {
                            RelatedIRISObjectID = replaceAuth.IRISObjectID,
                            ActivityObjectRelationshipTypeID = regimeAuthRelationshipType.ID,
                            CurrentFrom = options.ActionDate,
                            CreatedBy = user.AccountName,
                            DateCreated = DateTime.Now
                        };

                        RelationshipLinkCriteria regimeAuthLink = new RelationshipLinkCriteria
                        {
                            IRISObjectID = regime.IRISObjectID,
                            IncludedRelationshipTypeCodeList = new List<string> { ObjectRelationshipTypesCodes.RegimeSubjectAuthorisation },
                            IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Authorisation }
                        };
                        var link = linkingRepository.GetRelationshipLinks(regimeAuthLink).Where(r => r.LinkedIRISObjectID == replaceAuth.IRISObjectID).ToList();

                        if (link.Count == 0 && options.ReplaceAuthorisation)
                        {
                            regime.IRISObject.ActivityObjectRelationships.Add(regimeAuthRelationship);
                        }


                        LinkContactsAndLocationsToRegime(options, regime, replaceAuth, user);
                    }

                    EndDateRegimeContactsAndLocations(options, origAuth, regime, options.ActionDate);

                    //Update Regime Activities
                    bool HasLinkedObservations = false;
                    bool HasOutstandingMobileInspection = false;

                    var regimeActivitySchedules = regime.RegimeActivitySchedules.Where(rs => rs.ScheduleTypeREFID != Context.ReferenceDataValue.Where(r=>r.Code==ReferenceDataValueCodes.RegimeActivityScheduleType.Unscheduled).Single().ID).Select(rs => rs.ID).ToList();
                    var regimeActivities = Context.RegimeActivity
                        .Include(p => p.IRISObject.ActivityObjectRelationships)
                        .Include(p => p.IRISObject.RelatedActivityObjectRelationships)
                        .Include(x => x.EstimationLabours)
                        .Include("EstimationLabours.EstimationLabourAuthorisations")
                        .Include(x => x.EquipmentMaterials)
                        .Include("EquipmentMaterials.EquipmentMaterialAuthorisations")
                        .Where(ra => regimeActivitySchedules.Contains(ra.RegimeActivityScheduleID)).ToList();
                    foreach(RegimeActivity ra in regimeActivities)
                    {
                        var latestStatus = RepositoryMap.CommonRepository.GetLatestStatusByIrisId(ra.IRISObjectID);
                        if (latestStatus != null)
                        {
                            var refAttrib = Context.ReferenceDataValueAttribute.Where(a => a.ValueID == latestStatus.StatusREFID && a.AttrName == ReferenceDataValueAttributeCodes.IsOpenStatus && a.AttrValue.ToLower() == "true").SingleOrDefault();
                            bool isOpenStatus = (refAttrib == null) ? false : bool.Parse(refAttrib.AttrValue) == true;
                            if (!isOpenStatus)
                                continue;
                        }
                        else
                            continue;
                        var regimeActivityCompliance = ra as RegimeActivityCompliance;
                        regimeActivityCompliance.RegimeActivityComplianceAuthorisations.AddTrackableCollection(
                        Context.RegimeActivityComplianceAuthorisations
                            .Include(x => x.Authorisation.IRISObject)
                            .Include(x => x.Authorisation.IRISObject.ObjectTypeREF)
                            .Include(x => x.RegimeActivityComplianceConditions)
                            .Include("RegimeActivityComplianceConditions.Condition")

                            .Where(c => c.RegimeActivityComplianceID == regimeActivityCompliance.ID && !c.IsDeleted).ToList());

                        if (options.ReplaceAuthorisation)
                        {
                            var origAuthRegimeActivityLink = regimeActivityCompliance.RegimeActivityComplianceAuthorisations.Where(rc => rc.AuthorisationID == origAuth.ID).SingleOrDefault();
                            if (origAuthRegimeActivityLink == null)
                                continue;
                            var authRegimeActivityLink = regimeActivityCompliance.RegimeActivityComplianceAuthorisations.Where(rc => rc.AuthorisationID == replaceAuth.ID).SingleOrDefault();
                            if (authRegimeActivityLink == null)
                            {
                                var newCompAuth = new RegimeActivityComplianceAuthorisation
                                {
                                    AuthorisationID = replaceAuth.ID,
                                    CreatedBy = user.AccountName,
                                    DateCreated = DateTime.Now
                                };
                                if (options.CurrentSelectConditions == ConditionsEnum.SelectConditions || options.CurrentSelectConditions == ConditionsEnum.AddAllConditions)
                                    options.SelectedConditions.ForEach(x => newCompAuth.RegimeActivityComplianceConditions.Add(new RegimeActivityComplianceCondition
                                    {
                                        ConditionID = x.ConditionID,
                                        ConditionScheduleID = x.ConditionScheduleID,
                                        CreatedBy = user.AccountName,
                                        DateCreated = DateTime.Now
                                    }));                                

                                regimeActivityCompliance.RegimeActivityComplianceAuthorisations.Add(newCompAuth);
                                
                            }

                            LinkContactsAndLocationsToRegimeActivity(options, ra, replaceAuth, user);
                        }

                        EndDateRegimeActivityContactsAndLocations(options, origAuth, ra, options.ActionDate);

                        if (ra.IsForEstimation.HasValue && ra.IsForEstimation.Value)
                        {
                            foreach (EquipmentMaterial em in ra.EquipmentMaterials)
                            {
                                var equipmentMaterialAuthorisation = em.EquipmentMaterialAuthorisations.Where(x => x.AuthorisationID == origAuth.ID).SingleOrDefault();
                                if (equipmentMaterialAuthorisation == null)
                                    continue;
                                if (options.ReplaceAuthorisation)
                                    equipmentMaterialAuthorisation.AuthorisationID = replaceAuth.ID;
                                else
                                    equipmentMaterialAuthorisation.IsDeleted = true;
                            }
                            foreach (EstimationLabour el in ra.EstimationLabours)
                            {
                                var estimationLabourAuthorisation = el.EstimationLabourAuthorisations.Where(x => x.AuthorisationID == origAuth.ID).SingleOrDefault();
                                if (estimationLabourAuthorisation == null)
                                    continue;
                                if (options.ReplaceAuthorisation)
                                    estimationLabourAuthorisation.AuthorisationID = replaceAuth.ID;
                                else
                                    estimationLabourAuthorisation.IsDeleted = true;
                            }
                        }
                        var existingAuthorisation = regimeActivityCompliance.RegimeActivityComplianceAuthorisations.Where(x => x.AuthorisationID == origAuth.ID).SingleOrDefault();
                        HasLinkedObservations = RepositoryMap.MonitoringRepository.IsAuthorisationObservedWithinRegimeActivity(origAuth.IRISObjectID, ra.IRISObjectID);
                        HasOutstandingMobileInspection = RepositoryMap.MonitoringRepository.HasOutstandingMobileInspectionForRegimeActivity(ra.ID);
                        if (!HasLinkedObservations && !HasOutstandingMobileInspection)
                            if(existingAuthorisation != null)
                                existingAuthorisation.IsDeleted = true;
                    }
                    

                    if (options.CurrentMonitoringStructure == MonitoringStructure.RegimeLed)
                    {
                        RelationshipLinkCriteria programmeRegimeLink = new RelationshipLinkCriteria
                        {
                            IRISObjectID = regime.IRISObjectID,
                            IncludedRelationshipTypeCodeList = new List<string> { ObjectRelationshipTypesCodes.ProgrammeRegime, ObjectRelationshipTypesCodes.LinkedProgrammeRegime },
                            IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Programme }
                        };
                        var programmeLinks = linkingRepository.GetRelationshipLinks(programmeRegimeLink);
                        if (programmeLinks.Count > 0)
                        {
                            foreach (RelationshipEntry r in programmeLinks)
                            {
                                var programme = Context.Programme.Include(p=>p.IRISObject.ActivityObjectRelationships).Where(x => x.IRISObjectID == r.LinkedIRISObjectID).SingleOrDefault();
                                var programmeOrigAuthRelation = programme.IRISObject.ActivityObjectRelationships.
                                        Where(a => (a.IRISObjectID == origAuth.IRISObjectID || a.RelatedIRISObjectID == origAuth.IRISObjectID) && a.CurrentTo == null).SingleOrDefault()
                                        ?? programme.IRISObject.RelatedActivityObjectRelationships.
                                        Where(a => (a.IRISObjectID == origAuth.IRISObjectID || a.RelatedIRISObjectID == origAuth.IRISObjectID) && a.CurrentTo == null).SingleOrDefault();
                                if (programmeOrigAuthRelation == null)
                                    continue;
                                var linkIDs = Context.ActivityObjectRelationshipType
                                    .Where(at => at.Code == ObjectRelationshipTypesCodes.ProgrammeRegime || at.Code == ObjectRelationshipTypesCodes.LinkedProgrammeRegime).Select(a => a.ID).ToList();
                                var regimes = programme.IRISObject.ActivityObjectRelationships
                                    .Where(a => linkIDs.Contains(a.ActivityObjectRelationshipTypeID)
                                    && !(a.IRISObjectID == regime.IRISObjectID || a.RelatedIRISObjectID == regime.IRISObjectID) && a.CurrentTo == null).ToList();
                                regimes.AddRange(programme.IRISObject.RelatedActivityObjectRelationships
                                    .Where(a => linkIDs.Contains(a.ActivityObjectRelationshipTypeID)
                                    && !(a.IRISObjectID == regime.IRISObjectID || a.RelatedIRISObjectID == regime.IRISObjectID) && a.CurrentTo == null).ToList());
                                bool consentAddOnly = false;
                                foreach (ActivityObjectRelationship a in regimes)
                                {
                                    if (options.RegimesLinkedToAuth.Contains(a.IRISObject.LinkID.Value) && !options.SelectedRegimeIDs.Contains(a.IRISObject.LinkID.Value))
                                    {
                                        consentAddOnly = true;
                                        break;
                                    }
                                }

                                if (options.ReplaceAuthorisation)
                                {
                                    ActivityObjectRelationshipType programmeAuthRelationshipType = linkingRepository.GetActivityObjectRelationshipWithCode(ReferenceDataValueCodes.ObjectType.Authorisation,
                                                            ReferenceDataValueCodes.ObjectType.Programme,
                                                            ObjectRelationshipTypesCodes.ProgrammeSubject);
                                    ActivityObjectRelationship programmeAuthRelationship = new ActivityObjectRelationship
                                    {
                                        RelatedIRISObjectID = replaceAuth.IRISObjectID,
                                        ActivityObjectRelationshipTypeID = programmeAuthRelationshipType.ID,
                                        CurrentFrom = options.ActionDate,
                                        CreatedBy = user.AccountName,
                                        DateCreated = DateTime.Now
                                    };

                                    var programmeReplaceAuthRelation = programme.IRISObject.ActivityObjectRelationships
                                        .Where(a => (a.IRISObjectID == replaceAuth.IRISObjectID || a.RelatedIRISObjectID == replaceAuth.IRISObjectID)
                                        && a.CurrentTo == null).SingleOrDefault()
                                        ?? programme.IRISObject.RelatedActivityObjectRelationships
                                        .Where(a => (a.IRISObjectID == replaceAuth.IRISObjectID || a.RelatedIRISObjectID == replaceAuth.IRISObjectID)
                                        && a.CurrentTo == null).SingleOrDefault();
                                    if (programmeReplaceAuthRelation == null)
                                        programme.IRISObject.ActivityObjectRelationships.Add(programmeAuthRelationship);


                                    LinkContactsAndLocationsToProgramme(options, programme, replaceAuth, user);
                                }

                                EndDateProgrammeContactsAndLocations(options, origAuth, programme, options.ActionDate);

                                if (!consentAddOnly)
                                    programmeOrigAuthRelation.CurrentTo = options.ActionDate;

                                ApplyEntityChanges(programme);
                            }
                        }
                    }

                    var currentRegimeAuthRelationship = regime.IRISObject.ActivityObjectRelationships.Where(r => (r.RelatedIRISObjectID == options.AuthorisationIrisObjectID
                    || r.IRISObjectID == options.AuthorisationIrisObjectID) && r.CurrentTo == null).SingleOrDefault() ??  regime.IRISObject.RelatedActivityObjectRelationships.Where(r => (r.RelatedIRISObjectID == options.AuthorisationIrisObjectID
                        || r.IRISObjectID == options.AuthorisationIrisObjectID) && r.CurrentTo == null).SingleOrDefault();

                    if (currentRegimeAuthRelationship != null)
                        currentRegimeAuthRelationship.CurrentTo = options.ActionDate;                   

                    ApplyEntityChanges(regime);
                }
                
            }
            

            SaveChanges();

            
        }

        private void EndDateProgrammeContactsAndLocations(BulkReplaceAuthorisationOptions options, Authorisation origAuth, Programme programme, DateTime actionDate)
        {
            ILinkingRepository linkingRepository = RepositoryMap.LinkingRepository;
            RelationshipLinkCriteria AuthContactLink = new RelationshipLinkCriteria
            {
                IRISObjectID = origAuth.IRISObjectID,                                
                IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Contact },
                IncludeExpiredLinks = false
            };
            var relationshipEntries = linkingRepository.GetRelationshipLinks(AuthContactLink).ToList();
            List<long> selectedContacts = new List<long>();
            if (options.ContactProgrammeLinks != null && options.ContactProgrammeLinks.Count > 0)
             selectedContacts = (from cp in options.ContactProgrammeLinks select cp.Key).ToList();
            foreach (RelationshipEntry r in relationshipEntries)
            {
                bool endDate = true;
                var contactIrisObjectID = r.LinkedIRISObjectID;
                long contactID = Context.Contact.Where(c => c.IRISObjectID == contactIrisObjectID).Single().ID;
                if (selectedContacts.Contains(contactID))
                    continue;

                var contactLink = programme.IRISObject.ActivityObjectRelationships
                .Where(a => (a.IRISObjectID == contactIrisObjectID
                || a.RelatedIRISObjectID == contactIrisObjectID) && a.CurrentTo == null).SingleOrDefault()
                ?? programme.IRISObject.RelatedActivityObjectRelationships
                .Where(a => (a.IRISObjectID == contactIrisObjectID
                || a.RelatedIRISObjectID == contactIrisObjectID) && a.CurrentTo == null).SingleOrDefault();
                if(contactLink != null)
                {
                    RelationshipLinkCriteria contactAuthRel = new RelationshipLinkCriteria
                    {
                        IRISObjectID = contactIrisObjectID,
                        IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Authorisation },
                        IncludeExpiredLinks = false
                    };
                    var contactAuthLinks = linkingRepository.GetRelationshipLinks(contactAuthRel).Where(p=>p.LinkedIRISObjectID != origAuth.IRISObjectID).ToList();
                    if(contactAuthLinks.Count > 0)
                    {
                        foreach(RelationshipEntry rel in contactAuthLinks)
                        {
                            var programAuthRelation = programme.IRISObject.ActivityObjectRelationships.Where(a => (a.IRISObjectID == rel.LinkedIRISObjectID
                                || a.RelatedIRISObjectID == rel.LinkedIRISObjectID) && a.CurrentTo == null).SingleOrDefault()
                                ?? programme.IRISObject.RelatedActivityObjectRelationships.Where(a => (a.IRISObjectID == rel.LinkedIRISObjectID
                                || a.RelatedIRISObjectID == rel.LinkedIRISObjectID) && a.CurrentTo == null).SingleOrDefault();
                            if (programAuthRelation != null)
                            {
                                endDate = false;
                                break;
                            }
                        }
                    }
                    if (endDate)
                        contactLink.CurrentTo = actionDate;
                }

            }
            
            RelationshipLinkCriteria AuthLocationLink = new RelationshipLinkCriteria
            {
                IRISObjectID = origAuth.IRISObjectID,
                IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Location },
                IncludeExpiredLinks = false
            };
            var locationEntries = linkingRepository.GetRelationshipLinks(AuthLocationLink).ToList();
            List<long> selectedLocations = new List<long>();
            if(options.LocationProgrammeLinks != null && options.LocationProgrammeLinks.Count > 0)
                selectedLocations = (from lp in options.LocationProgrammeLinks select lp.Key).ToList();
            foreach (RelationshipEntry r in locationEntries)
            {
                bool endDate = true;
                var locationIrisObjectID = r.LinkedIRISObjectID;
                long locationID = Context.Location.Where(c => c.IRISObjectID == locationIrisObjectID).Single().ID;
                if (selectedLocations.Contains(locationID))
                    continue;

                var locationLink = programme.IRISObject.ActivityObjectRelationships
                    .Where(a => (a.IRISObjectID == locationIrisObjectID
                    || a.RelatedIRISObjectID == locationIrisObjectID) && a.CurrentTo == null).SingleOrDefault()
                    ?? programme.IRISObject.RelatedActivityObjectRelationships
                    .Where(a => (a.IRISObjectID == locationIrisObjectID
                    || a.RelatedIRISObjectID == locationIrisObjectID) && a.CurrentTo == null).SingleOrDefault();
                if (locationLink != null)
                {
                    RelationshipLinkCriteria locationAuthRel = new RelationshipLinkCriteria
                    {
                        IRISObjectID = locationIrisObjectID,
                        IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Authorisation },
                        IncludeExpiredLinks = false
                    };
                    var locationAuthLinks = linkingRepository.GetRelationshipLinks(locationAuthRel).Where(p => p.LinkedIRISObjectID != origAuth.IRISObjectID).ToList();
                    if (locationAuthLinks.Count > 0)
                    {
                        foreach (RelationshipEntry rel in locationAuthLinks)
                        {
                            var programmeAuthRelation = programme.IRISObject.ActivityObjectRelationships.Where(a => (a.IRISObjectID == rel.LinkedIRISObjectID
                                 || a.RelatedIRISObjectID == rel.LinkedIRISObjectID) && a.CurrentTo == null).SingleOrDefault()
                                 ?? programme.IRISObject.RelatedActivityObjectRelationships.Where(a => (a.IRISObjectID == rel.LinkedIRISObjectID
                                 || a.RelatedIRISObjectID == rel.LinkedIRISObjectID) && a.CurrentTo == null).SingleOrDefault();
                            if (programmeAuthRelation != null)
                            {
                                endDate = false;
                                break;
                            }
                        }
                    }
                    if (endDate)
                        locationLink.CurrentTo = actionDate;
                }

            }
        }

        private void EndDateRegimeContactsAndLocations(BulkReplaceAuthorisationOptions options, Authorisation origAuth, Regime regime, DateTime actionDate)
        {
            ILinkingRepository linkingRepository = RepositoryMap.LinkingRepository;
            RelationshipLinkCriteria AuthContactLink = new RelationshipLinkCriteria
            {
                IRISObjectID = origAuth.IRISObjectID,
                IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Contact },
                IncludeExpiredLinks = false
            };
            var relationshipEntries = linkingRepository.GetRelationshipLinks(AuthContactLink).ToList();

            List<long> selectedContacts = new List<long>();
            if(options.ContactRegimeLinks != null && options.ContactRegimeLinks.Count > 0)
                selectedContacts = (from cp in options.ContactRegimeLinks select cp.Key).ToList();
            foreach (RelationshipEntry r in relationshipEntries)
            {
                bool endDate = true;
                var contactIrisObjectID = r.LinkedIRISObjectID;
                long contactID = Context.Contact.Where(c => c.IRISObjectID == contactIrisObjectID).Single().ID;
                if (selectedContacts.Count > 0 && selectedContacts.Contains(contactID))
                    continue;
                var contactLink = regime.IRISObject.ActivityObjectRelationships
                    .Where(a => (a.IRISObjectID == contactIrisObjectID
                    || a.RelatedIRISObjectID == contactIrisObjectID) && a.CurrentTo == null).SingleOrDefault()
                    ?? regime.IRISObject.RelatedActivityObjectRelationships
                    .Where(a => (a.IRISObjectID == contactIrisObjectID
                    || a.RelatedIRISObjectID == contactIrisObjectID) && a.CurrentTo == null).SingleOrDefault();
                if (contactLink != null)
                {
                    RelationshipLinkCriteria contactAuthRel = new RelationshipLinkCriteria
                    {
                        IRISObjectID = contactIrisObjectID,
                        IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Authorisation },
                        IncludeExpiredLinks = false
                    };
                    var contactAuthLinks = linkingRepository.GetRelationshipLinks(contactAuthRel).Where(p => p.LinkedIRISObjectID != origAuth.IRISObjectID).ToList();
                    if (contactAuthLinks.Count > 0)
                    {
                        foreach (RelationshipEntry rel in contactAuthLinks)
                        {
                            var regimeAuthRelation = regime.IRISObject.ActivityObjectRelationships.Where(a => (a.IRISObjectID == rel.LinkedIRISObjectID
                                 || a.RelatedIRISObjectID == rel.LinkedIRISObjectID) && a.CurrentTo == null).SingleOrDefault()
                                 ?? regime.IRISObject.RelatedActivityObjectRelationships.Where(a => (a.IRISObjectID == rel.LinkedIRISObjectID
                                 || a.RelatedIRISObjectID == rel.LinkedIRISObjectID) && a.CurrentTo == null).SingleOrDefault();
                            if (regimeAuthRelation != null)
                            {
                                endDate = false;
                                break;
                            }
                        }
                    }
                    if (endDate)
                        contactLink.CurrentTo = actionDate;
                }

            }

            RelationshipLinkCriteria AuthLocationLink = new RelationshipLinkCriteria
            {
                IRISObjectID = origAuth.IRISObjectID,
                IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Location },
                IncludeExpiredLinks = false
            };
            var locationEntries = linkingRepository.GetRelationshipLinks(AuthLocationLink).ToList();
            List<long> selectedLocations = new List<long>();
            if(options.LocationRegimeLinks != null &&  options.LocationRegimeLinks.Count > 0)
                selectedLocations = (from lp in options.LocationRegimeLinks select lp.Key).ToList();
            foreach (RelationshipEntry r in locationEntries)
            {
                bool endDate = true;
                var locationIrisObjectID = r.LinkedIRISObjectID;
                long locationID = Context.Location.Where(c => c.IRISObjectID == locationIrisObjectID).Single().ID;
                if (selectedLocations.Count > 0 && selectedLocations.Contains(locationID))
                    continue;
                var locationLink = regime.IRISObject.ActivityObjectRelationships
                    .Where(a => (a.IRISObjectID == locationIrisObjectID
                    || a.RelatedIRISObjectID == locationIrisObjectID) && a.CurrentTo == null).SingleOrDefault();
                if (locationLink != null)
                {
                    RelationshipLinkCriteria locationAuthRel = new RelationshipLinkCriteria
                    {
                        IRISObjectID = locationIrisObjectID,
                        IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Authorisation },
                        IncludeExpiredLinks = false
                    };
                    var locationAuthLinks = linkingRepository.GetRelationshipLinks(locationAuthRel).Where(p => p.LinkedIRISObjectID != origAuth.IRISObjectID).ToList();
                    if (locationAuthLinks.Count > 0)
                    {
                        foreach (RelationshipEntry rel in locationAuthLinks)
                        {
                            var regimeAuthRelation = regime.IRISObject.ActivityObjectRelationships.Where(a => (a.IRISObjectID == rel.LinkedIRISObjectID
                                 || a.RelatedIRISObjectID == rel.LinkedIRISObjectID) && a.CurrentTo == null).SingleOrDefault()
                                 ?? regime.IRISObject.RelatedActivityObjectRelationships.Where(a => (a.IRISObjectID == rel.LinkedIRISObjectID
                                 || a.RelatedIRISObjectID == rel.LinkedIRISObjectID) && a.CurrentTo == null).SingleOrDefault();
                            if (regimeAuthRelation != null)
                            {
                                endDate = false;
                                break;
                            }
                        }
                    }
                    if (endDate)
                        locationLink.CurrentTo = actionDate;
                }

            }
        }

        private void EndDateRegimeActivityContactsAndLocations(BulkReplaceAuthorisationOptions options, Authorisation origAuth, RegimeActivity regimeActivity, DateTime actionDate)
        {
            ILinkingRepository linkingRepository = RepositoryMap.LinkingRepository;
            RelationshipLinkCriteria AuthContactLink = new RelationshipLinkCriteria
            {
                IRISObjectID = origAuth.IRISObjectID,
                IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Contact },
                IncludeExpiredLinks = false
            };
            var relationshipEntries = linkingRepository.GetRelationshipLinks(AuthContactLink).ToList();
            List<long> selectedContacts = new List<long>();
            if(options.ContactRegimeActivityLinks != null && options.ContactRegimeActivityLinks.Count > 0)
                selectedContacts = (from cp in options.ContactRegimeActivityLinks select cp.Key).ToList();
            foreach (RelationshipEntry r in relationshipEntries)
            {
                bool endDate = true;
                var contactIrisObjectID = r.LinkedIRISObjectID;
                long contactID = Context.Contact.Where(c => c.IRISObjectID == contactIrisObjectID).Single().ID;
                if (selectedContacts.Count > 0 && selectedContacts.Contains(contactID))
                    continue;
                var contactLink = regimeActivity.IRISObject.ActivityObjectRelationships
                    .Where(a => (a.IRISObjectID == contactIrisObjectID
                    || a.RelatedIRISObjectID == contactIrisObjectID) && a.CurrentTo == null).SingleOrDefault()
                    ?? regimeActivity.IRISObject.RelatedActivityObjectRelationships
                    .Where(a => (a.IRISObjectID == contactIrisObjectID
                    || a.RelatedIRISObjectID == contactIrisObjectID) && a.CurrentTo == null).SingleOrDefault();
                if (contactLink != null)
                {
                    RelationshipLinkCriteria contactAuthRel = new RelationshipLinkCriteria
                    {
                        IRISObjectID = contactIrisObjectID,
                        IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Authorisation },
                        IncludeExpiredLinks = false
                    };
                    var contactAuthLinks = linkingRepository.GetRelationshipLinks(contactAuthRel).Where(p => p.LinkedIRISObjectID != origAuth.IRISObjectID).ToList();
                    if (contactAuthLinks.Count > 0)
                    {
                        foreach (RelationshipEntry rel in contactAuthLinks)
                        {
                            var regimeActivityCompliance = regimeActivity as RegimeActivityCompliance;
                            var regimeActivityAuthRelation = regimeActivityCompliance
                                .RegimeActivityComplianceAuthorisations
                                .SingleOrDefault(a => a.Authorisation.IRISObjectID == rel.LinkedIRISObjectID);
                            if (regimeActivityAuthRelation != null)
                            {
                                endDate = false;
                                break;
                            }
                        }
                    }
                    if (endDate)
                        contactLink.CurrentTo = actionDate;
                }

            }

            RelationshipLinkCriteria AuthLocationLink = new RelationshipLinkCriteria
            {
                IRISObjectID = origAuth.IRISObjectID,
                IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Location },
                IncludeExpiredLinks = false
            };
            var locationEntries = linkingRepository.GetRelationshipLinks(AuthLocationLink).ToList();
            List<long> selectedLocations = new List<long>();
            if(options.LocationRegimeActivityLinks != null && options.LocationRegimeActivityLinks.Count > 0)
                selectedLocations = (from lp in options.LocationRegimeActivityLinks select lp.Key).ToList(); 
            foreach (RelationshipEntry r in locationEntries)
            {
                bool endDate = true;
                var locationIrisObjectID = r.LinkedIRISObjectID;
                long locationID = Context.Location.Where(c => c.IRISObjectID == locationIrisObjectID).Single().ID;
                if (selectedLocations.Count > 0 && selectedLocations.Contains(locationID))
                    continue;
                var locationLink = regimeActivity.IRISObject.ActivityObjectRelationships
                    .Where(a => (a.IRISObjectID == locationIrisObjectID
                    || a.RelatedIRISObjectID == locationIrisObjectID) && a.CurrentTo == null).SingleOrDefault()
                    ?? regimeActivity.IRISObject.RelatedActivityObjectRelationships
                    .Where(a => (a.IRISObjectID == locationIrisObjectID
                    || a.RelatedIRISObjectID == locationIrisObjectID) && a.CurrentTo == null).SingleOrDefault();
                if (locationLink != null)
                {
                    RelationshipLinkCriteria locationAuthRel = new RelationshipLinkCriteria
                    {
                        IRISObjectID = locationIrisObjectID,
                        IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Authorisation },
                        IncludeExpiredLinks = false
                    };
                    var locationAuthLinks = linkingRepository.GetRelationshipLinks(locationAuthRel).Where(p => p.LinkedIRISObjectID != origAuth.IRISObjectID).ToList();
                    if (locationAuthLinks.Count > 0)
                    {
                        foreach (RelationshipEntry rel in locationAuthLinks)
                        {
                            var regimeActivityCompliance = regimeActivity as RegimeActivityCompliance;
                            var regimeActivityAuthRelation = regimeActivityCompliance
                                .RegimeActivityComplianceAuthorisations
                                .SingleOrDefault(a => a.Authorisation.IRISObjectID == rel.LinkedIRISObjectID);
                            if (regimeActivityAuthRelation != null)
                            {
                                endDate = false;
                                break;
                            }
                        }
                    }
                    if (endDate)
                        locationLink.CurrentTo = actionDate;
                }

            }
        }

        private void LinkContactsAndLocationsToProgramme(BulkReplaceAuthorisationOptions options, Programme programme, Authorisation replaceAuth, User user )
        {
            if (options.ContactProgrammeLinks != null && options.ContactProgrammeLinks.Count > 0)
            {
                foreach (KeyValuePair<long, long> rel in options.ContactProgrammeLinks)
                {
                    long contactIrisObjectID = Context.Contact.Where(c => c.ID == rel.Key).Single().IRISObjectID;
                    var currentLink = programme.IRISObject.ActivityObjectRelationships.Where(a => (a.IRISObjectID == contactIrisObjectID
                    || a.RelatedIRISObjectID == contactIrisObjectID) && a.ActivityObjectRelationshipTypeID == rel.Value && a.CurrentTo == null).SingleOrDefault()
                    ?? programme.IRISObject.RelatedActivityObjectRelationships.Where(a => (a.IRISObjectID == contactIrisObjectID
                    || a.RelatedIRISObjectID == contactIrisObjectID) && a.ActivityObjectRelationshipTypeID == rel.Value && a.CurrentTo == null).SingleOrDefault();

                    long relID =(replaceAuth.IRISObject.ActivityObjectRelationships.Where(r => r.IRISObjectID == contactIrisObjectID || r.RelatedIRISObjectID == contactIrisObjectID).SingleOrDefault() 
                        ?? replaceAuth.IRISObject.RelatedActivityObjectRelationships.Where(r => r.IRISObjectID == contactIrisObjectID || r.RelatedIRISObjectID == contactIrisObjectID).SingleOrDefault()).ID;

                    var contactSubLink = Context.ContactSubLink.Where(x => x.ActivityObjectRelationshipID == relID &&
                    (x.ActivityObjectRelationship.IRISObjectID == contactIrisObjectID || x.ActivityObjectRelationship.RelatedIRISObjectID == contactIrisObjectID)).Single();
                    if (currentLink == null)
                    {
                        ActivityObjectRelationship contactProgrammeRelationship = new ActivityObjectRelationship
                        {
                            RelatedIRISObjectID = contactIrisObjectID,
                            ActivityObjectRelationshipTypeID = rel.Value,
                            ContactSubLink = CopyContactSubLink(contactSubLink, user),
                            CurrentFrom = options.ActionDate,
                            CreatedBy = user.AccountName,
                            DateCreated = DateTime.Now
                        };
                        programme.IRISObject.ActivityObjectRelationships.Add(contactProgrammeRelationship);
                    }
                    else
                    {
                        var sublink = Context.ContactSubLink.Where(c => c.ActivityObjectRelationshipID == currentLink.ID).SingleOrDefault();
                        if (sublink != null)
                        {
                            sublink.NameID = contactSubLink.NameID;
                            sublink.ContactAddressID = contactSubLink.ContactAddressID ?? null;
                            sublink.PhoneNumberID = contactSubLink.PhoneNumberID ?? null;
                            sublink.EmailID = contactSubLink.EmailID ?? null;
                            sublink.WebsiteID = contactSubLink.WebsiteID ?? null;
                            sublink.ModifiedBy = user.AccountName;
                            sublink.LastModified = DateTime.Now;
                        }
                    }
                }
            }

            if (options.LocationProgrammeLinks != null && options.LocationProgrammeLinks.Count > 0)
            {
                foreach (KeyValuePair<long, long> rel in options.LocationProgrammeLinks)
                {

                    long locationIrisObjectID = Context.Location.Where(c => c.ID == rel.Key).Single().IRISObjectID;
                    var currentLink = programme.IRISObject.ActivityObjectRelationships.Where(a => (a.IRISObjectID == locationIrisObjectID
                        || a.RelatedIRISObjectID == locationIrisObjectID) && a.ActivityObjectRelationshipTypeID == rel.Value && a.CurrentTo == null).SingleOrDefault();

                    if (currentLink == null)
                    {
                        ActivityObjectRelationship locationProgrammeRelationship = new ActivityObjectRelationship
                        {
                            RelatedIRISObjectID = Context.Location.Where(c => c.ID == rel.Key).Single().IRISObjectID,
                            ActivityObjectRelationshipTypeID = rel.Value,
                            CurrentFrom = options.ActionDate,
                            CreatedBy = user.AccountName,
                            DateCreated = DateTime.Now
                        };
                        programme.IRISObject.ActivityObjectRelationships.Add(locationProgrammeRelationship);
                    }
                }
            }
        }

        private void LinkContactsAndLocationsToRegime(BulkReplaceAuthorisationOptions options, Regime regime, Authorisation replaceAuth, User user)
        {
            if (options.ContactRegimeLinks != null && options.ContactRegimeLinks.Count > 0)
            {
                foreach (KeyValuePair<long, long> rel in options.ContactRegimeLinks)
                {
                    long contactIrisObjectID = Context.Contact.Where(c => c.ID == rel.Key).Single().IRISObjectID;
                    var currentLink = regime.IRISObject.ActivityObjectRelationships
                        .Where(a => (a.IRISObjectID == contactIrisObjectID || a.RelatedIRISObjectID == contactIrisObjectID)
                        && a.ActivityObjectRelationshipTypeID == rel.Value && a.CurrentTo == null).SingleOrDefault();

                    long relID = (replaceAuth.IRISObject.ActivityObjectRelationships
                        .Where(r => r.IRISObjectID == contactIrisObjectID || r.RelatedIRISObjectID == contactIrisObjectID).SingleOrDefault()
                        ?? replaceAuth.IRISObject.RelatedActivityObjectRelationships
                        .Where(r => r.IRISObjectID == contactIrisObjectID || r.RelatedIRISObjectID == contactIrisObjectID).SingleOrDefault()).ID;
                    
                    var contactSubLink = Context.ContactSubLink.Where(x => x.ActivityObjectRelationshipID == relID &&
                    (x.ActivityObjectRelationship.IRISObjectID == contactIrisObjectID || x.ActivityObjectRelationship.RelatedIRISObjectID == contactIrisObjectID)).Single();

                    if (currentLink == null)
                    {
                        ActivityObjectRelationship contactRegimeRelationship = new ActivityObjectRelationship
                        {
                            RelatedIRISObjectID = Context.Contact.Where(c => c.ID == rel.Key).Single().IRISObjectID,
                            ActivityObjectRelationshipTypeID = rel.Value,
                            ContactSubLink = CopyContactSubLink(contactSubLink, user),
                            CurrentFrom = options.ActionDate,
                            CreatedBy = user.AccountName,
                            DateCreated = DateTime.Now
                        };
                        regime.IRISObject.ActivityObjectRelationships.Add(contactRegimeRelationship);
                    }
                    else
                    {
                        var sublink = Context.ContactSubLink.Where(c => c.ActivityObjectRelationshipID == currentLink.ID).SingleOrDefault();
                        if (sublink != null)
                        {
                            sublink.NameID = contactSubLink.NameID;
                            sublink.ContactAddressID = contactSubLink.ContactAddressID ?? null;
                            sublink.PhoneNumberID = contactSubLink.PhoneNumberID ?? null;
                            sublink.EmailID = contactSubLink.EmailID ?? null;
                            sublink.WebsiteID = contactSubLink.WebsiteID ?? null;
                            sublink.ModifiedBy = user.AccountName;
                            sublink.LastModified = DateTime.Now;
                        }
                    }
                }
            }

            if (options.LocationRegimeLinks != null && options.LocationRegimeLinks.Count > 0)
            {
                foreach (KeyValuePair<long, long> rel in options.LocationRegimeLinks)
                {
                    long locationIrisObjectID = Context.Location.Where(c => c.ID == rel.Key).Single().IRISObjectID;
                    var currentLink = regime.IRISObject.ActivityObjectRelationships.Where(a => (a.IRISObjectID == locationIrisObjectID
                        || a.RelatedIRISObjectID == locationIrisObjectID) && a.ActivityObjectRelationshipTypeID == rel.Value && a.CurrentTo == null).SingleOrDefault()
                        ?? regime.IRISObject.RelatedActivityObjectRelationships.Where(a => (a.IRISObjectID == locationIrisObjectID
                        || a.RelatedIRISObjectID == locationIrisObjectID) && a.ActivityObjectRelationshipTypeID == rel.Value && a.CurrentTo == null).SingleOrDefault();

                    if (currentLink == null)
                    {
                        ActivityObjectRelationship locationRegimeRelationship = new ActivityObjectRelationship
                        {
                            RelatedIRISObjectID = Context.Location.Where(c => c.ID == rel.Key).Single().IRISObjectID,
                            ActivityObjectRelationshipTypeID = rel.Value,
                            CurrentFrom = options.ActionDate,
                            CreatedBy = user.AccountName,
                            DateCreated = DateTime.Now
                        };
                        regime.IRISObject.ActivityObjectRelationships.Add(locationRegimeRelationship);
                    }
                }
            }
        }

        private void LinkContactsAndLocationsToRegimeActivity(BulkReplaceAuthorisationOptions options, RegimeActivity regimeActivity, Authorisation replaceAuth, User user)
        {
            if (options.ContactRegimeActivityLinks != null && options.ContactRegimeActivityLinks.Count > 0)
            {
                foreach (KeyValuePair<long, long> rel in options.ContactRegimeActivityLinks)
                {
                    long contactIrisObjectID = Context.Contact.Where(c => c.ID == rel.Key).Single().IRISObjectID;
                    var currentLink = regimeActivity.IRISObject.ActivityObjectRelationships
                        .Where(a => (a.IRISObjectID == contactIrisObjectID || a.RelatedIRISObjectID == contactIrisObjectID)
                        && a.ActivityObjectRelationshipTypeID == rel.Value && a.CurrentTo == null).SingleOrDefault();

                    long relID = (replaceAuth.IRISObject.ActivityObjectRelationships
                        .Where(r => r.IRISObjectID == contactIrisObjectID || r.RelatedIRISObjectID == contactIrisObjectID).SingleOrDefault() 
                        ?? replaceAuth.IRISObject.RelatedActivityObjectRelationships
                        .Where(r => r.IRISObjectID == contactIrisObjectID || r.RelatedIRISObjectID == contactIrisObjectID).SingleOrDefault()).ID;

                    var contactSubLink = Context.ContactSubLink.Where(x => x.ActivityObjectRelationshipID == relID &&
                    (x.ActivityObjectRelationship.IRISObjectID == contactIrisObjectID || x.ActivityObjectRelationship.RelatedIRISObjectID == contactIrisObjectID)).Single();

                    if (currentLink == null)
                    {
                        ActivityObjectRelationship contactRegimeRelationship = new ActivityObjectRelationship
                        {
                            RelatedIRISObjectID = Context.Contact.Where(c => c.ID == rel.Key).Single().IRISObjectID,
                            ActivityObjectRelationshipTypeID = rel.Value,
                            ContactSubLink = CopyContactSubLink(contactSubLink, user),
                            CurrentFrom = options.ActionDate,
                            CreatedBy = user.AccountName,
                            DateCreated = DateTime.Now
                        };
                        regimeActivity.IRISObject.ActivityObjectRelationships.Add(contactRegimeRelationship);
                    }
                    else
                    {
                        var sublink = Context.ContactSubLink.Where(c => c.ActivityObjectRelationshipID == currentLink.ID).SingleOrDefault();
                        if (sublink != null)
                        {
                            sublink.NameID = contactSubLink.NameID;
                            sublink.ContactAddressID = contactSubLink.ContactAddressID ?? null;
                            sublink.PhoneNumberID = contactSubLink.PhoneNumberID ?? null;
                            sublink.EmailID = contactSubLink.EmailID ?? null;
                            sublink.WebsiteID = contactSubLink.WebsiteID ?? null;
                            sublink.ModifiedBy = user.AccountName;
                            sublink.LastModified = DateTime.Now;
                        }
                    }
                }
            }

            if (options.LocationRegimeActivityLinks != null && options.LocationRegimeActivityLinks.Count > 0)
            {
                foreach (KeyValuePair<long, long> rel in options.LocationRegimeActivityLinks)
                {
                    long locationIrisObjectID = Context.Location.Where(c => c.ID == rel.Key).Single().IRISObjectID;
                    var currentLink = regimeActivity.IRISObject.ActivityObjectRelationships.Where(a => (a.IRISObjectID == locationIrisObjectID
                        || a.RelatedIRISObjectID == locationIrisObjectID) && a.ActivityObjectRelationshipTypeID == rel.Value && a.CurrentTo == null).SingleOrDefault()
                        ?? regimeActivity.IRISObject.RelatedActivityObjectRelationships.Where(a => (a.IRISObjectID == locationIrisObjectID
                        || a.RelatedIRISObjectID == locationIrisObjectID) && a.ActivityObjectRelationshipTypeID == rel.Value && a.CurrentTo == null).SingleOrDefault();

                    if (currentLink == null)
                    {
                        ActivityObjectRelationship locationRegimeActivityRelationship = new ActivityObjectRelationship
                        {
                            RelatedIRISObjectID = Context.Location.Where(c => c.ID == rel.Key).Single().IRISObjectID,
                            ActivityObjectRelationshipTypeID = rel.Value,
                            CurrentFrom = options.ActionDate,
                            CreatedBy = user.AccountName,
                            DateCreated = DateTime.Now
                        };
                        regimeActivity.IRISObject.ActivityObjectRelationships.Add(locationRegimeActivityRelationship);
                    }
                }
            }
        }
        private ContactSubLink CopyContactSubLink(ContactSubLink sublink, User user)
        {
            
            var contactSubLink = new ContactSubLink
            {
                ContactID = sublink.ContactID,
                NameID = sublink.NameID,
                ContactAddressID = sublink.ContactAddressID ?? null,
                PhoneNumberID = sublink.PhoneNumberID ?? null,            
                EmailID = sublink.EmailID ?? null,
                WebsiteID = sublink.WebsiteID ?? null,
                CreatedBy = user.AccountName,
                DateCreated = DateTime.Now
            };
           return contactSubLink;
                
        }
    }
}
