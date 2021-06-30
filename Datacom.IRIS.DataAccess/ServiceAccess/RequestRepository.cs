using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common.Utils;
using Datacom.IRIS.DomainModel.DTO;


namespace Datacom.IRIS.DataAccess.ServiceAccess
{

    public class RequestRepository : RepositoryStore, IRequestRepository
    {
        public Request GetRequestByID(long requestId)
        {
            var requestQuery = from req in Context.Request
                               .Include(a => a.IRISObject)
                               .Include(a => a.IRISObject.ObjectTypeREF)
                               .Include(a => a.ContactMethodREF)
                               .Include(a => a.PriorityREF)
                               .Include(a => a.RequestTypeREF)
                               .Include(a => a.ResolutionREF)
                               .Include(a => a.ResponseMethodREF)
                               .Include(a => a.SatisfactionLevelREF)
                               .Include(a => a.ResolutionReferredToREF)
                               .Include(a => a.ResponseMethodREF)
                               .Include(a => a.RequestOfficerResponsibles)
                               .Include("RequestOfficerResponsibles.OfficerResponsible")
                               .Where(req => req.ID == requestId)
                               select req;


            return GetRequest(requestQuery);

        }

        public Request GetRequestByIRISObjectID(long irisObjectId)
        {
            var requestQuery = from req in Context.Request
                                .Include(a => a.IRISObject)
                                .Include(a => a.IRISObject.ObjectTypeREF)
                                .Include(a => a.IRISObject.SubClass1REF)
                                .Include(a => a.IRISObject.SubClass2REF)
                                .Include(a => a.IRISObject.SubClass3REF)
                                .Include("IRISObject.ActivityObjectRelationships.ActivityObjectRelationshipType")
                                .Include("IRISObject.ActivityObjectRelationships.ContactSublink")
                                     .Include(a => a.ContactMethodREF)
                                     .Include(a => a.PriorityREF)
                                     .Include(a => a.RequestTypeREF)
                                     .Include(a => a.ResolutionREF)
                                     .Include(a => a.ResponseMethodREF)
                                     .Include(a => a.SatisfactionLevelREF)
                                     .Include(a => a.ResolutionReferredToREF)
                                     .Include(a => a.ResponseMethodREF)
                                     .Include(a => a.RequestTypeIncident)
                                     .Include(a => a.RequestTypeRFS)
                                     .Include(a => a.RequestTypeIncident.RequestTypeIncidentResourceTypes)
                                     .Include(a => a.RequestTypeIncident.RequestTypeIncidentBreaches)
                                     .Where(req => req.IRISObjectID == irisObjectId)
                               select req;

            return requestQuery.Single().TrackAll();
        }


