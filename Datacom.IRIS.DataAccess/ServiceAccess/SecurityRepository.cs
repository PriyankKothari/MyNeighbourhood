using Datacom.IRIS.Common;
using Datacom.IRIS.Common.DependencyInjection;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.DomainModel.Domain;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Unity;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class SecurityRepository : RepositoryStore, ISecurityRepository
    {
        public List<DataSecurity> GetDataSecurityListById(long irisObjectId)
        {
            var dbquery = from dataSecurity in Context.DataSecurity.Where(ds => !ds.IsDeleted)
                          join user in Context.User on dataSecurity.UserID equals user.ID
                          join irisObject in Context.IRISObject on dataSecurity.IRISObjectID equals irisObject.ID
                          join objectType in Context.ReferenceDataValue on irisObject.ObjectTypeID equals objectType.ID
                          orderby dataSecurity.DateCreated descending
                          where irisObject.ID == irisObjectId
                          select new
                          {
                              dataSecurity,
                              irisObject,
                              objectType,
                              user
                          };
            var users = dbquery.Distinct().Select(x => x.user).ToList();
            return dbquery.Distinct().Select(x => x.dataSecurity).ToList();

            //return GetCachedDataSecurity()
            //    .Where(ds => ds.IRISObjectID == irisObjectId)
            //    .ToList();
        }

		public int GetDataSecurityCountForActiveUser(long irisObjectId)
        {
            //Do not use cached DataSecurity as this is for validation ValidateSecureDataUserIsAdded 
            //and cache may not get update quick enough for this particular validation
            return (from dataSecurity in Context.DataSecurity.Where(ds => ds.IRISObjectID == irisObjectId && !ds.IsDeleted)
                    join user in Context.User.Where(u => u.IsActive) on dataSecurity.UserID equals user.ID
                    select dataSecurity.ID).AsEnumerable().Count();

        }

        public DataSecurity GetDataSecurityById(long id)
        {
            return Context.DataSecurity
                    .Include(ds => ds.IRISObject.ObjectTypeREF)
                    .Where(ds => ds.ID == id)
                    .SingleOrDefault().TrackAll();
        }

        public List<User> GetUserList()
        {
            return GetCachedUsersWithFunctionsPermissions();
            //return Context.User.ToList().TrackAll();
        }

        public List<User> GetUsersByGroupId(long id)
        {
            var dbquery = from userGroup in Context.UserGroup.Where(c => c.GroupID == id && !c.IsDeleted)
                          join user in Context.User on userGroup.UserID equals user.ID
                          select new { userGroup, user };

            return dbquery.AsEnumerable().Select(u => u.user).ToList().TrackAll();
        }

        public List<Permission> GetPermissionList()
        {
            return GetCachedPermissionList();
        }

        public Permission GetPermissionById(long id)
        {
            var dbquery = from permission in Context.Permission.Where(p => p.ID == id)
                          join groupPermission in Context.GroupPermission on permission.ID equals groupPermission.PermissionID
                          select new { permission, groupPermission };

            return dbquery.AsEnumerable().Select(p => p.permission).Distinct().Single().TrackAll();
        }

        public List<Group> GetGroupList()
        {
            var dbquery = from @group in Context.Group.Where(grp => !grp.IsDeleted)
                          from userGroup in Context.UserGroup
                            .Where(ug => ug.GroupID == @group.ID && !ug.IsDeleted)
                            .DefaultIfEmpty()
                          select new { @group, userGroup };

            return dbquery.AsEnumerable().Select(u => u.group).Distinct().ToList();
        }

        public List<Group> GetGroupsByUser(User user)
        {
            //var dbquery = from userGroup in Context.UserGroup.Where(ug => ug.UserID == user.ID)
            //              join @group in Context.Group on userGroup.GroupID equals @group.ID
            //              join user in Context.User on userGroup.UserID equals user.ID
            //              select new { userGroup, @group, user };

            //return dbquery.AsEnumerable().Select(g => g.group).ToList().TrackAll();
            return GetCachedUserGroups()
                    .Where(x => x.UserID == user.ID)
                    .Select(x => x.Group)
                    .ToList();
        }
                
        public Group GetGroupById(long id)
        {
            var dbquery = from @group in Context.Group.Where(g => g.ID == id)
                          from userGroup in Context.UserGroup
                              .Where(ug => ug.GroupID == @group.ID && !ug.IsDeleted)
                              .DefaultIfEmpty()
                          from user in Context.User
                              .Where(w => w.ID == userGroup.UserID)
                              .DefaultIfEmpty()
                          from groupPermission in Context.GroupPermission
                              .Where(gp => gp.GroupID == @group.ID && !gp.IsDeleted)
                              .DefaultIfEmpty()
                          from function in Context.Function
                              .Where(f => f.ID == groupPermission.FunctionID)
                              .DefaultIfEmpty()
                          from permission in Context.Permission
                              .Where(p => p.ID == groupPermission.PermissionID)
                              .DefaultIfEmpty()
                          select new { @group, userGroup, user, groupPermission, function, permission };

            return dbquery.AsEnumerable().Select(p => p.group).Distinct().Single().TrackAll();
        }

        public List<Function> GetFunctionList()
        {
            return GetCachedFunctionList();

         /*   return Context.Function
                .Include(f => f.ObjectREF)
                .Include(f => f.SubClass1REF)
                .Include(f => f.SubClass2REF)
                .Where(f => !f.IsDeleted).ToList();*/
        }

        public List<Function> GetAdminFunctionList()
        {
            return GetCachedAdminFunctionList();
        }

        public List<Function> GetFunctionsByGroupId(long id)
        {
            var dbquery = from groupPermission in Context.GroupPermission.Where(c => c.GroupID == id)
                          join function in Context.Function on groupPermission.FunctionID equals function.ID
                          where !function.IsDeleted
                          select new { groupPermission, function };

            return dbquery.AsEnumerable().Select(s => s.function).ToList();
        }

        public Function GetFunctionById(long id)
        {
            var dbquery = from function in Context.Function.Where(f => f.ID == id)
                          from groupPermission in Context.GroupPermission
                              .Where(gp => gp.FunctionID == function.ID && !gp.IsDeleted)
                              .DefaultIfEmpty()
                          from @group in Context.Group
                              .Where(g => g.ID == groupPermission.GroupID && !g.IsDeleted)
                              .DefaultIfEmpty()
                          from permission in Context.Permission
                              .Where(p => p.ID == groupPermission.PermissionID)
                              .DefaultIfEmpty()
                          select new { function, groupPermission, @group, permission };

            return dbquery.AsEnumerable().Select(s => s.function).Distinct().Single().TrackAll();
        }

        public List<Function> GetFunctionListByObjectId(long objectId)
        {
            var dbquery = from function in Context.Function.Where(f => f.ObjectID == objectId)
                          from groupPermission in Context.GroupPermission
                              .Where(gp => gp.FunctionID == function.ID && !gp.IsDeleted)
                              .DefaultIfEmpty()
                          from @group in Context.Group
                              .Where(g => g.ID == groupPermission.GroupID && !g.IsDeleted)
                              .DefaultIfEmpty()
                          from permission in Context.Permission
                              .Where(p => p.ID == groupPermission.PermissionID)
                              .DefaultIfEmpty()
                          select new { function, groupPermission, @group, permission };

            return dbquery.AsEnumerable().Select(s => s.function).Distinct().ToList();
        }

        private IEnumerable<User> GetUserByObjectQuery(string accountName)
        {
            //var dbquery = from user in Context.User
            //              from userGroup in Context.UserGroup
            //                .Where(ug => ug.UserID == user.ID && !ug.IsDeleted)
            //                .DefaultIfEmpty()
            //              from @group in Context.Group
            //                .Where(g => g.ID == userGroup.GroupID && !g.IsDeleted)
            //                .DefaultIfEmpty()
            //              where user.AccountName == accountName
            //              select new { userGroup, user, @group };

            //return dbquery.AsEnumerable().Select(s => s.user);


            var dbquery = from user in Context.User
                          where user.AccountName == accountName
                          select new
                          {
                              user,
                              UserGroupAndGroup = from userGroup in user.UserGroups
                                                  join @group in Context.Group on userGroup.GroupID equals @group.ID
                                                  where !userGroup.IsDeleted && !userGroup.Group.IsDeleted
                                                  select new { userGroup, @group },
                          };


            return dbquery.AsEnumerable().Distinct().Select(s => s.user);
        }

        public User GetUserByAccountName(string accountName)
        {
            var dbquery = from user in Context.User
                          from delegation in Context.Delegation
                                .Where(d => d.UserID == user.ID && !d.IsDeleted)
                                .DefaultIfEmpty()
                          where user.AccountName == accountName
                          select new
                          {
                              user,
                              Group = from userGroup in user.UserGroups
                                      join @group in Context.Group on userGroup.GroupID equals @group.ID
                                      where !userGroup.IsDeleted && !userGroup.Group.IsDeleted
                                      select new { userGroup, @group },
                              delegation,
                              delegation.SectionREF,   //DelegationSections
                              delegation.SectionREF.ParentReferenceDataValue   //DelegationActs

                          };

            var result = dbquery.AsEnumerable().Select(s => s.user).Distinct().SingleOrDefault();
            if (result == null)
                return null;
            RepositoryMap.ActiveDirectoryRepository.PopulateUserInfoFromAD(result);
            return result.TrackAll();

        }
        
        public List<FunctionObjectPermission> GetFunctionObjectPermissionList()
        {
            return Context.FunctionObjectPermission
                    .Include(fop => fop.Function)
                    .Include(fop => fop.Permission)
                    .ToList();
        }

        /// <summary>
        ///    Gets a user as well as all functions/permissions/groups associated with that user
        /// </summary>
        public User GetUserAndPermissionsByAccountName(string accountName)
        {
            // return GetUserForExpression(c => c.AccountName == accountName);
            accountName = accountName.ToLower();
            return GetCachedUsersWithFunctionsPermissions().SingleOrDefault(u => u.AccountName.ToLower() == accountName);            
        }

        public User GetUserAndPermissionsByAccountNameForAdminSite(string accountName)
        {
            accountName = accountName.ToLower();
            return GetCachedUsersWithAdminFunctionsPermissions().SingleOrDefault(u => u.AccountName.ToLower() == accountName);    
        }

        public User GetUserById(long id)
        {

           // return GetUserForExpression(c => c.ID == id);
           return GetCachedUsersWithFunctionsPermissions().SingleOrDefault(u => u.ID == id).TrackAll();
        }

        private User GetUserForExpression(Expression<Func<User, bool>> expression)
        {
            var query = from user in Context.User.Where(expression)
                        from userGroup in Context.UserGroup
                            .Where(ug => !ug.IsDeleted && user.ID == ug.UserID)
                            .DefaultIfEmpty()
                        from @group in Context.Group
                            .Where(g => !g.IsDeleted && userGroup.GroupID == g.ID)
                            .DefaultIfEmpty()
                        from groupPermission in Context.GroupPermission
                            .Where(gp => !gp.IsDeleted && gp.GroupID == @group.ID)
                            .DefaultIfEmpty()
                        from function in Context.Function
                            .Where(f => !f.IsDeleted && f.ID == groupPermission.FunctionID)
                            .DefaultIfEmpty()
                        from objectTypeREF in Context.ReferenceDataValue
                            .Where(r => r.ID == function.ObjectID)
                            .DefaultIfEmpty()
                        from subClass1REF in Context.ReferenceDataValue
                            .Where(r => r.ID == function.SubClassID)
                            .DefaultIfEmpty()
                        select new { user, userGroup, @group, groupPermission, function, objectTypeREF, subClass1REF };

            return query.AsEnumerable().Select(s => s.user).Distinct().SingleOrDefault();
        }


        /// <summary>
        /// Returns list of ACTIVE users that have atleast one of the pemissions provided in the permissions list.
        /// Order the returned user by DisplayName
        /// </summary>
        /// <param name="objectTypeId"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        public List<User> GetUsersWithPermissionToObjectType(long objectTypeId, List<string> permissions)
        {   
            var permissionsLowerCase = permissions.Select(x => x.ToLower()).ToArray();
            var users = GetCachedUsersWithFunctionsPermissions()
                            .Where(u => u.IsActive &&
                                        u.UserGroups.Any(x => x.Group.GroupPermissions.Any(p => permissionsLowerCase.Contains(p.Permission.Name.ToLower()) &&
                                                                                                p.Function.ObjectID == objectTypeId)));
            return users.ToList();

            //var dbquery = from userList in Context.User
            //              from userGroup in Context.UserGroup.Where(x => userList.ID == x.UserID && !x.IsDeleted)
            //              from groupPermission in Context.GroupPermission.Where(x => userGroup.GroupID == x.GroupID && !x.IsDeleted)
            //              from function in Context.Function.Where(x => groupPermission.FunctionID == x.ID && !x.IsDeleted)
            //              from permission in Context.Permission.Where(x => groupPermission.PermissionID == x.ID)
            //              where function.ObjectID == objectTypeId && (permissions.Contains(permission.Name)) && userList.IsActive
            //              orderby userList.DisplayName
            //              select userList;

            //var user = dbquery.AsEnumerable().Distinct().ToList();
        }

        /// <summary>
        /// Populate inherited SecurityContextIRISID 
        /// </summary>
        public void PopulateInheritedIRISObjectSecurityContext(long irisObjectId)
        {
            Context.PopulateInheritedIRISObjectSecurityContext(irisObjectId);
        }


        public SecurityContext GetIRISObjectSecurityContext(long id)
        {
            return Context.IRISObject
                          .Include(o => o.ObjectTypeREF)
                          .Include(o => o.SecurityContextIRISObject.ObjectTypeREF)
                          .Where(x => x.ID == id)
                          .Single()
                          .SecurityContext;

        }

        public SecurityContext GetIRISObjectSecurityContext(string objectTypeCode, long linkId)
        {
            return Context.IRISObject
                          .Include(o => o.ObjectTypeREF)
                          .Include(o => o.SecurityContextIRISObject.ObjectTypeREF)
                          .Where(x => x.LinkID == linkId && x.ObjectTypeREF.Code == objectTypeCode)
                          .Single()
                          .SecurityContext;
        }

        public List<User> ListUsersWithEditAccessToIRISObjects(List<long> irisObjectIDs, bool activeUserOnly, bool ignoreDataSecurity)
        {
            return Context.ListUsersWithEditAccessToIRISObjects(string.Join(",", irisObjectIDs), activeUserOnly, ignoreDataSecurity).ToList();
        }

        public List<Group> ListGroupsWithEditAccessToIRISObjects(List<long> irisObjectIDs)
        {
            return Context.ListGroupsWithEditAccessToIRISObjects(string.Join(",", irisObjectIDs)).ToList();
        }

        public List<User> ListUsersWithEditOrEventAccessToIRISObjects(List<long> irisObjectIDs, bool activeUserOnly, bool ignoreDataSecurity)
        {
            return Context.ListUsersWithEditOrEventAccessToIRISObjects(string.Join(",", irisObjectIDs), activeUserOnly, ignoreDataSecurity).ToList();
        }

        public List<Group> ListGroupsWithEditOrEventAccessToIRISObjects(List<long> irisObjectIDs)
        {
            return Context.ListGroupsWithEditOrEventAccessToIRISObjects(string.Join(",", irisObjectIDs)).ToList();
        }
        public List<Group> ListGroupsWithEditOrEventAccessToIRISObjectsFilterOutGroupsWithAllSysAccount(List<long> irisObjectIDs)
        {
            var groups = ListGroupsWithEditOrEventAccessToIRISObjects(irisObjectIDs).Select(g => g.ID).ToList();

            return Context.Group
                .Include(g => g.UserGroups)
                .Include("UserGroups.User")
                .Where(g => groups.Contains(g.ID)).Where(g => g.UserGroups.Any(ug => !ug.User.IsSysAccount && !ug.IsDeleted)).ToList();
        }

        public void LoadSecurityCacheData(bool forAdminSite)
        {
            //Create a new IObjectContext before loading each cache otherwise all objects in the cache will be linked up
            using (IObjectContext context = DIContainer.Container.Resolve<IObjectContext>())
            {
                GetCachedPermissionList(context);
            }
            using (IObjectContext context = DIContainer.Container.Resolve<IObjectContext>())
            {
                GetCachedUserGroups(context);
            }
            using (IObjectContext context = DIContainer.Container.Resolve<IObjectContext>())
            {
                if (!forAdminSite)
                    GetCachedFunctionList(context);
                else
                    GetCachedAdminFunctionList(context);
            }
            using (IObjectContext context = DIContainer.Container.Resolve<IObjectContext>())
            {
                if (!forAdminSite)
                    GetCachedUsersWithFunctionsPermissions(context);
                else
                    GetCachedUsersWithAdminFunctionsPermissions(context);
            }
            //if (!forAdminSite)
            //{
            //    using (IObjectContext context = DIContainer.Container.Resolve<IObjectContext>())
            //    {
            //        GetCachedDataSecurity(context);
            //    }
            //}
        }

        public SortedDictionary<string, string> GetMobileInspectorForObjectType(long objectTypeId)
        {
            var query = from function in Context.Function.Where(f => f.ObjectID == objectTypeId)
                        from mobileInspectorFunction in Context.MobileInspectorFunctions.Where(r => r.FunctionID == function.ID)
                        from mobileInspector in Context.MobileInspectors.Where(i => i.ID == mobileInspectorFunction.MobileInspectorID && i.IsActive)
                        select new {mobileInspector};

            var result = new SortedDictionary<string, string>();

            query.ForEach(x => result.Add(x.mobileInspector.SphereUserName.ToLower(), FormatInspectorDisplayName(x.mobileInspector.DisplayName)));
            return result;
        }

        public string GetMobileInspectorDisplayName(string sphereUserName)
        {
            var externalInspector = Context.MobileInspectors.SingleOrDefault(x => x.SphereUserName == sphereUserName && x.IsActive);

            if (externalInspector != null)
            {
                return FormatInspectorDisplayName(externalInspector.DisplayName);
            }

            var internalInspector = Context.User.SingleOrDefault(x => x.AccountName == sphereUserName && x.IsActive);
            if (internalInspector != null)
            {
                return internalInspector.DisplayName;
            }

            return null;
        }

        private string FormatInspectorDisplayName(string displayName)
        {
            return string.Format("{0} (External)", displayName);
        }

        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Application 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Application object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        public List<User> GetSearchableOfficerResponsibleForApplications(long objectTypeId)
        {
            var permittedUsers = GetAllUsersWithPermissionToObjectType(objectTypeId, PermissionNames.Edit);

            var signedUsers = Context.Applications.Include(x => x.OfficerResponsible).Select(x => x.OfficerResponsible).Where(x => x != null).Distinct().ToList();

            return permittedUsers.Union(signedUsers, new UserComparerer()).ToList();
        }
        
        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Authorisation 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Authorisation object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        public List<User> GetSearchableOfficerResponsibleForAuthorisations(long objectTypeId)
        {
            var permittedUsers = GetAllUsersWithPermissionToObjectType(objectTypeId, PermissionNames.Edit);

            var signedUsers = Context.Authorisations.Include(x => x.OfficerResponsible).Select(x => x.OfficerResponsible).Distinct().ToList();

            return permittedUsers.Union(signedUsers, new UserComparerer()).ToList();
        }
        
        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Regime 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Regime object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        public List<User> GetSearchableOfficerResponsibleForRegimes(long objectTypeId)
        {
            var permittedUsers = GetAllUsersWithPermissionToObjectType(objectTypeId, PermissionNames.Edit);

            var signedUsers = Context.Regime.Include(x => x.OfficerResponsible).Select(x => x.OfficerResponsible).Distinct().ToList();

            return permittedUsers.Union(signedUsers, new UserComparerer()).ToList();
        }
        
        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Programme 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Programme object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        public List<User> GetSearchableOfficerResponsibleForProgrammes(long objectTypeId)
        {
            var permittedUsers = GetAllUsersWithPermissionToObjectType(objectTypeId, PermissionNames.Edit);

            var signedUsers = Context.Programme.Include(x => x.OfficerResponsible).Select(x => x.OfficerResponsible).Distinct().ToList();

            return permittedUsers.Union(signedUsers, new UserComparerer()).ToList();
        }
        
        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Enforcement 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Enforcement Action object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        public List<User> GetSearchableOfficerResponsibleForEnforcements(long objectTypeId)
        {
            var permittedUsers = GetAllUsersWithPermissionToObjectType(objectTypeId, PermissionNames.Edit);

            var signedUsers = Context.EnforcementActionOfficerResponsibles.Include(x => x.OfficerResponsible).Select(x => x.OfficerResponsible).Distinct().ToList();

            return permittedUsers.Union(signedUsers, new UserComparerer()).ToList();
        }
        
        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Request 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Request object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        public List<User> GetSearchableOfficerResponsibleForRequests(long objectTypeId)
        {
            var permittedUsers = GetAllUsersWithPermissionToObjectType(objectTypeId, PermissionNames.Edit);


            var signedUsers = from requestOfficerResponsible in Context.RequestOfficerResponsible
                              from user in Context.User.Where(u => u.ID == requestOfficerResponsible.OfficerResponsibleID)
                              where !requestOfficerResponsible.IsDeleted
                              group requestOfficerResponsible by requestOfficerResponsible.RequestID
                                  into grp
                                  select grp.OrderByDescending(s => s.LastModified)
                                            .ThenByDescending(s => s.ID)
                                            .FirstOrDefault();

            return permittedUsers.Union(signedUsers.Select(x => x.OfficerResponsible), new UserComparerer()).ToList();
        }
        
        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Management Site 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Management Site object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        public List<User> GetSearchableOfficerResponsibleForManagementSites(long objectTypeId)
        {
            var permittedUsers = GetAllUsersWithPermissionToObjectType(objectTypeId, PermissionNames.Edit);

            var signedUsers = (from user in Context.User
		                        from managementSite in Context.ManagementSites.Where(m => m.OfficerResponsibleID == user.ID)
                                select user).Distinct().ToList();
            
            return permittedUsers.Union(signedUsers, new UserComparerer()).ToList();
        }
        
        /// <summary>
        /// Returns list of ACTIVE AND INACTIVE users that have at least one of the pemissions provided in the permissions list.
        /// </summary>
        /// <param name="objectTypeId"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        private List<User> GetAllUsersWithPermissionToObjectType(long objectTypeId, string permissions)
        {
            var users = GetCachedUsersWithFunctionsPermissions()
                            .Where(u => u.UserGroups.Any(x => x.Group.GroupPermissions.Any(p => permissions.ToLower() == p.Permission.Name.ToLower() &&
                                                                                                p.Function.ObjectID == objectTypeId)));
            return users.ToList();
        }

        private List<Permission> GetCachedPermissionList()
        {
            return GetCachedPermissionList(Context);
        }

        private List<Permission> GetCachedPermissionList(IObjectContext context)
        {
            return context.Permission.OrderBy(x => x.Name).FromCached(CacheConstants.Permissions).ToList();
        }

        private List<UserGroup> GetCachedUserGroups()
        {
            return GetCachedUserGroups(Context);
        }

        private List<UserGroup> GetCachedUserGroups(IObjectContext context)
        {
            var dbquery = from userGroup in context.UserGroup
                          join @group in context.Group on userGroup.GroupID equals @group.ID
                          join user in context.User on userGroup.UserID equals user.ID
                          where !userGroup.IsDeleted && !@group.IsDeleted 
                          select new { userGroup, @group, user };

            return dbquery.FromCached(CacheConstants.UserGroups).Select(g => g.userGroup).ToList();
        }

        private List<Function> GetCachedAdminFunctionList()
        {
            return GetCachedAdminFunctionList(Context);
        }

        private List<Function> GetCachedAdminFunctionList(IObjectContext context)
        {
            return context.Function.Where(f => !f.IsDeleted && f.ID > 0)    //f.ID > 0 is a dummy clause to enusre the query is unique for Query notification registration
                                   .FromCached(CacheConstants.FunctionList_AdminFunctions).ToList();
        }



        private static object _cachedFunctionListSyncLock = new object();
        private static List<Function> _cachedFunctionList;
        
        private List<Function> GetCachedFunctionList()
        {
            return GetCachedFunctionList(Context);
        }

        private List<Function> GetCachedFunctionList(IObjectContext context)
        {

            lock (_cachedFunctionListSyncLock)
            {
                if (_cachedFunctionList != null)
                {
                    //UtilLogger.Instance.LogDebug("Cache Get for [GetCachedFunctionList] with DataSource LocalCopy.");
                    return _cachedFunctionList.ReturnFromCache();
                }
           

                Action<bool> onCacheInvalidated = (doRecache) =>
                {
                    lock (_cachedFunctionListSyncLock)
                    {
                        CacheHelper.RemoveCacheEntry<Function>(CacheConstants.FunctionList_Functions);
                        CacheHelper.RemoveCacheEntry<ReferenceDataValue>(CacheConstants.FunctionList_ObjectSubClass1and2s);
                        _cachedFunctionList = null;
                    }
                    //recache
                    if (doRecache)
                    {
                        using (var repository = RepositoryMap.SecurityRepository)
                        {
                            repository.GetFunctionList();
                        }
                    }
                };

                // NOTE: DON"T DELETE
                // It makes use of the Entity framework auto-fixup feature.  Once the entity has been pulled into the 
                // EF object graph, it will auto hook up the relationships. This is to work around the fact that caching  
                // (relying on sql dependency)  does not work with Left join.
                var objectQuery = (from val in context.ReferenceDataValue
                                   join coll in context.ReferenceDataCollection on val.CollectionID equals coll.ID
                                   where (coll.Code == ReferenceDataCollectionCode.IrisObjects ||
                                          coll.Code == ReferenceDataCollectionCode.IrisObjectsSubClass1 ||
                                          coll.Code == ReferenceDataCollectionCode.IrisObjectsSubClass2 ||
                                          coll.Code == "EnsureUniqueQuery_FunctionList")  //This is a dummy OR clause to make sure this query is unique for query notification registration
                                   select val).FromCached(CacheConstants.FunctionList_ObjectSubClass1and2s, onCacheInvalidated)
                                   .ToList();

                var functionsQuery = context.Function.Where(f => !f.IsDeleted)
                                            .OrderBy(f => f.Name)
                                            .FromCached(CacheConstants.FunctionList_Functions, onCacheInvalidated).ToList();

                _cachedFunctionList = functionsQuery;
                return _cachedFunctionList.ReturnFromCache();
            }
        }
     

        private static object _cachedUserAdminSyncLock = new object();
        private static List<User> _cachedUsersAdmin;

        private List<User> GetCachedUsersWithAdminFunctionsPermissions()
        {
            return GetCachedUsersWithAdminFunctionsPermissions(Context);
        }
        private List<User> GetCachedUsersWithAdminFunctionsPermissions(IObjectContext context)
        {
            lock (_cachedUserAdminSyncLock)
            {
                if (_cachedUsersAdmin != null)
                {
                    //UtilLogger.Instance.LogDebug("Cache Get for [GetCachedUsersWithAdminFunctionsPermissions] with DataSource LocalCopy.");
                    return _cachedUsersAdmin.ReturnFromCache();
                }

                //set up cache invalidated action to invalidate all related cache entries and recache.
                Action<bool> onCacheInvalidated = (doRecache) =>
                {
                    lock (_cachedUserAdminSyncLock)
                    {
                    
                        CacheHelper.RemoveCacheEntry<UserGroupFunctionPermission>(CacheConstants.UsersWithFunctionsPermissions_UserGroups_Admin);
                        CacheHelper.RemoveCacheEntry<User>(CacheConstants.UsersWithFunctionsPermissions_Users_Admin);

                        _cachedUsersAdmin = null;
                    }
                    //Recache
                    if (doRecache)
                    {
                        using (var repository = RepositoryMap.SecurityRepository)
                        {
                            repository.GetUserList();
                        }
                    }
                };

                // NOTE: DON"T DELETE
                // It makes use of the Entity framework auto-fixup feature.  Once the entity has been pulled into the 
                // EF object graph, it will auto hook up the relationships. This is to work around the fact that caching  
                // (relying on sql dependency)  does not work with Left join.
                var query = (from userGroup in context.UserGroup
                                 .Where(ug => !ug.IsDeleted)
                             from @group in context.Group
                                 .Where(g => !g.IsDeleted && userGroup.GroupID == g.ID)
                             from groupPermission in context.GroupPermission
                                 .Where(gp => !gp.IsDeleted && gp.GroupID == @group.ID)
                             from function in context.Function
                                 .Where(f => !f.IsDeleted && f.ID == groupPermission.FunctionID && f.ObjectID == null)
                             from permission in context.Permission
                                 .Where(p => groupPermission.PermissionID == p.ID)
                             select new UserGroupFunctionPermission
                             {
                                 UserGroup = userGroup,
                                 Group = @group,
                                 GroupPermission = groupPermission,
                                 Function = function,
                                 Permission = permission
                             }).FromCached(CacheConstants.UsersWithFunctionsPermissions_UserGroups_Admin, onCacheInvalidated)
                            .ToList();

                var users = (from user in context.User
                             where user.ID > 0  //This is a dummy where clause to make sure this query is unique for query notification registration
                             select user
                             ).FromCached(CacheConstants.UsersWithFunctionsPermissions_Users_Admin, onCacheInvalidated)
                                           .ToList();

                _cachedUsersAdmin = users;
                return _cachedUsersAdmin.ReturnFromCache();
            }

        }


        private static object _cachedUserSyncLock = new object();
        private static List<User> _cachedUsers;

        private List<User> GetCachedUsersWithFunctionsPermissions()
        {
            return GetCachedUsersWithFunctionsPermissions(Context);
        }
        private List<User> GetCachedUsersWithFunctionsPermissions(IObjectContext context)
        {
            lock (_cachedUserSyncLock)
            {
                if (_cachedUsers != null)
                {
                    //UtilLogger.Instance.LogDebug("Cache Get for [GetCachedUsersWithFunctionsPermissions] with DataSource LocalCopy.");
                    return _cachedUsers.ReturnFromCache();
                }

                //set up cache invalidated action to invalidate all related cache entries and recache.
                Action<bool> onCacheInvalidated = (doRecache) =>
                {
                    lock (_cachedUserSyncLock)
                    {
                        CacheHelper.RemoveCacheEntry<ReferenceDataValue>(CacheConstants.UsersWithFunctionsPermissions_ObjectSubClass1and2s);
                        CacheHelper.RemoveCacheEntry<UserGroupFunctionPermission>(CacheConstants.UsersWithFunctionsPermissions_UserGroups);
                        CacheHelper.RemoveCacheEntry<User>(CacheConstants.UsersWithFunctionsPermissions_Users);

                        _cachedUsers = null;
                    }
                    //Recache
                    if (doRecache)
                    {
                        using (var repository = RepositoryMap.SecurityRepository)
                        {
                            repository.GetUserList();
                        }
                    }
                };

                // NOTE: DON"T DELETE
                // It makes use of the Entity framework auto-fixup feature.  Once the entity has been pulled into the 
                // EF object graph, it will auto hook up the relationships. This is to work around the fact that caching  
                // (relying on sql dependency)  does not work with Left join.
                var objectQuery = (from val in context.ReferenceDataValue
                                   join coll in context.ReferenceDataCollection on val.CollectionID equals coll.ID
                                   where (coll.Code == ReferenceDataCollectionCode.IrisObjects ||
                                          coll.Code == ReferenceDataCollectionCode.IrisObjectsSubClass1 ||
                                          coll.Code == ReferenceDataCollectionCode.IrisObjectsSubClass2 ||
                                          coll.Code == "EnsureUniqueQuery_UsersWithFunctionsPermissions")  //This is a dummy OR clause to make sure this query is unique for query notification registration
                                   select val).FromCached(CacheConstants.UsersWithFunctionsPermissions_ObjectSubClass1and2s, onCacheInvalidated)
                                  .ToList();

                var query = (from userGroup in context.UserGroup
                                 .Where(ug => !ug.IsDeleted)
                             from @group in context.Group
                                 .Where(g => !g.IsDeleted && userGroup.GroupID == g.ID)
                             from groupPermission in context.GroupPermission
                                 .Where(gp => !gp.IsDeleted && gp.GroupID == @group.ID)
                             from function in context.Function
                                 .Where(f => !f.IsDeleted && f.ID == groupPermission.FunctionID)
                             from permission in context.Permission
                                 .Where(p => groupPermission.PermissionID == p.ID)
                             select new UserGroupFunctionPermission
                             {
                                 UserGroup = userGroup,
                                 Group = @group,
                                 GroupPermission = groupPermission,
                                 Function = function,
                                 Permission = permission
                             }).FromCached(CacheConstants.UsersWithFunctionsPermissions_UserGroups, onCacheInvalidated)
                            .ToList();

                var users = (from user in context.User
                             select user).FromCached(CacheConstants.UsersWithFunctionsPermissions_Users, onCacheInvalidated)
                                           .ToList();

                _cachedUsers = users;
                return _cachedUsers.ReturnFromCache();
            }

        }
        private List<DataSecurity> GetCachedDataSecurity()
        {
            return GetCachedDataSecurity(Context);
        }
        private List<DataSecurity> GetCachedDataSecurity(IObjectContext context)
        {


            var dbquery = from dataSecurity in context.DataSecurity.Where(ds => !ds.IsDeleted)
                        join user in context.User on dataSecurity.UserID equals user.ID
                        join irisObject in context.IRISObject on dataSecurity.IRISObjectID equals irisObject.ID
                        join objectType in context.ReferenceDataValue on irisObject.ObjectTypeID equals objectType.ID
                        orderby dataSecurity.DateCreated descending
                        select new
                        {
                            dataSecurity,
                            irisObject,
                            objectType,
                            user
                        };
            
            return dbquery.FromCached(CacheConstants.DataSecurity).Distinct().Select(x => x.dataSecurity).ToList();

        }

        private class UserGroupFunctionPermission
        {
            public UserGroup UserGroup { get; set; }
            public Group Group { get; set; }
            public GroupPermission GroupPermission { get; set; }
            public Function Function { get; set; }
            public Permission Permission { get; set; }
        }

        private class UserComparerer : IEqualityComparer<User>
        {
            public bool Equals(User x, User y)
            {
                return x.ID == y.ID;
            }

            public int GetHashCode(User user)
            {
                return user.ID.GetHashCode();
            }
        }
    }
}
