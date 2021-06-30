using System.Collections.Generic;
using Datacom.IRIS.Common;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface ISecurityRepository : IRepositoryBase
    {
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectId", PermissionName = PermissionNames.SecureData)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<DataSecurity> GetDataSecurityListById(long irisObjectId);

        int GetDataSecurityCountForActiveUser(long irisObjectId);  //This is used for the business validation layer

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterDataSecurityAccess")]
        DataSecurity GetDataSecurityById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetUserList();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetUsersByGroupId(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Permission> GetPermissionList();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Permission GetPermissionById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Group> GetGroupList();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]  
        List<Group> GetGroupsByUser([UseCurrentUserInBusinessWrapper] User user); 

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Group GetGroupById(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Function> GetFunctionList();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Function> GetFunctionsByGroupId(long id);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        Function GetFunctionById(long id);


        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission,FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Function> GetFunctionListByObjectId(long objectId);
        
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.Custom, MethodToInvoke = "FilterAccessToUserDetails")]
        User GetUserByAccountName(string accountName);
              
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<FunctionObjectPermission> GetFunctionObjectPermissionList();

		[DoNotGenerateBusinessWrapper] 
        User GetUserAndPermissionsByAccountName(string accountName);

        [DoNotGenerateBusinessWrapper]
        User GetUserAndPermissionsByAccountNameForAdminSite(string accountName);

        [DoNotGenerateBusinessWrapper]
        List<Function> GetAdminFunctionList();

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.FunctionPermission, FunctionName = BaseFunctionNames.Security, PermissionName = PermissionNames.Maintain)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        User GetUserById(long id);

        /// <summary>
        ///  Return only active users who have proper permissions.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default, ObjectTypeIDParameterName = "objectTypeId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetUsersWithPermissionToObjectType(long objectTypeId, List<string> permissions);

        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Authorisation 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Authorisation object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default, ObjectTypeIDParameterName = "objectTypeId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetSearchableOfficerResponsibleForAuthorisations(long objectTypeId);

        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Regime 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Regime object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default, ObjectTypeIDParameterName = "objectTypeId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetSearchableOfficerResponsibleForRegimes(long objectTypeId);

        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Programme 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Programme object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default, ObjectTypeIDParameterName = "objectTypeId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetSearchableOfficerResponsibleForProgrammes(long objectTypeId);

        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Enforcement 
        ///  object at any level of object type or subclassification, plus also any users CURRENTLY OR PREVIOUSLY
        ///  assigned to an Enforcement object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default, ObjectTypeIDParameterName = "objectTypeId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetSearchableOfficerResponsibleForEnforcements(long objectTypeId);

        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Request 
        ///  object at any level of object type or subclassification, plus also any users CURRENTLY (latest)
        ///  assigned to an Request object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default, ObjectTypeIDParameterName = "objectTypeId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetSearchableOfficerResponsibleForRequests(long objectTypeId);

        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Management Site 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Management Site object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default, ObjectTypeIDParameterName = "objectTypeId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetSearchableOfficerResponsibleForManagementSites(long objectTypeId);

        /// <summary>
        ///  Return both active and inactive users who have Edit permission to the Application 
        ///  object at any level of object type or subclassification, plus also any users 
        ///  assigned to an Application object who do not fall into the former set.
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="permissionNames"></param>
        /// <returns></returns>
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default, ObjectTypeIDParameterName = "objectTypeId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> GetSearchableOfficerResponsibleForApplications(long objectTypeId);

        void PopulateInheritedIRISObjectSecurityContext(long irisObjectId); //no security check is required as this is only used by the business layer postentitychangehook

        SecurityContext GetIRISObjectSecurityContext(long id);

        SecurityContext GetIRISObjectSecurityContext(string objectTypeCode,long linkId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectIDs")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> ListUsersWithEditAccessToIRISObjects(List<long> irisObjectIDs, bool activeUserOnly, bool ignoreDataSecurity);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectIDs")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Group> ListGroupsWithEditAccessToIRISObjects(List<long> irisObjectIDs);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectIDs")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<User> ListUsersWithEditOrEventAccessToIRISObjects(List<long> irisObjectIDs, bool activeUserOnly, bool ignoreDataSecurity);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectIDs")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Group> ListGroupsWithEditOrEventAccessToIRISObjects(List<long> irisObjectIDs);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.IRISObject, IRISObjectIDParameterName = "irisObjectIDs")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<Group> ListGroupsWithEditOrEventAccessToIRISObjectsFilterOutGroupsWithAllSysAccount(List<long> irisObjectIDs);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        void LoadSecurityCacheData(bool forAdminSite);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.Default, ObjectTypeIDParameterName = "objectTypeId")]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        SortedDictionary<string, string> GetMobileInspectorForObjectType(long objectTypeId);

        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        string GetMobileInspectorDisplayName(string sphereUserName);

    }
}