        private Request GetRequest(IQueryable<Request> requestQuery)
        {
            var request = requestQuery.First();

            request.IRISObject = (from n in Context.IRISObject
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
                                  where n.ID == request.IRISObjectID
                                  select n).Single();
                        
            /* 
             * The following set of queries will get the type specific fields for the appropriate Request type.
             * 
             */

            //Getting the Request Incident type specific fields.
            if (request.RequestTypeREF.Code == ReferenceDataValueCodes.RequestType.Incident)
            {

                var requestIncident = Context.RequestTypeIncident.Include(hs => hs.HazardousSubstances).Single(i => i.ID == request.RequestTypeIncidentID);
                requestIncident.IncidentSourceREF = Context.ReferenceDataValue.Single(rdv => rdv.ID == requestIncident.IncidentSourceREFID);

                if (requestIncident.AllegedSeverityREFID.HasValue)
                    requestIncident.AllegedSeverityREF = Context.ReferenceDataValue.Single(rdv => rdv.ID == requestIncident.AllegedSeverityREFID);

                if (requestIncident.AssessedSeverityREFID.HasValue)
                    requestIncident.AssessedSeverityREF = Context.ReferenceDataValue.Single(rdv => rdv.ID == requestIncident.AssessedSeverityREFID);

                if (requestIncident.IndustryPurposeREFID.HasValue)
                    requestIncident.IndustryPurposeREF = Context.ReferenceDataValue.Single(rdv => rdv.ID == requestIncident.IndustryPurposeREFID);

                request.RequestTypeIncident = requestIncident;

                // Getting Request Further Actions
                var requestTypeIncidentFurtherActions = Context.RequestTypeIncidentFurtherAction
                                                               .Include(f => f.FurtherActionTypeREF)
                                                               .Include(f => f.ConductedByREF)
                                                               .Where(f => f.RequestTypeIncidentID == requestIncident.ID && !f.IsDeleted);

                request.RequestTypeIncident.RequestTypeIncidentFurtherActions.AddTrackableCollection(requestTypeIncidentFurtherActions.ToList());


                // Getting Request Resource Types
                var requestTypeIncidentResourceTypes = Context.RequestTypeIncidentResourceType
                                                                .Include(r => r.ResourceTypeREF)
                                                                .Where(r => r.RequestTypeIncidentID == requestIncident.ID && !r.IsDeleted);

                request.RequestTypeIncident.RequestTypeIncidentResourceTypes.AddTrackableCollection(requestTypeIncidentResourceTypes.ToList());

                // Getting Request Breaches
                var requestTypeIncidentBreaches = Context.RequestTypeIncidentBreaches
                                                                .Include(r => r.BreachREF)
                                                                .Where(r => r.RequestTypeIncidentID == requestIncident.ID && !r.IsDeleted);

                request.RequestTypeIncident.RequestTypeIncidentBreaches.AddTrackableCollection(requestTypeIncidentBreaches.ToList());
            }

            //Get statuses and plans/rules/policies separately to make the resulting SQL query much easier
            var reqStatusesQuery = from reqStatus in Context.Statuses
                                                    .Include(s => s.StatusREF)
                                                    .Where(s => s.IRISObjectID == request.IRISObject.ID && !s.IsDeleted)
                                   select reqStatus;

            request.IRISObject.Statuses.AddRange(reqStatusesQuery.ToList());

            //Getting the Request For Service type specific fields.
            if (request.RequestTypeREF.Code == ReferenceDataValueCodes.RequestType.RequestForService)
            {
                var requestRFS = Context.RequestTypeRFS.Single(i => i.ID == request.RequestTypeRFSID);  

                requestRFS.RequestSourceREF = Context.ReferenceDataValue.Single(rdv => rdv.ID == requestRFS.RequestSourceREFID);
                requestRFS.User = Context.User.SingleOrDefault(u => u.ID == requestRFS.RequestingUserID);

                request.RequestTypeRFS = requestRFS;
            }

            //Getting the Biosecurity specific fields.
            if (request.IRISObject.SubClass1REF.Code == ReferenceDataValueCodes.RequestSubject.Biosecurity)
            {

                var dbquery = from reqSubjectBio in Context.RequestSubjectBiosecuritySpecies.Where(r => r.RequestID == request.ID && !r.IsDeleted)
                              from species in Context.Species.Where(s => s.ID == reqSubjectBio.SpeciesID)
                              from speciesType in Context.SpeciesTypes.Where(s => s.SpeciesID == species.ID)
                              select new { reqSubjectBio, species, speciesType};

                request.RequestSubjectBiosecuritySpecies.AddTrackableCollection(dbquery.AsEnumerable().Distinct().Select(x => x.reqSubjectBio).ToList());
            }

            //Getting the Transport specific fields.
            if (request.IRISObject.SubClass1REF.Code == ReferenceDataValueCodes.RequestSubject.Transport)
            {

                var dbquery = from reqSubjectTrans in Context.RequestSubjectTransportRoutes.Where(r => r.RequestID == request.ID && !r.IsDeleted)
                              from refData in Context.ReferenceDataValue.Where(s => s.ID == reqSubjectTrans.RouteREFID)
                              select new { reqSubjectTrans, refData };

                request.RequestSubjectTransportRoutes.AddTrackableCollection(dbquery.AsEnumerable().Distinct().Select(x => x.reqSubjectTrans).ToList());
            }

            // Plans and Rules
            RepositoryHelpers.LoadPlansRulesPoliciesForEntity(Context, request);

            // Get the Created By Display Name for the user
            var users = RepositoryMap.SecurityRepository.GetUserList();
            var user = users.FirstOrDefault(u => u.AccountName.ToLower() == request.CreatedBy.ToLower());
            request.CreatedByDisplayName = user != null ? user.DisplayName : request.CreatedBy;

            return request.TrackAll();

        }

        /// <summary>
        ///  Gets the full list of Officer Responsible for a Request
        /// </summary>
        public List<RequestOfficerResponsible> GetRequestOfficerResponsibleById(long requestId, long irisObjectId)
        {
            var dbOfficer = from request in Context.Request.Where(r => r.ID == requestId && r.IRISObjectID == irisObjectId)
                            from reqOfficer in Context.RequestOfficerResponsible.Where(r => r.RequestID == request.ID && !r.IsDeleted)
                            from refUserData in Context.User.Where(s => s.ID == reqOfficer.OfficerResponsibleID)
                            select new { reqOfficer, refUserData};

            return dbOfficer.AsEnumerable().Distinct().Select(x => x.reqOfficer).ToList();
        }

