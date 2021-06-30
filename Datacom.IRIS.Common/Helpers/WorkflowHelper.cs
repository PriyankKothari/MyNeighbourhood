using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datacom.IRIS.Common.Helpers
{
    /// <summary>
    /// Helper to manage User: and Group: prefixes on AssignedTo and EscalateTo fields on TaskInstance
    /// </summary>
    public static class WorkflowHelper
    {
        /// <summary>
        /// Adds the "User:" prefix to the given accountName
        /// </summary>
        /// <param name="accountName">the Account Name of the user (or the token "OfficerResponsible")</param>
        /// <returns>the user name prefixed by "User:"</returns>
        public static string AddUserPrefix(string accountName)
        {
            return WorkflowConstants.UserPrefix + accountName;
        }

        /// <summary>
        /// Adds the "Group:" prefix to the given groupName
        /// </summary>
        /// <param name="groupName">the Name of the group</param>
        /// <returns>the group name prefixed by "Group:"</returns>
        public static string AddGroupPrefix(string groupName)
        {
            return WorkflowConstants.GroupPrefix + groupName;
        }

        /// <summary>
        /// Remove the "User:" or "Group:" prefix from the given account or group name
        /// </summary>
        /// <param name="prefixedAccountOrGroupName">The account name of a user or a group name</param>
        /// <returns></returns>
        public static string RemovePrefix(string prefixedAccountOrGroupName)
        {
            if (!string.IsNullOrEmpty(prefixedAccountOrGroupName))
            {
                if (prefixedAccountOrGroupName.StartsWith(WorkflowConstants.UserPrefix))
                {
                    return prefixedAccountOrGroupName.Remove(0, WorkflowConstants.UserPrefix.Length);
                }
                else if (prefixedAccountOrGroupName.StartsWith(WorkflowConstants.GroupPrefix))
                {
                    return prefixedAccountOrGroupName.Remove(0, WorkflowConstants.GroupPrefix.Length);
                }
            }
            return prefixedAccountOrGroupName;
        }

        /// <summary>
        /// Check whether the userName is prefixed with "User:"
        /// </summary>
        /// <param name="userName">The user name to test</param>
        /// <returns>true if userName starts with "User:"</returns>
        public static bool IsUserPrefixed(string userName)
        {
            return userName.StartsWith(WorkflowConstants.UserPrefix);
        }


    }
}
