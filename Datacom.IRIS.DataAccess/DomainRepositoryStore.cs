using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using System.Transactions;
using Datacom.IRIS.Common;
using Datacom.IRIS.Common.Utils;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DataAccess.Security;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data.EntityClient;
using Datacom.IRIS.DataAccess.Utils;

namespace Datacom.IRIS.DataAccess
{
    public class DomainRepositoryStore : RepositoryStore
    {
        #region Constructors

        // Default constructor will always automatically set audit fields
        public DomainRepositoryStore() : this(true) { }

        /// <summary>
        ///    Making sure that the constructor to create an entity framework context with debugging turned on
        ///    is only available when deploying IRIS in DEBUG and not in RELEASE.
        /// </summary>
#if DEBUG
        public DomainRepositoryStore(bool automaticallySetAuditFields)
            : base(GlobalUtils.GetAppSettingsValueAsBoolean("LogEFSql"))
        {
            Context.AutomaticallySetAuditFields = automaticallySetAuditFields;
        }
#else
            public DomainRepositoryStore(bool automaticallySetAuditFields) : base(false)
            {
                Context.AutomaticallySetAuditFields = automaticallySetAuditFields;
            }
#endif

        #endregion

        #region Contact

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public Contact GetContactByID(long id)
        {
            IQueryable<Contact> query = (from c in Context.Contact where c.ID == id select c);
            return GetContactFromQueryable(query);
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public Contact GetContactByIRISObjectID(long irisObjectId)
        {
            IQueryable<Contact> query = (from c in Context.Contact where c.IRISObjectID == irisObjectId select c);
            return GetContactFromQueryable(query);
        }

        private Contact GetContactFromQueryable(IQueryable<Contact> contactQueryable)
        {
            Contact contact = contactQueryable.Single();

            /*
             * StartTracking instructs the change tracker on the entity to start recording changes applied
             * to this Contact entity. All sub-entities that belong to Contact will also have its tracking 
             * turned on. This is important for us to do so that the object we return back to any system
             * is ready to be modified as it is always tracked.
             */
            contact.StartTracking();

            contact.IRISObject = (from n in Context.IRISObject where n.ID == contact.IRISObjectID select n).Single();
            AddToTrackableCollection(contact.Names, (from n in Context.Name.Include("ContactSubLinks") where n.Contact.ID == contact.ID && !n.IsDeleted select n).ToList());
            AddToTrackableCollection(contact.ContactAddresses, (from n in Context.ContactAddress.Include("Address").Include("ContactSubLinks") where n.Contact.ID == contact.ID && !n.IsDeleted select n).ToList());
            AddToTrackableCollection(contact.PhoneNumbers, (from n in Context.PhoneNumber.Include("ContactSubLinks") where n.Contact.ID == contact.ID && !n.IsDeleted select n).ToList());
            AddToTrackableCollection(contact.Emails, (from n in Context.Email.Include("ContactSubLinks") where n.Contact.ID == contact.ID && !n.IsDeleted select n).ToList());
            AddToTrackableCollection(contact.Websites, (from n in Context.Website.Include("ContactSubLinks") where n.Contact.ID == contact.ID && !n.IsDeleted select n).ToList());

            return contact;
        }

        #endregion

        #region Common Elements (e.g. Notes, IRISObjects, Favourites, IrisMessages, etc)

        /// <summary>
        ///    This is a utility method that is used by the business layer for validation purposes.
        ///    It will not be exposed publicly outside the business layer, and hence it has no security
        ///    check attributes
        /// </summary>
        public List<IRISObject> GetIrisObjects(params long[] idValues)
        {
            return (from n in Context.IRISObject.WhereIn(i => i.ID, idValues) select n).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterNoteAccess")]
        public Note GetNoteByID(long id)
        {
            return (from n in Context.Note.Include("IRISObject") where n.ID == id select n).SingleOrDefault().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterNotesAccess")]
        public List<Note> GetNotesByID(long irisObjectId)
        {
            return (from n in Context.Note.Include("IRISObject") where n.IRISObjectID == irisObjectId orderby n.DateCreated descending select n).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public Dictionary<string, string> GetIrisMessages()
        {
            return (from i in Context.IRISMessage select i).ToDictionary(u => u.Code, u => u.Message);
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Favourite> GetFavouritesByUser(string loginName)
        {
            return (from n in Context.Favourite where n.UserLoginName == loginName && !n.IsDeleted select n).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<AppSetting> GetAllAppSettings()
        {
            return Context.AppSetting.ToList().Track();
        }

        #endregion

        #region Data Security

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterDataSecurityListAccess")]
        public List<DataSecurity> GetDataSecurityListById(long irisObjectId)
        {
            return (from n in Context.DataSecurity.Include("IRISObject").Include("User")
                    where n.IRISObjectID == irisObjectId && !n.IsDeleted
                    orderby n.DateCreated descending
                    select n).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterDataSecurityAccess")]
        public DataSecurity GetDataSecurityById(long id)
        {
            return (from n in Context.DataSecurity.Include("IRISObject") where n.ID == id select n).SingleOrDefault().Track();
        }

        #endregion

        #region Location and Location Groups

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        public LocationGroup GetLocationGroupByID(long id)
        {
            var dbquery = from entity in Context.LocationGroup.Where(l => l.ID == id && !l.IsDeleted)
                          select new
                          {
                              entity,
                              irisObjectId = from i in Context.IRISObject select i
                          };

            return dbquery.AsEnumerable().Select(c => c.entity).Single().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public Location GetLocationByID(long id)
        {
            Location location = Context.Location.Include("IRISObject").Single(l => l.ID == id).Track();
            location.LocationOtherIdentifiers.AddTrack((from n in Context.LocationOtherIdentifier where n.LocationID == location.ID && !n.IsDeleted select n).ToList());

            if (location.IsTextual)
            {
                ((TextualLocation)location).Address = Context.Address.Single(a => a.ID == ((TextualLocation)location).AddressID).Track();
            }

            return location;
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Location> GetSpatialLocationsByIrisObjectIds(params long[] irisObjectIdValues)
        {
            return Context.Location.Include("IRISObject").WhereIn(i => i.IRISObjectID, irisObjectIdValues).ToList().Track();
        }

        #endregion

        #region Reference Data and Activity Relationship types

        /*
         * It is important that a single a collection has its entired hierarchy populated. Hence
         * we fetch all collections (expensive call) and then return only the collection we are
         * interested in to ensure all navigation properties are populated properly.
         */
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public ReferenceDataCollection GetReferenceDataCollectionById(long id)
        {
            List<ReferenceDataCollection> referenceDataCollections = GetReferenceDataCollections();
            return referenceDataCollections.Single(c => c.ID == id).Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<ReferenceDataCollection> GetReferenceDataCollections()
        {
            /*
             * Because ObjectQuery<T>.Include(...) does not support conditional selection,
             * we run 2 queries instead - (a) all data collections (b) active data values only
             * 
             * The second query then uses AsEnumerable which 'unwraps' the refdatacollection
             * from its anonymous type warpper. EF then runs 'relationship fix-up' which ensures
             * that related objects loaded in the object context are linked automatically.
             */
            var dbquery = from refDataCollection in Context.ReferenceDataCollection
                          select new
                          {
                              refDataCollection,
                              refDataValues = from refDataValue in Context.ReferenceDataValue orderby refDataValue.DisplayOrder ascending select refDataValue,
                              refDataAttributes = from redDataAttribute in Context.ReferenceDataValueAttribute select redDataAttribute
                          };

            return dbquery.AsEnumerable().Select(c => c.refDataCollection).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public ActivityObjectRelationshipType GetActivityObjectRelationshipTypeById(long id)
        {
            return Context.ActivityObjectRelationshipType.SingleOrDefault(c => c.ID == id).Track();
        }


        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<ReferenceDataValueAttribute> GetAttributesForObjectType(long objectTypeId)
        {
            return Context.ReferenceDataValueAttribute.Where(a => a.ValueID == objectTypeId).ToList();

        }

        #endregion

        #region AppSetting

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public AppSetting GetAppSettingById(long id)
        {
            return (from n in Context.AppSetting where n.ID == id select n).SingleOrDefault().Track();
        }

        #endregion

        #region Authorisations

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        public Application GetApplicationByID(long id)
        {
            IQueryable<Application> applications = Context.Application.Where(l => l.ID == id);
            return GetApplicationFromQueryable(applications);
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        public Application GetApplicationByIRISObjectID(long irisObjectID)
        {
            IQueryable<Application> applications = Context.Application.Where(l => l.IRISObjectID == irisObjectID);
            return GetApplicationFromQueryable(applications);
        }

        private Application GetApplicationFromQueryable(IQueryable<Application> applications)
        {
            var dbQuery = from entity in applications
                          select new
                                     {
                                         entity,
                                         irisObject = from i in Context.IRISObject where i.ID == entity.IRISObjectID select i,
                                         applicationStatuses = from i in Context.ApplicationStatus where i.ApplicationID == entity.ID select i,
                                         extendedWorkingDays = from i in Context.ExtendedWorkingDays where i.ApplicationID == entity.ID select i
                                     };

            Application application = dbQuery.AsEnumerable().Select(c => c.entity).Single();
            application.ServiceContact = GetContactByID(application.ServiceContactID);
            application.StartTracking();
            return application;
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        public Activity GetActivityByID(long id)
        {
            return (from n in Context.Activity.Include("IRISObject") where n.ID == id select n).SingleOrDefault().Track();
        }

        #endregion

        #region CDF Lists

        /// <summary>
        ///    This method is typically called by Admin backend screens that require a listing of all
        ///    CDF Lists that have been created in the system. Associated Questions and Answers are 
        ///    not retrieved.
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.CustomFields, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<CDFList> GetCDFLists()
        {
            return Context.CDFList.Where(c => !c.IsDeleted)
                .OrderBy(c => c.ObjectTypeID).ThenBy(c => c.SubClass1ID).ThenBy(c => c.SubClass2ID)
                .ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterCDFListsAccess")]
        public List<CDFList> GetCDFLists(long irisObjectID, long objectTypeID, long? subClass1ID, long? subClass2ID)
        {
            var cdfListsQuery = Context.CDFList
                                    .Where(c => !c.IsDeleted)
                                    .Where(c =>
                                        (c.ObjectTypeID == objectTypeID && c.SubClass1ID == null && c.SubClass2ID == null) ||
                                        (c.ObjectTypeID == objectTypeID && c.SubClass1ID == subClass1ID && c.SubClass2ID == null) ||
                                        (c.ObjectTypeID == objectTypeID && c.SubClass1ID == subClass1ID && c.SubClass2ID == subClass2ID)
                                    );

            var dbquery = from cdfLists in cdfListsQuery
                          select new
                          {
                              cdfLists,
                              questions = from question in Context.QuestionDefinition
                                                                    .Where(q => !q.IsDeleted)
                                                                    .OrderBy(q => q.OrderNumber)
                                          select question,
                              answers = from answer in Context.Answer.Where(a => a.IRISObjectID == irisObjectID && !a.IsDeleted).OrderBy(a => a.Row) select answer
                          };

            return dbquery.AsEnumerable().Select(c => c.cdfLists).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterCDFListAccess")]
        public CDFList GetCDFList(long cdfListID, long irisObjectID)
        {
            var dbquery = from cdfList in Context.CDFList.Where(c => c.ID == cdfListID && !c.IsDeleted)
                          select new
                          {
                              cdfList,
                              questions = from question in Context.QuestionDefinition
                                                                    .Where(q => q.CDFListID == cdfListID && !q.IsDeleted)
                                                                    .OrderBy(q => q.OrderNumber)
                                          select question,
                              answers = from answer in Context.Answer.Where(a => a.IRISObjectID == irisObjectID && !a.IsDeleted).OrderBy(a => a.Row) select answer
                          };

            return dbquery.AsEnumerable().Select(c => c.cdfList).Single(c => c.ID == cdfListID).Track();
        }

        /// <summary>
        ///    This method is typically called by Admin backend screens. It will NOT include any Answer objects
        ///    inside any of the Question Definitions because an IRIS Object ID has not been passed. Use this
        ///    method typically to get an idea of what question definitions exist inside a given CDF List.
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.CustomFields, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public CDFList GetCDFList(long cdfListID)
        {
            var dbquery = from cdfList in Context.CDFList.Where(c => c.ID == cdfListID && !c.IsDeleted)
                          select new
                          {
                              cdfList,
                              questions = from question in Context.QuestionDefinition
                                            .Where(q => q.CDFListID == cdfListID && !q.IsDeleted)
                                            .OrderBy(q => q.OrderNumber)
                                          select question
                          };

            return dbquery.AsEnumerable().Select(c => c.cdfList).Single(c => c.ID == cdfListID).Track();
        }

        #endregion

        #region Business ID method(s)

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public string GetNextBusinessID(long objectTypeID)
        {
            IRISBusinessIDPattern businessIdPatternEntity = null;

            bool success = TransactionScopedQuery(() =>
            {
                businessIdPatternEntity = Context.IRISBusinessIDPattern.SingleOrDefault(p => p.ObjectTypeID == objectTypeID);
                if (businessIdPatternEntity != null)
                {
                    businessIdPatternEntity.Counter += 1;
                    ApplyEntityChanges(businessIdPatternEntity);
                }
            });

            return success && businessIdPatternEntity != null ? businessIdPatternEntity.GenerateBusinessID() : null;
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public bool IsBusinessIDUnique(long currentIrisID, string businessID)
        {
            return (from i in Context.IRISObject where i.BusinessID == businessID && i.ID != currentIrisID select i).Count() == 0;
        }

        #endregion

        #region Linking retrieval methods

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<ActivityObjectRelationshipType> GetAllActivityObjectRelationshipTypes()
        {
            return Context.ActivityObjectRelationshipType.ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public ActivityObjectRelationship GetRelationshipDetailsByID(long id)
        {
            return (from a in Context.ActivityObjectRelationship
                        .Include("ActivityObjectRelationshipType")
                        .Include("IRISObject")
                        .Include("RelatedIRISObject")
                        .Include("ContactSubLink")
                    where a.ID == id
                    select a).SingleOrDefault().Track();
        }

        //No need to protect this method at it's used internally only
        public ILookup<long, KeyValuePair<long, ActivityObjectRelationship>> GetRelatedIrisObjects(long irisObjectId, long[] objectTypeIDList)
        {
            var linq = from activityRelationship in Context.ActivityObjectRelationship
                       join irisObject in Context.IRISObject.WhereIn(e => e.ObjectTypeID, objectTypeIDList) on activityRelationship.RelatedIRISObjectID equals irisObject.ID
                       where activityRelationship.IRISObjectID == irisObjectId
                       select new { irisObject, activityRelationship };

            var linq2 = from activityRelationship in Context.ActivityObjectRelationship
                        join irisObject in Context.IRISObject.WhereIn(e => e.ObjectTypeID, objectTypeIDList) on activityRelationship.IRISObjectID equals irisObject.ID
                        where activityRelationship.RelatedIRISObjectID == irisObjectId
                        select new { irisObject, activityRelationship };

            return linq.Concat(linq2).ToLookup(k => k.irisObject.ObjectTypeID, k => new KeyValuePair<long, ActivityObjectRelationship>(k.irisObject.ID, k.activityRelationship));
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        public List<LinkContactInformation> GetContactLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {
            IEnumerable<long> irisObjectIds = idsAndRelationships.Select(idr => idr.Key);
            var dbquery = from contacts in Context.Contact.Include("IRISObject").Where(c => irisObjectIds.Contains(c.IRISObjectID))
                          select new
                          {
                              contacts,
                              names = from name in Context.Name.Where(n => n.ContactID == contacts.ID && n.IsPreferred) select name
                          };

            return dbquery.AsEnumerable().Select(c => c.contacts).Select(contact => new LinkContactInformation
            {
                Name = contact.Names.Single(),
                ObjectID = contact.ID,
                IRISObjectID = contact.IRISObjectID,
                IRISObject = contact.IRISObject,
                WarningComments = contact.WarningComments
            }).ToList();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        public List<LinkInformation> GetLocationLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {
            IEnumerable<long> ids = idsAndRelationships.Select(idr => idr.Key);
            return (from n in Context.Location.Include("IRISObject").Include("TextualLocation").Include("SpatialLocation")
                    where ids.Contains(n.IRISObjectID)
                    select new LinkInformation
                    {
                        Value = n.CommonName,
                        ObjectID = n.ID,
                        IRISObjectID = n.IRISObjectID,
                        IRISObject = n.IRISObject,
                        WarningComments = n.WarningComments
                    }).ToList();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Default)]
        public List<LinkInformation> GetLocationGroupLinkInformation(IEnumerable<KeyValuePair<long, ActivityObjectRelationship>> idsAndRelationships)
        {
            IEnumerable<long> ids = idsAndRelationships.Select(idr => idr.Key);
            return (from n in Context.LocationGroup.Include("IRISObject")
                    where ids.Contains(n.IRISObjectID)
                    select new LinkInformation
                    {
                        Value = n.CommonName,
                        ObjectID = n.ID,
                        IRISObjectID = n.IRISObjectID,
                        IRISObject = n.IRISObject,
                        WarningComments = n.WarningComments
                    }).ToList();
        }

        #endregion

        #region User Setup Methods

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public User GetUserAndDelegations(string accountName)
        {
            User user = (from u in Context.User where u.AccountName == accountName select u).SingleOrDefault();

            if (user == null)
            {
                user = new User { AccountName = accountName };
            }
            else
            {
                user.StartTracking();
                user.Delegations.AddTrack((from d in Context.Delegation where d.UserID == user.ID && !d.IsDeleted select d).ToList());
            }
            return user;
        }

        #endregion

        #region Tasks Methods

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Task> GetTaskList(long irisObjectID)
        {
            IQueryable<Task> queryable = Context.Task.Where(c => c.IRISObjectID == irisObjectID);
            return GetTaskListFromQueryable(queryable);
        }

        /// <summary>
        /// Gets a task list for a specified user (My Tasks)
        /// </summary>
        /// <param name="username">The full username including domain (e.g. DOMAIN\\User)</param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Task> GetTaskList(string username)
        {
            IQueryable<Task> queryable = Context.Task.Where(c => c.AssignedTo == username);
            return GetTaskListFromQueryable(queryable);
        }

        /// <summary>
        /// Gets a task list for a specified users colleagues' (Colleagues' Tasks)
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="groupId">The group Id </param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Task> GetTasksForColleagues(long userId, string groupId)
        {
            string accountName = GetUserById(userId).AccountName;
            long selectedGroupId = groupId.ParseAsLong();
            IQueryable<Task> queryable;

            if (selectedGroupId == 0)
            {
                List<long> relevantGroupIds = GetGroupsByUserId(userId).Select(c => c.ID).ToList();

                var selectedUsers = from userList in GetUserList()
                                    join userGroup in Context.UserGroup on userList.ID equals userGroup.UserID
                                    where relevantGroupIds.Contains(userGroup.GroupID)
                                    select new { userList };

                List<string> selectedUserNames = selectedUsers.AsEnumerable().Select(c => c.userList.AccountName).ToList();
                queryable = Context.Task.Where(c => c.AssignedTo != accountName && selectedUserNames.Contains(c.AssignedTo));
            }

            else
            {
                var selectedUsers = from userList in GetUserList()
                                    join userGroup in Context.UserGroup on userList.ID equals userGroup.UserID
                                    where userGroup.GroupID == selectedGroupId
                                    select new { userList };

                List<string> selectedUserNames = selectedUsers.AsEnumerable().Select(c => c.userList.AccountName).ToList();
                queryable = Context.Task.Where(c => c.AssignedTo != accountName && selectedUserNames.Contains(c.AssignedTo));
            }

            return GetTaskListFromQueryable(queryable);
        }

        /// <summary>
        /// Gets a task list for a specified users team (Team Tasks)
        /// </summary>
        /// <param name="username">The full username including domain (e.g. DOMAIN\\User)</param>
        ///  <param name="groupId">The group Id </param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Task> GetTasksForTeam(long userId, string groupId)
        {
            long selectedGroupId = groupId.ParseAsLong();
            IQueryable<Task> queryable;
            if (selectedGroupId == 0)
            {
                List<string> relevantGroupNames = GetGroupsByUserId(userId).Select(c => c.Name).ToList();
                queryable = Context.Task.Where(c => relevantGroupNames.Contains(c.AssignedTo));
            }
            else
            {
                var selectedGroup = GetGroupById(selectedGroupId);
                queryable = Context.Task.Where(c => c.AssignedTo == selectedGroup.Name);
            }
            return GetTaskListFromQueryable(queryable);
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<User> GetTaskUsers(List<long> objectTypeIds)
        {

            List<Group> allGroups = GetTaskGroups(objectTypeIds);
            List<User> allUsers = new List<User>();

            foreach (Group grp in allGroups)
            {
                List<User> selectedUsers = (from users in Context.User
                                            where users.UserGroups.Select(g => g.GroupID).Contains(grp.ID)
                                            select users).ToList();

                allUsers = objectTypeIds.Count > 1 ? allUsers.IntersectBy(selectedUsers, u => u.ID).ToList() : allUsers.Concat(selectedUsers).ToList();
            }
            return allUsers.DistinctBy(u => u.ID).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Group> GetTaskGroups(List<long> objectTypeIds)
        {
            List<Group> allGroups = new List<Group>();

            foreach (long objectId in objectTypeIds)
            {
                List<Group> selectedGroups = (from groups in Context.Group
                                              where groups.GroupPermissions.Where(g => g.Function.ObjectID == objectId).Select(g => g.Permission.Name).Contains(PermissionNames.Edit)
                                              select groups).ToList();

                allGroups = objectTypeIds.Count > 1 ? allGroups.IntersectBy(selectedGroups, g => g.ID).ToList() : allGroups.Concat(selectedGroups).ToList();
            }
            return allGroups.DistinctBy(g => g.ID).ToList().Track();
        }

        /// <summary>
        /// Gets all tasks (All Tasks)
        /// </summary>
        /// <param name="username">The full username including domain (e.g. DOMAIN\\User)</param>
        /// <param name="groupId">The group Id </param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Task> GetTasksForAll(long userId, string groupId)
        {
            var user = GetUserById(userId);
            return GetTaskList(user.AccountName).Concat(GetTasksForColleagues(userId, groupId)).Concat(GetTasksForTeam(userId, groupId)).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public Task GetTaskByID(long taskID)
        {
            IQueryable<Task> queryable = Context.Task.Where(c => c.ID == taskID);
            return GetTaskFromQueryable(queryable);
        }

        private Task GetTaskFromQueryable(IQueryable<Task> queryable)
        {
            var dbquery = from tasks in queryable
                          select new
                           {
                               tasks,
                               taskDefinitions = from taskDefinition in Context.TaskDefinition select taskDefinition,
                               irisObjects = from irisObject in Context.IRISObject select irisObject,
                               workflows = from workflow in Context.Workflow select workflow,
                               workflowDefinitions = from workflowDefinition in Context.WorkflowDefinition select workflowDefinition,
                           };

            return dbquery.AsEnumerable().Select(c => c.tasks).FirstOrDefault().Track();
        }

        private List<Task> GetTaskListFromQueryable(IQueryable<Task> queryable)
        {
            var dbquery = from tasks in queryable
                          select new
                          {
                              tasks,
                              taskDefinitions = from taskDefinition in Context.TaskDefinition select taskDefinition,
                              irisObjects = from irisObject in Context.IRISObject select irisObject,
                              workflows = from workflow in Context.Workflow select workflow,
                              workflowDefinitions = from workflowDefinition in Context.WorkflowDefinition select workflowDefinition,
                          };

            return dbquery.AsEnumerable().Select(c => c.tasks).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Task> GetTaskListByIDs(List<long> taskIDs)
        {
            IQueryable<Task> queryable = Context.Task.Where(c => taskIDs.Contains(c.ID));
            return GetTaskListFromQueryable(queryable);
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public TaskDefinition GetTaskDefinitionByID(long taskDefinitionID)
        {
            return Context.TaskDefinition.Single(td => td.ID == taskDefinitionID).Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public WorkflowDefinition GetWorkflowDefinitionByID(long workflowDefinitionID)
        {
            return Context.WorkflowDefinition.Single(td => td.ID == workflowDefinitionID).Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<WorkflowDefinition> GetWorkflowDefinitionList(bool enabledOnly)
        {
            return (from w in Context.WorkflowDefinition where !enabledOnly || w.IsEnabledFlag select w).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public Workflow GetWorkflowByGuid(Guid workflowInstanceGUID)
        {
            return Context.Workflow.Include("Task").Single(t => t.WorkflowInstanceUID == workflowInstanceGUID).Track();
        }
        #endregion

        #region Business Validation methods

        // A location group must always have a unique name. Hence this method performs check
        public bool IsLocationGroupCommonNameUnique(LocationGroup locationGroup)
        {
            var matchedLocationGroups = (from i in Context.LocationGroup
                                         where i.CommonName.ToLower() == locationGroup.CommonName.ToLower()
                                         && i.ID != locationGroup.ID // if locationGroup is an existing one, the select not include itself. (used when modifying a location group)
                                         select i);
            return matchedLocationGroups.Count() == 0;
        }

        public bool IsFavoriteTitleUnique(Favourite favourite)
        {
            return (from i in Context.Favourite where i.IsDeleted == false && i.Title.ToLower() == favourite.Title.ToLower() && i.UserLoginName == favourite.UserLoginName select i).Count() == 0;
        }

        /// <summary>
        ///    An activity relationship is bi-directional for each relationship type. Hence this method performs check. 
        ///    In the meanwhile it also check if the found relationship is currently valid. It will return the 
        ///    relationshipID if it exists.
        /// </summary>
        public ActivityObjectRelationship HasExistingCurrentActivityRelationship(long irisObjectId, long relatedIrisObjectId, long relationshipTypeId)
        {
            return (from i in Context.ActivityObjectRelationship
                    where
                        ((i.IRISObjectID == irisObjectId && i.RelatedIRISObjectID == relatedIrisObjectId)
                        || (i.IRISObjectID == relatedIrisObjectId && i.RelatedIRISObjectID == irisObjectId)) &&
                        i.ActivityObjectRelationshipTypeID == relationshipTypeId &&
                        !i.CurrentTo.HasValue
                    select i).SingleOrDefault();
        }

        public bool HasExistingCurrentActivityRelationshipType(long id, long? objectTypeId, long? relatedObjectId, string relationship)
        {
            if (objectTypeId == null && relatedObjectId == null)
            {

                var blah = (from i in Context.ActivityObjectRelationshipType
                            where
                                i.ObjectTypeID == null && i.RelatedObjectTypeID == null &&
                                i.Relationship == relationship && i.ID != id
                            select i).FirstOrDefault();

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

        #endregion

        #region SQL Search

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public SearchResultsIndices GetSearchResults(long searchHeaderID, int maximumRows, int startPageIndex, int startRowIndex, string sortExpression)
        {
            SearchResultsIndices results = new SearchResultsIndices(searchHeaderID);

            ObjectParameter searchCount = new ObjectParameter("search_count", typeof(int));
            var tempResult = Context.SelectSearchResultsByPage(searchHeaderID, sortExpression, maximumRows, (short)(startPageIndex + 1), searchCount);
            results.SearchIndices = tempResult.ToList();
            results.Count = (int)(searchCount.Value);
            return results;
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public long CreateSearch(string searchCriteria, long? scopeID, string userAccountName, bool isSpatialSearch, string spatialSearchResult)
        {
            ObjectParameter searchHeaderID = new ObjectParameter("SearchHeaderID", typeof(long));

            if (scopeID.HasValue)
            {
                Context.ExecuteFunction("CreateSearch", new ObjectParameter[]
                { 
                    new ObjectParameter("SearchCriteria", searchCriteria), 
                    new ObjectParameter("ScopeID", scopeID), 
                    new ObjectParameter("UserAccountName", userAccountName), 
                    new ObjectParameter("IsSpatialSearch", isSpatialSearch),
                    searchHeaderID
                });
            }
            else
            {
                Context.ExecuteFunction("CreateSearch", new ObjectParameter[]
                { 
                    new ObjectParameter("SearchCriteria", searchCriteria), 
                    new ObjectParameter("UserAccountName", userAccountName), 
                    new ObjectParameter("IsSpatialSearch", isSpatialSearch),
                    searchHeaderID
                });
            }

            if (isSpatialSearch)
            {
                Context.ExecuteFunction("AddSpatialResult", new ObjectParameter[]
                                                             {
                                                                 searchHeaderID,
                                                                 new ObjectParameter("SpatialIDs", spatialSearchResult)
                                                             });
            }

            return (long)searchHeaderID.Value;
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public void ExecuteSearch(long searchHeaderID)
        {
            Context.ExecuteFunction("RunSearch", new ObjectParameter[]
            { 
                new ObjectParameter("SearchHeaderID", searchHeaderID), 
            });

        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public SearchResultsIndices ExecuteAdvanceSearch(long searchID, Dictionary<string, string> advancedConditions, string scope, List<long> irisObjectIDs, int maximumRows, bool spatialFilter)
        {

            if (scope == "Contact")
            {
                Context.ExecuteFunction("SearchAdvancedContact", new ObjectParameter[]
                                                             {
                                                                 new ObjectParameter("SearchID", searchID), 
                                                                 CreateConditionObjectParameter<string>("ContactType", advancedConditions),
                                                                 CreateConditionObjectParameter<string>("FirstName", advancedConditions),
                                                                 CreateConditionObjectParameter<string>("LastName", advancedConditions),
                                                                 CreateConditionObjectParameter<DateTime>("DOBFrom", advancedConditions),
                                                                 CreateConditionObjectParameter<DateTime>("DOBTo", advancedConditions),
                                                                 CreateConditionObjectParameter<string>("EmailAdd", advancedConditions),
                                                                 CreateConditionObjectParameter<string>("PhoneNum", advancedConditions),
                                                             });
            }

            return GetSearchResults(searchID, maximumRows, 0, 0, string.Empty);
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public void IndexSearchableObject(long searchableObjectID, string objectType)
        {
            Context.ExecuteFunction("ReIndexObject", new ObjectParameter[]
                                                             {
                                                                 new ObjectParameter("objectID", searchableObjectID), 
                                                                 new ObjectParameter("objectType", objectType), 
                                                             });
        }

        /// <summary>
        ///  Pass the IRISObjectID into the ReIndexSearchIndexSpatialID stored proc;
        ///  will update the SpatialID column of the SearchIndex of this Object.
        /// </summary>
        /// <param name="irisObjectID"></param>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public void ReIndexSearchIndexSpatialID(long irisObjectID)
        {
            Context.ExecuteFunction("ReIndexSearchIndexSpatialID", new ObjectParameter[]
                                                             {
                                                                 new ObjectParameter("IRISObjectID", irisObjectID), 
                                                             });
        }

        /// <summary>
        ///  For unit test purpose only
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="objectTypeID"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public SearchIndex GetSearchIndex(long objectID, long objectTypeID)
        {
            return Context.SearchIndex.SingleOrDefault(si => si.ObjectID == objectID && si.ObjectTypeID == objectTypeID);
        }

        /// <summary>
        ///  For unit test purpose only
        /// </summary>
        /// <param name="searchIndexID"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<SearchKeyword> GetSearchKeyword(long searchIndexID)
        {
            return Context.SearchKeyword.Where(sk => sk.SearchIndexID == searchIndexID).ToList();
        }

        /// <summary>
        ///  For unit test purpose only
        /// </summary>
        /// <param name="searchIndexID"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public SearchHeader GetSearchHeader(long searchHeaderID)
        {
            return Context.SearchHeader.SingleOrDefault(sh => sh.SearchHeaderID == searchHeaderID);
        }

        /// <summary>
        ///  For unit test purpose only
        /// </summary>
        /// <param name="searchIndexID"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<SearchTerm> GetSearchTerm(long searchHeaderID)
        {
            return Context.SearchTerm.Where(st => st.SearchHeaderID == searchHeaderID).ToList();
        }

        /// <summary>
        ///  For unit test purpose only
        /// </summary>
        /// <param name="searchIndexID"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<SearchResultSpatial> GetSearchResultSpatial(long searchHeaderID)
        {
            return Context.SearchResultSpatial.Where(srs => srs.SearchHeaderID == searchHeaderID).ToList();
        }

        private ObjectParameter CreateConditionObjectParameter<T>(string paramName, Dictionary<string, string> conditions)
        {
            ObjectParameter newParam = new ObjectParameter(paramName, typeof(T));

            try
            {
                newParam.Value = conditions[paramName];
            }
            catch (Exception ex) { }

            return newParam;
        }

        #endregion

        #region Security

        // TODO: Pagination support?
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.AdministrationAccess, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<User> GetUserList()
        {
            return Context.User.ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<User> GetUsersByGroupId(long id)
        {
            var dbquery = from userGroupList in Context.UserGroup.Where(c => c.GroupID == id)
                          select new
                          {
                              userGroupList,
                              users = from users in Context.User.Where(u => u.ID == userGroupList.UserID) select users
                          };

            return dbquery.AsEnumerable().SelectMany(ul => ul.users).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Permission> GetPermissionList()
        {
            return Context.Permission.ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public Permission GetPermissionById(long id)
        {
            var dbquery = from permission in Context.Permission.Where(p => p.ID == id)
                          select new
                          {
                              permission,
                              groupPermissions = from groupPermission in Context.GroupPermission.Where(gp => gp.PermissionID == id) select groupPermission
                          };

            return dbquery.AsEnumerable().Select(p => p.permission).Single(p => p.ID == id).Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Group> GetGroupList()
        {
            return (from a in Context.Group.Include("UserGroups").Where(g => !g.IsDeleted) select a).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Group> GetGroupsByUserId(long userId)
        {
            var groupsForUser = from groups in GetGroupList()
                                join userGroup in Context.UserGroup on groups.ID equals userGroup.GroupID
                                join userList in GetUserList() on userGroup.UserID equals userList.ID
                                where userList.ID == userId
                                select new { groups };

            return groupsForUser.AsEnumerable().Select(g => g.groups).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public Group GetGroupById(long id)
        {
            var dbquery = from groups in Context.Group.Where(g => g.ID == id)
                          select new
                          {
                              groups,
                              usergroups = from userGroup in Context.UserGroup.Where(ug => !ug.IsDeleted) select userGroup,
                              groupPermissions = from groupPermission in Context.GroupPermission.Where(gp => !gp.IsDeleted) select groupPermission,
                              functions = from function in Context.Function.Where(f => !f.IsDeleted) select function,
                              permissions = from permission in Context.Permission select permission,
                              users = from user in Context.User select user
                          };
            return dbquery.AsEnumerable().Select(p => p.groups).Single(g => g.ID == id).Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Function> GetFunctionList()
        {
            return Context.Function.Where(f => !f.IsDeleted).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<Function> GetFunctionsByGroupId(long id)
        {
            var dbquery = from groupPermissionList in Context.GroupPermission.Where(c => c.GroupID == id)
                          select new
                          {
                              groupPermissionList,
                              functions = from functions in Context.Function.Where(f => f.ID == groupPermissionList.FunctionID && f.IsDeleted != true) select functions
                          };

            return dbquery.AsEnumerable().SelectMany(fl => fl.functions).ToList().Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public Function GetFunctionById(long id)
        {
            var dbquery = from function in Context.Function.Where(p => p.ID == id)
                          select new
                          {
                              function,
                              groupPermissions = from groupPermission in Context.GroupPermission.Where(gp => gp.FunctionID == id && !gp.IsDeleted) select groupPermission,
                              groups = from groups in Context.Group.Where(g => !g.IsDeleted) select groups,
                              permissions = from permission in Context.Permission select permission,
                          };

            return dbquery.AsEnumerable().Select(p => p.function).Single(p => p.ID == id).Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public User GetUserByAccountName(string accountName)
        {
            var dbquery = from userList in Context.User.Where(c => c.AccountName == accountName)
                          select new
                          {
                              userList,
                              userGroups = from userGroup in Context.UserGroup.Where(ug => ug.UserID == userList.ID) select userGroup
                          };

            return dbquery.AsEnumerable().Select(c => c.userList).Single(c => c.AccountName == accountName).Track();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<FunctionObjectPermission> GetFunctionObjectPermissionList()
        {
            return Context.FunctionObjectPermission.Include("Function").Include("Permission").ToList().Track();
        }

        /// <summary>
        ///    Gets a user as well as all functions/permissions/groups associated with that user
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public User GetUserAndPermissionsByAccountName(string accountName)
        {
            IQueryable<User> queryable = Context.User.Where(c => c.AccountName.ToLower() == accountName.ToLower());
            return GetUserFromQueryable(queryable);
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public User GetUserById(long id)
        {
            IQueryable<User> queryable = Context.User.Where(c => c.ID == id);
            return GetUserFromQueryable(queryable);
        }

        private User GetUserFromQueryable(IQueryable<User> queryable)
        {
            var dbquery = from userList in queryable
                          select new
                          {
                              userList,
                              userGroups = from userGroup in Context.UserGroup.Where(ug => ug.UserID == userList.ID) select userGroup,
                              groupPermissions = from groupPermission in Context.GroupPermission.Where(gp => !gp.IsDeleted) select groupPermission,
                              functions = from function in Context.Function.Where(f => !f.IsDeleted) select function,
                              permissions = from permission in Context.Permission select permission,
                              groups = from groups in Context.Group.Where(g => !g.IsDeleted) select groups
                          };

            return dbquery.AsEnumerable().Select(c => c.userList).FirstOrDefault().Track();
        }
        /// <summary>
        /// Returns list of users that have atleast one of the pemissions provided in the permissions list.
        /// </summary>
        /// <param name="objectTypeId"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<User> GetUsersWithPermissionToObjectType(long objectTypeId, List<string> permissions)
        {
            var dbquery = from userList in GetUserList()
                          join userGroup in Context.UserGroup on userList.ID equals userGroup.UserID
                          join groupPermission in Context.GroupPermission on userGroup.GroupID equals
                              groupPermission.GroupID
                          join function in Context.Function on groupPermission.FunctionID equals function.ID
                          join permission in Context.Permission on groupPermission.PermissionID equals permission.ID
                          where function.ObjectID == objectTypeId && (permissions.Contains(permission.Name))
                          select new { userList };

            var user = dbquery.AsEnumerable().Select(c => c.userList).Distinct().ToList().Track();
            return user;
        }

        #endregion

        #region Document Generation

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]//Everyone has view permission for contacts
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterContactSubLinksListAccess")]
        public List<ContactSubLink> GetContactsSublinksByIRISObjectID(long irisObjectID)
        {
            //We need to do 2 queries as linq does not support joining with OR conditions
            var query1 = from subLink in Context.ContactSubLink
                         join act in Context.ActivityObjectRelationship on subLink.ActivityObjectRelationshipID equals act.ID
                         where act.IRISObjectID == irisObjectID && !act.CurrentTo.HasValue
                         select subLink;

            var query2 = from subLink in Context.ContactSubLink
                         join act in Context.ActivityObjectRelationship on subLink.ActivityObjectRelationshipID equals act.ID
                         where act.RelatedIRISObjectID == irisObjectID && !act.CurrentTo.HasValue
                         select subLink;

            return GetSubLinksFromQueryable(query1.Concat(query2));

            //var linq = from sublink in query1.Concat(query2)
            //           select new
            //           {
            //               sublink,
            //               contacts = from contact in Context.Contact where contact.ID == sublink.ContactID select contact,
            //               irisObjects = from i in Context.IRISObject select i,
            //               names = from name in Context.Name where !name.IsDeleted select name,
            //               contactAddresses = from contactAddress in Context.ContactAddress.Include("Address").Include("ContactSubLinks") where !contactAddress.IsDeleted select contactAddress,
            //               emails = from email in Context.Email.Include("ContactSubLinks") where !email.IsDeleted select email,
            //               addresses = from address in Context.Address select address,
            //          };
            //return linq.AsEnumerable().Select(c => c.sublink).ToList();

        }


        /// <summary>
        /// Retrieves a list of contact based on a Predefined contact query
        /// The navigation properties of the contacts returned are not fully populated:
        /// Only IRISObject, Name,addresses and emails are populated
        /// </summary>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]//Everyone has view permission for contacts
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterContactSubLinksListAccess")]
        public List<ContactSubLink> GetContactsSublinksByPredefinedQuery(long predefinedQueryID, long irisObjectID)
        {
            //We first retrieve the name of the SP to call based on the query ID, then we call it
            //to retrieve a list of contact IDs
            //We then populate the contact objects normally, using linq
            var q = from contactQuery in Context.ContactQuery.Where(cq => cq.ID == predefinedQueryID)
                    select contactQuery;
            var spName = q.AsEnumerable().First().StoredProcName;

            //ALL predefined queries should have a single parameter, called IRISObjectID of type int64 (bigint in SQL)
            var p = new SqlParameter("IRISObjectID", System.Data.DbType.Int64) { Value = irisObjectID };

            List<long> contactSubLinksIDs = Context.ExecuteListScalarSP<long>(spName, p);

            var subLinks = from subLink in Context.ContactSubLink
                           where contactSubLinksIDs.Contains(subLink.ID)
                           select subLink;

            return GetSubLinksFromQueryable(subLinks);

            //var query1 = from subLink in Context.ContactSubLink
            //             where contactSubLinksIDs.Contains(subLink.ID)
            //             select new
            //             {
            //                 subLink,
            //                 contacts = from contact in Context.Contact where contact.ID == subLink.ContactID select contact,
            //                 irisObjects = from i in Context.IRISObject select i,
            //                 names = from name in Context.Name where !name.IsDeleted select name,
            //                 contactAddresses = from contactAddress in Context.ContactAddress.Include("Address").Include("ContactSubLinks") where !contactAddress.IsDeleted select contactAddress,
            //                 emails = from email in Context.Email.Include("ContactSubLinks") where !email.IsDeleted select email,
            //                 addresses = from address in Context.Address select address,
            //             };

            //return query1.AsEnumerable().Select(c => c.subLink).ToList();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]//Everyone has view permission for contacts
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterContactSubLinkAccess")]
        public ContactSubLink GetContactSublinkByID(long contactSublinkID)
        {
            var subLinks = from subLink in Context.ContactSubLink
                           where subLink.ID == contactSublinkID
                           select subLink;

            return GetSubLinksFromQueryable(subLinks).FirstOrDefault();
        }

        private List<ContactSubLink> GetSubLinksFromQueryable(IQueryable<ContactSubLink> query)
        {
            var query1 = from subLink in query
                         select new
                         {
                             subLink,
                             contacts = from contact in Context.Contact where contact.ID == subLink.ContactID select contact,
                             irisObjects = from i in Context.IRISObject select i,
                             names = from name in Context.Name where !name.IsDeleted select name,
                             contactAddresses = from contactAddress in Context.ContactAddress.Include("Address").Include("ContactSubLinks") where !contactAddress.IsDeleted select contactAddress,
                             emails = from email in Context.Email.Include("ContactSubLinks") where !email.IsDeleted select email,
                             addresses = from address in Context.Address select address,
                         };

            return query1.AsEnumerable().Select(c => c.subLink).ToList();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<ContactQuery> GetContactQueriesForObjectType(long objectTypeID)
        {
            return Context.ContactQuery.Where(cq => cq.ObjectTypeID == objectTypeID).ToList();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public List<DocumentTemplate> GetDocumentTemplatesForObjectType(long objectTypeID)
        {
            return Context.DocumentTemplate.Where(t => t.ObjectTypeID == objectTypeID).ToList();
        }

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        public DocumentTemplate GetDocumentTemplateByID(long templateID)
        {
            return Context.DocumentTemplate.Where(t => t.ID == templateID).Single();
        }

        #endregion

        #region Private Helper methods

        private void AddToTrackableCollection<T>(TrackableCollection<T> toList, List<T> fromList) where T : IDomainObjectBase
        {
            fromList.ForEach(u => toList.Add(u));
        }

        /// <summary>
        ///    Utility function that encapsulates transaction management complexity
        ///    away from the user. Simply call and pass in an action with n number
        ///    of database calls. They will all be run as part of one transaction.
        /// </summary>
        private bool TransactionScopedQuery(Action action)
        {
            bool success = false;

            // Define a transaction scope for the operations.
            using (TransactionScope scope = new TransactionScope())
            {
                // Need to use a try statement so that 'success=true' is not run if there is an exception
                try
                {
                    action.Invoke();

                    // Save changes pessimistically. This means that changes 
                    // must be accepted manually once the transaction succeeds.
                    SaveChanges(SaveOptions.DetectChangesBeforeSave);

                    // Mark the transaction as complete.
                    scope.Complete();
                    success = true;
                }
                catch { }
            }

            if (success)
            {
                Context.AcceptAllChanges();
            }

            return success;
        }

        #endregion
    }
}