        /// <summary>
        ///  1) Gets the Cause and Effect record for given a give Id
        ///  2) Attaches the resulting record to RequestTypeIncident for viewing / editing purposes
        /// </summary>
        public RequestTypeIncidentCauseEffect GetRequestCauseAndEffectById(RequestTypeIncident requestTypeIncident, long id, long requestIRISObjectId)
        {
            var dbRequestCauseAndEffect = (from request in Context.Request.Where(x => x.IRISObjectID == requestIRISObjectId && x.RequestTypeIncidentID == requestTypeIncident.ID)
                                           from reqCauseEffect in Context.RequestTypeIncidentCauseEffect.Where(r => r.RequestTypeIncidentID == request.RequestTypeIncidentID &&  r.ID == id && !r.IsDeleted)
                                           select reqCauseEffect).AsEnumerable().Single();

            requestTypeIncident.RequestTypeIncidentCauseEffects.Add(dbRequestCauseAndEffect);
            return dbRequestCauseAndEffect;
          
        }

        /// <summary>
        ///  Gets the full list of Causes and Effects for a Request Incident type
        /// </summary>
        public List<RequestTypeIncidentCauseEffect> GetRequestCausesAndEffectsById(long requestTypeIncidentId, long requestIRISObjectId)
        {
            var dbRequestTypeIncidentCausesAndEffects = from request in Context.Request.Where(x => x.IRISObjectID == requestIRISObjectId && x.RequestTypeIncidentID == requestTypeIncidentId)
                                                        from reqCauseEffect in Context.RequestTypeIncidentCauseEffect.Where(r => r.RequestTypeIncidentID == request.RequestTypeIncidentID && !r.IsDeleted)
                                                        from refCause in Context.ReferenceDataValue.Where(r => r.ID == reqCauseEffect.CauseREFID)
                                                        from refEffect in Context.ReferenceDataValue.Where(r => r.ID == reqCauseEffect.EffectREFID)                                        
                                                        select new { reqCauseEffect, refCause, refEffect };

            return dbRequestTypeIncidentCausesAndEffects.AsEnumerable().Distinct().Select(x => x.reqCauseEffect).ToList();
        }

        public RequestOfficerResponsible GetLatestOfficerResponsible(long requestId, long irisObjectId)
        {
            return      Context.RequestOfficerResponsible
                        .Include(r => r.OfficerResponsible)        
                        .OrderByDescending(r => r.Date)
                        .ThenByDescending(r => r.ID)
                        .Where( r => !r.IsDeleted && r.RequestID == requestId && r.Request.IRISObjectID == irisObjectId)
                        .FirstOrDefault();
        }

        /// <summary>
        /// Get the status of the given request irisobjects for the maptips.
        /// </summary>
        /// <param name="irisObjectIDs"></param>
        /// <returns>Dictionary<request.IRISObjectID, KeyValuePair<RequestType, LatestStatus></returns>
        public Dictionary<long, KeyValuePair<string, string>> GetRequestStatusPairs(List<long> irisObjectIDs)
        {
            var result = (from request in Context.Request
                            .Where(r => irisObjectIDs.Contains(r.IRISObjectID))
                          from objectTypeREF in Context.ReferenceDataValue
                            .Where(r => r.ID == request.RequestTypeREFID)
                          from searchIndex in Context.SearchIndex
                            .Where(si => si.IRISObjectID == request.IRISObjectID)
                          select new { request.IRISObjectID, objectTypeREF.DisplayValue, searchIndex.Status })
                          .ToDictionary(r => r.IRISObjectID, r => new KeyValuePair<string, string>(r.DisplayValue, r.Status));
            return result;
        }

        public List<RequestObservationRow> GetRequestLinkedObservations(long requestID, long requestIRISID)
        {
            var result = from obs in Context.Observations.Where(o => o.RequestID == requestID)
                select new {obs};

            return result.Select(x => new RequestObservationRow
            {
                ID = x.obs.ID,
                IRISObjectID = x.obs.IRISObject.ID,
                ObservationDate = x.obs.ObservationDate,
                ObservationTime = x.obs.ObservationTime,
                ObservationIRISID = x.obs.IRISObject.BusinessID,
                Type = x.obs.IRISObject.SubClass2REF.DisplayValue,
                ObservingOfficers = x.obs.ObservationObservingOfficers.Where(o => !o.IsDeleted).Select(o => o.User.DisplayName)
            }).ToList();
        }
    }
}
