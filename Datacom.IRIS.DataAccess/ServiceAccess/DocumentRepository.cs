using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.Common;
using Datacom.IRIS.DomainModel.DTO;
using LinqKit;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class DocumentRepository : RepositoryStore, IDocumentRepository
    {
        /// <summary>
        /// Retrieves a list of all contact sublinks for a given IRIS object
        /// </summary>
        /// <param name="irisObjectID"></param>
        /// <returns></returns>
        public List<ContactSubLink> GetContactsSublinksByIRISObjectID(long irisObjectID,
                                                                      bool excludeDeceasedContact,
                                                                      ContactSelectionSendViaOption? sendViaOption,
                                                                      List<CDFQuestionAnswerSearchCriterion> contactGroupCDFSearchCriteria)
        {
            List<long> irisObjectIds = new List<long>();
            IRISObject irisObject = Context.IRISObject.Include(x => x.ObjectTypeREF).Single(x => x.ID == irisObjectID);
            irisObjectIds.Add(irisObjectID);

            switch (irisObject.ObjectTypeREF.Code)
            {
                case ReferenceDataValueCodes.ObjectType.RegimeActivity:
                    // Get Contacts from Regime Activity, Regime and Regime Subjects
                    irisObjectIds.AddRange(GetRegimeActivityIrisObjectIds(irisObjectID));
                    break;
                case ReferenceDataValueCodes.ObjectType.Observation:
                    // Get Contacts from Observation and related objects
                    irisObjectIds.AddRange(GetObservationObjectIds(irisObjectID));
                    break;
                case ReferenceDataValueCodes.ObjectType.Activity:
                    // Get Contacts of Activity and Application.
                    var applicationIrisObjectId = (from activity in Context.Activities
                                                   join app in Context.Applications on activity.ApplicationID equals app.ID
                                                   where activity.IRISObjectID == irisObjectID
                                                   select app.IRISObjectID).FirstOrDefault();
                    irisObjectIds.Add(applicationIrisObjectId);
                    break;
                case ReferenceDataValueCodes.ObjectType.EnforcementAction:
                    // Get Contacts to the Enforcement Action and Enforcement.
                    var enforcementIrisObjectId = (from act in Context.EnforcementActions
                                                   join enforcement in Context.Enforcements on act.EnforcementID equals enforcement.ID
                                                   where act.IRISObjectID == irisObjectID
                                                   select enforcement.IRISObjectID).FirstOrDefault();
                    irisObjectIds.Add(enforcementIrisObjectId);
                    break;
                case ReferenceDataValueCodes.ObjectType.Remediation:
                    var regimeActivityIrisObjectId = (from rem in Context.Remediations
                                                      join ra in Context.RegimeActivity on rem.RegimeActivityID equals ra.ID
                                                      where rem.IRISObjectID == irisObjectID
                                                      select ra.IRISObjectID).FirstOrDefault();
                    var mgtSiteIrisObjectId = (from rem in Context.Remediations
                                               join mgt in Context.ManagementSites on rem.ManagementSiteID equals mgt.ID
                                               where rem.IRISObjectID == irisObjectID
                                               select mgt.IRISObjectID).FirstOrDefault();
                    if (regimeActivityIrisObjectId != default(long)) irisObjectIds.Add(regimeActivityIrisObjectId);
                    if (mgtSiteIrisObjectId != default(long)) irisObjectIds.Add(mgtSiteIrisObjectId);
                    break;
                case ReferenceDataValueCodes.ObjectType.Regime:
                    irisObjectIds.AddRange(GetRegimeObjectIds(irisObjectID));
                    break;
                case ReferenceDataValueCodes.ObjectType.Enforcement:
                    irisObjectIds.AddRange(GetEnforcementObjectIds(irisObjectID));
                    break;
                case ReferenceDataValueCodes.ObjectType.LocationGroup:
                    irisObjectIds.AddRange(GetLocationGroupObjectIds(irisObjectID));
                    break;
            }

            List<ContactSubLink> list = new List<ContactSubLink>();
            foreach (var objectId in irisObjectIds)
            {
                var linkQuery = GetContactSubLinkQuery(objectId, excludeDeceasedContact, sendViaOption, contactGroupCDFSearchCriteria);
                var records = GetSubLinksFromQueryable(linkQuery);
                list.AddRange(records);
            }
            return list.Distinct().ToList();
        }

        private IQueryable<ContactSubLink> GetContactSubLinkQuery(long irisObjectID, bool excludeDeceasedContact,
                                                                  ContactSelectionSendViaOption? sendViaOption,
                                                                  List<CDFQuestionAnswerSearchCriterion> contactGroupCDFSearchCriteria = null)
        {
             var query =  from subLink in Context.ContactSubLink
                          from act in Context.ActivityObjectRelationship.Where(x => x.ID == subLink.ActivityObjectRelationshipID)
                          where (act.IRISObjectID == irisObjectID || act.RelatedIRISObjectID == irisObjectID) && !act.CurrentTo.HasValue
                          select subLink;

             if (excludeDeceasedContact)
                 query = from subLink in query
                         from contact in Context.Contact.Where(x => x.ID == subLink.ContactID)
                         where (contact.PersonIsDeceased == null || !contact.PersonIsDeceased.Value)
                         select subLink;

             switch (sendViaOption)
             {
                 case ContactSelectionSendViaOption.Mail:
                     query = from sublink in query
                             from contactAddress in Context.ContactAddress.Where(x => x.ID == sublink.ContactAddressID)
                             where !contactAddress.CurrentTo.HasValue
                             select sublink;
                     break;
                 case ContactSelectionSendViaOption.Email:
                     query = from sublink in query
                             from email in Context.Email.Where(x => x.ID == sublink.EmailID)
                             where !email.CurrentTo.HasValue
                             select sublink;
                     break;
                 case ContactSelectionSendViaOption.Either:
                     query = from sublink in query
                             from email in Context.Email.Where(x => x.ID == sublink.EmailID).DefaultIfEmpty()
                             from contactAddress in Context.ContactAddress.Where(x => x.ID == sublink.ContactAddressID).DefaultIfEmpty()
                             where (email != null && !email.CurrentTo.HasValue) || (contactAddress != null && !contactAddress.CurrentTo.HasValue)
                             select sublink;
                     break;
                 default:
                     break;
             }

            if (contactGroupCDFSearchCriteria != null && contactGroupCDFSearchCriteria.Count > 0)
            {
                var subLinkIDs = FilterContactGroupMemberByCriteria(irisObjectID, contactGroupCDFSearchCriteria);

                query = from sublink in query
                        where subLinkIDs.Contains(sublink.ID)
                        select sublink;
            }

            return query;
        }

        /// <summary>
        /// Get contacts related to Regime Activity
        ///   Get Contacts related to the parent Regime and Regime Subjects
        /// </summary>
        /// <param name="irisObjectId"></param>
        /// <returns></returns>
        private List<long> GetRegimeActivityIrisObjectIds(long irisObjectId)
        {
            var regimeQuery = from regime in Context.Regime
                        join schedule in Context.RegimeActivitySchedule on regime.ID equals schedule.RegimeID
                        join activity in Context.RegimeActivity on schedule.ID equals activity.RegimeActivityScheduleID
                        where activity.IRISObjectID == irisObjectId
                        select regime.IRISObjectID;
            long regimeIrisObjectID = regimeQuery.FirstOrDefault();

            var regimeSubjects = new List<string>() {
                                ObjectRelationshipTypesCodes.RegimeSubjectAuthorisation,
                                ObjectRelationshipTypesCodes.RegimeSubjectDamRegister,
                                ObjectRelationshipTypesCodes.RegimeSubjectManagementSite,
                                ObjectRelationshipTypesCodes.RegimeSubjectSelectedLandUseSite
                            };

            var relationshipQuery = from relationship in Context.ActivityObjectRelationship
                                    join relType in Context.ActivityObjectRelationshipType on relationship.ActivityObjectRelationshipTypeID equals relType.ID
                                    where regimeSubjects.Contains(relType.Code) &&
                                    relationship.CurrentTo == null &&
                                    (relationship.IRISObjectID == regimeIrisObjectID || relationship.RelatedIRISObjectID == regimeIrisObjectID)
                                    select relationship;
            var rels = relationshipQuery.ToList();
            var relationships = rels.Select(x => x.IRISObjectID == regimeIrisObjectID ? x.RelatedIRISObjectID : x.IRISObjectID).ToList();
            relationships.Add(regimeIrisObjectID);
            return relationships;
        }

        /// <summary>
        /// Get Contacts related to the Observation
        ///   If Observation belongs to Regime Activity then get contacts from Regime Activity, Regime and Regime Subjects
        ///   If Observation belongs to Management Site then get contacts from Management Site
        ///   If Observation belongs to Authorisation then get contacts from Authorisation
        ///   If Observation belongs to Request then get contacts from Request
        /// </summary>
        /// <param name="irisObjectId"></param>
        /// <returns></returns>
        private List<long> GetObservationObjectIds(long irisObjectId)
        {
            List<long> objectIds = new List<long>();
            Observation observation = Context.Observations.Single(x => x.IRISObjectID == irisObjectId);
            if (observation.RegimeActivityID.HasValue)
            {
                RegimeActivity activity = Context.RegimeActivity.FirstOrDefault(x => x.ID == observation.RegimeActivityID);
                var regimeObjectIds = GetRegimeActivityIrisObjectIds(activity.IRISObjectID);
                objectIds.Add(activity.IRISObjectID);
                objectIds.AddRange(regimeObjectIds);
            }
            else if (observation is ObservationMngt)
            {
                ObservationMngt obmgmt = observation as ObservationMngt;
                var mgmtIrisObjectId = (from mgmt in Context.ManagementSites
                                        where mgmt.ID == obmgmt.ManagementSiteID
                                        select mgmt.IRISObjectID).FirstOrDefault();
                objectIds.Add(mgmtIrisObjectId);
            }
            else if (observation.AuthorisationID.HasValue)
            {
                var authorisationObjectId = (from auth in Context.Authorisations
                                             where auth.ID == observation.AuthorisationID.Value
                                             select auth.IRISObjectID).FirstOrDefault();
                objectIds.Add(authorisationObjectId);
            }
            else if (observation.RequestID.HasValue)
            {
                var requestObjectId = (from request in Context.Request
                                       where request.ID == observation.RequestID.Value
                                       select request.IRISObjectID).FirstOrDefault();
                objectIds.Add(requestObjectId);
            }
            return objectIds;
        }

        /// <summary>
        /// Get Contacts related to Enforcement
        ///     Get contacts of linked Incident, Observation, Monitoring, Authorisation
        /// </summary>
        /// <param name="irisObjectId"></param>
        /// <returns></returns>
        private List<long> GetEnforcementObjectIds(long irisObjectId)
        {
            var enforcementRelations = new List<string>() {
                                ObjectRelationshipTypesCodes.IncidentEnforcement,
                                ObjectRelationshipTypesCodes.IncidentObservationEnforcement,
                                ObjectRelationshipTypesCodes.MonitoringEnforcement,
                                ObjectRelationshipTypesCodes.ObservationEnforcement,
                                ObjectRelationshipTypesCodes.ManagementSiteEnforcement,
                                ObjectRelationshipTypesCodes.AuthorisationEnforcement
                            };

            var relationshipQuery = from relationship in Context.ActivityObjectRelationship
                                    join relType in Context.ActivityObjectRelationshipType on relationship.ActivityObjectRelationshipTypeID equals relType.ID
                                    where enforcementRelations.Contains(relType.Code) &&
                                    relationship.CurrentTo == null &&
                                    (relationship.IRISObjectID == irisObjectId || relationship.RelatedIRISObjectID == irisObjectId)
                                    select relationship;
            var rels = relationshipQuery.ToList();
            var relationships = rels.Select(x => x.IRISObjectID == irisObjectId ? x.RelatedIRISObjectID : x.IRISObjectID).ToList();
            return relationships;
        }

        /// <summary>
        /// Get contacts related to the Regime
        ///   Get contacts of linked programme
        /// </summary>
        /// <param name="irisObjectId"></param>
        /// <returns></returns>
        private List<long> GetRegimeObjectIds(long irisObjectId)
        {
            var programmeRelations = new List<string>() {
                ObjectRelationshipTypesCodes.LinkedProgrammeRegime
            };
            var relationshipQuery = from relationship in Context.ActivityObjectRelationship
                                    join relType in Context.ActivityObjectRelationshipType on relationship.ActivityObjectRelationshipTypeID equals relType.ID
                                    where programmeRelations.Contains(relType.Code) &&
                                    relationship.CurrentTo == null &&
                                    (relationship.IRISObjectID == irisObjectId || relationship.RelatedIRISObjectID == irisObjectId)
                                    select relationship;
            var rels = relationshipQuery.ToList();
            var relationships = rels.Select(x => x.IRISObjectID == irisObjectId ? x.RelatedIRISObjectID : x.IRISObjectID).ToList();
            return relationships;
        }

        private List<long> GetLocationGroupObjectIds(long irisObjectId)
        {
            var relationshipQuery = from relationship in Context.ActivityObjectRelationship
                                    join relType in Context.ActivityObjectRelationshipType on relationship.ActivityObjectRelationshipTypeID equals relType.ID
                                    join obj in Context.ReferenceDataValue on relType.ObjectTypeID equals obj.ID
                                    join relObj in Context.ReferenceDataValue on relType.RelatedObjectTypeID equals relObj.ID
                                    where relationship.CurrentTo == null
                                        && (relationship.IRISObjectID == irisObjectId || relationship.RelatedIRISObjectID == irisObjectId)
                                        && ((obj.Code == ReferenceDataValueCodes.ObjectType.Location && relObj.Code == ReferenceDataValueCodes.ObjectType.LocationGroup)
                                            || (obj.Code == ReferenceDataValueCodes.ObjectType.LocationGroup && relObj.Code == ReferenceDataValueCodes.ObjectType.Location))
                                    select relationship;

            var rels = relationshipQuery.ToList();
            var relationships = rels.Select(x => x.IRISObjectID == irisObjectId ? x.RelatedIRISObjectID : x.IRISObjectID).ToList();
            return relationships;

        }

        /// <summary>
        /// Additional filtering on contact Group CDF member answer
        /// </summary>
        /// <param name="contactGroupIrisObjectID"></param>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private List<long> FilterContactGroupMemberByCriteria(long contactGroupIrisObjectID, List<CDFQuestionAnswerSearchCriterion> criteria)
        {
            var criteriaCount = criteria.Count();

            var queryable = from contactSubLink in Context.ContactSubLink
                            join activityObjectRelationship in Context.ActivityObjectRelationship on contactSubLink.ActivityObjectRelationshipID equals activityObjectRelationship.ID
                            join contactGroupMember in Context.ContactGroupMember on activityObjectRelationship.ID equals contactGroupMember.ActivityObjectRelationshipID
                            join contactGroup in Context.ContactGroup on contactGroupMember.ContactGroupID equals contactGroup.ID
                            join contactGroupQD in Context.ContactGroupQuestionDefinition on contactGroup.ID equals contactGroupQD.ContactGroupID
                            join cdfQuestion in Context.QuestionDefinition on contactGroupQD.QuestionDefinitionID equals cdfQuestion.ID
                            join cdfAnswer in Context.CDFFormattedAnswers on new { IRISObjectID = contactGroupMember.IRISObjectID, QuestionID = cdfQuestion.ID } equals
                                                                              new { IRISObjectID = cdfAnswer.IRISObjectID, QuestionID = cdfAnswer.QuestionID }
                            where contactGroup.IRISObjectID == contactGroupIrisObjectID && !cdfQuestion.IsDeleted && !cdfAnswer.IsDeleted
                            select new ContactSubLinkCDFAnswer
                            {
                                ContactSubLink = contactSubLink,
                                CDFQuestion = cdfQuestion,
                                CDFAnswer = cdfAnswer

                            };

            var cdfFilterPredicate = PredicateBuilder.False<ContactSubLinkCDFAnswer>();
            foreach (var criterion in criteria)
            {
                var temp = criterion;  //use of this temporary variable in the loop is required to avoid the outer vriable trap, where the same variable is catpured for each iteration of the foreach loop.
                cdfFilterPredicate = cdfFilterPredicate.Or(x => x.CDFQuestion.ID == temp.QuestionID &&
                                                                ((x.CDFQuestion.TypeCode != QuestionTypes.DateField && x.CDFQuestion.TypeCode != QuestionTypes.TwoChoiceRadioButton && x.CDFAnswer.Value == temp.TextAnswer) ||
                                                                 (x.CDFQuestion.TypeCode == QuestionTypes.DateField && temp.DateAnswerFromDate <= x.CDFAnswer.DateValue && temp.DateAnswerToDate >= x.CDFAnswer.DateValue) ||
                                                                 (x.CDFQuestion.TypeCode == QuestionTypes.TwoChoiceRadioButton && temp.TextAnswer == x.CDFAnswer.BoolValue)));
            }

            return queryable.AsExpandable().Where(cdfFilterPredicate)
                            .GroupBy(x => new
                            {
                                ContactSubLinkID = x.ContactSubLink.ID,
                                RowNumber = x.CDFAnswer.Row
                            })
                            .Where(x => x.Count() == criteriaCount)
                            .Select(x => x.Key.ContactSubLinkID)
                            .ToList();
        }

    



        /// <summary>
        /// Retrieves a list of contact based on a Predefined contact query
        /// The navigation properties of the contacts returned are not fully populated:
        /// Only IRISObject, Name,addresses and emails are populated
        /// </summary>
        public List<ContactSubLink> GetContactsSublinksByPredefinedQuery(long predefinedQueryID, long irisObjectID)
        {
            //We first retrieve the name of the SP to call based on the query ID, then we call it
            //to retrieve a list of contact IDs
            //We then populate the contact objects normally, using linq
            var q = from contactQuery in Context.ContactQueries.Where(cq => cq.ID == predefinedQueryID)
                    select contactQuery;
            var spName = q.AsEnumerable().First().StoredProcName;

            //ALL predefined queries should have a single parameter, called IRISObjectID of type int64 (bigint in SQL)
            var p = new SqlParameter("IRISObjectID", System.Data.DbType.Int64) { Value = irisObjectID };

            List<long> contactSubLinksIDs = Context.ExecuteListScalarSP<long>(spName, p);

            return GetContactsSublinksByIDs(contactSubLinksIDs);
        }

        public List<ContactSubLink> GetContactsSublinksByIDs(List<long> contactSubLinkIDs)
        {   
            var subLinks = from subLink in Context.ContactSubLink
                           where contactSubLinkIDs.Contains(subLink.ID)
                           select subLink;

            return GetSubLinksFromQueryable(subLinks);
        }

        public List<ContactQuery> GetContactQueries(long objectTypeID, long irisObjectID)
        {
            return Context.ContactQueries.Where(cq => cq.ObjectTypeREFID == objectTypeID && (!cq.IRISObjectID.HasValue || cq.IRISObjectID == irisObjectID)).ToList();
        }

        public List<ContactQuery> GetContactQueries(long objectTypeID)
        {
            return Context.ContactQueries.Where(cq => cq.ObjectTypeREFID == objectTypeID).ToList();
        }

        public List<ContactQuery> GetAllContactQueries()
        {
            return Context.ContactQueries.ToList();
        }

        public List<DocumentTemplate> GetDocumentTemplatesForObject(long irisObjectID)
        {
            return (from irisObject in Context.IRISObject
                    join template in Context.DocumentTemplates on irisObject.ObjectTypeID equals template.ObjectTypeREFID
                    where irisObject.ID == irisObjectID &&
                    !template.IsDeleted &&
                    ((template.SubClassification1REFID == null && template.SubClassification2REFID == null && template.SubClassification3REFID == null) 
                      ||
                     (template.SubClassification1REFID == irisObject.SubClass1ID && template.SubClassification2REFID == null && template.SubClassification3REFID == null)
                     ||
                     (template.SubClassification1REFID == irisObject.SubClass1ID && template.SubClassification2REFID == irisObject.SubClass2ID && template.SubClassification3REFID == null)
                     ||
                     (template.SubClassification1REFID == irisObject.SubClass1ID && template.SubClassification2REFID == irisObject.SubClass2ID && template.SubClassification3REFID == irisObject.SubClass3ID)
                    )
                    orderby template.DisplayName ascending 
                    select template).ToList();
        }

        public DocumentTemplate GetDocumentTemplateByID(long templateID)
        {
            return Context.DocumentTemplates
                          .Include(t => t.ObjectTypeREF)
                          .Include(t => t.DocumentTypeREF)
                          .Include(t => t.DocumentSubtypeREF)
                          .Where(t => t.ID == templateID)
                          .Single().TrackAll();
        }

        public List<DocumentTemplate> GetAllDocumentTemplates()
        {
            return Context.DocumentTemplates
                .Include(t => t.ObjectTypeREF)
                .Include(t => t.SubClassification1REF)
                .Include(t => t.SubClassification2REF)
                .Include(t => t.SubClassification3REF)
                .Where(t => !t.IsDeleted).ToList();
        }

        public List<MobileDocumentReferenceCriteria> GetMobileDocumentReferenceCriterion(long irisObjectId, bool isUserReferenced)
        {
            return Context.MobileDocumentReferenceCriterias.Where(
                    m => m.IRISObjectID == irisObjectId && m.IsUserReferenced == isUserReferenced).ToList();
        }

        public MobileDocumentReferenceCriteria DeleteMobileDocumentReferenceCriteria(long irisObjectId, string documentID, bool isUserReferenced)
        {
            var row = Context.MobileDocumentReferenceCriterias.SingleOrDefault(x => x.IRISObjectID == irisObjectId && x.DocumentID == documentID && x.IsUserReferenced == isUserReferenced);
            Context.MobileDocumentReferenceCriterias.DeleteObject(row);
            SaveChanges();

            return row;
        }

        public MobileDocumentReferenceCriteria AddMobileDocumentReferenceCriteria(long irisObjectId, string documentID, bool isUserReferenced)
        {
            var row = new MobileDocumentReferenceCriteria
            {
                IRISObjectID = irisObjectId,
                DocumentID = documentID,
                IsUserReferenced = isUserReferenced
            };
            Context.MobileDocumentReferenceCriterias.AddObject(row);
            SaveChanges();

            return row;
        }

        public List<DirectDocumentReferenceCriteria> GetDirectDocumentReferenceCriterionByIRISObjectID(long irisObjectId)
        {
            return Context.DirectDocumentReferenceCriterias.Include(x => x.IRISObject.ObjectTypeREF)
                              .Where(s => s.IRISObjectID == irisObjectId && !s.IsDeleted).ToList();
        }

        public DirectDocumentReferenceCriteria GetDirectDocumentReferenceCriteriaByID(long irisObjectId, long directDocumentReferenceCriteriaId)
        {
            return Context.DirectDocumentReferenceCriterias.Include(x => x.IRISObject.ObjectTypeREF)
                              .SingleOrDefault(s => s.ID == directDocumentReferenceCriteriaId && s.IRISObjectID == irisObjectId && !s.IsDeleted).TrackAll();
        }

        public bool IsDocumentIDUniqueForIRISObject(string documentID, long irisObjectID)
        {
            return Context.DirectDocumentReferenceCriterias.Where(i => i.IRISObjectID == irisObjectID && i.DocumentID == documentID && !i.IsDeleted).Count() == 0;
        }

        public Report GetReportByID(long reportID)
        {
            return Context.Reports.Where(r => r.ID == reportID).Single().TrackAll();
        }

        public Report GetInstanceReport(long irisObjectID, long reportID)
        {
            return (from report in GetReportsForIRISObject(irisObjectID)
                    where report.ID == reportID
                    select report).Single();
        }


        public Report GetGlobalReportByID(long reportID)
        {
            return Context.Reports.Include(r => r.ObjectTypeREF).Where(r => r.ID == reportID && r.IsGlobal).Single();
        }

        public List<Report> GetAllReports()
        {
            return Context.Reports
                .Include(r => r.ObjectTypeREF)
                .Include(r => r.SubClassification1REF)
                .Include(r => r.SubClassification2REF)
                .Include(r => r.SubClassification3REF)
                .Include(r => r.FolderREF)
                .Where(r => !r.IsDeleted).ToList();
        }

        private static object _lockObject = new object();

        /// <summary>
        /// Gets the next document reference number, for document generated by the user
        /// </summary>
        /// <returns></returns>
        public long GetNextDocumentReferenceNumber()
        {
            //Note: We use a lock to ensure that 2 reports generated at the same time do not have the same reference number
            //TODO - this should use transaction instead of lock as we need to cater for web farm scenario.
            //See GetNextBusinessID
            lock (_lockObject)
            {
                var setting = Context.Sequence.Single(s => s.Key == SequencesCodes.NextDocumentReferenceNumber).TrackAll();
                long refNumber = setting.Value;
                setting.Value++;
                ApplyEntityChanges(setting);
                SaveChanges();
                return refNumber;
            }
        }

        /// <summary>
        /// Returns all Instance reports for a given object type.
        /// </summary>
        /// <param name="objectTypeID"></param>
        /// <param name="subClass1ID"></param>
        /// <param name="subClass2ID"></param>
        /// <returns></returns>
        public List<Report> GetReportsForObject(long irisObjectID)
        {
            var results = from query in GetReportsForIRISObject(irisObjectID)
                        join report in Context.Reports on query.ID equals report.ID
                        select new
                        {
                            report,
                            report.ObjectTypeREF,
                            report.FolderREF
                        };

            return results.AsEnumerable().Select(x => x.report).Distinct().ToList();
        }

        /// <summary>
        /// Gets a list of all Global reports, with their folder names populated
        /// </summary>
        /// <returns></returns>
        public List<Report> GetGlobalReports()
        {
            var query = from report in Context.Reports.Where(r => r.IsGlobal && !r.IsDeleted)
                        select new
                        {
                            report,
                            report.ObjectTypeREF,
                            report.FolderREF
                        };
            return query.AsEnumerable().Select(x => x.report).Distinct().ToList();
        }



        private List<ContactSubLink> GetSubLinksFromQueryable(IQueryable<ContactSubLink> subLinkQuery)
        {
            var subLinks = from subLink in subLinkQuery
                           from act in Context.ActivityObjectRelationship.Where(x => x.ID == subLink.ActivityObjectRelationshipID)
                           from relType in Context.ActivityObjectRelationshipType.Where(x => x.ID == act.ActivityObjectRelationshipTypeID)
                           from contact in Context.Contact.Where(x => x.ID == subLink.ContactID)
                           from irisObject in Context.IRISObject.Where(x => x.ID == contact.IRISObjectID)
                           from contactSubClass1 in Context.ReferenceDataValue.Where(x => x.ID == irisObject.SubClass1ID)
                           from name in Context.Name.Where(x => x.ID == subLink.NameID)
                           from nameTitle in Context.ReferenceDataValue.Where(x => x.ID == name.PersonTitleID).DefaultIfEmpty()
                           from contactAddress in Context.ContactAddress.Where(x => x.ID == subLink.ContactAddressID).DefaultIfEmpty()
                           from email in Context.Email.Where(x => x.ID == subLink.EmailID).DefaultIfEmpty()
                           from address in Context.Address.Where(x => x.ID == contactAddress.AddressID).DefaultIfEmpty()
                           from deliveryAddress in Context.Address.OfType<DeliveryAddress>().Where(x => x.ID == address.ID).DefaultIfEmpty()
                           from deliveryAddressType in Context.ReferenceDataValue.Where(x => x.ID == deliveryAddress.DeliveryAddressTypeID).DefaultIfEmpty()
                           from overseasAddress in Context.Address.OfType<OverseasAddress>().Where(x => x.ID == address.ID).DefaultIfEmpty()
                           from country in Context.ReferenceDataValue.Where(x => x.ID == overseasAddress.CountryID).DefaultIfEmpty()
                           from urbanRuralAddress in Context.Address.OfType<UrbanRuralAddress>().Where(x => x.ID == address.ID).DefaultIfEmpty()
                           from floorType in Context.ReferenceDataValue.Where(x => x.ID == urbanRuralAddress.FloorTypeID).DefaultIfEmpty()
                           from unitType in Context.ReferenceDataValue.Where(x => x.ID == urbanRuralAddress.UnitTypeID).DefaultIfEmpty()
                           select new { subLink, act, relType, contact, contact.OrganisationStatusREF, irisObject, contactSubClass1, name, nameTitle, contactAddress, email,
                                        address, deliveryAddressType, country, floorType, unitType };

            var results = subLinks.AsEnumerable().Select(x => x.subLink).Distinct().ToList();
            return results;
        }


        private IQueryable<Report> GetReportsForIRISObject(long irisObjectID)
        {

            return from irisObject in Context.IRISObject
                   join report in Context.Reports on irisObject.ObjectTypeID equals report.ObjectTypeREFID
                   where irisObject.ID == irisObjectID && !report.IsDeleted && !report.IsGlobal &&
                     ((report.SubClassification1REFID == null && report.SubClassification2REFID == null && report.SubClassification3REFID == null)
                       ||
                      (report.SubClassification1REFID == irisObject.SubClass1ID && report.SubClassification2REFID == null && report.SubClassification3REFID == null)
                      ||
                      (report.SubClassification1REFID == irisObject.SubClass1ID && report.SubClassification2REFID == irisObject.SubClass2ID && report.SubClassification3REFID == null)
                      ||
                      (report.SubClassification1REFID == irisObject.SubClass1ID && report.SubClassification2REFID == irisObject.SubClass2ID && report.SubClassification3REFID == irisObject.SubClass3ID)
                     )
                   select report;
        }

        private class ContactSubLinkCDFAnswer
        {
            public ContactSubLink ContactSubLink { get; set; }
            public QuestionDefinition CDFQuestion { get; set; }
            public CDFFormattedAnswer CDFAnswer { get; set; }
        }
    }
}
