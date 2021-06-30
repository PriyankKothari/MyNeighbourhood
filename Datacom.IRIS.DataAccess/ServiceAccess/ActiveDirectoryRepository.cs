using System;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Security.Authentication;
using System.Text;
using Datacom.IRIS.Common;
using Datacom.IRIS.Common.Exceptions;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DomainModel.Domain;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class ActiveDirectoryRepository : IActiveDirectoryRepository
    {
        /// <summary>
        /// Checks if a given user exists in Active directory
        /// This is used when an IRIS admin adds a new IRIS user: this new IRIS user must be a valid AD User
        /// 
        /// Note: we do not care whether the AD user is active or inactive, as IRIS manages the user status
        /// (and also if the account is inactive in AD, the user won't be able to log in anyway).
        /// </summary>
        /// <param name="accountName">The name of the user to verify</param>
        /// <returns></returns>
        public bool UserExistsInAD(string accountName)
        {
            if (string.IsNullOrEmpty(accountName))
                return false;

            return GetUserSearchResult(accountName) != null;
        }


        /// <summary>
        /// Populates a User object with its information from AD.
        /// Note: The user must be a valid AD user, or an exception will be thrown
        /// </summary>
        /// <param name="user"></param>
        public void PopulateUserInfoFromAD(User user)
        {
            var searchResult = GetUserSearchResult(user.AccountName);
            if (searchResult != null)
            {
                user.Phone = GetProperty(searchResult, "telephonenumber");
                user.Mobile = GetProperty(searchResult, "mobile");
                user.Email = GetProperty(searchResult, "mail");
                user.PositionDescription = GetProperty(searchResult, "title");
            }
            else throw new IRISException(string.Format("The user {0} does not exist in AD", user.AccountName));
        }

        #region Private Methods

        /// <summary>
        /// Searches for a user in the current domain in AD, given the user's account name (without the domain)
        /// </summary>
        /// <param name="accountName"></param>
        /// <returns></returns>
        private static System.DirectoryServices.SearchResult GetUserSearchResult(string accountName)
        {
            string domainName = string.Empty, loginName = string.Empty;
            if (accountName.Contains("\\"))
            {
                loginName = accountName.Split('\\')[1];
                domainName = accountName.Split('\\')[0];
            }
            else
            {
                loginName = accountName;
            }

            Domain domain = null;
            if (!string.IsNullOrEmpty(domainName))
            {
                try
                {
                    domain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, domainName));
                }
                catch (ActiveDirectoryObjectNotFoundException ex)
                {
                    // the domain not exist
                    return null;
                }
                catch (AuthenticationException ex)
                {
                    // the app pool user not authorised to access the target domain
                    domain = null;
                }
                catch (Exception ex)
                {
                    // hopefully there will be no other exceptions, but just in case.
                    UtilLogger.Instance.LogError(string.Format("Error accessing target domain {0} : {1}", domainName, ex.ToString()));
                    domain = null;
                }
            }

            using (DirectorySearcher directorySearcher = domain == null ? new DirectorySearcher() : new DirectorySearcher(domain.GetDirectoryEntry())
            {
                Filter = String.Format("(&(objectCategory=person)(objectClass=user)(samaccountname={0}))", EscapeLdapSearchFilter(loginName))
            })
            {
                return directorySearcher.FindOne();
            }
        }

        /// <summary>
        /// Returns the value from a given property of a search result (or empty string if the property does not exist)
        /// </summary>
        /// <param name="searchResult"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string GetProperty(System.DirectoryServices.SearchResult searchResult, string propertyName)
        {
            if (searchResult.Properties.Contains(propertyName))
            {
                return searchResult.Properties[propertyName][0].ToString();
            }

            return string.Empty;
        }

        /// <summary>
        /// This methods escapes characters from an LDAP search filter, to avoid LDAP injections
        /// when we are using a filter entered by the user.
        /// 
        /// See list of characters that must be filtered:
        /// http://msdn.microsoft.com/en-us/library/aa746475.aspx
        /// 
        /// Copied from http://stackoverflow.com/questions/649149/how-to-escape-a-string-in-c-for-use-in-an-ldap-query
        /// </summary>
        /// <param name="searchFilter"></param>
        /// <returns></returns>
        private static string EscapeLdapSearchFilter(string searchFilter)
        {
            StringBuilder escape = new StringBuilder();
            for (int i = 0; i < searchFilter.Length; ++i)
            {
                char current = searchFilter[i];
                switch (current)
                {
                    case '\\':
                        escape.Append(@"\5c");
                        break;
                    case '*':
                        escape.Append(@"\2a");
                        break;
                    case '(':
                        escape.Append(@"\28");
                        break;
                    case ')':
                        escape.Append(@"\29");
                        break;
                    case '\u0000':
                        escape.Append(@"\00");
                        break;
                    case '/':
                        escape.Append(@"\2f");
                        break;
                    default:
                        escape.Append(current);
                        break;
                }
            }

            return escape.ToString();
        }

        #endregion


    }
}
