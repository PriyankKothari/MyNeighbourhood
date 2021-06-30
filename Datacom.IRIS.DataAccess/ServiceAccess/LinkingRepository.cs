using System;
using System.Collections.Generic;
using System.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.Common.Utils;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.DTO;
using Datacom.IRIS.DomainModel.Domain.Constants;
using WarningMessage = Datacom.IRIS.DomainModel.DTO.WarningMessage;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class LinkingRepository : RepositoryStore, ILinkingRepository
    {
        public List<RelationshipEntry> GetRelationshipLinks(RelationshipLinkCriteria criteria)
        {
            // Building the Activity Object Relationship Queryable
            IQueryable<ActivityObjectRelationship> activityObjectRelQueryable = Context.ActivityObjectRelationship.AsQueryable();

            if (!criteria.IncludeExpiredLinks)
            {
                activityObjectRelQueryable = activityObjectRelQueryable.Where(a => a.CurrentTo == null);
            }

            // Building the IRIS Object Queryable
            IQueryable<IRISObject> irisObjectQueryable = Context.IRISObject.AsQueryable();

            if (criteria.SearchForObjectTypeIDsSummary == null)
            {
                var objectTypeCollection = RepositoryMap.ReferenceDataRepository.GetReferenceDataCollections(new[] { ReferenceDataCollectionCode.IrisObjects }).Single();
                criteria.TranslateObjecTypeCodes(objectTypeCollection);
                if (criteria.IncludedObjectTypeIDList.Count() > 0)
                {
                    irisObjectQueryable = irisObjectQueryable.Where(i => criteria.IncludedObjectTypeIDList.Contains(i.ObjectTypeID));
                }

                if (criteria.ExcludedObjectTypeIDList.Count() > 0)
                {
                    irisObjectQueryable = irisObjectQueryable.Where(i => !criteria.ExcludedObjectTypeIDList.Contains(i.ObjectTypeID));
                }
            }
            else
            {
                //if SearchForObjectTypeIDsSummary is already populated, use it directly
                irisObjectQueryable = irisObjectQueryable.Where(i => criteria.SearchForObjectTypeIDsSummary.Contains(i.ObjectTypeID));
            }

            // Building the Relationship Queryable
            IQueryable<ActivityObjectRelationshipType> activityObjectRelTypeQueryable = Context.ActivityObjectRelationshipType.AsQueryable();

            if (criteria.IncludedRelationshipTypeCodeList != null)  // only set if a UI provides a value. no need to check for count()
            {
                activityObjectRelTypeQueryable = activityObjectRelTypeQueryable.Where(rt => criteria.IncludedRelationshipTypeCodeList.Contains(rt.Code));
            }

            if (criteria.ExcludedRelationshipTypeCodeList != null)
            {
                activityObjectRelTypeQueryable = activityObjectRelTypeQueryable.Where(rt => !criteria.ExcludedRelationshipTypeCodeList.Contains(rt.Code));
            }

            /**
             * Find query after queryable buildup.
             * We need to join to the ReferenceDataValue table to populate the IRIObject ObjectTypeREF 
             * property which is used when generating URL's for the different IRIS Objects
             */
            var queryDirection1 = from relationship in activityObjectRelQueryable.Where(r => r.IRISObjectID == criteria.IRISObjectID)
                                  from relatedIrisObject in irisObjectQueryable.Where(i => i.ID == relationship.RelatedIRISObjectID)
                                  from refData in Context.ReferenceDataValue.Where(v => v.ID == relatedIrisObject.ObjectTypeID)
                                  from relationshipType in activityObjectRelTypeQueryable.Where(rt => rt.ID == relationship.ActivityObjectRelationshipTypeID)
                                  from contact in Context.Contact.Where(c => c.ID == relationship.ContactSubLink.Name.ContactID).Distinct().DefaultIfEmpty()
                                  from statusRef in Context.ReferenceDataValue.Where(r => r.ID == contact.OrganisationStatusID).DefaultIfEmpty()
                                  let warningMessage = Context.WarningMessage.FirstOrDefault(x => x.IRISObjectID == relatedIrisObject.ID)
                                  let sublinkName = relationship.ContactSubLink.Name
                                  select new
                                  {
                                      relationship,
                                      relatedIrisObject,
                                      refData,
                                      relationshipType,
                                      sublinkName,
                                      sublinkName.PersonTitleREF,
                                      sublinkName.Contact,
                                      sublinkName.Contact.OrganisationStatusREF,
                                      warningMessage,
                                      relatedIrisObject.ObjectTypeREF,
                                      relatedIrisObject.SecurityContextIRISObject,
                                      SecurityContextIRISObjectTypeREF = relatedIrisObject.SecurityContextIRISObject.ObjectTypeREF
                                  };   //these last three properties are for security checking

            var queryDirection2 = from relationship in activityObjectRelQueryable.Where(r => r.RelatedIRISObjectID == criteria.IRISObjectID)
                                  from irisObject in irisObjectQueryable.Where(i => i.ID == relationship.IRISObjectID)
                                  from refData in Context.ReferenceDataValue.Where(v => v.ID == irisObject.ObjectTypeID)
                                  from relationshipType in activityObjectRelTypeQueryable.Where(rt => rt.ID == relationship.ActivityObjectRelationshipTypeID)
                                  from contact in Context.Contact.Where(c => c.ID == relationship.ContactSubLink.Name.ContactID).Distinct().DefaultIfEmpty()
                                  from statusRef in Context.ReferenceDataValue.Where(r => r.ID == contact.OrganisationStatusID).DefaultIfEmpty()
                                  let warningMessage = Context.WarningMessage.FirstOrDefault(x => x.IRISObjectID == irisObject.ID)
                                  let sublinkName = relationship.ContactSubLink.Name
                                  select new
                                  {
                                      relationship,
                                      irisObject,
                                      refData,
                                      relationshipType,
                                      sublinkName,
                                      sublinkName.PersonTitleREF,
                                      sublinkName.Contact,
                                      sublinkName.Contact.OrganisationStatusREF,
                                      warningMessage,
                                      irisObject.ObjectTypeREF,
                                      irisObject.SecurityContextIRISObject,
                                      SecurityContextIRISObjectTypeREF = irisObject.SecurityContextIRISObject.ObjectTypeREF
                                  };   //these last three properties are for security checking

            List<RelationshipEntry> irisObjects1 = queryDirection1.AsEnumerable().Distinct().Select(q => new RelationshipEntry
            {
                ActivityObjectRelationship = q.relationship,
                ActivityObjectRelationshipType = q.relationshipType,
                LinkedIRISObject = q.relatedIrisObject,
                WarningMessage = new WarningMessage { Message = q.warningMessage == null ? "" : q.warningMessage.Message },
                CustomLinkedItemDescription = q.relatedIrisObject.ObjectTypeREF.Code == ReferenceDataValueCodes.ObjectType.Contact ? q.sublinkName != null ? q.sublinkName.FormattedNameSingleInstance : null : null,
                ContactSubLink = q.relatedIrisObject.ObjectTypeREF.Code == ReferenceDataValueCodes.ObjectType.Contact ? AddAddressToSubLink(RepositoryMap.LinkingRepository.GetContactSubLinks(new List<long> {q.relationship.ID}).FirstOrDefault()) : null
            }).ToList();

            List<RelationshipEntry> irisObjects2 = queryDirection2.AsEnumerable().Distinct().Select(q => new RelationshipEntry
            {
                ActivityObjectRelationship = q.relationship,
                ActivityObjectRelationshipType = q.relationshipType,
                LinkedIRISObject = q.irisObject,
                WarningMessage = new WarningMessage { Message = q.warningMessage == null ? "" : q.warningMessage.Message },
                CustomLinkedItemDescription = q.irisObject.ObjectTypeREF.Code == ReferenceDataValueCodes.ObjectType.Contact ? q.sublinkName != null ? q.sublinkName.FormattedNameSingleInstance : null : null,
                ContactSubLink = q.irisObject.ObjectTypeREF.Code == ReferenceDataValueCodes.ObjectType.Contact ? AddAddressToSubLink(RepositoryMap.LinkingRepository.GetContactSubLinks(new List<long> { q.relationship.ID }).FirstOrDefault()) : null
            }).ToList();

            var relationshipEntries = irisObjects1.Concat(irisObjects2).ToList();
            if (criteria.IncludedObjectTypeCodeList.Contains(ReferenceDataValueCodes.ObjectType.Contact))
            {
                return relationshipEntries.DistinctBy(x => x.ContactID).ToList();
            }
            return relationshipEntries.ToList();
        }

        public ContactSubLink AddAddressToSubLink(ContactSubLink sublink)
        {
            if (sublink.ContactAddressID != null)
            {
                ContactAddress address = Context.ContactAddress.Include(n => n.Address).SingleOrDefault(x => x.ID == sublink.ContactAddressID);
                if (address.Address is OverseasAddress)
                {
                    var oc = address.Address as OverseasAddress;
                    oc.CountryREF = Context.ReferenceDataValue.SingleOrDefault(x => x.ID == oc.CountryID);
                }
                sublink.ContactAddress = address;
            }
            return sublink;
        }

        public List<ChangeSublinksItem> GetRelationshipLinksByContactSublinkType(long contactIRISObjectId, string sublinkType, long? sublinkTypeId)
        {
            var contactSublinkQueryable = Context.ContactSubLink.AsQueryable();

            switch (sublinkType)
            {
                case SublinkType.Names:
                    contactSublinkQueryable = sublinkTypeId == null
                        ? contactSublinkQueryable
                        : contactSublinkQueryable.Where(x => x.NameID == sublinkTypeId);
                    break;
                case SublinkType.Addresses:
                    contactSublinkQueryable = sublinkTypeId == null
                        ? contactSublinkQueryable
                        : contactSublinkQueryable.Where(x => x.ContactAddressID == sublinkTypeId);
                    break;
                case SublinkType.PhoneNumbers:
                    contactSublinkQueryable = sublinkTypeId == null
                        ? contactSublinkQueryable
                        : contactSublinkQueryable.Where(x => x.PhoneNumberID == sublinkTypeId);
                    break;
                case SublinkType.Emails:
                    contactSublinkQueryable = sublinkTypeId == null
                        ? contactSublinkQueryable
                        : contactSublinkQueryable.Where(x => x.EmailID == sublinkTypeId);
                    break;
                case SublinkType.Websites:
                    contactSublinkQueryable = sublinkTypeId == null
                        ? contactSublinkQueryable
                        : contactSublinkQueryable.Where(x => x.WebsiteID == sublinkTypeId);
                    break;
            }

            var queryDirection1 = from relationship in Context.ActivityObjectRelationship.Where(r => r.IRISObjectID == contactIRISObjectId && r.CurrentTo == null)
                                  from relatedIrisObject in Context.IRISObject.Where(i => i.ID == relationship.RelatedIRISObjectID)
                                  from refData in Context.ReferenceDataValue.Where(v => v.ID == relatedIrisObject.ObjectTypeID)
                                  from relationshipType in Context.ActivityObjectRelationshipType.Where(rt => rt.ID == relationship.ActivityObjectRelationshipTypeID)
                                  from contactSublink in contactSublinkQueryable.Where(s => s.ActivityObjectRelationshipID == relationship.ID).DefaultIfEmpty()
                                  from status in Context.Statuses.Where(s => s.IRISObjectID == relatedIrisObject.ID).DefaultIfEmpty()
                                  from contactGroup in Context.ContactGroup.Where(s => s.IRISObjectID == relatedIrisObject.ID).DefaultIfEmpty()
                                  select new
                                  {
                                      relationship,
                                      relatedIrisObject,
                                      status,
                                      status.StatusREF,
                                      refData,
                                      relationshipType,
                                      contactSublink,
                                      relatedIrisObject.ObjectTypeREF,
                                      relatedIrisObject.SecurityContextIRISObject,                // For security only
                                      SecurityContextIRISObjectTypeREF = relatedIrisObject.SecurityContextIRISObject.ObjectTypeREF, // For security only
                                      contactGroup,
                                      ContactGroupStatusREF = contactGroup.StatusREF  //To get contactGroup status - which is not stored in the Status table

                                  };
            var irisObjects1 = queryDirection1.AsEnumerable().Distinct().Select(q => new ChangeSublinksItem
            {
                ItemId = q.relationship.ID,
                As = q.relationship.ActivityObjectRelationshipType.Relationship,
                CurrentFrom = q.relationship.CurrentFrom,
                LinkedIRISObject = q.relatedIrisObject,
                ContactSubLink = q.contactSublink,
                LinkedIRISObjectCurrentStatus = q.contactGroup.Eval(x => x.StatusREF)
            }).ToList();

            var queryDirection2 = from relationship in Context.ActivityObjectRelationship.Where(r => r.RelatedIRISObjectID == contactIRISObjectId && r.CurrentTo == null)
                                  from relatedIrisObject in Context.IRISObject.Where(i => i.ID == relationship.IRISObjectID)
                                  from refData in Context.ReferenceDataValue.Where(v => v.ID == relatedIrisObject.ObjectTypeID)
                                  from relationshipType in Context.ActivityObjectRelationshipType.Where(rt => rt.ID == relationship.ActivityObjectRelationshipTypeID)
                                  from contactSublink in contactSublinkQueryable.Where(s => s.ActivityObjectRelationshipID == relationship.ID).DefaultIfEmpty()
                                  from status in Context.Statuses.Where(s => s.IRISObjectID == relatedIrisObject.ID).DefaultIfEmpty()
                                  from contactGroup in Context.ContactGroup.Where(s => s.IRISObjectID == relatedIrisObject.ID).DefaultIfEmpty()
                                  select new
                                  {
                                      relationship,
                                      relatedIrisObject,
                                      status,
                                      status.StatusREF,
                                      refData,
                                      relationshipType,
                                      contactSublink,
                                      relatedIrisObject.ObjectTypeREF,
                                      relatedIrisObject.SecurityContextIRISObject,                // For security only
                                      SecurityContextIRISObjectTypeREF = relatedIrisObject.SecurityContextIRISObject.ObjectTypeREF, // For security only
                                      contactGroup,
                                      ContactGroupStatusREF = contactGroup.StatusREF  //To get contactGroup status - which is not stored in the Status table
                                  };
            var irisObjects2 = queryDirection2.AsEnumerable().Distinct().Select(q => new ChangeSublinksItem
            {
                ItemId = q.relationship.ID,
                As = q.relationship.ActivityObjectRelationshipType.Relationship,
                CurrentFrom = q.relationship.CurrentFrom,
                LinkedIRISObject = q.relatedIrisObject,
                ContactSubLink = q.contactSublink,
                LinkedIRISObjectCurrentStatus = q.contactGroup.Eval(x => x.StatusREF)
            }).ToList();
            var relationshipEntries = irisObjects1.Concat(irisObjects2).DistinctBy(x => x.ItemId);

            var relationshipEntriesWithMatchingSubLinks = relationshipEntries.Where(x => x.ContactSubLink != null).ToList();


            if (sublinkType == SublinkType.Names || sublinkType == SublinkType.Websites || sublinkType == SublinkType.PhoneNumbers) return relationshipEntriesWithMatchingSubLinks;

            var appServicesAddresses = from appLink in relationshipEntries.Where(x => x.ObjectType == ReferenceDataValueCodes.ObjectType.Application)
                                       from app in Context.Applications.Where(a => a.ID == appLink.LinkedIRISObject.LinkID)
                                       from contact in Context.Contact.Where(c => c.IRISObjectID == contactIRISObjectId)
                                       from serviceAddress in Context.ServiceAddresses
                                       .Where(s => s.ContactID == contact.ID && s.ID == app.ServiceAddressID &&
                                              ((sublinkTypeId == null && sublinkType == SublinkType.Addresses && s.ContactAddressID != null) ||
                                               (sublinkTypeId == null && sublinkType == SublinkType.Emails && s.EmailID != null) ||
                                               (sublinkTypeId == s.ContactAddressID && sublinkType == SublinkType.Addresses) ||
                                               (sublinkTypeId == s.EmailID && sublinkType == SublinkType.Emails)))
                                       select new { appLink, app, serviceAddress };
            var appEntries = appServicesAddresses.AsEnumerable().Distinct().Select(q => new ChangeSublinksItem
            {
                ItemId = q.serviceAddress.ID,
                As = q.appLink.As,  //This will be replaced in the frontend to "Address For Service" anyway
                ServiceAddress = q.serviceAddress,
                LinkedIRISObject = q.appLink.LinkedIRISObject
            }).ToList();

            var authServicesAddresses = from authLink in relationshipEntries.Where(x => x.ObjectType == ReferenceDataValueCodes.ObjectType.Authorisation)
                                        from auth in Context.Authorisations.Where(a => a.ID == authLink.LinkedIRISObject.LinkID)
                                        from contact in Context.Contact.Where(c => c.IRISObjectID == contactIRISObjectId)
                                        from serviceAddress in Context.ServiceAddresses
                                       .Where(s => s.ContactID == contact.ID && s.ID == auth.ServiceAddressID &&
                                              ((sublinkTypeId == null && sublinkType == SublinkType.Addresses && s.ContactAddressID != null) ||
                                               (sublinkTypeId == null && sublinkType == SublinkType.Emails && s.EmailID != null) ||
                                               (sublinkTypeId == s.ContactAddressID && sublinkType == SublinkType.Addresses) ||
                                               (sublinkTypeId == s.EmailID && sublinkType == SublinkType.Emails)))
                                        select new { authLink, auth, serviceAddress };
            var authEntries = authServicesAddresses.AsEnumerable().Distinct().Select(q => new ChangeSublinksItem
            {
                ItemId = q.serviceAddress.ID,
                As = q.authLink.As,  //This will be replaced in the frontend to "Address For Service" anyway
                ServiceAddress = q.serviceAddress,
                LinkedIRISObject = q.authLink.LinkedIRISObject
            }).ToList();

            return relationshipEntriesWithMatchingSubLinks.Concat(appEntries).Concat(authEntries).ToList();
        }

        public List<ActivityObjectRelationship> GetRelatedLinksForObject(long IRISObjectID)
        {
            var linq = from activityRelationship in Context.ActivityObjectRelationship
                       join irisObject in Context.IRISObject on activityRelationship.IRISObjectID equals irisObject.ID
                       join relatedIrisObject in Context.IRISObject on activityRelationship.RelatedIRISObjectID equals relatedIrisObject.ID
                       join linkType in Context.ActivityObjectRelationshipType on activityRelationship.ActivityObjectRelationshipTypeID equals linkType.ID
                       join contactSublink in Context.ContactSubLink on activityRelationship.ID equals contactSublink.ActivityObjectRelationshipID into contacSublinkLeftJoin
                       from sublink in contacSublinkLeftJoin.DefaultIfEmpty()
                       where activityRelationship.IRISObjectID == IRISObjectID
                       select new
                       {
                           activityRelationship,
                           irisObject,
                           irisObject.ObjectTypeREF,
                           relatedIrisObject,
                           relatedIrisObjectTypeREF = relatedIrisObject.ObjectTypeREF,
                           linkType,
                           sublink
                       };

            var linq2 = from activityRelationship in Context.ActivityObjectRelationship
                        join irisObject in Context.IRISObject on activityRelationship.IRISObjectID equals irisObject.ID
                        join relatedIrisObject in Context.IRISObject on activityRelationship.RelatedIRISObjectID equals relatedIrisObject.ID
                        join linkType in Context.ActivityObjectRelationshipType on activityRelationship.ActivityObjectRelationshipTypeID equals linkType.ID
                        join contactSublink in Context.ContactSubLink on activityRelationship.ID equals contactSublink.ActivityObjectRelationshipID into contacSublinkLeftJoin
                        from sublink in contacSublinkLeftJoin.DefaultIfEmpty()
                        where activityRelationship.RelatedIRISObjectID == IRISObjectID
                        select new
                        {
                            activityRelationship,
                            irisObject,
                            irisObject.ObjectTypeREF,
                            relatedIrisObject,
                            relatedIrisObjectTypeREF = relatedIrisObject.ObjectTypeREF,
                            linkType,
                            sublink
                        };

            return linq.Concat(linq2).AsEnumerable().Select(x =>x.activityRelationship).ToList();

        }

        public ActivityObjectRelationshipType GetActivityObjectRelationshipTypeById(long id)
        {
            return Context.ActivityObjectRelationshipType
                    .Include(a => a.ObjectTypeREF)
                    .Include(a => a.RelatedObjectREF)
                    .SingleOrDefault(c => c.ID == id).TrackAll();
        }

        public List<ActivityObjectRelationshipType> GetAllActivityObjectRelationshipTypes()
        {
            return GetCachedAllActivityObjectRelationshipTypes();


            /*
            return Context.ActivityObjectRelationshipType
                    .Include(a => a.ObjectTypeREF)
                    .Include(a => a.RelatedObjectREF)
                    .ToList().TrackAll();*/
        }

        public List<ActivityObjectRelationshipType> GetAllActivityObjectRelationshipTypesFromDatabase()
        {
            return Context.ActivityObjectRelationshipType.Include(a => a.ObjectTypeREF).Include(a => a.RelatedObjectREF)
                .ToList();
        }

        /// <summary>
        ///    This method is to be used if a very particular relationship needs to be fetched.
        ///    So for example, in the case of a new application that is being created from a contact,
        ///    the initial link between the two entities need to be Contact vs. Application for
        ///    Relationship=Applicant.
        ///    
        ///    *Original Notes*
        ///    This method does not fetch the ObjectTypeREF and RelatedObjectTypeREF
        ///    for the given relationship. These two entities are also fetched as part of the Application
        ///    entity on create. Having the same entities fetched (albeit unchanged) will cause an 
        ///    entity framework error that complains about duplicate ID's.
        ///    ** Important Updated Notes **
        ///    By overriding the equality check in ReferenceDataValue we no longer have issue
        ///    with entity framework duplicate IDs error.  So it is safe to use the cache that has ObjectTypeREF
        ///    and RelatedObjectTypeREF populated.
        /// </summary>
        public ActivityObjectRelationshipType GetActivityObjectRelationshipWithCode(string fromObjectTypeCode, string toObjectTypeCode, string code = null)
        {
            fromObjectTypeCode = fromObjectTypeCode.ToLower();
            toObjectTypeCode = toObjectTypeCode.ToLower();
            var relationshipTypes = GetCachedAllActivityObjectRelationshipTypes()
                                    .Where(r => r.ObjectTypeREF != null && r.RelatedObjectREF != null &&
                                                ((r.ObjectTypeREF.Code.ToLower() == fromObjectTypeCode && r.RelatedObjectREF.Code.ToLower() == toObjectTypeCode) ||
                                                 (r.ObjectTypeREF.Code.ToLower() == toObjectTypeCode && r.RelatedObjectREF.Code.ToLower() == fromObjectTypeCode)) && r.IsActive
                                           );

            if (!string.IsNullOrEmpty(code))
            {
                code = code.ToLower();
                return relationshipTypes.Single(r => r.Code.ToLower() == code);
            }
            else
            {
                return relationshipTypes.Single();
            }
        }

        public ActivityObjectRelationship GetRelationshipDetailsByID(long id)
        {
            return (from a in Context.ActivityObjectRelationship
                        .Include("ActivityObjectRelationshipType")
                        .Include("IRISObject")
                        .Include("IRISObject.ObjectTypeREF")
                        .Include("IRISObject.SecurityContextIRISObject.ObjectTypeREF")
                        .Include("RelatedIRISObject")
                        .Include("RelatedIRISObject.ObjectTypeREF")
                        .Include("RelatedIRISObject.SecurityContextIRISObject.ObjectTypeREF")
                        .Include("ContactSubLink")
                    where a.ID == id
                    select a).SingleOrDefault().TrackAll();
        }

        public ILookup<long, KeyValuePair<long, ActivityObjectRelationship>> GetRelatedIrisObjects(long irisObjectId, long[] objectTypeIDList)
        {
            var linq = from activityRelationship in Context.ActivityObjectRelationship.Include("ActivityObjectRelationshipType")
                       join irisObject in Context.IRISObject
                            .WhereIn(e => e.ObjectTypeID, objectTypeIDList) on activityRelationship.RelatedIRISObjectID equals irisObject.ID
                       join relType in Context.ActivityObjectRelationshipType on activityRelationship.ActivityObjectRelationshipTypeID equals relType.ID
                       where activityRelationship.IRISObjectID == irisObjectId
                       select new
                       {
                           irisObject,
                           activityRelationship,
                           relType,
                           irisObject.ObjectTypeREF,
                           irisObject.SecurityContextIRISObject,
                           SecurityContextIRISObjectObjectTypeREF = irisObject.SecurityContextIRISObject.ObjectTypeREF  //for security checking
                       };

            var linq2 = from activityRelationship in Context.ActivityObjectRelationship.Include("ActivityObjectRelationshipType")
                        join irisObject in Context.IRISObject
                            .WhereIn(e => e.ObjectTypeID, objectTypeIDList) on activityRelationship.IRISObjectID equals irisObject.ID
                        join relType in Context.ActivityObjectRelationshipType on activityRelationship.ActivityObjectRelationshipTypeID equals relType.ID
                        where activityRelationship.RelatedIRISObjectID == irisObjectId
                        select new
                        {
                            irisObject,
                            activityRelationship,
                            relType,
                            irisObject.ObjectTypeREF,
                            irisObject.SecurityContextIRISObject,
                            SecurityContextIRISObjectObjectTypeREF = irisObject.SecurityContextIRISObject.ObjectTypeREF  //for security checking
                        };

            return linq.Concat(linq2).ToLookup(k => k.irisObject.ObjectTypeID, k => new KeyValuePair<long, ActivityObjectRelationship>(k.irisObject.ID, k.activityRelationship));
        }

        public ILookup<long, KeyValuePair<long, ActivityObjectRelationship>> GetRelatedIrisObjectsForManagementSiteIds(long[] managementSiteIds, long[] objectTypeIDList)
        {
            var linq = from mgmtSiteIO in Context.IRISObject
                       join mgmtTypeREF in Context.ReferenceDataValue on mgmtSiteIO.ObjectTypeID equals mgmtTypeREF.ID
                       join activityRelationship in Context.ActivityObjectRelationship.Include("ActivityObjectRelationshipType") on mgmtSiteIO.ID equals activityRelationship.RelatedIRISObjectID
                       join contactIO in Context.IRISObject on activityRelationship.IRISObjectID equals contactIO.ID
                       join linkedObjectTypeREF in Context.ReferenceDataValue
                            .WhereIn(e => e.ID, objectTypeIDList)  on contactIO.ObjectTypeID equals linkedObjectTypeREF.ID
                       join relType in Context.ActivityObjectRelationshipType on activityRelationship.ActivityObjectRelationshipTypeID equals relType.ID
                       where (mgmtSiteIO.LinkID.HasValue && managementSiteIds.Contains(mgmtSiteIO.LinkID.Value))
                       && mgmtTypeREF.Code == "ManagementSite"
                       select new
                       {
                           irisObject = contactIO,
                           activityRelationship,
                           relType,
                           contactIO.ObjectTypeREF,
                           contactIO.SecurityContextIRISObject,
                           SecurityContextIRISObjectObjectTypeREF = contactIO.SecurityContextIRISObject.ObjectTypeREF  //for security checking
                       };

            var linq2 = from mgmtSiteIO in Context.IRISObject
                        join mgmtTypeREF in Context.ReferenceDataValue on mgmtSiteIO.ObjectTypeID equals mgmtTypeREF.ID
                        join activityRelationship in Context.ActivityObjectRelationship.Include("ActivityObjectRelationshipType") on mgmtSiteIO.ID equals activityRelationship.IRISObjectID
                        join contactIO in Context.IRISObject on activityRelationship.RelatedIRISObjectID equals contactIO.ID
                        join linkedObjectTypeREF in Context.ReferenceDataValue
                            .WhereIn(e => e.ID, objectTypeIDList) on contactIO.ObjectTypeID equals linkedObjectTypeREF.ID
                        join relType in Context.ActivityObjectRelationshipType on activityRelationship.ActivityObjectRelationshipTypeID equals relType.ID
                        where (mgmtSiteIO.LinkID.HasValue && managementSiteIds.Contains(mgmtSiteIO.LinkID.Value))
                        && mgmtTypeREF.Code == "ManagementSite"
                        select new
                        {
                            irisObject = contactIO,
                            activityRelationship,
                            relType,
                            contactIO.ObjectTypeREF,
                            contactIO.SecurityContextIRISObject,
                            SecurityContextIRISObjectObjectTypeREF = contactIO.SecurityContextIRISObject.ObjectTypeREF  //for security checking
                        };

            return linq.Concat(linq2).ToLookup(k => k.irisObject.ObjectTypeID, k => new KeyValuePair<long, ActivityObjectRelationship>(k.irisObject.ID, k.activityRelationship));
        }

        /// <summary>
        ///  Get related IRISObjects which are one of objectTypeCodeList for the given irisObjectID.
        ///  The result will return in the following dictionary format:
        ///  ("Activity", new List<IRISObject> { object1, object2 })
        ///  ("Authorisation", new List<IRISObject> { object3, object4 })
        /// </summary>
        /// <param name="irisObjectId"></param>
        /// <param name="objectTypeCodeList"></param>
        /// <returns></returns>
        public Dictionary<string, List<IRISObject>> GetRelatedIrisObjects(long irisObjectId, string[] objectTypeCodeList)
        {
            // get linked IRISObjectID where the given irisObjectID is in either "IRISObjectID" column and "RelatedIRISObjectID" column.
            var linq = from activityRelationship in Context.ActivityObjectRelationship
                       join irisObject in Context.IRISObject
                            .Include(e => e.SubClass1REF)
                            .Include(e => e.SubClass2REF)
                            .Include(e => e.SubClass3REF)
                            .Include(e => e.ObjectTypeREF)
                            .WhereIn(e => e.ObjectTypeREF.Code, objectTypeCodeList) on activityRelationship.RelatedIRISObjectID equals irisObject.ID
                       where activityRelationship.IRISObjectID == irisObjectId
                       select new
                       {
                           irisObject.ObjectTypeREF,
                           irisObject.ObjectTypeREF.Code,
                           irisObject,
                           irisObject.SubClass1REF,
                           irisObject.SubClass2REF,
                           irisObject.SubClass3REF,
                           irisObject.SecurityContextIRISObject,
                           activityRelationship,
                           SecurityIRISObjectTypeREF = irisObject.SecurityContextIRISObject.ObjectTypeREF  //for security checking
                       };

            var linq2 = from activityRelationship in Context.ActivityObjectRelationship
                        join irisObject in Context.IRISObject
                             .Include(e => e.SubClass1REF)
                             .Include(e => e.SubClass2REF)
                             .Include(e => e.SubClass3REF)
                             .Include(e => e.ObjectTypeREF)
                             .WhereIn(e => e.ObjectTypeREF.Code, objectTypeCodeList) on activityRelationship.IRISObjectID equals irisObject.ID
                        where activityRelationship.RelatedIRISObjectID == irisObjectId
                        select new
                        {
                            irisObject.ObjectTypeREF,
                            irisObject.ObjectTypeREF.Code,
                            irisObject,
                            irisObject.SubClass1REF,
                            irisObject.SubClass2REF,
                            irisObject.SubClass3REF,
                            irisObject.SecurityContextIRISObject,
                            activityRelationship,
                            SecurityIRISObjectTypeREF = irisObject.SecurityContextIRISObject.ObjectTypeREF  //for security checking
                        };

            linq = linq.Concat(linq2);

            // sort the result in the lists, and put the lists in the dictionary.
            Dictionary<string, List<IRISObject>> result = new Dictionary<string, List<IRISObject>>();

            foreach (var temp in linq)
            {
                if (result.ContainsKey(temp.Code))
                {
                    result[temp.Code].Add(temp.irisObject);
                }
                else
                {
                    result.Add(temp.Code, new List<IRISObject> { temp.irisObject });
                }
            }

            return result;
        }

        public List<ActivityObjectRelationship> GetActivityObjectRelationshipByIrisObjectIDs(long irisObjectID1, long irisObjectID2)
        {
            //Try both ways as objects can be link from either direction e.g. Enforcement --> Contact  ||  Contact --> Enforcement
            //The include statements are for security check.
            return Context.ActivityObjectRelationship
                .Include("IRISObject")
                .Include("IRISObject.ObjectTypeREF")
                .Include("IRISObject.SecurityContextIRISObject.ObjectTypeREF")
                .Include("RelatedIRISObject")
                .Include("RelatedIRISObject.ObjectTypeREF")
                .Include("RelatedIRISObject.SecurityContextIRISObject.ObjectTypeREF")
                .Include("ActivityObjectRelationshipType")
                .Include("ContactSublink")
                .Where(x =>
                (
                    (x.IRISObjectID == irisObjectID1 && x.RelatedIRISObjectID == irisObjectID2)
                    ||
                    (x.IRISObjectID == irisObjectID2 && x.RelatedIRISObjectID == irisObjectID1)
                )).ToList();
        }

        public List<ActivityObjectRelationship> GetRelatedIrisObjects(long irisObjectId, long objectTypeID)
        {

            var queryDirection1 = from relationship in Context.ActivityObjectRelationship
                        .Where(r => r.IRISObjectID == irisObjectId)

                                  from relatedIrisObject in Context.IRISObject
                                    .Where(i => i.ID == relationship.RelatedIRISObjectID)
                                    .Where(i => i.ObjectTypeID == objectTypeID)

                                  from refData in Context.ReferenceDataValue
                                    .Where(v => v.ID == relatedIrisObject.ObjectTypeID)

                                  from relationshipType in Context.ActivityObjectRelationshipType
                                    .Where(rt => rt.ID == relationship.ActivityObjectRelationshipTypeID)

                                  let contactSublink = relationship.ContactSubLink

                                  select new { relationship, contactSublink, relatedIrisObject, refData, relationshipType };

            var queryDirection2 = from relationship in Context.ActivityObjectRelationship.Include(r => r.ContactSubLink)
                        .Where(r => r.RelatedIRISObjectID == irisObjectId)

                                  from relatedIrisObject in Context.IRISObject
                                    .Where(i => i.ID == relationship.IRISObjectID)
                                    .Where(i => i.ObjectTypeID == objectTypeID)

                                  from refData in Context.ReferenceDataValue
                                    .Where(v => v.ID == relatedIrisObject.ObjectTypeID)

                                  from relationshipType in Context.ActivityObjectRelationshipType
                                    .Where(rt => rt.ID == relationship.ActivityObjectRelationshipTypeID)

                                  let contactSublink = relationship.ContactSubLink

                                  select new { relationship, contactSublink, relatedIrisObject, refData, relationshipType };

            IEnumerable<ActivityObjectRelationship> relationships1 = queryDirection1.AsEnumerable().Distinct().Select(s => s.relationship);
            IEnumerable<ActivityObjectRelationship> relationships2 = queryDirection2.AsEnumerable().Distinct().Select(s => s.relationship);

            return relationships1.Concat(relationships2).ToList();

        }

        public List<LinkContactInformation> GetContactLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {
            IEnumerable<long> irisObjectIds = idsAndRelationships.Select(idr => idr.Key);

            var dbquery = from contact in Context.Contact
                                .Include(n => n.IRISObject)
                                .Include(n => n.IRISObject.ObjectTypeREF)
                          join preferredName in Context.Name on contact.ID equals preferredName.ContactID
                          where irisObjectIds.Contains(contact.IRISObjectID)
                          && preferredName.IsPreferred
                          select new
                          {
                              contacts = contact,
                              IRISObject = contact.IRISObject,
                              ObjectTypeREF = contact.IRISObject.ObjectTypeREF,
                              SubClass1REF = contact.IRISObject.SubClass1REF,
                              OrganisationStatusREF = contact.OrganisationStatusREF,
                              Name = preferredName,
                              NameTitleREF = preferredName.PersonTitleREF
                          };

            return dbquery.AsEnumerable().Select(c => c.contacts).Select(contact => new LinkContactInformation
            {
                Name = contact.Names.Single(n => n.IsPreferred),
                ObjectID = contact.ID,
                IRISObjectID = contact.IRISObjectID,
                IRISObject = contact.IRISObject,
                WarningComments = contact.WarningComments,
                ConfidentialReason = contact.ConfidentialReason
                //Type = LinkedObjectType.Contact
            }).ToList();
        }

        public string HasLinkedContactsWithNonCurrentAddressesAndEmails(long irisObjectId)
        {
            var relationships = from activityRelationship in Context.ActivityObjectRelationship.Where(x => !x.CurrentTo.HasValue && (x.IRISObjectID == irisObjectId || x.RelatedIRISObjectID == irisObjectId))
                                select activityRelationship;

            var linksWithContactsIds = (from relationship in relationships
                                        let irisObject = relationship.IRISObject
                                        let objectType = irisObject.ObjectTypeREF
                                        where objectType.Code == ReferenceDataValueCodes.ObjectType.Contact
                                        select relationship)
                                       .Union
                                           (from relationship in relationships
                                            let irisObject = relationship.RelatedIRISObject
                                            let objectType = irisObject.ObjectTypeREF
                                            where objectType.Code == ReferenceDataValueCodes.ObjectType.Contact
                                            select relationship);


            var sublinks = (from r in linksWithContactsIds
                            from sublink in Context.ContactSubLink.Where(sl => sl.ActivityObjectRelationshipID == r.ID)
                            from contactAddress in Context.ContactAddress.Where(ca => ca.ID == sublink.ContactAddressID).DefaultIfEmpty()
                            from emailAddress in Context.Email.Where(e => e.ID == sublink.EmailID).DefaultIfEmpty()
                            select new
                            {
                                AddressCurrentTo = contactAddress.CurrentTo,
                                EmailAddressCurrentTo = emailAddress.CurrentTo
                            }).ToList();

            var result = string.Empty;
            result += sublinks.Any(x => x.AddressCurrentTo != null) ? "address" : "";
            result += sublinks.Any(x => x.EmailAddressCurrentTo != null) ? "email" : "";




            return result.ToLower();
        }

        public string HasServiceAddressWithANonCurrentAddress(long irisObjectId, bool isAuth)
        {
            var serviceAddressAddresses = (isAuth
                                           ?
                                           (from item in Context.Authorisations.Where(a => a.IRISObjectID == irisObjectId)
                                            from serviceAddress in Context.ServiceAddresses.Where(x => item.ServiceAddressID == x.ID)
                                            select new { serviceAddress.ContactAddress, serviceAddress.Email })
                                           :
                                           (from item in Context.Applications.Where(a => a.IRISObjectID == irisObjectId)
                                            from serviceAddress in Context.ServiceAddresses.Where(x => item.ServiceAddressID == x.ID)
                                            select new { serviceAddress.ContactAddress, serviceAddress.Email })
                                           )
                                           .ToList();

            var contactAddresses = serviceAddressAddresses.Where(x => x.ContactAddress != null).Select(x => x.ContactAddress).ToList();
            var emailAddresses = serviceAddressAddresses.Where(x => x.Email != null).Select(x => x.Email).ToList();

            var result = string.Empty;

            result += contactAddresses.Any(x => !x.IsCurrent) ? "address" : "";
            //CR031 
            result += emailAddresses.Any(x => !x.IsCurrent) ? "email" : "";

            return result;
        }

        public List<LinkInformation> GetLocationLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {
            IEnumerable<long> ids = idsAndRelationships.Select(idr => idr.Key);
            var dbquery = from n in Context.Location
                         .Include(n => n.IRISObject)
                         .Include(n => n.IRISObject.ObjectTypeREF)
                         .Include("TextualLocation")
                         .Include("SpatialLocation")
                          where ids.Contains(n.IRISObjectID)
                          select new
                                {
                                    locations = n,
                                    IRISObject = n.IRISObject,
                                    ObjectTypeREF = n.IRISObject.ObjectTypeREF
                                };

            return dbquery.AsEnumerable().Select(c => c.locations).Select(location => new LinkInformation()
            {
                Value = location.CommonName,
                ObjectID = location.ID,
                IRISObjectID = location.IRISObjectID,
                IRISObject = location.IRISObject,
                WarningComments = location.WarningComments,

            }).ToList();
        }

        public List<LinkInformation> GetLocationGroupLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {
            IEnumerable<long> ids = idsAndRelationships.Select(idr => idr.Key);
            var dbquery = from n in Context.LocationGroup
                       .Include("IRISObject")
                       .Include("IRISObject.ObjectTypeREF")
                          where ids.Contains(n.IRISObjectID)
                          select new
                              {
                                  locationGroups = n,
                                  IRISObject = n.IRISObject,
                                  ObjectTypeREF = n.IRISObject.ObjectTypeREF
                              };

            return dbquery.AsEnumerable().Select(c => c.locationGroups).Select(locationGroup => new LinkInformation()
           {
               Value = locationGroup.CommonName,
               ObjectID = locationGroup.ID,
               IRISObjectID = locationGroup.IRISObjectID,
               IRISObject = locationGroup.IRISObject,
               WarningComments = locationGroup.WarningComments,
           }).ToList();

        }

        public List<LinkInformation> GetConditionScheduleLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {

            IEnumerable<long> ids = idsAndRelationships.Select(idr => idr.Key);
            var dbquery = from n in Context.ConditionSchedules
                       .Include(n => n.IRISObject)
                       .Include(n => n.IRISObject.ObjectTypeREF)
                          where ids.Contains(n.IRISObjectID)
                          select new
                         {
                             schedules = n,
                             IRISObject = n.IRISObject,
                             ObjectTypeREF = n.IRISObject.ObjectTypeREF
                         };

            return dbquery.AsEnumerable().Select(c => c.schedules).Select(schedule => new LinkInformation()
             {
                 Value = schedule.Name,
                 ObjectID = schedule.ID,
                 IRISObjectID = schedule.IRISObjectID,
                 IRISObject = schedule.IRISObject
             }).ToList();


        }

        public List<LinkInformation> GetApplicationLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {
            IEnumerable<long> ids = idsAndRelationships.Select(idr => idr.Key);
            var dbquery = from n in Context.Applications
                       .Include(n => n.IRISObject)
                       .Include(n => n.IRISObject.ObjectTypeREF)
                          where ids.Contains(n.IRISObjectID)
                          select new
                          {
                              application = n,
                              IRISObject = n.IRISObject,
                              ObjectTypeREF = n.IRISObject.ObjectTypeREF
                          };

            return dbquery.AsEnumerable().Select(c => c.application).Select(application => new LinkInformation()
            {
                Value = application.IRISObject.LinkDetails,
                ObjectID = application.ID,
                IRISObjectID = application.IRISObjectID,
                IRISObject = application.IRISObject
            }).ToList();
        }

        public List<LinkInformation> GetAuthorisationLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {

            IEnumerable<long> ids = idsAndRelationships.Select(idr => idr.Key);
            var dbquery = from n in Context.Authorisations
                       .Include(n => n.IRISObject)
                       .Include(n => n.IRISObject.ObjectTypeREF)
                          where ids.Contains(n.IRISObjectID)
                          select new
                          {
                              authorisation = n,
                              IRISObject = n.IRISObject,
                              ObjectTypeREF = n.IRISObject.ObjectTypeREF
                          };

            return dbquery.AsEnumerable().Select(c => c.authorisation).Select(authorisation => new LinkInformation()
            {
                Value = authorisation.IRISObject.LinkDetails,
                ObjectID = authorisation.ID,
                IRISObjectID = authorisation.IRISObjectID,
                IRISObject = authorisation.IRISObject
            }).ToList();
        }

        public List<LinkInformation> GetAuthorisationGroupLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {

            IEnumerable<long> ids = idsAndRelationships.Select(idr => idr.Key);
            var dbquery = from n in Context.AuthorisationGroups
                       .Include(n => n.IRISObject)
                       .Include(n => n.IRISObject.ObjectTypeREF)
                          where ids.Contains(n.IRISObjectID)
                          select new
                          {
                              authorisationGroup = n,
                              IRISObject = n.IRISObject,
                              ObjectTypeREF = n.IRISObject.ObjectTypeREF
                          };

            return dbquery.AsEnumerable().Select(c => c.authorisationGroup).Select(authorisationGroup => new LinkInformation()
            {
                Value = authorisationGroup.IRISObject.LinkDetails,
                ObjectID = authorisationGroup.ID,
                IRISObjectID = authorisationGroup.IRISObjectID,
                IRISObject = authorisationGroup.IRISObject
            }).ToList();
        }

        public List<LinkInformation> GetContactGroupLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {

            IEnumerable<long> ids = idsAndRelationships.Select(idr => idr.Key);
            var dbquery = from n in Context.ContactGroup
                       .Include(n => n.IRISObject)
                       .Include(n => n.IRISObject.ObjectTypeREF)
                          where ids.Contains(n.IRISObjectID)
                          select new
                          {
                              contactGroup = n,
                              IRISObject = n.IRISObject,
                              ObjectTypeREF = n.IRISObject.ObjectTypeREF
                          };

            return dbquery.AsEnumerable().Select(c => c.contactGroup).Select(contactGroup => new LinkInformation()
            {
                Value = contactGroup.IRISObject.LinkDetails,
                ObjectID = contactGroup.ID,
                IRISObjectID = contactGroup.IRISObjectID,
                IRISObject = contactGroup.IRISObject
            }).ToList();
        }

        /// <summary>
        ///    An activity relationship is bi-directional for each relationship type. Hence this method performs check. 
        ///    In the meanwhile it also check if the found relationship is currently valid. It will return the 
        ///    relationshipID if it exists.
        /// </summary>
        public ActivityObjectRelationship HasExistingCurrentActivityRelationship(ActivityObjectRelationship relationship)
        {
            return (from i in Context.ActivityObjectRelationship
                    where
                        ((i.IRISObjectID == relationship.IRISObjectID && i.RelatedIRISObjectID == relationship.RelatedIRISObjectID)
                        || (i.IRISObjectID == relationship.RelatedIRISObjectID && i.RelatedIRISObjectID == relationship.IRISObjectID)) &&
                        i.ActivityObjectRelationshipTypeID == relationship.ActivityObjectRelationshipTypeID &&
                        i.ID != relationship.ID &&
                        !i.CurrentTo.HasValue
                    select i).SingleOrDefault();
        }

        //This is used only by the admin module validation.  Since this is not used frequent, not need to use the cache.
        public bool HasExistingCurrentActivityRelationshipType(long id, long? objectTypeId, long? relatedObjectId, string relationship)
        {
            if (objectTypeId == null && relatedObjectId == null)
            {
                return (from i in Context.ActivityObjectRelationshipType
                        where
                            i.ObjectTypeID == null && i.RelatedObjectTypeID == null && i.Relationship == relationship && i.ID != id
                        select i).FirstOrDefault() != null;
            }
            return (from i in Context.ActivityObjectRelationshipType
                    where
                        ((i.ObjectTypeID == objectTypeId && i.RelatedObjectTypeID == relatedObjectId)
                         || (i.ObjectTypeID == relatedObjectId && i.RelatedObjectTypeID == objectTypeId)) &&
                        i.Relationship == relationship && i.ID != id
                    select i).FirstOrDefault() != null;
        }

        //This is used only by the admin module validation.  Since this is not used frequent, not need to use the cache.
        public bool HasExistingCurrentActivityRelationshipTypeCode(long id, long? objectTypeId, long? relatedObjectId, string relationshipCode)
        {
            if (objectTypeId == null && relatedObjectId == null)
            {
                return (from i in Context.ActivityObjectRelationshipType
                        where
                            i.ObjectTypeID == null && i.RelatedObjectTypeID == null && i.Code == relationshipCode && i.ID != id
                        select i).FirstOrDefault() != null;
            }
            return (from i in Context.ActivityObjectRelationshipType
                    where
                        ((i.ObjectTypeID == objectTypeId && i.RelatedObjectTypeID == relatedObjectId)
                         || (i.ObjectTypeID == relatedObjectId && i.RelatedObjectTypeID == objectTypeId)) &&
                        i.Code == relationshipCode && i.ID != id
                    select i).FirstOrDefault() != null;
        }

        /// <summary>
        ///    Updates the link details column for linkable objects
        /// </summary>
        public void PopulateLinkDetails(long? irisObjectID)
        {
            Context.PopulateLinkDetails(irisObjectID);
        }

        /// <summary>
        /// Add ObjectID to the IRISObject table
        /// </summary>
        public void PopulateObjectID(long? objectId, long? irisObjectId)
        {
            Context.PopulateObjectID(objectId, irisObjectId);
        }

        private static object _cachedRelationshipTypesSyncLock = new object();
        private static List<ActivityObjectRelationshipType> _cachedRelationshipTypes;
        private List<ActivityObjectRelationshipType> GetCachedAllActivityObjectRelationshipTypes()
        {
            lock (_cachedRelationshipTypesSyncLock)
            {
                if (_cachedRelationshipTypes != null)
                {
                    //UtilLogger.Instance.LogDebug("Cache Get for [GetCachedAllActivityObjectRelationshipTypes] with DataSource LocalCopy.");
                    return _cachedRelationshipTypes.ReturnFromCache();
                }

                Action<bool> onCacheInvalidated = (doRecache) =>
                {
                    lock (_cachedRelationshipTypesSyncLock)
                    {
                        CacheHelper.RemoveCacheEntry<ActivityObjectRelationshipType>(CacheConstants.ActivityObjectRelationshipTypes_RelationshipTypes);
                        CacheHelper.RemoveCacheEntry<ReferenceDataValue>(CacheConstants.ActivityObjectRelationshipTypes_ObjectTypes);
                        _cachedRelationshipTypes = null;
                    }

                    //recache
                    if (doRecache)
                    {
                        using (var repository = RepositoryMap.LinkingRepository)
                        {
                            repository.GetAllActivityObjectRelationshipTypes();
                        }
                    }
                };

                // NOTE: DON"T DELETE
                // It makes use of the Entity framework auto-fixup feature.  Once the entity has been pulled into the 
                // EF object graph, it will auto hook up the relationships. This is to work around the fact that caching  
                // (relying on sql dependency)  does not work with Left join.
                var objectQuery = (from val in Context.ReferenceDataValue
                                   join coll in Context.ReferenceDataCollection on val.CollectionID equals coll.ID
                                   where (coll.Code == ReferenceDataCollectionCode.IrisObjects ||
                                          coll.Code == "EnsureUniqueQuery_ActivityObjectRelationshipTypes")  //This is a dummy OR clause to make sure this query is unique for query notification registration
                                   select val).FromCached(CacheConstants.ActivityObjectRelationshipTypes_ObjectTypes, onCacheInvalidated)
                                   .ToList();

                var relationshipTypeQuery = Context.ActivityObjectRelationshipType.FromCached(CacheConstants.ActivityObjectRelationshipTypes_RelationshipTypes, onCacheInvalidated).ToList();

                _cachedRelationshipTypes = relationshipTypeQuery;


                return _cachedRelationshipTypes.ReturnFromCache();
            }
        }

        public List<ContactSubLink> GetContactSubLinks(List<long> relationshipIDs)
        {
            return Context.ContactSubLink.Include(x => x.Name)
                                         .Include(x => x.Name.PersonTitleREF)
                                         .Include(x => x.PhoneNumber)
                                         .Include(x => x.Email)
                                         .Include(x => x.Contact.OrganisationStatusREF)
                                         .Where(x => relationshipIDs.Contains(x.ActivityObjectRelationshipID)).ToList();

            //TODO - Untested code to support additional security check, see comments in ILinkingRepository
            //return Context.ContactSubLink
            //        .Include(x=> x.ActivityObjectRelationship)
            //        .Include(x => x.ActivityObjectRelationship.IRISObject)
            //        .Include(x => x.ActivityObjectRelationship.IRISObject.ObjectTypeREF)
            //        .Include(x => x.ActivityObjectRelationship.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
            //        .Include(x => x.ActivityObjectRelationship.RelatedIRISObject)
            //        .Include(x => x.ActivityObjectRelationship.RelatedIRISObject.ObjectTypeREF)
            //        .Include(x => x.ActivityObjectRelationship.RelatedIRISObject.SecurityContextIRISObject.ObjectTypeREF)
            //    .Where(x => relationshipIDs.Contains(x.ActivityObjectRelationshipID)).ToList();
        }


        //Check if there is any linked Contacts/linked Location have warnings.  
        //Note that it is including non-current linked objects as per current implementation
        //IRIS-1286 add inculdeNonCurrentObjects parameter 
        public bool HasAnyLinkedObjectsHaveWarning(long irisObjectID, bool checkLinkedContacts, bool checkLinkedLocations,bool includeNonCurrentObjects)
        {
            bool isLinkedObjectIDWithWarning = false;
            if (checkLinkedContacts)
            {
                isLinkedObjectIDWithWarning = (from r in Context.ActivityObjectRelationship
                              join irisObject in Context.IRISObject on r.IRISObjectID equals irisObject.ID
                              join contact in Context.Contact on irisObject.ID equals contact.IRISObjectID
                              where r.RelatedIRISObjectID == irisObjectID && !string.IsNullOrEmpty(contact.WarningComments) && (includeNonCurrentObjects || r.CurrentTo == null || r.CurrentTo > DateTime.Now)
                              select true
                              ).FirstOrDefault();

                if (!isLinkedObjectIDWithWarning)
                {
                    isLinkedObjectIDWithWarning = (from r in Context.ActivityObjectRelationship
                                  join irisObject in Context.IRISObject on r.RelatedIRISObjectID equals irisObject.ID
                                  join contact in Context.Contact on irisObject.ID equals contact.IRISObjectID
                                  where r.IRISObjectID == irisObjectID && !string.IsNullOrEmpty(contact.WarningComments) && (includeNonCurrentObjects || r.CurrentTo == null || r.CurrentTo > DateTime.Now)
                                                   select true
                                  ).FirstOrDefault();
                }
            }

            if (!isLinkedObjectIDWithWarning && checkLinkedLocations)
            {
                isLinkedObjectIDWithWarning = (from r in Context.ActivityObjectRelationship
                              join irisObject in Context.IRISObject on r.IRISObjectID equals irisObject.ID
                              join location in Context.Location on irisObject.ID equals location.IRISObjectID
                              where r.RelatedIRISObjectID == irisObjectID && !string.IsNullOrEmpty(location.WarningComments) && (includeNonCurrentObjects || r.CurrentTo == null || r.CurrentTo > DateTime.Now)
                                               select true
                              ).FirstOrDefault();

                if (!isLinkedObjectIDWithWarning)
                {
                    isLinkedObjectIDWithWarning = (from r in Context.ActivityObjectRelationship
                                  join irisObject in Context.IRISObject on r.RelatedIRISObjectID equals irisObject.ID
                                  join location in Context.Location on irisObject.ID equals location.IRISObjectID
                                  where r.IRISObjectID == irisObjectID && !string.IsNullOrEmpty(location.WarningComments) && (includeNonCurrentObjects || r.CurrentTo == null || r.CurrentTo > DateTime.Now)
                                                   select true
                                  ).FirstOrDefault();
                }
            }

            // if any linked object has warning, return true.
            return isLinkedObjectIDWithWarning;
        }

        //Check if there is any linked Location have warnings or any child location linked contact have warnings.  
        //Note that it is including non-current linked objects as per current implementation
        public bool HasAnyLinkedObjectsHaveWarningForLocationGroup(long locationGroupIRISObjectID)
        {
            bool hasWarning = false;

            // Location - LocationGroup
            hasWarning = (from r in Context.ActivityObjectRelationship
                          join irisObject in Context.IRISObject on r.IRISObjectID equals irisObject.ID
                          join location in Context.Location on irisObject.ID equals location.IRISObjectID
                          where r.RelatedIRISObjectID == locationGroupIRISObjectID && !string.IsNullOrEmpty(location.WarningComments)
                          select r.ID
                                ).ToList().Any();

            if (!hasWarning)
            {
                // LocationGroup - Location
                hasWarning = (from r in Context.ActivityObjectRelationship
                              join irisObject in Context.IRISObject on r.RelatedIRISObjectID equals irisObject.ID
                              join location in Context.Location on irisObject.ID equals location.IRISObjectID
                              where r.IRISObjectID == locationGroupIRISObjectID && !string.IsNullOrEmpty(location.WarningComments)
                              select r.ID
                               ).ToList().Any();
            }

            if (!hasWarning)
            {
                // Contact - Location, Location - LocationGroup
                hasWarning = (from LCRelationship in Context.ActivityObjectRelationship
                              join contact in Context.Contact on LCRelationship.IRISObjectID equals contact.IRISObjectID
                              join location in Context.Location on LCRelationship.RelatedIRISObjectID equals location.IRISObjectID

                              join LGLRelationship in Context.ActivityObjectRelationship on location.IRISObjectID equals LGLRelationship.IRISObjectID
                              where LGLRelationship.RelatedIRISObjectID == locationGroupIRISObjectID && !string.IsNullOrEmpty(contact.WarningComments)
                              select LCRelationship.ID).ToList().Any();
            }

            if (!hasWarning)
            {
                // Contact - Location, LocationGroup - Location
                hasWarning = (from LCRelationship in Context.ActivityObjectRelationship
                              join contact in Context.Contact on LCRelationship.IRISObjectID equals contact.IRISObjectID
                              join location in Context.Location on LCRelationship.RelatedIRISObjectID equals location.IRISObjectID

                              join LGLRelationship in Context.ActivityObjectRelationship on location.IRISObjectID equals LGLRelationship.RelatedIRISObjectID
                              where LGLRelationship.IRISObjectID == locationGroupIRISObjectID && !string.IsNullOrEmpty(contact.WarningComments)
                              select LCRelationship.ID).ToList().Any();
            }

            if (!hasWarning)
            {
                // Location - Contact, Location - LocationGroup
                hasWarning = (from LCRelationship in Context.ActivityObjectRelationship
                              join contact in Context.Contact on LCRelationship.RelatedIRISObjectID equals contact.IRISObjectID
                              join location in Context.Location on LCRelationship.IRISObjectID equals location.IRISObjectID

                              join LGLRelationship in Context.ActivityObjectRelationship on location.IRISObjectID equals LGLRelationship.IRISObjectID
                              where LGLRelationship.RelatedIRISObjectID == locationGroupIRISObjectID && !string.IsNullOrEmpty(contact.WarningComments)
                              select LCRelationship.ID).ToList().Any();
            }

            if (!hasWarning)
            {
                // Location - Contact, LocationGroup - Location
                hasWarning = (from LCRelationship in Context.ActivityObjectRelationship
                              join contact in Context.Contact on LCRelationship.RelatedIRISObjectID equals contact.IRISObjectID
                              join location in Context.Location on LCRelationship.IRISObjectID equals location.IRISObjectID

                              join LGLRelationship in Context.ActivityObjectRelationship on location.IRISObjectID equals LGLRelationship.RelatedIRISObjectID
                              where LGLRelationship.IRISObjectID == locationGroupIRISObjectID && !string.IsNullOrEmpty(contact.WarningComments)
                              select LCRelationship.ID).ToList().Any();
            }

            return hasWarning;
        }

        public bool HasRelationshipLinksFor(long irisObjectID, LinkedObjectType linkedObjectType, bool includeNonCurrent)
        {
            string objectTypeCode = linkedObjectType.GetStringValue();

            var hasLinkedObject = (from r in Context.ActivityObjectRelationship
                                 join relationshipType in Context.ActivityObjectRelationshipType on r.ActivityObjectRelationshipTypeID equals relationshipType.ID
                                 join objectTypeREF in Context.ReferenceDataValue on relationshipType.ObjectTypeID equals objectTypeREF.ID
                                 join relatedObjectTypeREF in Context.ReferenceDataValue on relationshipType.RelatedObjectTypeID equals relatedObjectTypeREF.ID
                                 where ((includeNonCurrent || !r.CurrentTo.HasValue) &&
                                       ((r.RelatedIRISObjectID == irisObjectID && relatedObjectTypeREF.Code == objectTypeCode)
                                       || (r.IRISObjectID == irisObjectID && objectTypeREF.Code == objectTypeCode)))
                                 select true
                                ).FirstOrDefault();

            return hasLinkedObject;
        }

        public List<LinkedObjectDTO> ListRelationshipLinksWithPaging(User user, RelationshipLinkCriteria criteria, PagingCriteria pagingCriteria)
        {
            var searchResults = Context.ListLinkedObjects(user.AccountName,
                                       criteria.IRISObjectID,
                                       criteria.IncludeExpiredLinks,
                                       criteria.SearchForObjectTypeIDsSummary == null ? null : string.Join(",", criteria.SearchForObjectTypeIDsSummary),
                                       criteria.IncludedObjectTypeCodeList == null ? null : string.Join(",", criteria.IncludedObjectTypeCodeList),
                                       criteria.ExcludedObjectTypeCodeList == null ? null : string.Join(",", criteria.ExcludedObjectTypeCodeList),
                                       criteria.IncludedRelationshipTypeCodeList == null ? null : string.Join(",", criteria.IncludedRelationshipTypeCodeList),
                                       criteria.ExcludedRelationshipTypeCodeList == null ? null : string.Join(",", criteria.ExcludedRelationshipTypeCodeList),
                                       pagingCriteria.SortColumn,
                                       pagingCriteria.SortDirection,
                                       pagingCriteria.PageSize,
                                       pagingCriteria.PageNumber
                                       );

            return searchResults.ToList();

        }

        public List<ActivityObjectRelationship> ListActivityObjectRelationsipByIDs(User user, List<long> activityObjectRelationshipIDs)
        {
            return Context.ListActivityObjectRelationships(user.AccountName, string.Join(",", activityObjectRelationshipIDs)).ToList();
        }

        public List<ThirdPartyInvolvementRow> GetThirdPartyInvolvementGridRows(long irisObjectID, int? startRowIndex, int? maximumRows)
        {
            string[] thirdPartyInvolvementReplationships = Context.AppSetting.Single(x => x.Key == AppSettingConstants.AppAuth_3rdPartyInvolvementRelationships).Value.Split('|');
            var linq = from activityRelationship in Context.ActivityObjectRelationship.Where(x => x.RelatedIRISObjectID == irisObjectID)
                       from thirdPartyInvolvement in Context.ThirdPartyInvolvement.Where(x => x.ActivityObjectRelationshipID == activityRelationship.ID).DefaultIfEmpty()
                       from responseREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.ResponseREFID).DefaultIfEmpty()
                       from appearingREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.WishToBeHeardREFID).DefaultIfEmpty()
                       from statusREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.StatusREFID).DefaultIfEmpty()
                       from activityRelationshipType in Context.ActivityObjectRelationshipType.Where(x => x.ID == activityRelationship.ActivityObjectRelationshipTypeID && thirdPartyInvolvementReplationships.Contains(x.Relationship))
                       from contactIO in Context.IRISObject.Where(x => x.ID == activityRelationship.IRISObjectID)
                       from contactSubLink in Context.ContactSubLink.Where(x => x.ActivityObjectRelationshipID == activityRelationship.ID)
                       from name in Context.Name.Where(x => x.ID == contactSubLink.NameID)
                       select new
                       {
                           thirdPartyInvolvement,
                           activityRelationship,
                           name,
                           name.Contact,
                           name.Contact.OrganisationStatusREF,
                           activityRelationshipType,
                           responseREF,
                           appearingREF,
                           statusREF,
                           contactIO
                       };

            var linq2 = from activityRelationship in Context.ActivityObjectRelationship.Where(x => x.IRISObjectID == irisObjectID)
                        from thirdPartyInvolvement in Context.ThirdPartyInvolvement.Where(x => x.ActivityObjectRelationshipID == activityRelationship.ID).DefaultIfEmpty()
                        from responseREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.ResponseREFID).DefaultIfEmpty()
                        from appearingREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.WishToBeHeardREFID).DefaultIfEmpty()
                        from statusREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.StatusREFID).DefaultIfEmpty()
                        from activityRelationshipType in Context.ActivityObjectRelationshipType.Where(x => x.ID == activityRelationship.ActivityObjectRelationshipTypeID && thirdPartyInvolvementReplationships.Contains(x.Relationship))
                        from contactIO in Context.IRISObject.Where(x => x.ID == activityRelationship.RelatedIRISObjectID)
                        from contactSubLink in Context.ContactSubLink.Where(x => x.ActivityObjectRelationshipID == activityRelationship.ID)
                        from name in Context.Name.Where(x => x.ID == contactSubLink.NameID)
                        select new
                        {
                            thirdPartyInvolvement,
                            activityRelationship,
                            name,
                            name.Contact,
                            name.Contact.OrganisationStatusREF,
                            activityRelationshipType,
                            responseREF,
                            appearingREF,
                            statusREF,
                            contactIO
                        };

            var result = linq.Concat(linq2).AsEnumerable().Distinct();

            return result.Select(x => new ThirdPartyInvolvementRow
                        {
                            RelationshipID = x.activityRelationship.ID,
                            ContactIRISObjectID = x.contactIO.ID,
                            ContactID = x.contactIO.LinkID.Value,
                            ContactDetail = x.name.FormattedNameMultiInstance,
                            LinkedAs = x.activityRelationshipType.Eval(y => y.Relationship),
                            AdvisedDate = x.thirdPartyInvolvement.Eval(y => y.DateAdvised),
                            ReceivedDate = x.thirdPartyInvolvement.Eval(y => y.DateReceived),
                            FinalisedDate = x.thirdPartyInvolvement.Eval(y => y.DateFinalised),
                            Response = x.responseREF.Eval(y => y.DisplayValue),
                            Appearing = x.appearingREF.Eval(y => y.DisplayValue),
                            Status = x.statusREF.Eval(y => y.DisplayValue),
                        }).OrderBy(x => x.LinkedAs)
                        .ThenBy(x => x.ContactDetail)
                        .Skip(startRowIndex.GetValueOrDefault())
                        .Take(maximumRows.GetValueOrDefault())
                        .ToList();
 
        }

        public int GetThirdPartyInvolvementGridRowsCount(long irisObjectID)
        {
            string[] thirdPartyInvolvementReplationships = Context.AppSetting.Single(x => x.Key == AppSettingConstants.AppAuth_3rdPartyInvolvementRelationships).Value.Split('|');
            var linq = from activityRelationship in Context.ActivityObjectRelationship.Where(x => x.RelatedIRISObjectID == irisObjectID)
                       from thirdPartyInvolvement in Context.ThirdPartyInvolvement.Where(x => x.ActivityObjectRelationshipID == activityRelationship.ID).DefaultIfEmpty()
                       from responseREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.ResponseREFID).DefaultIfEmpty()
                       from appearingREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.WishToBeHeardREFID).DefaultIfEmpty()
                       from statusREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.StatusREFID).DefaultIfEmpty()
                       from activityRelationshipType in Context.ActivityObjectRelationshipType.Where(x => x.ID == activityRelationship.ActivityObjectRelationshipTypeID && thirdPartyInvolvementReplationships.Contains(x.Relationship))
                       from contactIO in Context.IRISObject.Where(x => x.ID == activityRelationship.IRISObjectID)
                       from contactSubLink in Context.ContactSubLink.Where(x => x.ActivityObjectRelationshipID == activityRelationship.ID)
                       from name in Context.Name.Where(x => x.ID == contactSubLink.NameID)
                       select new
                       {
                           thirdPartyInvolvement,
                           activityRelationship,
                           name,
                           name.Contact,
                           name.Contact.OrganisationStatusREF,
                           activityRelationshipType,
                           responseREF,
                           appearingREF,
                           statusREF,
                           contactIO
                       };

            var linq2 = from activityRelationship in Context.ActivityObjectRelationship.Where(x => x.IRISObjectID == irisObjectID)
                        from thirdPartyInvolvement in Context.ThirdPartyInvolvement.Where(x => x.ActivityObjectRelationshipID == activityRelationship.ID).DefaultIfEmpty()
                        from responseREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.ResponseREFID).DefaultIfEmpty()
                        from appearingREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.WishToBeHeardREFID).DefaultIfEmpty()
                        from statusREF in Context.ReferenceDataValue.Where(x => x.ID == thirdPartyInvolvement.StatusREFID).DefaultIfEmpty()
                        from activityRelationshipType in Context.ActivityObjectRelationshipType.Where(x => x.ID == activityRelationship.ActivityObjectRelationshipTypeID && thirdPartyInvolvementReplationships.Contains(x.Relationship))
                        from contactIO in Context.IRISObject.Where(x => x.ID == activityRelationship.RelatedIRISObjectID)
                        from contactSubLink in Context.ContactSubLink.Where(x => x.ActivityObjectRelationshipID == activityRelationship.ID)
                        from name in Context.Name.Where(x => x.ID == contactSubLink.NameID)
                        select new
                        {
                            thirdPartyInvolvement,
                            activityRelationship,
                            name,
                            name.Contact,
                            name.Contact.OrganisationStatusREF,
                            activityRelationshipType,
                            responseREF,
                            appearingREF,
                            statusREF,
                            contactIO
                        };

            return linq.Concat(linq2).AsEnumerable().Distinct().Count();
        }
            public ActivityObjectRelationship GetRelationshipWIthThirdPartyInvolvement(long relationshipID, long applicationIRISObjectID)
        {
            return Context.ActivityObjectRelationship
                    .Include(x => x.ThirdPartyInvolvement)
                    .Include(x => x.ContactSubLink.Name)
                    .Include(x => x.ContactSubLink.Name.Contact)
                    .Include(x => x.ContactSubLink.Name.Contact.OrganisationStatusREF)
                    .Include(x => x.ActivityObjectRelationshipType)
                    .SingleOrDefault(x => x.ID == relationshipID);
        }

        public List<ActivityObjectRelationship> GetProgrammeRegimeSubjects(long IRISObjectID, string[] objectTypeCodeList)
        {
            List<string> includedRelationships = new List<string> { ObjectRelationshipTypesCodes.ProgrammeRegime, ObjectRelationshipTypesCodes.LinkedProgrammeRegime };
            var programmeQuery = (from activityRelationship in Context.ActivityObjectRelationship
                                 join linkType in Context.ActivityObjectRelationshipType on activityRelationship.ActivityObjectRelationshipTypeID equals linkType.ID
                                 where activityRelationship.IRISObjectID == IRISObjectID &&
                                 includedRelationships.Contains(linkType.Code)
                                 select activityRelationship.RelatedIRISObjectID)
                                 .Union
                                 (from activityRelationship in Context.ActivityObjectRelationship
                                  join linkType in Context.ActivityObjectRelationshipType on activityRelationship.ActivityObjectRelationshipTypeID equals linkType.ID
                                  where activityRelationship.RelatedIRISObjectID == IRISObjectID &&
                                  includedRelationships.Contains(linkType.Code)
                                  select activityRelationship.IRISObjectID);
            var programmeSubjectQuery = from activityRelationship in Context.ActivityObjectRelationship
                                            .Include("ActivityObjectRelationshipType")
                                            .Include("IRISObject")
                                            .Include("IRISObject.ObjectTypeREF")
                                            .Include("IRISObject.SecurityContextIRISObject.ObjectTypeREF")
                                            .Include("RelatedIRISObject")
                                            .Include("RelatedIRISObject.Statuses.StatusREF")
                                            .Include("RelatedIRISObject.ObjectTypeREF")
                                            .Include("RelatedIRISObject.SecurityContextIRISObject.ObjectTypeREF")
                                            .Include("ContactSubLink")
                                            // If inlcude is used and then a join is used then Eager loading of entities does not work
                                            //join linkType in Context.ActivityObjectRelationshipType on activityRelationship.ActivityObjectRelationshipTypeID equals linkType.ID
                                            //where linkType.Code == ObjectRelationshipTypesCodes.ProgrammeSubject &&
                                        where activityRelationship.ActivityObjectRelationshipType.Code == ObjectRelationshipTypesCodes.ProgrammeSubject &&
                                        programmeQuery.Contains(activityRelationship.IRISObjectID) &&
                                        objectTypeCodeList.Contains (activityRelationship.RelatedIRISObject.ObjectTypeREF.Code )
                                        select activityRelationship;
            return programmeSubjectQuery
                .ToList();

        }
    }
}
