using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.Common;
using Datacom.IRIS.DomainModel.Domain.Constants;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
	public class ContactRepository : RepositoryStore, IContactRepository
	{

        public Contact GetContactByID(long id, bool isJointFinancialCustomer)
		{
            string contactObjectTypeCode = isJointFinancialCustomer?
                                            ReferenceDataValueCodes.ObjectType.JointFinancialCustomer :
                                            ReferenceDataValueCodes.ObjectType.Contact;

            IQueryable<Contact> query = ContactObjectQuery().Where(c => c.ID == id && c.IRISObject.ObjectTypeREF.Code == contactObjectTypeCode);

			Contact contact = GetContactFromQueryable(query);

            // CR305: show/hide OtherIdentifier grid in ContactOtherLink page.
            // we only run the following GetOtherIdentifiers query when necessary.
            if (!isJointFinancialCustomer)
            {
                CommonRepository commonRepository = new CommonRepository();
                bool doesShowOtherIdentifierGridForContact;
                bool.TryParse(commonRepository.GetAppSettingByKey(AppSettingConstants.Contact_ShowOtherIdentifier).Value, out doesShowOtherIdentifierGridForContact);
                if (doesShowOtherIdentifierGridForContact)
                {
                    contact.IRISObject.OtherIdentifiers.AddRange(
                        Context.OtherIdentifiers
                               .Include(oi => oi.IdentifierContextREF)
                               .Where(oi => oi.IRISObjectID == contact.IRISObjectID && !oi.IsDeleted)
                               .ToList()
                        );
                }
            }
            return contact;
		}

		public Contact GetContactByIRISObjectID(long irisObjectId)
		{
			IQueryable<Contact> query = ContactObjectQuery().Where(c => c.IRISObjectID == irisObjectId);

            Contact contact = GetContactFromQueryable(query);

            // include otheridentifiers for merge contact
            contact.IRISObject.OtherIdentifiers.AddRange(
                Context.OtherIdentifiers
                       .Where(i => i.IRISObjectID == contact.IRISObjectID && !i.IsDeleted)
                       .ToList()
                );

            // include service address/app&auth for merge contact
            contact.ServiceAddresses.AddRange(
                Context.ServiceAddresses
                       .Include(x => x.Applications)
                       .Include(x => x.Authorisations)
                       .Where(s => s.ContactID == contact.ID)
                       .ToList()
                );
            return contact;
		}

	    public List<ContactGroupMember> GetContactGroupMembersByContactID(long id)
	    {
	        return Context.ContactGroupMember.Where(m => m.ContactID == id).ToList();
	    }

	    public List<Contact> GetNotDuplicateContacts(long contactID)
	    {
	        var query = from contact in Context.Contact.Where(c => c.ID == contactID)
                        from link in Context.ActivityObjectRelationship.Where(l => !l.CurrentTo.HasValue && (l.IRISObjectID == contact.IRISObjectID || l.RelatedIRISObjectID == contact.IRISObjectID))
                        from linkType in Context.ActivityObjectRelationshipType.Where(t => t.Code == ObjectRelationshipTypesCodes.NotDuplicateContact && link.ActivityObjectRelationshipTypeID == t.ID)
                        from duplicateContact in Context.Contact.Where(d => d.IRISObjectID == link.IRISObjectID && link.RelatedIRISObjectID == contact.IRISObjectID || d.IRISObjectID == link.RelatedIRISObjectID && link.IRISObjectID == contact.IRISObjectID)
                        select new {duplicateContact};

	        return query.AsEnumerable().Select(x => x.duplicateContact).Distinct().ToList();
	    }

	    /// <summary>
        /// Return contact record or null if contact is not found. 
        /// Unlike GetContactById method, this method does not take isJointFinancialCustomer parameter so does not verify objectType Contact (Person/Organiastion) vs Joint Financial Customer
        /// It also won't return sublink information and call TrackAll()
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Contact GetContactByIDReadOnly(long id)
        {
            IQueryable<Contact> query = ContactObjectQuery().Where(c => c.ID == id);
            return GetContactFromQueryableReadOnly(query, new List<long>() { id }).SingleOrDefault();  
        }


        public List<Contact> GetChangedContacts(DateTime changedSince)
        {
            var changedContacts = Context.GetChangedContacts(changedSince).AsEnumerable().Select(x => x.ContactID).ToList();
            IQueryable<Contact> query = ContactObjectQuery().Where(c => changedContacts.Contains(c.ID));
            return GetContactFromQueryableReadOnly(query, changedContacts);
        }


        public List<Contact> GetChangedContacts(DateTime changedSince, bool contactCallUpdateContactFinCustomerOnly = false)
        {
            var changedContacts = Context.GetChangedContacts(changedSince).AsEnumerable().Select(x => x.ContactID).ToList();
            IQueryable<Contact> query = ContactObjectQuery(contactCallUpdateContactFinCustomerOnly).Where(c => changedContacts.Contains(c.ID));
            return GetContactFromQueryableReadOnly(query, changedContacts);
        }


        public List<Contact> ListJointFinancialCustomersAssociatedWithContacts(params long[] associatedContactIds)
        {

            return (from associatedContacts in Context.JFCAssociatedContacts
                    join personOrganisationContact in Context.Contact on associatedContacts.AssociatedContactID equals personOrganisationContact.ID
                    join jfc in Context.Contact on associatedContacts.JFCContactID equals jfc.ID
                    where associatedContactIds.Contains(personOrganisationContact.ID)
                          && !associatedContacts.IsDeleted
                    select new
                    {
                        jfc,
                        jfc.IRISObject,
                        jfc.IRISObject.ObjectTypeREF,
                        jfc.IRISObject.SubClass1REF,
                        jfc.Names,
                        jfc.ContactAddresses,
                        jfc.AssociatedContactsForJFC
                    }
                    ).AsEnumerable()
                    .Select(x => x.jfc).Distinct().ToList().TrackAll();
        }




        private ObjectQuery<Contact> ContactObjectQuery(bool contactCallUpdateContactFinCustomerOnly = false)
        {
            List<long> nameContactIds = null;
            List<long> emailContactIds = null;
            List<long> billingAddressContactIds = null;
            ObjectQuery<Contact> returnResult = null;

            ObjectQuery<Contact> query = null;

            if (contactCallUpdateContactFinCustomerOnly)
            {
                //If you have time, please find a way to minimize thess database hits. Dont hate me. I didnt time at that situation.
                nameContactIds = Context.Name.Where(x => x.IsBilling).Select(c=>c.ContactID).ToList();
                emailContactIds = Context.Email.Where(x => x.IsBilling).Select(c => c.ContactID).ToList();
                billingAddressContactIds = Context.ContactAddress.Where(x => x.IsBilling).Select(c => c.ContactID).ToList();

                query = (ObjectQuery<Contact>) Context.Contact
                    .Include(c => c.IRISObject)
                    .Include(c => c.IRISObject.ObjectTypeREF)
                    .Include(c => c.IRISObject.SubClass1REF)
                    .Include(c => c.IRISObject.SubClass2REF)
                    .Include(c => c.IRISObject.SubClass3REF)
                    .Include(c => c.OrganisationStatusREF)
                    .Include(c => c.OrganisationTypeREF)
                    .Include(c => c.PersonGenderREF)
                    .Where(c => nameContactIds.Contains(c.ID) && (emailContactIds.Contains(c.ID) || billingAddressContactIds.Contains(c.ID)));
              return query;
            }

            returnResult = Context.Contact
                .Include(c => c.IRISObject)
                .Include(c => c.IRISObject.ObjectTypeREF)
                .Include(c => c.IRISObject.SubClass1REF)
                .Include(c => c.IRISObject.SubClass2REF)
                .Include(c => c.IRISObject.SubClass3REF)
                .Include(c => c.OrganisationStatusREF)
                .Include(c => c.OrganisationTypeREF)
                .Include(c => c.PersonGenderREF);
            return returnResult;
        }




        private Contact GetContactFromQueryable(IQueryable<Contact> contactQueryable)
		{
            Contact contact = contactQueryable.Single();

            if (contact == null)
                return null;

			/*
			 * StartTracking instructs the change tracker on the entity to start recording changes applied
			 * to this Contact entity. All sub-entities that belong to Contact will also have its tracking 
			 * turned on. This is important for us to do so that the object we return back to any system
			 * is ready to be modified as it is always tracked.
			 */
		    contact.TrackAll();

            bool isJointFinancialCustomer = contact.IRISObject.ObjectTypeREF.Code == ReferenceDataValueCodes.ObjectType.JointFinancialCustomer;


			contact.Names.AddRange(
				Context.Name
					.Include(n => n.ContactSubLinks)
					.Include(n => n.NameTypeREF)
					.Include(n => n.PersonTitleREF)
					.Where(n => n.ContactID == contact.ID && !n.IsDeleted).ToList()
			);

			contact.Emails.AddRange(
				Context.Email
					.Include(n => n.ContactSubLinks)
                    .Include(x => x.ServiceAddresses)
                    .Include("ServiceAddresses.Applications")
                    .Include("ServiceAddresses.Authorisations")
					.Include(n => n.TypeREF)
					.Where(n => n.ContactID == contact.ID ).ToList()
            );

            contact.OnlineServicesAccounts.AddRange(
                Context.OnlineServicesAccount
                    .Where(n => n.ContactID == contact.ID).ToList()
            );

			// Contact Addresses
			// We do this collection first to load up the Contact Sublinks

			contact.ContactAddresses.AddRange(
				Context.ContactAddress
					.Include(a => a.ContactAddressTypeREF)
					.Include(a => a.ContactSubLinks)
					.Include(a => a.ServiceAddresses)
                    .Include("ServiceAddresses.Applications")
                    .Include("ServiceAddresses.Authorisations")
					.Where(a => a.ContactID == contact.ID ).ToList()
				);


			// Delivery Addresses.
			// Note that we load the Addresses into a lookup so that we can manually associate them with the correct ContactAddress

			var deliveryAddresses =
				(
					(
						from contactAddress in Context.ContactAddress.Where(contactAddress => contactAddress.ContactID == contact.ID )
						join deliveryAddress in Context.Address.OfType<DeliveryAddress>() on contactAddress.AddressID equals deliveryAddress.ID
						select deliveryAddress
					) as ObjectQuery<DeliveryAddress>
				)
				.IncludeTypeSpecificReferenceData()
				.ToLookup(a => a.ID);

			// Now add them to ContactAddresses

			foreach (var contactAddress in contact.ContactAddresses)
			{
				if (contactAddress.Address == null)
				{
					contactAddress.Address = deliveryAddresses[contactAddress.AddressID] as Datacom.IRIS.DomainModel.Domain.Address;
				}
			}

			// UrbanRural Addresses
			// Note that we load the Addresses into a lookup so that we can manually associate them with the correct ContactAddress

			var urbanRuralAddresses =
				(
					(
						from contactAddress in Context.ContactAddress.Where(contactAddress => contactAddress.ContactID == contact.ID )
						join urbanRuralAddress in Context.Address.OfType<UrbanRuralAddress>() on contactAddress.AddressID equals urbanRuralAddress.ID
						select urbanRuralAddress
					) as ObjectQuery<UrbanRuralAddress>
				)
				.IncludeTypeSpecificReferenceData()
				.ToLookup(a => a.ID);

			// Now add them to ContactAddresses

			foreach (var contactAddress in contact.ContactAddresses)
			{
				if (contactAddress.Address == null)
				{
					contactAddress.Address = urbanRuralAddresses[contactAddress.AddressID] as Address;
				}
			}

			// Overseas Addresses 
			// Note that we load the Addresses into a lookup so that we can manually associate them with the correct ContactAddress

			var overseasAddresses =
				(
					(
						from contactAddress in Context.ContactAddress.Where(contactAddress => contactAddress.ContactID == contact.ID )
						join overseasAddress in Context.Address.OfType<OverseasAddress>() on contactAddress.AddressID equals overseasAddress.ID
						select overseasAddress
					) as ObjectQuery<OverseasAddress>
				)
				.IncludeTypeSpecificReferenceData()
				.ToLookup(a => a.ID);

			// Now add them to ContactAddresses

			foreach (var contactAddress in contact.ContactAddresses)
			{
				if (contactAddress.Address == null)
				{
					contactAddress.Address = overseasAddresses[contactAddress.AddressID] as Address;
				}
			}

            // Joint financial customer does not have Phone number / web sites
            if (!isJointFinancialCustomer)
            {
                contact.PhoneNumbers.AddRange(
                    Context.PhoneNumber
                        .Include(n => n.ContactSubLinks)
                        .Include(n => n.TypeREF)
                        .Where(n => n.ContactID == contact.ID && !n.IsDeleted).ToList()
                );

                contact.Websites.AddRange(
                    Context.Website
                        .Include(n => n.ContactSubLinks)
                        .Include(n => n.TypeREF)
                        .Where(n => n.ContactID == contact.ID && !n.IsDeleted).ToList()
                );

            }
            else
            {
                //Add returning associated contacts
                contact.AssociatedContactsForJFC.AddRange(
                        Context.JFCAssociatedContacts
                            .Include(jfc => jfc.AssociatedContact)
                            .Include(jfc => jfc.AssociatedContact.IRISObject)
                            .Include(jfc => jfc.AssociatedContact.IRISObject.ObjectTypeREF)
                            .Include(jfc => jfc.AssociatedContact.IRISObject.SubClass1REF)
                            .Include(jfc => jfc.AssociatedContact.Names)
                            .Include("AssociatedContact.Names.PersonTitleREF")
                            .Where(jfc => jfc.JFCContactID == contact.ID && !jfc.IsDeleted ).ToList()
                );
            }

            contact.ContactVerifications.AddRange(
                    Context.ContactVerifications.Where(x => x.ContactID == contact.ID && !x.IsDeleted).ToList()
                );
			return contact;
		}

        /// <summary>
        /// This version of contact query does not include sublink information and does not include TrackAll
        /// </summary>
        /// <param name="contactQueryable"></param>
        /// <param name="contactIDs"></param>
        /// <returns></returns>
        private List<Contact> GetContactFromQueryableReadOnly(IQueryable<Contact> contactQueryable, List<long> contactIDs)
        {
           
            Context.Name
                     .Include(n => n.NameTypeREF)
                     .Include(n => n.PersonTitleREF)
                     .Where(n => contactIDs.Contains(n.ContactID) && !n.IsDeleted)
                     .ToList();

            Context.PhoneNumber
                   .Include(n => n.TypeREF)
                   .Where(n => contactIDs.Contains(n.ContactID) && !n.IsDeleted)
                   .ToList();

            Context.Email
                .Include(n => n.TypeREF)
                .Where(n => contactIDs.Contains(n.ContactID))
                .ToList();

            Context.Website
                 .Include(n => n.TypeREF)
                 .Where(n => contactIDs.Contains(n.ContactID) && !n.IsDeleted)
                 .ToList();

            //// Contact Addresses
            var contactAddr = Context.ContactAddress
                            .Include(a => a.ContactAddressTypeREF)
                            .Where(a => contactIDs.Contains(a.ContactID) )
                            .ToList();
            var contactAddrIDs = contactAddr.Select(x => x.AddressID).ToList();

            //// Delivery Addresses.
            Context.Address.OfType<DeliveryAddress>()
                    .IncludeTypeSpecificReferenceData()
                    .Where(a => contactAddrIDs.Contains(a.ID))
                    .ToList();
            //// UrbanRuralAddress Addresses.
            Context.Address.OfType<UrbanRuralAddress>()
                  .IncludeTypeSpecificReferenceData()
                  .Where(a => contactAddrIDs.Contains(a.ID))
                  .ToList();
            //// OverseasAddress Addresses.
            Context.Address.OfType<OverseasAddress>()
                 .IncludeTypeSpecificReferenceData()
                 .Where(a => contactAddrIDs.Contains(a.ID))
                 .ToList();

            return contactQueryable.ToList();
        }

        public List<ContactSubLink> GetContactSubLinksByRelationshipIDs(List<long> relationshipIDs, long irisObjectID)
        {
            return Context.ContactSubLink
                .Include(c => c.Contact)
                .Include(c => c.Contact.IRISObject)
                .Include(c => c.Contact.IRISObject.SubClass1REF)
                .Include(c => c.Contact.Names)
                .Include(c => c.Contact.OrganisationStatusREF)
                .Include(c => c.Contact.Emails)
                .Include(c => c.Contact.OnlineServicesAccounts)
                .Include("Contact.Names.PersonTitleREF")
                .Include(c => c.Contact.ContactAddresses)
                .Include(c => c.ActivityObjectRelationship)
                .Include(c => c.ActivityObjectRelationship.ActivityObjectRelationshipType)
                .Where(x => relationshipIDs.Contains(x.ActivityObjectRelationshipID) && 
                            (x.ActivityObjectRelationship.IRISObjectID == irisObjectID || x.ActivityObjectRelationship.RelatedIRISObjectID == irisObjectID)
                      )
                .ToList();
        }

		/// <summary>
		///    This method is used to support the linking grids. Given a list of iris object
		///    Id's (what is being rendered in a grid), this method will return back a 
		///    dictionary mapping between ID and warning comments to be used for rendering
		///    at the front end.
		/// </summary>
		/// <param name="irisObjectIdList"></param>
		/// <returns></returns>
		public Dictionary<long, string> GetContactWarnings(List<long> irisObjectIdList)
		{
			return Context.Contact
						.Where(c => !string.IsNullOrEmpty(c.WarningComments))
						.Where(c => irisObjectIdList.Contains(c.IRISObjectID))
						.Select(c => new { c.IRISObjectID, c.WarningComments })
						.ToDictionary(c => c.IRISObjectID, c => c.WarningComments);
		}


	    public ContactGroup GetContactGroupByIRISObjectID(long irisObjectID)
	    {
            ContactGroup contactGroup = Context.ContactGroup
                                            .Include(cg => cg.IRISObject)
                                            .Include(cg => cg.IRISObject.ObjectTypeREF)
                                            .Include(cg => cg.StatusREF)
                                            .Include(cg => cg.TypeREF)
                                            .Include(cg => cg.Owner)
                                            .Single(cg => cg.IRISObjectID == irisObjectID)
                                            .TrackAll();

            contactGroup.ContactGroupQuestionDefinitions.AddTrackableCollection(GetContactGroupQuestionDefinitions(contactGroup.ID));
            return contactGroup;
	    }

	    public bool ContactGroupHasMember(long contactGroupID)
	    {
	        return Context.ContactGroupMember.Any(m => m.ContactGroupID == contactGroupID);
	    }

	    public bool IsContactGroupNameUnique(ContactGroup contactGroup)
		{
			return !Context.ContactGroup.Any(cg => contactGroup.Name == cg.Name && contactGroup.ID != cg.ID);
		}

        public bool IsContactOnlineServiceAccountUserIDUnique(OnlineServicesAccount onlineServiceAccount)
        {
            ObjectSet<OnlineServicesAccount> a = Context.OnlineServicesAccount;
            OnlineServicesAccount account = a.FirstOrDefault(acc => acc.Value.ToLower() == onlineServiceAccount.Value.ToLower() && acc.IsActive && acc.ID != onlineServiceAccount.ID);
            return account == null;
        }

		public ContactGroup GetContactGroupByIDWithQuestionsAndAnswers(long contactGroupMemberID)
		{
			var contactGroupMember = Context.ContactGroupMember
			                            .Include(cgm => cgm.ContactGroup)
			                            .Include(cgm => cgm.ContactGroup.IRISObject)
                                        .Include(cgm => cgm.ContactGroup.IRISObject.ObjectTypeREF)
                                        .Include(cgm => cgm.ContactGroup.StatusREF)
                                        .Include(cgm => cgm.ContactGroup.TypeREF)
			                            .Include(cgm => cgm.ContactGroup.Owner)
                                        .Single(cgm => cgm.ID == contactGroupMemberID);

		    ContactGroup contactGroup = contactGroupMember.ContactGroup;
		    long memberIrisObjectID = contactGroupMember.IRISObjectID;

            contactGroup.ContactGroupQuestionDefinitions.AddTrackableCollection(GetContactGroupQuestionDefinitionsWithAnswers(contactGroup.ID, memberIrisObjectID));
            return contactGroup.TrackAll();
		}

		public ContactGroup GetContactGroupByID(long id)
		{
			ContactGroup contactGroup = Context.ContactGroup
											.Include(cg => cg.IRISObject)
                                            .Include(cg => cg.IRISObject.ObjectTypeREF)
                                            .Include(cg => cg.StatusREF)
                                            .Include(cg => cg.TypeREF)
											.Include(cg => cg.Owner)
											.Single(cg => cg.ID == id)
											.TrackAll();

			contactGroup.ContactGroupQuestionDefinitions.AddTrackableCollection(GetContactGroupQuestionDefinitions(id));
			return contactGroup;
		}

        public List<ContactGroupQuestionDefinition> GetContactGroupQuestionDefinitionsByContactGroupIRISObjectID(long irisObjectID)
        {
            var dbquery = from contactGroup in Context.ContactGroup.Where(cg => cg.IRISObjectID == irisObjectID)
                          from contactGroupQD in Context.ContactGroupQuestionDefinition.Where(cgQD => cgQD.ContactGroupID == contactGroup.ID)
                          from questionDef in Context.QuestionDefinition.Where(q => q.ID == contactGroupQD.QuestionDefinitionID && !q.IsDeleted)
                          orderby questionDef.OrderNumber
                          select new { contactGroupQD, questionDef };

            return dbquery.AsEnumerable().Select(p => p.contactGroupQD).Distinct().ToList();
        }

		private List<ContactGroupQuestionDefinition> GetContactGroupQuestionDefinitions(long contactGroupID)
		{
			var dbquery = from contactGroupQD in Context.ContactGroupQuestionDefinition
							.Where(cgQD => cgQD.ContactGroupID == contactGroupID)
						  from questionDef in Context.QuestionDefinition
							.Where(q => q.ID == contactGroupQD.QuestionDefinitionID && !q.IsDeleted)
                          orderby questionDef.OrderNumber
						  select new { contactGroupQD, questionDef };

			return dbquery.AsEnumerable().Select(p => p.contactGroupQD).Distinct().ToList();
		}

		private List<ContactGroupQuestionDefinition> GetContactGroupQuestionDefinitionsWithAnswers(long contactGroupID, long contactGroupMemberIrisObjectID)
		{
			var dbquery = from contactGroupQD in Context.ContactGroupQuestionDefinition
							.Where(cgQD => cgQD.ContactGroupID == contactGroupID)
						  from questionDef in Context.QuestionDefinition
							.Where(q => q.ID == contactGroupQD.QuestionDefinitionID && !q.IsDeleted)
						  from answer in Context.Answer
							.Where(a => a.QuestionID == questionDef.ID && !a.IsDeleted)
							.Where(a => a.IRISObjectID == contactGroupMemberIrisObjectID)
							.DefaultIfEmpty()
                          orderby questionDef.OrderNumber, answer.Row
						  select new { contactGroupQD, questionDef, answer };

			return dbquery.AsEnumerable().Select(p => p.contactGroupQD).Distinct().ToList();
		}

		public List<ContactGroupMember> GetContactGroupMembersByActivityObjectRelationshipIDs(List<long> activityObjectRelationshipIDs)
		{
			return Context.ContactGroupMember
                          .Include(cgm => cgm.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                          .Include("ContactGroup.ContactGroupQuestionDefinitions.QuestionDefinition")
						  .Where(cgm => activityObjectRelationshipIDs.Contains(cgm.ActivityObjectRelationshipID))
						  .ToList();
		}

        //Get a mapping of ActivityObjectRelationshipID with ContactGroupMemberID
        public Dictionary<long, long> GetContactGroupMemberIDsByActivityObjectRelationshipIDs(List<long> activityObjectRelationshipIDs)
        {
            return (from m in Context.ContactGroupMember
                    where activityObjectRelationshipIDs.Contains(m.ActivityObjectRelationshipID)
                    select new
                    {
                        m.ID,
                        m.ActivityObjectRelationshipID
                    }).ToDictionary(m => m.ActivityObjectRelationshipID, m => m.ID);
        }
        
		public ContactGroupMember GetContactGroupMemberById(long id)
		{
			return Context.ContactGroupMember
						  .Include(cgm => cgm.Contact.IRISObject)
                          .Include(cgm => cgm.Contact.IRISObject.SubClass1REF)
                          .Include(cgm => cgm.Contact.OrganisationStatusREF)
                          .Include(cgm => cgm.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
						  .Include(cgm => cgm.ActivityObjectRelationship.ContactSubLink.Name)
                          .Include(cgm => cgm.ActivityObjectRelationship.ContactSubLink.Name.PersonTitleREF)
						  .Single(cgm => cgm.ID == id)
						  .TrackAll();
		}
		
        public long GetContactIDByIRISObjectID(long irisObjectID)
        {
            return Context.IRISObject.SingleOrDefault(x => x.ID == irisObjectID).LinkID.Value;
        }

        public ContactBulkLoadItem GetContactBulkLoadItem(Guid contactBulkLoadItemGuid)
        {
            return Context.ContactBulkLoadItems.SingleOrDefault(x => x.ItemGUID == contactBulkLoadItemGuid).TrackAll();
        }

        public List<ContactBulkLoadItem> GetFailedContactBulkLoadItems(Guid bulkLoadBatchGuid)
        {
            return Context.ContactBulkLoadItems.Where(x => x.BatchGUID == bulkLoadBatchGuid && x.IsProcessed && !x.IsSuccess).ToList();
        }

        public int GetIncompletedBulkLoadItemCount(Guid bulkLoadBatchGuid)
        {
            return Context.ContactBulkLoadItems.Count(x => x.BatchGUID == bulkLoadBatchGuid && !x.IsProcessed);
        }

        public void DeleteContactBulkLoadItems(Guid bulkLoadBatchGuid)
        {
            Context.ContactBulkLoadItemDelete(bulkLoadBatchGuid);
        }

        public Contact GetContactBySphereUserID(string sphereUserID)
        {
            var onlineServiceAcc = Context.OnlineServicesAccount.SingleOrDefault(x => x.IsActive && x.Value == sphereUserID);
            if (onlineServiceAcc == null)
            {
                return null;
            }

            IQueryable<Contact> query = ContactObjectQuery().Where(c => c.ID == onlineServiceAcc.ContactID);

           return GetContactFromQueryable(query);
	    }
	}
}
