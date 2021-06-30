using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Datacom.IRIS.Common;
using Datacom.IRIS.Common.Utils;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.DTO;
using Datacom.IRIS.DomainModel.DTO.Enforcement;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class EnforcementRepository : RepositoryStore, IEnforcementRepository
    {
        public Enforcement GetEnforcementByID(long enforcementId)
        {
            var enforcementQuery = from enf in Context.Enforcements
                               .Include(a => a.OriginatedFromREF)
                               .Include(a => a.IRISObject)
                               .Include(a => a.IRISObject.ObjectTypeREF)
                               .Include(a => a.IRISObject.SubClass1REF)
                               .Include(a => a.IRISObject.OtherIdentifiers)
                               .Include("IRISObject.OtherIdentifiers.IdentifierContextREF")
                               .Include("IRISObject.ActivityObjectRelationships.ActivityObjectRelationshipType")
                               .Where(enf => enf.ID == enforcementId)
                                   select enf;

            return enforcementQuery.Single().TrackAll();
        }

        public bool HasOutstandingInspectionForEnforcement(long enforcementId)
        {
            return Context.EnforcementActions.Any(x =>
                x.EnforcementID == enforcementId && x.IRISObject.MobileInspectionStatus ==
                (long) MobileInspectionStatus.SyncedToSphere);
        }

        public bool HasSyncedMobileEnforcementAction(long enforcementId)
        {
            return Context.EnforcementActions.Any(x =>
                x.EnforcementID == enforcementId && x.IRISObject.MobileInspectionStatus ==
                (long)MobileInspectionStatus.SyncedBackToIRIS);
        }

        public Enforcement GetEnforcementByIDForDetailsPage(long enforcementId)
        {
            var enforcement = GetEnforcementByID(enforcementId);

            // Get the Created By Display Name for the user
            var users = RepositoryMap.SecurityRepository.GetUserList();
            var user = users.FirstOrDefault(u => u.AccountName.ToLower() == enforcement.CreatedBy.ToLower());
            enforcement.CreatedByDisplayName = user != null ? user.DisplayName : enforcement.CreatedBy;

            enforcement.EnforcementAllegedOffences.AddTrackableCollection(
                GetEnforcementAllegedOffenceListById(enforcement.ID));
            return enforcement.TrackAll();
        }

        public Enforcement GetEnforcementByIRISObjectID(long irisObjectId)
        {
            var enforcementQuery = from enf in Context.Enforcements
                                       .Include(a => a.IRISObject)
                                       .Include(a => a.IRISObject.ObjectTypeREF)
                                       .Where(enf => enf.IRISObjectID == irisObjectId)
                                   select enf;

            return enforcementQuery.Single();
        }

        /// <summary>
        ///    This method returns 'light' Enforcement objects that are fetched based on IRIS Object IDS.
        ///    This method is invoked by the GIS map tips functionality and simply wants simple
        ///    information about an Enforcement such as IRIS ID, and Brief Description
        /// </summary>
        public List<Enforcement> GetEnforcementsByIRISObjectIDs(List<long> irisObjectIDsList)
        {
            var dbquery = Context.Enforcements
                .Include(a => a.IRISObject)
                .Include(a => a.IRISObject.SecurityContextIRISObject)
                .Include(a => a.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                .Where(enf => irisObjectIDsList.Contains(enf.IRISObjectID));

            return dbquery.AsEnumerable().ToList();
        }

        /// <summary>
        ///    This method returns 'light' Enforcement Alleged Offence objects that are fetched based on IRIS Object IDS.
        ///    This method is invoked by the GIS map tips functionality and simply wants simple
        ///    information about an Enforcement Alleged Offence such as Date, offence section and nature of offence
        /// </summary>
        public List<EnforcementAllegedOffence> GetEnforcementAllegedOffencesByIRISObjectIDs(List<long> irisObjectIDsList)
        {
            var dbquery = Context.EnforcementAllegedOffences
                .Include(a => a.IRISObject)
                .Include(a => a.IRISObject.SecurityContextIRISObject)
                .Include(a => a.IRISObject.SecurityContextIRISObject.ObjectTypeREF).Include(x => x.OffenceSectionREF)
                .Include(x => x.NatureOfOffenceREF)
                .Where(a => irisObjectIDsList.Contains(a.IRISObjectID) && !a.IsDeleted);

            return dbquery.AsEnumerable().Distinct().ToList();
        }

        /// <summary>
        ///  Gets the full list of Alleged Offences for an Enforcement 
        /// </summary>
        private List<EnforcementAllegedOffence> GetEnforcementAllegedOffenceListById(long enforcementId)
        {
            var dbOffence =
                from enforcementAllegedOffence in
                    Context.EnforcementAllegedOffences.Where(e => e.EnforcementID == enforcementId && !e.IsDeleted)
                from offenceDateType in
                    Context.ReferenceDataValue.Where(r => r.ID == enforcementAllegedOffence.OffenceDateTypeREFID)
                from offenceAct in
                    Context.ReferenceDataValue.Where(r => r.ID == enforcementAllegedOffence.IRISObject.SubClass1ID)
                from offenceSection in
                    Context.ReferenceDataValue.Where(r => r.ID == enforcementAllegedOffence.OffenceSectionREFID)
                from natureOfOffence in
                    Context.ReferenceDataValue.Where(r => r.ID == enforcementAllegedOffence.NatureOfOffenceREFID)
                from irisObject in Context.IRISObject.Where(i => i.ID == enforcementAllegedOffence.IRISObjectID)

                select
                    new
                    {
                        enforcementAllegedOffence,
                        offenceDateType,
                        offenceAct,
                        offenceSection,
                        natureOfOffence,
                        irisObject
                    };

            return dbOffence.AsEnumerable().Distinct().Select(x => x.enforcementAllegedOffence).ToList();
        }

        /// <summary>
        ///    This method returns 'light' Enforcement Alleged Offence objects that are fetched based on IRIS Object IDS.
        ///    This method is invoked by the GIS map tips functionality and simply wants simple
        ///    information about an Enforcement Alleged Offence such as Date, offence section and nature of offence
        /// </summary>
        public List<long> GetCountOfAllegedOffenceAssociated(long enforcementIrisObjectID, long allegedOffenceID)
        {
            var idList = (from enforcement in Context.Enforcements.Where(x => x.IRISObjectID == enforcementIrisObjectID)
                          join offence in Context.EnforcementAllegedOffences.Where(x => !x.IsDeleted) on
                              enforcement.ID equals offence.EnforcementID
                          join actionOffence in Context.EnforcementActionAllegedOffences.Where(x => !x.IsDeleted) on offence.ID
                              equals actionOffence.EnforcementAllegedOffenceID
                          where offence.ID == allegedOffenceID && !actionOffence.IsDeleted
                          select new { actionOffence }).AsEnumerable().Distinct().Select(x => x.actionOffence.ID).ToList();

            idList.AddRange((from enforcement in Context.Enforcements.Where(x => x.IRISObjectID == enforcementIrisObjectID)
                             join offence in Context.EnforcementAllegedOffences.Where(x => !x.IsDeleted) on
                                 enforcement.ID equals offence.EnforcementID
                             join charge in Context.EnforcementActionProsecutionCharges.Where(x => !x.IsDeleted) on offence.ID
                                 equals charge.EnforcementAllegedOffenceID
                             where offence.ID == allegedOffenceID && !charge.IsDeleted
                             select new { charge }).AsEnumerable().Distinct().Select(x => x.charge.ID).ToList());

            return idList;
        }

        public List<MOJExportReportLineDTO> GetMOJExportReportByInfringementIDs(List<long> infringementIds)
        {
            var result =
                //
                // Query
                //
                from infringement in Context.EnforcementActions.OfType<EnforcementActionInfringementNotice>().Where(x => infringementIds.Contains(x.ID))
                join actionOffence in Context.EnforcementActionAllegedOffences.Where(x => !x.IsDeleted) on infringement.ID
                    equals actionOffence.EnforcementActionID
                join offence in Context.EnforcementAllegedOffences.Where(x => !x.IsDeleted) on
                    actionOffence.EnforcementAllegedOffenceID equals offence.ID
                join actionRecipient in Context.EnforcementActionRecipientOrRespondents.Where(x => !x.IsDeleted) on
                    infringement.ID equals actionRecipient.EnforcementActionId
                join ra in Context.ContactAddress on
                    infringement.ReminderNoticeServedAddressID equals ra.ID into
                        reminderContactAddressLeftJoin
                from reminderContactAddress in reminderContactAddressLeftJoin.DefaultIfEmpty()
                join raddr in Context.Address on
                    reminderContactAddress.AddressID equals raddr.ID into
                        reminderAddressLeftJoin
                from reminderAddress in reminderAddressLeftJoin.DefaultIfEmpty()

                    //
                    // Paths --> let ...
                    //
                    // Main object paths...
                    //
                let contact = actionRecipient.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.Contact
                let name = actionRecipient.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.Name
                let contactAddress = actionRecipient.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.ContactAddress
                let address = actionRecipient.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.ContactAddress.Address

                //
                //Contact Info paths... Note: IsPreferred is across all phone numbers and not by type so we just get the last one for the type.
                //
                let homePhone = contact.PhoneNumbers.Where(x => x.TypeREF.Code == "Home" && !x.IsDeleted).OrderByDescending(x => x.ID).FirstOrDefault()
                let homeFax = contact.PhoneNumbers.Where(x => x.TypeREF.Code == "HomeFax" && !x.IsDeleted).OrderByDescending(x => x.ID).FirstOrDefault()
                let mobile = contact.PhoneNumbers.Where(x => x.TypeREF.Code == "Mobile" && !x.IsDeleted).OrderByDescending(x => x.ID).FirstOrDefault()
                let workPhone = contact.PhoneNumbers.Where(x => x.TypeREF.Code == "Work" && !x.IsDeleted).OrderByDescending(x => x.ID).FirstOrDefault()
                let workFax = contact.PhoneNumbers.Where(x => x.TypeREF.Code == "WorkFax" && !x.IsDeleted).OrderByDescending(x => x.ID).FirstOrDefault()
                let homeEmail = contact.Emails.Where(x => x.TypeREF.Code == "Home").OrderByDescending(x => x.ID).FirstOrDefault()
                let workEmail = contact.Emails.Where(x => x.TypeREF.Code == "Work").OrderByDescending(x => x.ID).FirstOrDefault()
                let contactSubClass1Code = contact.IRISObject.SubClass1REF.Code

                select new MOJExportReportLineDTO
                {
                    Infringement = new MOJExportInfringementDTO
                    {
                        Id = infringement == null ? 0 : infringement.ID,
                        CourtOfHearing = infringement.CourtofHearingREF.Code,
                        Fee = infringement.AmountOutstanding,
                        //TotalFee = infringement.AmountOutstanding, DTO just returns fee.
                        PartPaymentArranged = infringement.PartPaymentArranged,
                        IssuedBy = infringement.IssuedBy.AccountName,
                        ReissuedReminder = infringement.ReissueUnderS78B,
                        PreviousCrn = infringement.PreviousCRN,
                        NoticeNumber = infringement.IRISObject.BusinessID,
                        NoticeServedBy = infringement.ServedMethodREF.DisplayValue,
                        NoticeServedOn = infringement.ServedDate,
                        ReminderServedBy = infringement.ReminderNoticeServedMethodREF.DisplayValue,
                        ReminderServedOn = infringement.ReminderNoticeServedDate,
                        OffenceLocation = infringement.MOJLocationDescription
                    },

                    Contact = new MOJExportContactDTO
                    {
                        Id = contact == null ? 0 : contact.ID,
                        OrganisationYn = contactSubClass1Code == Datacom.IRIS.Common.ReferenceDataValueCodes.ContactType.Organisation ? "Y" : "N",
                        Gender = contact.PersonGenderREF.DisplayValue,
                        DateOfBirth = contact.PersonDateOfBirth,
                        Name = new MOJExportNameDTO
                        {
                            Id = name == null ? 0 : name.ID,
                            OrganisationName = name.OrganisationName,
                            NamePrefix = name.PersonTitleREF.DisplayValue,
                            LastName = name.PersonLastName,
                            MiddleNames = name.PersonMiddleNames,
                            FirstName = name.PersonFirstName,
                            NameType = name.NameTypeREF.Code
                        }
                    },

                    Addresses = new MOJExportAddressesDTO
                    {

                        //
                        // Addresses
                        //
                        // MOJ: Indicator for physical or postal address, Values are 1 = Postal and 2 = Street. Overseas address are not allowed so street = Urban Rural.
                        //     Note: Ignore the contactAddress.IsPostal flag this is the preferred postal address and nothing to do with the MOJ process, 
                        //           the sublinked address is the one we use and it is the councils responsibility to link to the correct address when adding the recipient.
                        //
                        // So for now we get all the information for the Urban Rural address and all the information for the delivery address and apply formatting as an additional step.
                        // This may change e.g. if we add a Table Value function to the DB to perform the formatting to Single / Multi-line in the query.
                        //

                        IssueAddressType = contactAddress.Address.Type,

                        //
                        // Issue Address Details
                        //                               
                        IssueAddress = new MOJExportAddressDTO
                        {

                            Id = address == null ? 0 : address.ID,
                            Prologue = contactAddress.Prologue,
                            IsCareOf = contactAddress == null ? false : contactAddress.IsCareOf,
                            //UrbanRuralFields
                            StreetAddressUnitType = (address as UrbanRuralAddress).UnitTypeREF.DisplayValue,
                            StreetAddressIdentifier = (address as UrbanRuralAddress).UnitIdentifier,
                            StreetAddressFloorType = (address as UrbanRuralAddress).FloorTypeREF.DisplayValue,
                            StreetAddressFloorIdentifier = (address as UrbanRuralAddress).FloorIdentifier,
                            StreetAddressBuildingName = (address as UrbanRuralAddress).BuildingName,
                            StreetAddressStreetNumber = (address as UrbanRuralAddress).StreetNumber,
                            //Need to concat with address 2 once the query has been run 'Unable to cast the type 'System.Nullable`1' to type 'System.Object'. LINQ to Entities only supports casting Entity Data Model primitive types.'																		 
                            StreetAddressAlpha = (address as UrbanRuralAddress).StreetAlpha,
                            StreetAddressStreetName = (address as UrbanRuralAddress).StreetName,
                            StreetAddressRuralDeliveryIdentifier = (address as UrbanRuralAddress).RuralDeliveryIdentifier,
                            StreetAddressSuburb = (address as UrbanRuralAddress).Suburb,
                            StreetAddressTownOrCity = (address as UrbanRuralAddress).TownCity,
                            StreetAddressPostCode = (address as UrbanRuralAddress).PostCode,
                            //DeliveryFields
                            DeliveryAddressType = (address as DeliveryAddress).DeliveryAddressTypeREF.DisplayValue,
                            DeliveryAddressServiceIdentifier = (address as DeliveryAddress).DeliveryServiceIdentifier,
                            DeliveryAddressBoxLobby = (address as DeliveryAddress).BoxLobby,
                            DeliveryAddressTownCity = (address as DeliveryAddress).TownCity,
                            DeliveryAddressPostCode = (address as DeliveryAddress).PostCode,
                        },

                        //
                        // ReminderAddress Details
                        //

                        ReminderAddressType = reminderAddress.Type,

                        //
                        // Issue Address Details
                        //                               
                        ReminderAddress = new MOJExportAddressDTO
                        {
                            Id = address == null ? 0 : address.ID,
                            Prologue = reminderContactAddress.Prologue,
                            IsCareOf = reminderContactAddress == null ? false : reminderContactAddress.IsCareOf,
                            //UrbanRuralFields
                            StreetAddressUnitType = (reminderAddress as UrbanRuralAddress).UnitTypeREF.DisplayValue,
                            StreetAddressIdentifier = (reminderAddress as UrbanRuralAddress).UnitIdentifier,
                            StreetAddressFloorType = (reminderAddress as UrbanRuralAddress).FloorTypeREF.DisplayValue,
                            StreetAddressFloorIdentifier = (reminderAddress as UrbanRuralAddress).FloorIdentifier,
                            StreetAddressBuildingName = (reminderAddress as UrbanRuralAddress).BuildingName,
                            StreetAddressStreetNumber = (reminderAddress as UrbanRuralAddress).StreetNumber,
                            //Need to concat with address 2 once the query has been run 'Unable to cast the type 'System.Nullable`1' to type 'System.Object'. LINQ to Entities only supports casting Entity Data Model primitive types.'																		 
                            StreetAddressAlpha = (reminderAddress as UrbanRuralAddress).StreetAlpha,
                            StreetAddressStreetName = (reminderAddress as UrbanRuralAddress).StreetName,
                            StreetAddressRuralDeliveryIdentifier = (reminderAddress as UrbanRuralAddress).RuralDeliveryIdentifier,
                            StreetAddressSuburb = (reminderAddress as UrbanRuralAddress).Suburb,
                            StreetAddressTownOrCity = (reminderAddress as UrbanRuralAddress).TownCity,
                            StreetAddressPostCode = (reminderAddress as UrbanRuralAddress).PostCode,
                            //DeliveryFields
                            DeliveryAddressType = (reminderAddress as DeliveryAddress).DeliveryAddressTypeREF.DisplayValue,
                            DeliveryAddressServiceIdentifier = (reminderAddress as DeliveryAddress).DeliveryServiceIdentifier,
                            DeliveryAddressBoxLobby = (reminderAddress as DeliveryAddress).BoxLobby,
                            DeliveryAddressTownCity = (reminderAddress as DeliveryAddress).TownCity,
                            DeliveryAddressPostCode = (reminderAddress as DeliveryAddress).PostCode,
                        },

                    },

                    Offence = new MOJExportOffenceDTO
                    {
                        Id = offence == null ? 0 : offence.ID,
                        OffenceDate = offence.OffenceDateFrom,
                        OffenceDateTo = offence.OffenceDateTo,
                        OffenceTime = offence.OffenceTime,
                        OffenceDateType = offence.OffenceDateTypeREF.Code,
                        OffenceDetails = offence.OffenceDetails,
                        OffenceAgainstText = offence.IRISObject.SubClass1REF.DisplayValue + ", " + offence.OffenceSectionREF.DisplayValue
                    },

                    ContactDetails = new MOJExportReportContactDetailsDTO
                    {
                        //
                        //Contact Details
                        //
                        HomeTelephone = new MOJExportPhoneNumberDTO
                        {
                            Id = homePhone == null ? 0 : homePhone.ID,
                            CountryCode = homePhone.CountryCode,
                            Number = homePhone.Number,
                            Extension = homePhone.Extension,
                        },
                        HomeFax = new MOJExportPhoneNumberDTO
                        {
                            Id = homeFax == null ? 0 : homeFax.ID,
                            CountryCode = homeFax.CountryCode,
                            Number = homeFax.Number,
                            Extension = homeFax.Extension,
                        },
                        WorkTelephone = new MOJExportPhoneNumberDTO
                        {
                            Id = workPhone == null ? 0 : workPhone.ID,
                            CountryCode = workPhone.CountryCode,
                            Number = workPhone.Number,
                            Extension = workPhone.Extension,
                        },
                        WorkFax = new MOJExportPhoneNumberDTO
                        {
                            Id = workFax == null ? 0 : workFax.ID,
                            CountryCode = workFax.CountryCode,
                            Number = workFax.Number,
                            Extension = workFax.Extension,
                        },
                        Mobile = new MOJExportPhoneNumberDTO
                        {
                            Id = mobile == null ? 0 : mobile.ID,
                            CountryCode = mobile.CountryCode,
                            Number = mobile.Number,
                            Extension = mobile.Extension,
                        },

                        HomeEmailAddress = homeEmail.Value,
                        WorkEmailAddress = workEmail.Value,

                        Aliases = contact.Names.Where(x => x.NameTypeREF.Code == "KnownAs"),
                    }
                };
            return result.ToList();
        }

        private static object _lockObject = new object();
        public string GetMOJExportReportSequenceNumber()
        {
            //Note: We use a lock to ensure that 2 reports generated at the same time do not have the same reference number
            //TODO - this should use transaction instead of lock as we need to cater for web farm scenario. See also CommonRepository.GetNextBusinessID
            lock (_lockObject)
            {
                var setting = Context.Sequence.Single(s => s.Key == SequencesCodes.NextMOJExportSequenceNumber).TrackAll();
                var refNumber = setting.Value;
                var isUsedToday = setting.LastModified.ToDateString() == DateTime.Today.ToDateString();
                return isUsedToday ? refNumber.ToString("00") : "01";
            }
        }

        public string MOJExportUpdateInfringementNoticeActionsAndIncrementBatchSequenceOnCompletion(List<long> infringementNoticeIds)
        {
            var infringementNoticeClosedStatusId = RepositoryMap.ReferenceDataRepository
                                                                .GetReferenceDataCollectionsWithParentValues(new string[] { ReferenceDataCollectionCode.EnforcementActionStatus }, 1)
                                                                .Single()
                                                                .ReferenceDataValues
                                                                .Single(rdv => rdv.ParentReferenceDataValue.Code == ReferenceDataValueCodes.EnforcementAction.InfringementNotice &&
                                                                               rdv.Code == ReferenceDataValueCodes.EnforcementActionStatus.Closed).ID;

            infringementNoticeIds.ForEach(x => MOJExportUpdateInfringementNoticeActionAndIncrementBatchSequenceOnCompletion(x, infringementNoticeClosedStatusId));
            return IncrementMOJExportReportSequenceNumber();
        }

        private void MOJExportUpdateInfringementNoticeActionAndIncrementBatchSequenceOnCompletion(long enforcementActionInfringementNoticeId, long infringementNoticeClosedStatusId)
        {
            var action = Context.EnforcementActions.Include(x => x.IRISObject).Single(x => x.ID == enforcementActionInfringementNoticeId);

            (action as EnforcementActionInfringementNotice).FiledWithMOJ = true;

            var currentDT = DateTime.Now;

            var closedStatus = new Status
            {
                IRISObjectID = action.IRISObject.ID,
                StatusDate = DateTime.Today,
                StatusREFID = infringementNoticeClosedStatusId,
                CreatedBy = SecurityHelper.CurrentUserName,
                DateCreated = currentDT,
                IsDeleted = false
                //,StatusTime = DateTime.Now.TimeOfDay
            };

            (action as EnforcementActionInfringementNotice).IRISObject.Statuses.Add(closedStatus);
            ApplyEntityChanges(action);

            SaveChanges();
        }

        private string IncrementMOJExportReportSequenceNumber()
        {
            //Note: We use a lock to ensure that 2 reports generated at the same time do not have the same reference number
            //TODO - this should use transaction instead of lock as we need to cater for web farm scenario. See also CommonRepository.GetNextBusinessID
            lock (_lockObject)
            {
                var setting = Context.Sequence.Single(s => s.Key == SequencesCodes.NextMOJExportSequenceNumber).TrackAll();
                var refNumber = setting.Value;
                var isUsedToday = setting.LastModified.ToDateString() == DateTime.Today.ToDateString();
                setting.Value = isUsedToday ? (refNumber + 1) : 2;
                setting.LastModified = DateTime.Now;
                setting.ModifiedBy = SecurityHelper.CurrentUserName;
                ApplyEntityChanges(setting);
                SaveChanges();
                return refNumber.ToString();
            }
        }

        public List<EnforcementAllegedOffence> GetAllegedOffencesAndOffendersByEnforcementID(long enforcementID, long enforcementIRISObjectID)
        {
            var offences = Context.EnforcementAllegedOffences
                .Include(x => x.OffenceSectionREF)
                .Include(x => x.NatureOfOffenceREF)
                .Where(x => !x.IsDeleted && x.EnforcementID == enforcementID);

            foreach (var offence in offences)
            {
                var offence1 = offence;   //Avoid modified closure.
                offence.EnforcementAllegedOffenceOffenders.AddTrackableCollection(Context.EnforcementAllegedOffenceOffenders.Where(x => !x.IsDeleted && x.EnforcementAllegedOffenceID == offence1.ID).ToList());
            }

            return offences.ToList();
        }

        public List<EnforcementUnpaidInfringementDTO> GetUnpaidInfringements()
        {
            var activeStatusId = RepositoryMap.ReferenceDataRepository.GetReferenceDataValuesForCollection(ReferenceDataCollectionCode.EnforcementActionStatus, ReferenceDataValueCodes.EnforcementActionInfringementNoticeStatus.Active).First().ID;

            var infringements = (Context.EnforcementActions.OfType<EnforcementActionInfringementNotice>()
                .Include(x => x.IRISObject)
                .Include(x => x.IRISObject.Statuses)
                .Include("IRISObject.Statuses.StatusREF")
                .Where(x =>
                    x.InfringementOutcome == ReferenceDataValueCodes.EnforcementActionInfringementOutcome.Unpaid
                    &&
                    (x.FiledWithMOJ == null || x.FiledWithMOJ == false)
                ).ToList());

            var result = new List<EnforcementUnpaidInfringementDTO>();

            var activeInfringements = infringements.Where(x => x.IRISObject.LatestStatus.StatusREFID == activeStatusId).ToList();

            foreach (var infringement in activeInfringements)
            {
                // Get Recipient (Should only ever be one for an Infringement so we use single)
                var recipient = Context.EnforcementActionRecipientOrRespondents.Include(x => x.RecipientOrRespondentActivityObjectRelationship).Single(x => !x.IsDeleted && x.EnforcementActionId == infringement.ID);
                recipient.RecipientOrRespondentActivityObjectRelationship.ContactSubLink = GetContactSubLinkByActivityObjectRelationship(recipient.RecipientOrRespondentActivityObjectRelationshipID, true);

                // Get Offence (Should only ever be one for an Infringement so we use Single)                
                var infringement1 = infringement; //Modified closure.
                var offenceAction = Context.EnforcementActionAllegedOffences.Include(x => x.EnforcementAllegedOffence).Where(x => !x.IsDeleted && x.EnforcementActionID == infringement1.ID);

                foreach (var item in offenceAction.Select(x => x.EnforcementAllegedOffence))
                {
                    item.OffenceDateTypeREF = Context.ReferenceDataValue.Single(x => x.ID == item.OffenceDateTypeREFID);
                    item.IRISObject = Context.IRISObject.Include(x => x.SubClass1REF).Single(x => x.ID == item.IRISObjectID);

                }
                var offence = offenceAction.Select(x => x.EnforcementAllegedOffence).Single();

                var dto = new EnforcementUnpaidInfringementDTO
                {
                    ID = infringement.ID,
                    BusinessId = infringement.IRISObject.BusinessID,
                    Recipient = recipient.RecipientOrRespondentActivityObjectRelationship.ContactNameMultiInstance,
                    Date = offence.DateStringForAllegedOffence,
                    Act = offence.IRISObject.SubClass1REF.DisplayValue
                };

                if (recipient.RecipientOrRespondentActivityObjectRelationship.IsCurrent == false)
                {
                    dto.Recipient = "(NON-CURRENT) " + recipient.RecipientOrRespondentActivityObjectRelationship.ContactNameMultiInstance;
                }

                result.Add(dto);
            }

            return result;
        }

        /// <summary>
        ///  Gets the full list of Alleged Offenders for an Alleged Offence
        /// </summary>
        public List<EnforcementAllegedOffenceOffender> GetAllegedOffendersList(long enforcementAllegedOffenceIRISObjectID)
        {
            var dbOffenders = from offences in Context.EnforcementAllegedOffences.Where(e => e.IRISObjectID == enforcementAllegedOffenceIRISObjectID)
                              from offenders in Context.EnforcementAllegedOffenceOffenders
                              .Where(e => e.EnforcementAllegedOffenceID == offences.ID && !e.IsDeleted)
                              select new { offenders, offenders.AllegedOffenderActivityObjectRelationship };

            List<EnforcementAllegedOffenceOffender> enforcementAllegedOffenceOffenders =
                dbOffenders.AsEnumerable().Select(s => s.offenders).Distinct().ToList();

            foreach (var offender in enforcementAllegedOffenceOffenders)
            {
                offender.ContactSubLink =
                    GetContactSubLinkByActivityObjectRelationship(offender.AllegedOffenderActivityObjectRelationshipID, true);
                if (ContactHasAnAddress(offender))
                    offender.ContactSubLink.ContactAddress = Context.ContactAddress.Single(a => a.ID == offender.ContactSubLink.ContactAddressID);
            }

            return enforcementAllegedOffenceOffenders;
        }

        /// <summary>
        ///  Gets the full list of Alleged Offenders for an Enforcement
        /// </summary>
        public List<RelationshipEntry> GetAllegedOffendersListByEnforcementIRISObjectID(long enforcementIRISObjectID, long enforcementID)
        {
            //get all relationships where Contact is Alleged Offender
            //this call returns ContactSubLink with Name property
            var allegedOffendersRelationships = GetActivityObjectRelationshipsWithNamesByEnforcementID(
                ObjectRelationshipTypesCodes.EnforcementAllegedOffender, enforcementID, new List<long>(), null, null, true);

            //Add ContactAddress object
            foreach (var relationship in allegedOffendersRelationships)
            {
                if (ContactHasAnAddress(relationship))
                    relationship.ContactSubLink.ContactAddress = Context.ContactAddress.Single(a => a.ID == relationship.ContactSubLink.ContactAddressID);
            }

            return allegedOffendersRelationships;
        }

        private static bool ContactHasAnAddress(EnforcementAllegedOffenceOffender offender)
        {
            return offender.ContactSubLink != null && offender.ContactSubLink.ContactAddressID != null;
        }

        private static bool ContactHasAnAddress(RelationshipEntry relationship)
        {
            return relationship.ContactSubLink != null && relationship.ContactSubLink.ContactAddressID != null;
        }

        /// <summary>
        ///  Gets the full list of Alleged Offences for an Enforcement
        /// </summary>
        public List<EnforcementAllegedOffence> GetAllegedOffencesList(long enforcementId)
        {
            var dbOffences = from offences in Context.EnforcementAllegedOffences
                                 .Include(o => o.IRISObject)
                                 .Include(o => o.IRISObject.SubClass1REF)
                                 .Include(o => o.IRISObject.SecurityContextIRISObject)
                                 .Include(o => o.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                                 .Where(e => e.EnforcementID == enforcementId && !e.IsDeleted)
                             select offences;

            return dbOffences.AsEnumerable().Distinct().ToList();
        }

        /// <summary>
        ///  Gets the full list of Alleged Offenders for an Alleged Offence with Name through ContactSublink
        /// </summary>
        public List<EnforcementAllegedOffenceOffender> GetAllegedOffendersListWithContactSublinks(long enforcementAllegedOffenceIRISObjectID)
        {
            var dbOffenders = from offences in Context.EnforcementAllegedOffences.Where(e => e.IRISObjectID == enforcementAllegedOffenceIRISObjectID)
                              from offenders in Context.EnforcementAllegedOffenceOffenders.Where(e => e.EnforcementAllegedOffenceID == offences.ID && !e.IsDeleted)
                              select new { offenders, offenders.ActionToBeTakenREF, offenders.NoActionReasonREF };

            List<EnforcementAllegedOffenceOffender> EnforcementAllegedOffenceOffenders =
                dbOffenders.AsEnumerable().Select(s => s.offenders).Distinct().ToList();

            foreach (var enforcementAllegedOffenceOffender in EnforcementAllegedOffenceOffenders)
            {
                EnforcementAllegedOffenceOffender offender = enforcementAllegedOffenceOffender;
                offender.ContactSubLink = GetContactSubLinkByActivityObjectRelationship(offender.AllegedOffenderActivityObjectRelationshipID, true);
                if (offender.AllegedOffenderActivityObjectRelationship.IsCurrent == false)
                {
                    offender.FormattedNameSingleInstance = "(NON-CURRENT) " + offender.ContactSubLink.Name.FormattedNameSingleInstance;
                }
            }

            return EnforcementAllegedOffenceOffenders;
        }

        public long GetactivityRelationshipObjectIdByEnforcementActionId(long enforcementActionIRISObjectID)
        {
            var enforcementActionRecipientOrRespondent = Context.EnforcementActionRecipientOrRespondents.SingleOrDefault(x => x.IsDeleted == false && x.EnforcementActionId == enforcementActionIRISObjectID);

            if (enforcementActionRecipientOrRespondent != null)
            {
                return enforcementActionRecipientOrRespondent.RecipientOrRespondentActivityObjectRelationshipID;
            }

            return 0;
            //return 39;

        }



        public List<ContactAddress> GetContactAddressesByActivityRelationshipObjectId(long enforcementIRISObjectID, long activityRelationshipObjectId)
        {
            var contactId = GetContactIdFromActivityObjectRelationshipId(activityRelationshipObjectId);
            var contactAddresses = Context.ContactAddress.Where(x => x.ContactID == contactId).ToList();

            contactAddresses.ForEach(a =>
                a.Address = RepositoryMap.AddressRepository.GetAddressByID(a.AddressID)
            );

            return contactAddresses;
        }

        public ActivityObjectRelationship GetActivityObjectRelationshipByIRISObjectIDAndContactID(long irisObjectID, long contactID, string linkType)
        {
            if (contactID <= 0) return null;
            return (from contact in Context.Contact.Where(c => c.ID == contactID)
                    from type in Context.ActivityObjectRelationshipType.Where(t => t.Code == linkType)
                    from relationship in Context.ActivityObjectRelationship
                        .Where(r => r.ActivityObjectRelationshipTypeID == type.ID &&
                            (
                                (r.IRISObjectID == irisObjectID && r.RelatedIRISObjectID == contact.IRISObjectID) ||
                                (r.RelatedIRISObjectID == irisObjectID && r.IRISObjectID == contact.IRISObjectID))
                            )
                    select relationship).SingleOrDefault();
        }

        /// <summary>
        ///  Gets Enforcement the full list of Alleged Offenders for an Enforcement with Name through ContactSublink
        /// </summary>
        public Enforcement GetEnforcementWithOffendersWhereActionToBeTakenYes(long enforcementId)
        {
            long actionToBeTakenYesID = RepositoryMap.ReferenceDataRepository.GetReferenceDataValuesForCollection(
                ReferenceDataCollectionCode.EnforcementActionToBeTaken,
                ReferenceDataValueCodes.EnforcementActionToBeTaken.Yes).First().ID;

            Enforcement enforcement = GetEnforcementByIDForDetailsPage(enforcementId);

            RelationshipLinkCriteria criteria = new RelationshipLinkCriteria
            {
                IRISObjectID = enforcement.IRISObjectID,
                IncludedRelationshipTypeCodeList = new List<string> { "AllegedOffender" },
                IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Contact }
            };

            ILinkingRepository linkingRepository = RepositoryMap.LinkingRepository;
            List<RelationshipEntry> relationshipEntries = linkingRepository.GetRelationshipLinks(criteria);

            IEnumerable<long> relationshipIDs = relationshipEntries.Select(r => r.ActivityObjectRelationshipID);

            foreach (var offence in enforcement.EnforcementAllegedOffences)
            {
                offence.EnforcementAllegedOffenceOffenders.AddRange(
                    (from offenders in Context.EnforcementAllegedOffenceOffenders
                     where offence.ID == offenders.EnforcementAllegedOffenceID &&
                         offenders.ActionToBeTakenREFID == actionToBeTakenYesID
                     select new { offenders, offenders.AllegedOffenderActivityObjectRelationship })
                    .AsEnumerable().Select(s => s.offenders).Distinct().ToList());

                // Get Names
                foreach (var offender in offence.EnforcementAllegedOffenceOffenders)
                {
                    // Get Alleged Offender name from the linked contact sub-link.
                    if (offender.AllegedOffenderActivityObjectRelationship.IsCurrent == false)
                    {
                        offender.FormattedNameSingleInstance = GetContactSubLinkByActivityObjectRelationship(offender.AllegedOffenderActivityObjectRelationshipID, true).Name.FormattedNameSingleInstance;
                        offender.AllegedOffenderActivityObjectRelationship.ContactName = "(NON-CURRENT) " + offender.FormattedNameSingleInstance;
                    }
                    else
                    {
                        offender.FormattedNameSingleInstance = GetContactSubLinkByActivityObjectRelationship(offender.AllegedOffenderActivityObjectRelationshipID).Name.FormattedNameSingleInstance;
                        offender.AllegedOffenderActivityObjectRelationship.ContactName = offender.FormattedNameSingleInstance;
                    }
                }

            }

            return enforcement;
        }

        /// <summary>
        ///    Returns a list of offender entries for this enforcement action that are linked as alleged offenders to any enforcement offence where action is to be taken
        /// </summary>
        public List<RelationshipEntry> GetUnassignedOffenderRelationshipsWhereActionIsToBeTakenByEnforcementActionIRISObjectIDForRespondentsRecipientsDropdown(
            long enforcementActionID, long enforcementActionIRISObjectID, long enforcementID)
        {
            var actionToBeTakenYesID = RepositoryMap.ReferenceDataRepository.GetReferenceDataValuesForCollection(
                ReferenceDataCollectionCode.EnforcementActionToBeTaken,
                ReferenceDataValueCodes.EnforcementActionToBeTaken.Yes).First().ID;

            //get respondentsRecipients where actionToBeTaken is Yes
            var respondentsRecipients =
                from enforcement in Context.Enforcements
                join enforcementAction in Context.EnforcementActions on enforcement.ID equals enforcementAction.EnforcementID
                join offences in Context.EnforcementAllegedOffences.Where(x => !x.IsDeleted) on enforcement.ID equals offences.EnforcementID
                join offenceOffender in Context.EnforcementAllegedOffenceOffenders.Where(x => !x.IsDeleted) on offences.ID equals offenceOffender.EnforcementAllegedOffenceID
                where enforcement.ID == enforcementID
                    && enforcementAction.ID == enforcementActionID
                    && enforcementAction.IRISObjectID == enforcementActionIRISObjectID
                    && offenceOffender.ActionToBeTakenREFID == actionToBeTakenYesID
                select new
                {
                    offenceOffender
                };

            // Offence offenders can be linked to more than one offence so only select the first occurrence of the alleged offender as a defendant.
            var existingActivityObjectRelationshipID = respondentsRecipients.GroupBy(d => d.offenceOffender.AllegedOffenderActivityObjectRelationshipID)
                .Select(d => d.FirstOrDefault().offenceOffender.AllegedOffenderActivityObjectRelationshipID).ToList();

            //get all relationships where Contact is Alleged Offender
            var allegedOffendersRelationships = GetActivityObjectRelationshipsWithNamesByEnforcementID(
                ObjectRelationshipTypesCodes.EnforcementAllegedOffender, enforcementID, new List<long>(), null, null, true);

            //get all offenders that are already linked to this action
            var assignedRespondentsRecipientsRelationshipIDs = Context.EnforcementActionRecipientOrRespondents.Where(x =>
                x.EnforcementActionId == enforcementActionID && !x.IsDeleted).Select(x => x.RecipientOrRespondentActivityObjectRelationshipID);

            //get unassigned relationships
            allegedOffendersRelationships = allegedOffendersRelationships.Where(x =>
                existingActivityObjectRelationshipID.Contains(x.ActivityObjectRelationshipID)
                && !assignedRespondentsRecipientsRelationshipIDs.Contains(x.ActivityObjectRelationshipID)).ToList();

            foreach (var offenderRelationship in allegedOffendersRelationships)
            {
                if (offenderRelationship.ActivityObjectRelationship.IsCurrent == false)
                {
                    offenderRelationship.FormattedName = "(NON-CURRENT) "+offenderRelationship.ContactSubLink.Name.FormattedNameSingleInstance;
                }
                else
                {
                    offenderRelationship.FormattedName = offenderRelationship.ContactSubLink.Name.FormattedNameSingleInstance;
                }
            }

            return allegedOffendersRelationships;
        }

        public List<EnforcementActionRecipientOrRespondent> GetEnforcementActionRecipientsByOffenderID(long offenderID, long enforementIRISObjectID)
        {
            return (from enforcements in Context.Enforcements.Where(e => e.IRISObjectID == enforementIRISObjectID)
                    from actions in Context.EnforcementActions.Where(a => a.EnforcementID == enforcements.ID)
                    from offenders in Context.EnforcementAllegedOffenceOffenders.Where(o => o.ID == offenderID)
                    from recipientOrRespondents in Context.EnforcementActionRecipientOrRespondents
                        .Where(r => r.RecipientOrRespondentActivityObjectRelationshipID == offenders.AllegedOffenderActivityObjectRelationshipID && r.EnforcementActionId == actions.ID && !r.IsDeleted)
                    select recipientOrRespondents).ToList();
        }

        /// <summary>
        ///   Gets the Alleged Offence record for given a give Id
        /// </summary>
        public EnforcementAllegedOffence GetEnforcementAllegedOffenceById(long EnforcementAllegedOffenceId)
        {
            EnforcementAllegedOffence allegedOffence = Context.EnforcementAllegedOffences
                                                                .Include(a => a.OffenceSectionREF)
                                                                .Include(a => a.NatureOfOffenceREF)
                                                                .Include(a => a.OffenceDateTypeREF)
                                                                .Include(x => x.IRISObject)
                                                                .Include(x => x.IRISObject.ObjectTypeREF)
                                                                .Include(o => o.IRISObject.SubClass1REF)
                                                                .Include(o => o.IRISObject.SecurityContextIRISObject)
                                                                .Include(o => o.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                                                                .First(e => e.ID == EnforcementAllegedOffenceId && !e.IsDeleted);

            return allegedOffence;
        }

        public EnforcementAllegedOffence GetEnforcementAllegedOffenceByIdForDetailsPage(long EnforcementAllegedOffenceId)
        {
            EnforcementAllegedOffence allegedOffence = GetEnforcementAllegedOffenceById(EnforcementAllegedOffenceId);

            // Get Alleged OffendersList 
            allegedOffence.EnforcementAllegedOffenceOffenders.AddTrackableCollection(GetAllegedOffendersList(allegedOffence.IRISObjectID));

            // Get assigned Actions
            allegedOffence.EnforcementActionAllegedOffences.AddTrackableCollection((
                from res in Context.EnforcementActionAllegedOffences
                .Where(r => r.EnforcementAllegedOffenceID == allegedOffence.ID && !r.IsDeleted)
                select res).AsEnumerable().Distinct().ToList());

            // Plans and Rules
            RepositoryHelpers.LoadPlansRulesPoliciesForEntity(Context, allegedOffence);

            //add TrackAll as we are using a AddTrackableCollection
            return allegedOffence.TrackAll();
        }

        #region Fetching Enforcement Actions

        /// <summary>
        ///    This method will return a list of enforcement actions for a given enforcement. 
        /// </summary>
        public List<EnforcementAction> GetEnforcementActions(long enforcementId)
        {
            var actionsExcludingProsecution = Context.EnforcementActions
                .Include(x => x.IRISObject)
                .Include(x => x.IRISObject.SubClass1REF)
                .Include(x => x.IRISObject.SubClass2REF)
                .Include(x => x.IRISObject.SecurityContextIRISObject)
                .Include(x => x.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                .Include(x => x.IRISObject.Statuses)
                .Include("IRISObject.Statuses.StatusREF")
                .Include(x => x.EnforcementActionOfficerResponsibles)
                .Include("EnforcementActionOfficerResponsibles.OfficerResponsible")
                .Include(x => x.EnforcementActionRecipientOrRespondents)
                .Include("EnforcementActionRecipientOrRespondents.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.Name.PersonTitleREF")
                .Include("EnforcementActionRecipientOrRespondents.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.Name.Contact.OrganisationStatusREF")
                .Where(a => a.EnforcementID == enforcementId && a.IRISObject.SubClass1REF.Code != "Prosecution")
                .ToList();

            var actionsProsecution = Context.EnforcementActions.OfType<EnforcementActionProsecution>()
                .Include(x => x.IRISObject)
                .Include(x => x.IRISObject.SubClass1REF)
                .Include(x => x.IRISObject.SubClass2REF)
                .Include(x => x.IRISObject.SecurityContextIRISObject)
                .Include(x => x.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                .Include(x => x.IRISObject.Statuses)
                .Include("IRISObject.Statuses.StatusREF")
                .Include(x => x.EnforcementActionOfficerResponsibles)
                .Include("EnforcementActionOfficerResponsibles.OfficerResponsible")
                .Include("EnforcementActionProsecutionDefendants.EnforcementAllegedOffenceOffender.AllegedOffenderActivityObjectRelationship.ContactSubLink.Name.PersonTitleREF")
                .Include("EnforcementActionProsecutionDefendants.EnforcementAllegedOffenceOffender.AllegedOffenderActivityObjectRelationship.ContactSubLink.Name.Contact.OrganisationStatusREF")
                .Where(a => a.EnforcementID == enforcementId)
                .ToList();

            return actionsExcludingProsecution.Concat(actionsProsecution).ToList();
        }
        #endregion

        /// <summary>
        ///  Gets an Enforcement Action  
        /// </summary>
        public EnforcementAction GetEnforcementActionById(long EnforcementActionId)
        {
            EnforcementAction action = Context.EnforcementActions
                                        .Include(a => a.ActREF)
                                        .Include(a => a.ActionSectionREF)
                                        .Include(x => x.IRISObject)
                                        .Include(x => x.IRISObject.SubClass1REF)
                                        .Include(x => x.IRISObject.SubClass2REF)
                                        .Include(x => x.IRISObject.SecurityContextIRISObject)
                                        .Include(x => x.IRISObject.SecurityContextIRISObject.ObjectTypeREF)
                                        .Include(o => o.IRISObject.ObjectTypeREF)
                                        .Include("IRISObject.Statuses.StatusREF")
                                        .Include("IRISObject.OtherIdentifiers.IdentifierContextREF")
                                        .Include(o => o.Enforcement)
                                        .Include(o => o.Enforcement.IRISObject)
                                        .Include(o => o.Enforcement.IRISObject.SubClass1REF)
                                        .Include(o => o.EnforcementActionRecipientOrRespondents)
                                        .Include("EnforcementActionRecipientOrRespondents.RecipientOrRespondentActivityObjectRelationship.ContactSublink.Contact")
                                        .Include("EnforcementActionRecipientOrRespondents.RecipientOrRespondentActivityObjectRelationship.ContactSublink.Name")
                                        .Include("EnforcementActionRecipientOrRespondents.RecipientOrRespondentActivityObjectRelationship.ContactSublink.ContactAddress")
                                        .Include("EnforcementActionRecipientOrRespondents.RecipientOrRespondentActivityObjectRelationship.ContactSublink.PhoneNumber")
                                        .Include("EnforcementActionRecipientOrRespondents.RecipientOrRespondentActivityObjectRelationship.ContactSublink.Email")
                                        .Include(o => o.EnforcementActionAllegedOffences)
                                       .First(e => e.ID == EnforcementActionId);

            // Extract action type specific properties and append to the returning object
            EnforcementActionEnforcementOrder actionOrder = action as EnforcementActionEnforcementOrder;
            if (actionOrder != null)
            {
                actionOrder.DecisionREF = (from n in Context.ReferenceDataValue
                                           where n.ID == actionOrder.DecisionREFID
                                           select n).SingleOrDefault().TrackAll();
            }

            EnforcementActionWarningAndLetter actionWarningAndLetter = action as EnforcementActionWarningAndLetter;
            if (actionWarningAndLetter != null)
            {
                actionWarningAndLetter.ServedMethodREF = (from n in Context.ReferenceDataValue
                                                          where n.ID == actionWarningAndLetter.ServedMethodREFID
                                                          select n).SingleOrDefault().TrackAll();
            }

            EnforcementActionNotice actionNotice = action as EnforcementActionNotice;
            if (actionNotice != null)
            {
                actionNotice.ServedMethodREF = (from n in Context.ReferenceDataValue
                                                where n.ID == actionNotice.ServedMethodREFID
                                                select n).SingleOrDefault().TrackAll();
            }

            EnforcementActionInfringementNotice actionInfringement = action as EnforcementActionInfringementNotice;
            if (actionInfringement != null)
            {
                actionInfringement.IssuedBy = (from n in Context.User
                                               where n.ID == actionInfringement.IssuedByID
                                               select n).SingleOrDefault().TrackAll();

                actionInfringement.ServedMethodREF = (from n in Context.ReferenceDataValue
                                                      where n.ID == actionInfringement.ServedMethodREFID
                                                      select n).SingleOrDefault().TrackAll();

                actionInfringement.CourtofHearingREF = (from n in Context.ReferenceDataValue
                                                        where n.ID == actionInfringement.CourtOfHearingREFID
                                                        select n).SingleOrDefault().TrackAll();

                actionInfringement.ReminderNoticeServedMethodREF = (from n in Context.ReferenceDataValue
                                                                    where n.ID == actionInfringement.ReminderNoticeServedMethodREFID
                                                                    select n).SingleOrDefault().TrackAll();

                actionInfringement.ReminderNoticeServedAddress = (from n in Context.ContactAddress
                                                                  where n.ID == actionInfringement.ReminderNoticeServedAddressID
                                                                  select n).SingleOrDefault().TrackAll();

                if (actionInfringement.ReminderNoticeServedAddress != null)
                    actionInfringement.ReminderNoticeServedAddress.Address =
                        RepositoryMap.AddressRepository.GetAddressByID(actionInfringement.ReminderNoticeServedAddress.AddressID);
            }

            EnforcementActionProsecution actionProsecution = action as EnforcementActionProsecution;
            if (actionProsecution != null)
            {
                // Get Prosecutor name from the linked contact sub-link.
                actionProsecution.ProsecutorActivityObjectRelationship = (from n in Context.ActivityObjectRelationship
                                                                          where n.ID == actionProsecution.ProsecutorActivityObjectRelationshipID
                                                                          select n).SingleOrDefault();
                if (actionProsecution.ProsecutorActivityObjectRelationship.IsCurrent == false)
                {
                    actionProsecution.ProsecutorActivityObjectRelationship.ContactSubLink =
                        GetContactSubLinkByActivityObjectRelationship(actionProsecution.ProsecutorActivityObjectRelationshipID, true);
                    actionProsecution.ProsecutorActivityObjectRelationship.ContactName = "(NON-CURRENT) " +
                        actionProsecution.ProsecutorActivityObjectRelationship.ContactSubLink.Name.FormattedNameSingleInstance;
                }
                else
                {
                    actionProsecution.ProsecutorActivityObjectRelationship.ContactSubLink =
                        GetContactSubLinkByActivityObjectRelationship(actionProsecution.ProsecutorActivityObjectRelationshipID);
                    actionProsecution.ProsecutorActivityObjectRelationship.ContactName =
                        actionProsecution.ProsecutorActivityObjectRelationship.ContactSubLink.Name.FormattedNameSingleInstance;
                }

                actionProsecution.Enforcement = (from n in Context.Enforcements
                                                 where n.ID == actionProsecution.EnforcementID
                                                 select n).SingleOrDefault();

                var defendants = Context.EnforcementActionProsecutionDefendants
                    .Include(x => x.EnforcementAllegedOffenceOffender)
                    .Include(x => x.DefenseCounselActivityObjectRelationship)
                    .Where(x => x.EnforcementActionProsecutionID == actionProsecution.ID && !x.IsDeleted);

                // Get Names
                //
                // Note: We Just pass back the Name, if we include the sublink(Contact/Name objects) we get an error in the graph 
                // when attempting to modify (add/edit/delete) the action, this may be because it tries to track contacts more than once.
                // This will require further investigation if you need to pass back the hydrated sublink/contact/name objects
                foreach (var defendant in defendants)
                {
                    // Get Alleged Offender ContactSubLink as we need Name; FormattedName can be extracted from ContactSubLink object
                    defendant.EnforcementAllegedOffenceOffender.ContactSubLink = GetContactSubLinkByActivityObjectRelationship(defendant.EnforcementAllegedOffenceOffender.AllegedOffenderActivityObjectRelationshipID, true);
                    // Get Defense Counsel name from the linked contact sub-link.
                    if (defendant.EnforcementAllegedOffenceOffender.AllegedOffenderActivityObjectRelationship.IsCurrent == false)
                    {
                        defendant.EnforcementAllegedOffenceOffender.FormattedNameMultiInstance = "(NON-CURRENT) " +
                            defendant.EnforcementAllegedOffenceOffender.FormattedNameMultiInstance;
                    }
                    if (defendant.DefenseCounselActivityObjectRelationshipID.HasValue)
                    {
                        if (defendant.DefenseCounselActivityObjectRelationship.IsCurrent == false)
                        {
                            defendant.DefenseCounselActivityObjectRelationship.ContactNameMultiInstance = "(NON-CURRENT) " + GetContactSubLinkByActivityObjectRelationship(
                                    defendant.DefenseCounselActivityObjectRelationshipID.Value, true).Name.FormattedNameMultiInstance;
                        }
                        else
                        {
                            defendant.DefenseCounselActivityObjectRelationship.ContactNameMultiInstance = GetContactSubLinkByActivityObjectRelationship(
                                    defendant.DefenseCounselActivityObjectRelationshipID.Value, true).Name.FormattedNameMultiInstance;
                        }
                    }
                    defendant.EnforcementActionProsecutionCharges.AddTrackableCollection(
                        Context.EnforcementActionProsecutionCharges
                            .Include(x => x.PleaREF)
                            .Include(x => x.ChargeOutcomeREF)
                            .Include(x => x.ChargeOutcomeStatusREF)
                            .Include(x => x.EnforcementAllegedOffence)
                            .Include("EnforcementAllegedOffence.OffenceSectionREF")
                            .Include("EnforcementAllegedOffence.NatureOfOffenceREF")
                            .Where(x => x.EnforcementActionProsecutionDefendantID == defendant.ID && !x.IsDeleted).ToList()
                        );

                    foreach (var charge in defendant.EnforcementActionProsecutionCharges)
                    {
                        charge.EnforcementActionProsecutionSentences.AddTrackableCollection(
                            Context.EnforcementActionProsecutionSentences
                            .Include(x => x.SentenceREF)
                            .Where(x => x.EnforcementActionProsecutionChargesID == charge.ID && !x.IsDeleted).ToList());
                    }
                }

                actionProsecution.EnforcementActionProsecutionDefendants.AddTrackableCollection(defendants.ToList());

                actionProsecution.TrackAll();
            }

            action.EnforcementActionOfficerResponsibles.AddTrackableCollection(
                GetEnforcementActionOfficerResponsibleById(action.IRISObjectID));

            action.EnforcementActionRecipientOrRespondents.AddTrackableCollection(
                (from res in Context.EnforcementActionRecipientOrRespondents
                                .Include(r => r.RecipientOrRespondentActivityObjectRelationship)
                               .Where(r => r.EnforcementActionId == action.ID && !r.IsDeleted)
                 select res).AsEnumerable().Distinct().ToList());


            return action.TrackAll();
        }

        public EnforcementAction GetEnforcementActionByDefendantId(long enforcementActionProsecutionDefendantId)
        {
            var enforcementActionID = Context.EnforcementActionProsecutionDefendants.Single(x => x.ID == enforcementActionProsecutionDefendantId).EnforcementActionProsecutionID;
            return GetEnforcementActionById(enforcementActionID);
        }

        public EnforcementAction GetEnforcementActionByIdForDetailsPage(long EnforcementActionId)
        {
            var action = GetEnforcementActionById(EnforcementActionId);

            action.EnforcementActionAllegedOffences.AddTrackableCollection(
                (from res in Context.EnforcementActionAllegedOffences
                               .Where(r => r.EnforcementActionID == action.ID && !r.IsDeleted)
                 select res).AsEnumerable().Distinct().ToList());

            // Get Names
            foreach (var recipientRespondents in action.EnforcementActionRecipientOrRespondents.Where(x => !x.IsDeleted))
            {
                // Get name from the linked contact sub-link.
                recipientRespondents.RecipientOrRespondentActivityObjectRelationship.ContactName =
                    GetContactSubLinkByActivityObjectRelationship(recipientRespondents.RecipientOrRespondentActivityObjectRelationshipID, true).Name.FormattedNameSingleInstance;
            }

            return action.TrackAll();
        }

        // Note: the parameter enforcementIRISObjectID is included for security and not used by the method. See interface attributes on the method.
        /// <summary>
        ///    Returns a list of offender entries for this enforcement that are linked as alleged offenders to any enforcement offence where action is to be taken,
        ///    and are not already associated as a Defendant for this Prosecution Enforcement Action.
        /// </summary>
        public List<EnforcementAllegedOffenceOffender> GetOffendersWhereActionIsToBeTakenWithNamesByEnforcementActionIDForDropdown(long enforcementID, long enforcementIRISObjectID, long enforcementActionID, long? offenderIDToInclude = new long?())
        {
            var actionToBeTakenYesID = RepositoryMap.ReferenceDataRepository.GetReferenceDataValuesForCollection(
                ReferenceDataCollectionCode.EnforcementActionToBeTaken,
                ReferenceDataValueCodes.EnforcementActionToBeTaken.Yes).First().ID;

            var potentialDefendants =
                from enforcement in Context.Enforcements
                join enforcementAction in Context.EnforcementActions on enforcement.ID equals enforcementAction.EnforcementID
                join offences in Context.EnforcementAllegedOffences.Where(x => !x.IsDeleted) on enforcement.ID equals offences.EnforcementID
                join offenceOffender in Context.EnforcementAllegedOffenceOffenders.Where(x => x.ID == offenderIDToInclude || !x.IsDeleted) on offences.ID equals offenceOffender.EnforcementAllegedOffenceID

                where enforcement.ID == enforcementID
                    && enforcementAction.ID == enforcementActionID
                    && offenceOffender.ActionToBeTakenREFID == actionToBeTakenYesID
                select new
                {
                    offenceOffender
                };

            var result = new List<EnforcementAllegedOffenceOffender>();

            // Offence offenders can be linked to more than one offence so only select the first occurrence of the alleged offender as a defendant.
            potentialDefendants = potentialDefendants.GroupBy(d => d.offenceOffender.AllegedOffenderActivityObjectRelationshipID).Select(d => d.FirstOrDefault());

            var existingDefendantIDs = GetExistingDefendantIDs(enforcementActionID, offenderIDToInclude);

            potentialDefendants.ToList().ForEach(defendant =>
            {
                var item = defendant.offenceOffender;
                if (IsAlreadyADefendantForThisProsecutionAction(item, existingDefendantIDs, offenderIDToInclude))
                {
                    // Skip as we should not add an offender twice as a defendant to a prosecution action.
                }
                else
                {
                    // Get offenders name from the linked contact sub-linked.
                    var sublink = GetContactSubLinkByActivityObjectRelationship(item.AllegedOffenderActivityObjectRelationshipID, true);
                    if (item.AllegedOffenderActivityObjectRelationship.IsCurrent == false)
                    {
                        item.FormattedNameSingleInstance = "(NON-CURRENT) " + sublink.Name.FormattedNameSingleInstance;
                        item.FormattedNameMultiInstance = "(NON-CURRENT)" + sublink.Name.FormattedNameMultiInstance;
                    }
                    else
                    {
                        item.FormattedNameSingleInstance = sublink.Name.FormattedNameSingleInstance;
                        item.FormattedNameMultiInstance = sublink.Name.FormattedNameMultiInstance;
                    }
                    result.Add(item);
                }
            });

            return result;
        }

        private List<long> GetExistingDefendantIDs(long enforcementActionID, long? offenceIDToInclude)
        {
            var existingDefendants = Context.EnforcementActionProsecutionDefendants.Where(x => x.EnforcementActionProsecutionID == enforcementActionID && !x.IsDeleted).ToList();

            if (offenceIDToInclude.HasValue)
                existingDefendants.RemoveAll(x => x.EnforcementAllegedOffenceOffenderID == offenceIDToInclude);

            var existingDefendantOffenderIDs = existingDefendants.Select(x => x.EnforcementAllegedOffenceOffenderID).ToList();
            return existingDefendantOffenderIDs;
        }

        public List<EnforcementAllegedOffence> GetChargeOffencesByDefendantIDForDropdown(long enforcementID, long enforcementIRISObjectID, long defendantID, long? offenceIDToInclude = new long?())
        {
            //get existing charges already assigned to a defendant - these will be excluded from the final list
            var existingChargesIDs = Context.EnforcementActionProsecutionCharges
                .Where(x => x.EnforcementActionProsecutionDefendantID == defendantID && (!x.IsDeleted || x.ID == offenceIDToInclude))
                .Select(x => x.EnforcementAllegedOffenceID).ToList();
            //if in edit mode, ensure the selected charge is not excluded, hence, remove it from the list that will be excluded
            existingChargesIDs.RemoveAll(x => x == offenceIDToInclude);

            //get relationship ID for the offender who is the defendant
            var offenderRelationshipID = (from defendant in Context.EnforcementActionProsecutionDefendants.Where(x => x.ID == defendantID)
                                          from offender in Context.EnforcementAllegedOffenceOffenders.Where(x => x.ID == defendant.EnforcementAllegedOffenceOffenderID)
                                          select offender).Select(x => x.AllegedOffenderActivityObjectRelationshipID).Single();

            //get all offence IDs for that relationship
            var existingOffenceIDs = (from offender in Context.EnforcementAllegedOffenceOffenders.Where(x => x.AllegedOffenderActivityObjectRelationshipID == offenderRelationshipID)
                                      select offender).Select(x => x.EnforcementAllegedOffenceID).ToList();

            var offences =
                from offence in
                    Context.EnforcementAllegedOffences.Where(x => !existingChargesIDs.Contains(x.ID) && existingOffenceIDs.Contains(x.ID) && !x.IsDeleted)
                join offenceSection in Context.ReferenceDataValue on offence.OffenceSectionREFID equals offenceSection.ID
                join natureOfOffence in Context.ReferenceDataValue on offence.NatureOfOffenceREFID equals natureOfOffence.ID

                select new { offence, offenceSection, natureOfOffence };

            var result = new List<EnforcementAllegedOffence>();

            offences.ToList().ForEach(item =>
            {
                var offence = item.offence;
                offence.OffenceSectionREF = item.offenceSection;
                offence.NatureOfOffenceREF = item.natureOfOffence;
                result.Add(offence);
            });

            return result;
        }

        private static bool IsAlreadyADefendantForThisProsecutionAction(
            EnforcementAllegedOffenceOffender item, ICollection<long> existingDefendantOffenderIDs, long? offenceIDToInclude = new long?())
        {
            return existingDefendantOffenderIDs.Contains(item.ID) && item.ID != offenceIDToInclude;
        }

        public ContactSubLink GetContactSubLinkByActivityObjectRelationship(long activityObjectRelationshipID, bool isIncludeExpiredRelationships = false)
        {
            var activityRelationshipsIDToInclude = new List<long>();
            if (isIncludeExpiredRelationships) activityRelationshipsIDToInclude.Add(activityObjectRelationshipID);

            var subLinkQuery =

                from activityObjectRelationship in Context.ActivityObjectRelationship.Where(r => activityRelationshipsIDToInclude.Contains(r.ID) || r.CurrentTo == null)
                join contactSubLink in Context.ContactSubLink on activityObjectRelationship.ID equals contactSubLink.ActivityObjectRelationshipID
                join contactName in Context.Name.Where(x => !x.IsDeleted) on contactSubLink.NameID equals contactName.ID
                join contact in Context.Contact on contactName.ContactID equals contact.ID
                join contactIrisObject in Context.IRISObject on contact.IRISObjectID equals contactIrisObject.ID

                where activityObjectRelationship.ID == activityObjectRelationshipID
                select new
                {
                    activityObjectRelationship,
                    contactSubLink,
                    contactName,
                    contactName.NameTypeREF,
                    contactName.PersonTitleREF,
                    contact,
                    contact.OrganisationStatusREF,
                    contactIrisObject,
                    contactIrisObject.SubClass1REF
                };

            return subLinkQuery.AsEnumerable().Select(x => x.contactSubLink).SingleOrDefault();
        }

        // Note: the parameter enforcementIRISObjectID is included for security and not used by the method. See interface attributes on the method.
        /// <summary>
        ///    Returns a list of a list of relationship entries for this enforcement that are linked as Defense Counsel.
        /// </summary>
        public List<RelationshipEntry> GetDefenseCouncillorsByEnforcementIRISObjectIDForDropdown(long enforcementID, long enforcementIRISObjectID, long? activityRelationshipObjectIDToInclude = new long?())
        {
            var contactIDsToInclude = new List<long>();
            if (activityRelationshipObjectIDToInclude.HasValue)
            {
                var contactId = GetContactIdFromActivityObjectRelationshipId(activityRelationshipObjectIDToInclude.Value);
                if (contactId > 0)
                    contactIDsToInclude.Add(contactId);
            }

            return GetActivityObjectRelationshipsWithNamesByEnforcementID(ObjectRelationshipTypesCodes.DefenseCounsel, enforcementID, contactIDsToInclude, null, null, true);
        }

        private long GetContactIdFromActivityObjectRelationshipId(long activityObjectRelationshipId)
        {
            var returnObj = (from activityObjectRelationship in Context.ActivityObjectRelationship
                join subLink in Context.ContactSubLink on activityObjectRelationship.ID equals
                subLink.ActivityObjectRelationshipID
                where activityObjectRelationship.ID == activityObjectRelationshipId
                select subLink).SingleOrDefault();

            if (returnObj != null)
            {
                return
                (
                    from activityObjectRelationship in Context.ActivityObjectRelationship
                    join subLink in Context.ContactSubLink on activityObjectRelationship.ID equals
                    subLink.ActivityObjectRelationshipID
                    where activityObjectRelationship.ID == activityObjectRelationshipId
                    select subLink
                ).Single().ContactID;
            }
            
            return 0;
            
        }

        /// Note: the parameter enforcementIRISObjectID is included for security and not used by the method. See interface attributes on the method.
        /// <summary>
        ///    Returns a list of a list of relationship entries for this enforcement that are linked as alleged offenders,
        ///    and are not already associated as an Offender for this Offence.
        /// </summary>
        public List<RelationshipEntry> GetUnassociatedAllegedOffendersForOffenceByEnforcementIDAndOffenceIDForDropdown(
            long allegedOffenceIRISObjectID, long enforcementId, long? relationshipIDToInclude = null)
        {
            // Exclude contacts already added as offenders to the offence.
            var associatedOffenceOffenders = from offences in Context.EnforcementAllegedOffences.Where(e => e.IRISObjectID == allegedOffenceIRISObjectID)
                                             from offenders in Context.EnforcementAllegedOffenceOffenders
                                                 .Where(e => e.EnforcementAllegedOffenceID == offences.ID && !e.IsDeleted)
                                             select offenders;

            //extract relationshipIds of associated offenders
            var relationshipIDsToExclude = associatedOffenceOffenders.Where(x => !x.IsDeleted).Select(s => s.AllegedOffenderActivityObjectRelationshipID).ToList();

            //make sure the currently offender selected for editing is still in the dropdown list, hence, leave it in and don't exclude
            if (relationshipIDToInclude.HasValue)
                relationshipIDsToExclude = relationshipIDsToExclude
                    .Where(a => a != relationshipIDToInclude.Value).ToList();

            //get all offender relationships
            var allOffenderRelationships = GetActivityObjectRelationshipsWithNamesByEnforcementID(
                ObjectRelationshipTypesCodes.EnforcementAllegedOffender, enforcementId,
                new List<long>(), new List<long>(), new List<long>(), true);

            //exclude the associated ones
            allOffenderRelationships = allOffenderRelationships
                .Where(a => !relationshipIDsToExclude.Contains(a.ActivityObjectRelationshipID))
                .Select(a => a).ToList();

            return allOffenderRelationships;
        }

        private List<RelationshipEntry> GetActivityObjectRelationshipsWithNamesByEnforcementID(
            string objectRelationshipCode, long enforcementId,
            IEnumerable<long> contactIDsToInclude = null, IEnumerable<long> contactIDsToExclude = null,
            IEnumerable<long> contactIDsToFilterBy = null, bool isIncludeExpiredRelationships = false)
        {
            var enforcementIRISObjectID = Context.Enforcements.Single(e => e.ID == enforcementId).IRISObjectID;

            var criteria = new RelationshipLinkCriteria
            {
                IRISObjectID = enforcementIRISObjectID,
                IncludedObjectTypeCodeList = new List<string> { ReferenceDataValueCodes.ObjectType.Contact },
                IncludedRelationshipTypeCodeList = new List<string> { objectRelationshipCode },
                IncludeExpiredLinks = true
            };

            var linkingRepository = RepositoryMap.LinkingRepository;
            var relationshipEntriesEnforcementContacts = linkingRepository.GetRelationshipLinks(criteria);
            var expiredEnforcementContactIRisOjbectIDs = isIncludeExpiredRelationships ? new List<long>() : relationshipEntriesEnforcementContacts.Where(r => r.CurrentTo != null).Select(r => r.LinkedIRISObject.ID);

            const int dummyLinkId = -1;
            const int includeEverythingNoFilterDummyId = -1;

            var excludeIDs = contactIDsToExclude == null ? new List<long>() : contactIDsToExclude.ToList();
            var filterIDs = contactIDsToFilterBy == null ? new List<long> { includeEverythingNoFilterDummyId } : contactIDsToFilterBy.ToList();
            var includeIDs = contactIDsToInclude == null ? new List<long>() : contactIDsToInclude.ToList();

            // Exclude

            if (excludeIDs.Any())
            {
                excludeIDs = excludeIDs.Where(r => !includeIDs.Contains(r)).ToList();  //Remove includes they take precedent.
                relationshipEntriesEnforcementContacts = relationshipEntriesEnforcementContacts.Where(r => !excludeIDs.Contains(r.LinkedIRISObject.LinkID ?? dummyLinkId)).ToList();
            }

            // Filter with includes            
            if (filterIDs.Any())
            {
                if (includeIDs.Any())
                    filterIDs.AddRange(includeIDs.Where(x => !filterIDs.Contains(x)));

                relationshipEntriesEnforcementContacts = relationshipEntriesEnforcementContacts.Where
                    (r =>
                        filterIDs.Contains(includeEverythingNoFilterDummyId)
                        ||
                        filterIDs.Contains(r.LinkedIRISObject.LinkID ?? dummyLinkId)
                    ).ToList();
            }

            // Remove remaining expired items unless in the Include list.
            relationshipEntriesEnforcementContacts = relationshipEntriesEnforcementContacts.Where
                (r =>
                    includeIDs.Contains(r.LinkedIRISObject.LinkID ?? dummyLinkId)
                    ||
                    !expiredEnforcementContactIRisOjbectIDs.Contains(r.LinkedIRISObject.ID)
                ).ToList();

            // Get related name via contact sub link
            relationshipEntriesEnforcementContacts.ForEach(relationship =>
            {
                relationship.ContactSubLink = GetContactSubLinkByActivityObjectRelationship(relationship.ActivityObjectRelationshipID, isIncludeExpiredRelationships);
            });

            return relationshipEntriesEnforcementContacts;
        }

        /// <summary>
        ///   Gets the Alleged Offender record for given a give Id
        /// </summary>
        public EnforcementAllegedOffenceOffender GetEnforcementAllegedOffenderById(long id, long enforcementIRISObjectID)
        {
            var allegedOffender = from enforcements in Context.Enforcements.Where(e => e.IRISObjectID == enforcementIRISObjectID)
                                  from offences in Context.EnforcementActionAllegedOffences.Where(e =>
                                      e.EnforcementAllegedOffence.EnforcementID == enforcements.ID)
                                  from offenders in Context.EnforcementAllegedOffenceOffenders
                                  .Include(a => a.NoActionReasonREF)
                                  .Include(a => a.ActionToBeTakenREF)
                                  .Where(e => e.ID == id && e.EnforcementAllegedOffenceID == offences.ID && !e.IsDeleted)
                                  select offenders;
            return allegedOffender.AsEnumerable().First().TrackAll();
        }

        /// <summary>
        ///   Gets the Alleged Offender record for given a give Id
        /// </summary>
        public List<EnforcementActionRecipientOrRespondent> GetRecipientsAndRespondentsByEnforcementActionIRISObjectId(long enforcementActionIRISObjectID)
        {
            var dbRecipientsRespondents = from actions in Context.EnforcementActions.Where(a => a.IRISObjectID == enforcementActionIRISObjectID)
                                          from recipientsRespondents in Context.EnforcementActionRecipientOrRespondents
                                            .Where(r => r.EnforcementActionId == actions.ID && !r.IsDeleted)
                                          select new
                                          {
                                              recipientsRespondents,
                                              recipientsRespondents.DocumentServedREF,
                                              recipientsRespondents.RecipientOrRespondentActivityObjectRelationship
                                          };

            List<EnforcementActionRecipientOrRespondent> recipientsRespondentsList =
                dbRecipientsRespondents.AsEnumerable().Select(s => s.recipientsRespondents).Distinct().ToList();

            foreach (var recipientsRespondent in recipientsRespondentsList)
            {
                // Get contact sub link and name from the linked contact sub-link.
                recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.ContactSubLink =
                    GetContactSubLinkByActivityObjectRelationship(recipientsRespondent.RecipientOrRespondentActivityObjectRelationshipID, true);

                if (recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.IsCurrent == false)
                {
                    recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.ContactName = "(NON-CURRENT) " +
                        recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.Name
                            .FormattedNameSingleInstance;
                    recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.ContactNameMultiInstance =
                        "(NON-CURRENT) " +
                        recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.Name
                            .FormattedNameMultiInstance;
                }
                else
                {
                    recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.ContactName =
                        recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.Name
                            .FormattedNameSingleInstance;
                    recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.ContactNameMultiInstance =
                        recipientsRespondent.RecipientOrRespondentActivityObjectRelationship.ContactSubLink.Name
                            .FormattedNameMultiInstance;
                }
            }

            return recipientsRespondentsList;
        }

        /// <summary>
        ///  Gets the full list of Officer Responsible for a Enforcement Action
        ///  irisObjectId - is Enforcement Action IRISObjectID
        /// </summary>
        public List<EnforcementActionOfficerResponsible> GetEnforcementActionOfficerResponsibleById(long irisObjectId)
        {
            var dbOfficer = from action in Context.EnforcementActions.Where(a => a.IRISObjectID == irisObjectId)
                            from officer in Context.EnforcementActionOfficerResponsibles
                                .Where(o => o.EnforcementActionID == action.ID && !o.IsDeleted)
                            from refUserData in Context.User.Where(s => s.ID == officer.OfficerResponsibleID)
                            select new { officer, refUserData };

            return dbOfficer.AsEnumerable().Distinct().Select(x => x.officer).ToList();
        }

        /// <summary>
        ///  Gets the full list of Alleged Offences for an Enforcement Action
        /// </summary>
        public List<EnforcementAllegedOffence> GetAllegedOffencesListByEnforcementAction(long enforcementActionIRISObjectID)
        {
            var dbOffences = from actions in Context.EnforcementActions.Where(a => a.IRISObjectID == enforcementActionIRISObjectID)
                             from actionOffences in Context.EnforcementActionAllegedOffences
                                .Where(a => a.EnforcementActionID == actions.ID && !a.IsDeleted)
                             from offences in Context.EnforcementAllegedOffences
                                .Where(o => o.ID == actionOffences.EnforcementAllegedOffenceID && !o.IsDeleted)
                             select new { offences, offences.OffenceSectionREF, offences.NatureOfOffenceREF };

            return dbOffences.AsEnumerable().Select(s => s.offences).Distinct().ToList();
        }

        /// <summary>
        ///  Gets a list of Alleged Offences for an Enforcement filtered by Act and filtered to offences for the offdenders of the enforcement action
        /// </summary>
        public List<EnforcementAllegedOffence> GetAllegedOffencesFilteredByActAndOffenders(long enforcementActionIRISObjectID)
        {
            var offendersBelongingToThisAction = from actions in Context.EnforcementActions.Where(a => a.IRISObjectID == enforcementActionIRISObjectID)
                                                 from actionOffenders in Context.EnforcementActionRecipientOrRespondents
                                                     .Where(o => o.EnforcementActionId == actions.ID && !o.IsDeleted)
                                                 from enforcementOffenders in Context.EnforcementAllegedOffenceOffenders
                                                  .Where(o => o.AllegedOffenderActivityObjectRelationshipID == actionOffenders.RecipientOrRespondentActivityObjectRelationshipID && !o.IsDeleted)
                                                 select enforcementOffenders;

            var offenderIDsToFilterBy = offendersBelongingToThisAction.Where(x => !x.IsDeleted).Select(s => s.AllegedOffenderActivityObjectRelationshipID);

            var dbOffences = from actions in Context.EnforcementActions.Where(a => a.IRISObjectID == enforcementActionIRISObjectID)
                             from enforcements in Context.Enforcements.Where(e => e.ID == actions.EnforcementID)
                             from offences in Context.EnforcementAllegedOffences
                                .Where(o => o.EnforcementID == enforcements.ID && o.IRISObject.SubClass1ID == actions.ActREFID && !o.IsDeleted)
                             from offenders in Context.EnforcementAllegedOffenceOffenders
                                .Where(o => o.EnforcementAllegedOffenceID == offences.ID && offenderIDsToFilterBy.Contains(o.AllegedOffenderActivityObjectRelationshipID) && !o.IsDeleted)
                             select new { offences, offences.OffenceSectionREF, offences.NatureOfOffenceREF };

            return dbOffences.AsEnumerable().Select(s => s.offences).Distinct().ToList();
        }

        /// <summary>
        ///  Get the parent enforcement IRISObject for a given child Enforcement Action IRISObjectID.
        ///  It's used by search indexing Enforcement when its child Enforcement Action IRISObject 
        ///  changed (the FIN Code changed).
        /// </summary>
        /// <param name="enforcementActionIRISObjectID"></param>
        /// <returns></returns>
        public long GetParentEnforcementID(long enforcementActionIRISObjectID)
        {
            return Context.EnforcementActions.Where(x => x.IRISObjectID == enforcementActionIRISObjectID).Select(x => x.EnforcementID).Single();
        }
    }
}
