using System;
using System.Collections.Generic;
using System.Data.Objects;
using System.Linq;
using Datacom.IRIS.Common.Exceptions;
using Datacom.IRIS.DataAccess.ServiceAccess.Interfaces;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.DTO.AdvancedSearch;
using Datacom.IRIS.DataAccess.Utils;
using Datacom.IRIS.Common;

namespace Datacom.IRIS.DataAccess.ServiceAccess
{
    public class SearchRepository : RepositoryStore, ISearchRepository
    {
        public SearchResultsIndices GetSearchResultContent(long searchHeaderID, int rowsPerPage, int startPageIndex, int startRowIndex, string sortExpression, string excludeObjectIDs)
        {
            SearchResultsIndices results = new SearchResultsIndices(searchHeaderID);

            ObjectParameter searchCount = new ObjectParameter("searchCount", typeof(int));
            //var tempResult = Context.SelectSearchResultsByPage(searchHeaderID, sortExpression, rowsPerPage, (short)(startPageIndex + 1), excludeObjectIDs, searchCount).ToList();
            var tempResult = Context.SelectSearchResultsByPageV2(searchHeaderID, sortExpression, rowsPerPage, (short)(startPageIndex + 1), excludeObjectIDs, searchCount).ToList();

            // add reference data to the searchindex in the back end instead of match 
            // objectTypeID with ref data in the front end.
            foreach (SearchIndex searchIndex in tempResult)
            {
                searchIndex.ObjectTypeREF = Context.ReferenceDataValue.Single(r => r.ID == searchIndex.ObjectTypeID);
            }

            results.SearchIndices = tempResult;
            results.Count = (int)(searchCount.Value);
            return results;
        }

        public List<SearchSuggestions> GetSearchSuggestions(string searchCriteria, string searchScopeObjectType, long userId)
        {           
            
            var results = new Dictionary<string, bool>();
            long? scopeId = GetObjectTypeIdForCode(searchScopeObjectType);
            var searchSuggestions = Context.GetSearchSuggestions(searchCriteria, scopeId, userId).ToList();
            return searchSuggestions;
        }    

        public int? GetSearchResultsCount(string searchCriteria, string searchScopeObjectType, long userId)
        {
            int? searchResultsCount = null;
            long? scopeId = GetObjectTypeIdForCode(searchScopeObjectType);
            var result = Context.GetSearchResultsCount(searchCriteria, scopeId, userId);
            foreach (int i in result)
                searchResultsCount = i;
            return searchResultsCount;
        }

        public void DeleteSearchSuggestion(long searchHeaderId)
        {
            Context.DeleteSearchSuggestion(searchHeaderId);
        }

        /// <summary>
        ///     Execute different search store procedures depends on the type of ISearchTextCriteria object
        /// </summary>
        /// <param name="searchTextCriteria">SearchTextCriteria criteria fields allow to be empty except when doing spatial search only.</param>
        /// <param name="userID"></param>
        /// <param name="isSpatialSearch"></param>
        /// <param name="spatialSearchResult"></param>
        /// <param name="searchHeaderID"></param>
        /// <returns></returns>
        public int ExecuteSearch(ISearchTextCriteria searchTextCriteria, long userID, bool isSpatialSearch, string spatialSearchResult, out long searchHeaderID)
        {
            // Setup output parameters
            ObjectParameter searchHeaderIDParameter = new ObjectParameter("SearchHeaderID", typeof(long));
            ObjectParameter errorCodeParameter = new ObjectParameter("ErrorCode", typeof(int)) { Value = 0 };

            // Depending on type of search criteria being passed, called the relevant stored Proc
            if (searchTextCriteria is BasicSearchTextCriteria)
            {
                BasicSearchTextCriteria textCriteria = (BasicSearchTextCriteria)searchTextCriteria;
                ExecuteBasicSearch(textCriteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedLocationSearchTextCriteria)
            {
                AdvancedLocationSearchTextCriteria criteria = (AdvancedLocationSearchTextCriteria)searchTextCriteria;
                ExecuteAdvancedLocationSearch(criteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedContactSearchTextCriteria)
            {
                AdvancedContactSearchTextCriteria criteria = (AdvancedContactSearchTextCriteria)searchTextCriteria;
                ExecuteAdvancedContactSearch(criteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedApplicationSearchTextCriteria)
            {
                AdvancedApplicationSearchTextCriteria criteria = (AdvancedApplicationSearchTextCriteria)searchTextCriteria;
                ExecuteAdvancedApplicationSearch(criteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedAuthorisationSearchTextCriteria)
            {
                AdvancedAuthorisationSearchTextCriteria criteria = (AdvancedAuthorisationSearchTextCriteria)searchTextCriteria;
                ExecuteAdvancedAuthorisationSearch(criteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedRegimeSearchTextCriteria)
            {
                AdvancedRegimeSearchTextCriteria criteria = (AdvancedRegimeSearchTextCriteria)searchTextCriteria;
                ExecuteAdvancedRegimeSearch(criteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedProgrammeSearchCriteria)
            {
                AdvancedProgrammeSearchCriteria criteria = (AdvancedProgrammeSearchCriteria)searchTextCriteria;
                ExecuteAdvancedProgrammeSearch(criteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is DuplicateCheckTextCriteria)
            {
                DuplicateCheckTextCriteria criteria = (DuplicateCheckTextCriteria)searchTextCriteria;
                ExecuteDuplicateContactCheckSearch(criteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedRequestSearchTextCriteria)
            {
                AdvancedRequestSearchTextCriteria criteria = (AdvancedRequestSearchTextCriteria)searchTextCriteria;
                ExecuteAdvancedRequestSearch(criteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedManagementSiteTextCriteria)
            {
                AdvancedManagementSiteTextCriteria criteria = (AdvancedManagementSiteTextCriteria)searchTextCriteria;
                ExecuteAdvancedManagementSiteSearch(criteria, userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedEnforcementSearchTextCriteria)
            {
                ExecuteAdvancedEnforcementSearch(((AdvancedEnforcementSearchTextCriteria)searchTextCriteria), userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedSelectedLandUseSiteSearchTextCriteria)
            {
                ExecuteAdvancedSelectedLandUseSiteSearch(((AdvancedSelectedLandUseSiteSearchTextCriteria)searchTextCriteria), userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else if (searchTextCriteria is AdvancedGeneralRegisterTextCriteria)
            {
                ExecuteAdvancedGeneralRegisterSearch(((AdvancedGeneralRegisterTextCriteria)searchTextCriteria), userID, isSpatialSearch, spatialSearchResult, errorCodeParameter, searchHeaderIDParameter);
            }
            else
            {
                throw new IRISException(string.Format("Search Criteria is of an unsupported type of '{0}'", searchTextCriteria.GetType()));
            }

            searchHeaderID = (long)searchHeaderIDParameter.Value;
            return Convert.ToInt32(errorCodeParameter.Value);
        }

        #region Search Stored Procedure Calls

        /// <summary>
        ///    We seperate the create search and execute search into two stored procs because:
        ///       1. It gives us the option to seperate search criteria validation steps, and return error for invalid criteria without continue search
        ///       2. At the design time, we not sure how we going to insert spatial search result into the searchheader without a searchheaderID
        /// </summary>
        private void ExecuteBasicSearch(BasicSearchTextCriteria textCriteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            long? scopeId = GetObjectTypeIdForCode(textCriteria.Scope);
            // Context.CreateSearch(textCriteria.KeywordString, scopeId, textCriteria.SubClass1ID, userID, isSpatialSearch, searchHeaderIDParameter, errorCodeParameter);
            Context.CreateSearchV2(textCriteria.KeywordString, scopeId, textCriteria.SubClass1ID, userID, isSpatialSearch, searchHeaderIDParameter, errorCodeParameter);

            if (isSpatialSearch)
            {
                Context.AddSpatialResult((long)searchHeaderIDParameter.Value, spatialSearchResult);
            }

            //Context.RunSearch((long)searchHeaderIDParameter.Value);
            Context.RunSearchV2((long)searchHeaderIDParameter.Value);
        }

        private void ExecuteAdvancedProgrammeSearch(AdvancedProgrammeSearchCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchProgramme(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                criteria.StartDateFrom, criteria.StartDateTo, criteria.EndDateFrom, criteria.EndDateTo,
                criteria.ProgrammeTypeIDs, criteria.PriorityList, criteria.OfficerResponsibleIDs, criteria.Description,
                criteria.KeywordString, criteria.LinkedPersonOrganisationName, criteria.LinkedContactRelationshipTypeIDs, criteria.GetCdfSearchCriteriaXml(), errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedRegimeSearch(AdvancedRegimeSearchTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchRegime(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                criteria.RegimeTypeIDs, criteria.ClassificationIDs, criteria.RegimeActivityTypeIDs, criteria.ActivityName,
                criteria.OfficerResponsibleIDs, criteria.Description, criteria.StatusIDs, criteria.FinancialYearIDs,
                criteria.KeywordString, criteria.LinkedPersonOrganisationName, criteria.LinkedContactRelationshipTypeIDs, criteria.GetCdfSearchCriteriaXml(), errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedLocationSearch(AdvancedLocationSearchTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchLocation(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                criteria.CommonName, criteria.Description, criteria.FeatureTypeIDs, criteria.CreatedFrom,
                criteria.CreatedTo, criteria.Restricted, criteria.RestrictedComments, criteria.LegalDescription, criteria.LocationGroupIRISObjectID,
                criteria.KeywordString, criteria.LinkedPersonOrganisationName, criteria.LinkedContactRelationshipTypeIDs, criteria.GetCdfSearchCriteriaXml(), errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedContactSearch(AdvancedContactSearchTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchContact(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                criteria.ContactID, criteria.IncludeDuplicates, criteria.ExcludeDeceased, criteria.FirstName, criteria.LastName,
                criteria.OrganisationName, criteria.CompanyNumber, criteria.StreetNumber, criteria.StreetAlpha, criteria.StreetName, criteria.Suburb, criteria.PhoneNumber, criteria.TownCityUrban,
                criteria.TownCityDelivery, criteria.DeliveryServiceIdentifier, criteria.BoxLobby,
                criteria.Address, criteria.CountryID, criteria.KeywordString, criteria.LinkedPersonOrganisationName, criteria.LinkedContactRelationshipTypeIDs,
                criteria.GetCdfSearchCriteriaXml(), errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedApplicationSearch(AdvancedApplicationSearchTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchApplication(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                criteria.ApplicationTypeID, criteria.ApplicationPurposeID, criteria.ActivityTypeIDs, criteria.ActivitySubtypeIDs,
                criteria.StatusIDs, criteria.LodgedFrom, criteria.LodgedTo, criteria.OfficerResponsibleID, criteria.Description,
                criteria.KeywordString, criteria.LinkedPersonOrganisationName, criteria.LinkedContactRelationshipTypeIDs, criteria.GetCdfSearchCriteriaXml(), errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedRequestSearch(AdvancedRequestSearchTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchRequest(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                criteria.RequestTypeIDs, criteria.RequestSubjectTypeIDs, criteria.RequestSubjectIDs, criteria.RequestPriorityIDs,
                criteria.OfficerResponsibleID, criteria.RequestPersonOrganisationName, criteria.RequestContactRelationshipTypeIDs, criteria.RequestDetails,
                criteria.RequestDateFrom, criteria.RequestDateTo, criteria.StatusIDs,
                criteria.KeywordString, criteria.ThreatSpeciesTypeID, criteria.ThreatSpeciesID, criteria.LinkedPersonOrganisationName, criteria.LinkedContactRelationshipTypeIDs, criteria.GetCdfSearchCriteriaXml(), errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedManagementSiteSearch(AdvancedManagementSiteTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            string managementTypeIDs = null;
            int i = 1;
            int count = criteria.ManagementSiteTypeID != null ? criteria.ManagementSiteTypeID.Count : 0;
            if (count > 0)
            {
                foreach (long id in criteria.ManagementSiteTypeID)
                {
                    if (i < count)
                    {
                        managementTypeIDs += id + ",";
                    }
                    if (i == count)
                    {
                        managementTypeIDs += id;
                    }
                    i++;
                }
            }
            Context.AdvancedSearchManagementSite(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                managementTypeIDs, criteria.ManagementSiteSubtypeIDs, criteria.OfficerResponsibleID, criteria.HabitatID,
                criteria.StatusID, criteria.ClassificationTypeID, criteria.ClassificationIDs, criteria.ConservationSpeciesTypeID,
                criteria.ConservationSpeciesID, criteria.ThreatSpeciesTypeID, criteria.ThreatSpeciesID, criteria.IndustryPurposeID,
                criteria.SituationID, criteria.Description,
                criteria.KeywordString, criteria.LinkedPersonOrganisationName, criteria.LinkedContactRelationshipTypeIDs,
                criteria.GetCdfSearchCriteriaXml(), errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedEnforcementSearch(AdvancedEnforcementSearchTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchEnforcement(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                    criteria.BriefDescription,
                    criteria.ActionID,
                    criteria.ActionTypeID,
                    criteria.ActID,
                    criteria.OffenceSectionID,
                    criteria.NatureOfOffenceID,
                    criteria.OffenceStartDate,
                    criteria.OffenceEndDate,
                    criteria.OfficerResponsibleID,
                    criteria.StatusID,
                    criteria.KeywordString,
                    criteria.LinkedPersonOrganisationName,
                    criteria.LinkedContactRelationshipTypeIDs,
                    criteria.GetCdfSearchCriteriaXml(),
                    errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedSelectedLandUseSiteSearch(AdvancedSelectedLandUseSiteSearchTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchSLUS(GetObjectTypeIdForCode(criteria.ObjectType), userID, criteria.SiteDescription, isSpatialSearch, spatialSearchResult,
                    criteria.StatusIDs,
                    criteria.SiteClassificationIDs,
                    criteria.SiteClassificationContextIDs,
                    criteria.SiteHAILGroupIDs,
                    criteria.SiteHAILCategoryIDs,
                    criteria.SiteContaminantTypeIDs,
                    criteria.SiteContaminantIDs,
                    criteria.KeywordString,
                    criteria.LinkedPersonOrganisationName,
                    criteria.LinkedContactRelationshipTypeIDs,
                    criteria.GetCdfSearchCriteriaXml(),
                    errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedAuthorisationSearch(AdvancedAuthorisationSearchTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchAuthorisation(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                criteria.AuthorisationTypeID, criteria.ActivityTypeIDs, criteria.ActivitySubtypeIDs, criteria.StatusIDs,
                criteria.CommencedFrom, criteria.CommencedTo, criteria.OfficerResponsibleID, criteria.AuthorisationDescription,
                criteria.KeywordString, criteria.LinkedPersonOrganisationName, criteria.LinkedContactRelationshipTypeIDs,
                criteria.GetCdfSearchCriteriaXml(), errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteAdvancedGeneralRegisterSearch(AdvancedGeneralRegisterTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.AdvancedSearchGeneralRegister(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                criteria.TypeID, criteria.StatusIDs,
                criteria.KeywordString, criteria.LinkedPersonOrganisationName, criteria.LinkedContactRelationshipTypeIDs,
                criteria.GetCdfSearchCriteriaXml(), errorCodeParameter, searchHeaderIDParameter);
        }

        private void ExecuteDuplicateContactCheckSearch(DuplicateCheckTextCriteria criteria, long userID, bool isSpatialSearch, string spatialSearchResult, ObjectParameter errorCodeParameter, ObjectParameter searchHeaderIDParameter)
        {
            Context.DuplicateContactSearch(GetObjectTypeIdForCode(criteria.ObjectType), userID, isSpatialSearch, spatialSearchResult,
                                           criteria.FirstName, criteria.LastName, criteria.OrganisationName,
                                           criteria.EmailAddress, criteria.WebsiteURL,
                                           criteria.PhoneNumber, criteria.DeliveryServiceIdentifier,
                                           criteria.DeliveryBoxLobby, criteria.DeliveryTownCity,
                                           criteria.UrbanStreetName, criteria.UrbanSuburb,
                                           criteria.UrbanTownCity,
                                           criteria.OverseasAddressLine1, criteria.OverseasAddressLine2,
                                           criteria.OverseasAddressLine3, criteria.OverseasAddressLine4,
                                           criteria.OverseasAddressLine5, criteria.OverseasCountryREFID,
                                           errorCodeParameter, searchHeaderIDParameter);
        }

        // TODO: The UI has reference to the object type ID. This is recommended to be used
        // instead of the backend having to go and do the translation itself; expensive!
        private long? GetObjectTypeIdForCode(string objectTypeCode)
        {
            ReferenceDataValue referenceDataValue = Context.ReferenceDataValue
                                                        .Where(rdv => rdv.Code == objectTypeCode &&
                                                                      rdv.ReferenceDataCollection.Code == ReferenceDataCollectionCode.IrisObjects)
                                                        .SingleOrDefault();
            if (referenceDataValue == null) return null;
            return referenceDataValue.ID;
        }

        #endregion

        public void IndexSearchableObject(long searchableObjectID, string objectType)
        {
            Context.ReIndexObject(searchableObjectID, objectType, null);
        }

        public void IndexSearchableObject(long irisObjectID)
        {
            Context.ReIndexIRISObject(irisObjectID);
        }

        /// <summary>
        ///  Pass the IRISObjectID into the ReIndexSearchIndexSpatialID stored proc;
        ///  will update the CurrentSpatialIDList and AllSpatialIDList columns of the SearchIndex of this Object.
        /// </summary>
        public void ReIndexSearchIndexSpatialID(long irisObjectID)
        {
            Context.ReIndexSearchIndexSpatialID(irisObjectID);
        }

        public void ReIndexSearchIndexForCDFQuestionDefintion(long questionDefinitionId)
        {
            Context.IndexForCDFSearchableQuestionDefinition(questionDefinitionId);
        }

        public List<SearchIndex> GetSearchIndexByIrisObjectIds(List<long> irisObjectIds)
        {
            return Context.SearchIndex.Include(si => si.ObjectTypeREF)
                .Where(si => si.IRISObjectID != null)
                .Where(si => irisObjectIds.Contains((long)si.IRISObjectID)).ToList();
        }

        #region Search methods for unit test purposes only

        /// <summary>
        ///    For unit test purpose only
        /// </summary>
        public SearchIndex GetSearchIndex(long objectID, long objectTypeID)
        {
            return Context.SearchIndex.Include(si => si.ObjectTypeREF).SingleOrDefault(si => si.ObjectID == objectID && si.ObjectTypeID == objectTypeID);
        }

        /// <summary>
        ///    For unit test purpose only
        /// </summary>
        public List<SearchKeyword> GetSearchKeyword(long searchIndexID)
        {
            return Context.SearchKeyword.Where(sk => sk.SearchIndexID == searchIndexID).ToList();
        }

        /// <summary>
        ///  For unit test purpose only
        /// </summary>
        /// <param name="searchHeaderID"></param>
        /// <returns></returns>
        public SearchHeader GetSearchHeader(long searchHeaderID)
        {
            return Context.SearchHeader.SingleOrDefault(sh => sh.SearchHeaderID == searchHeaderID);
        }

        /// <summary>
        ///  For unit test purpose only
        /// </summary>
        /// <param name="searchHeaderID"></param>
        /// <returns></returns>
        public List<SearchTerm> GetSearchTerm(long searchHeaderID)
        {
            return Context.SearchTerm.Where(st => st.SearchHeaderID == searchHeaderID).ToList();
        }

        /// <summary>
        ///  For unit test purpose only
        /// </summary>
        /// <param name="searchHeaderID"></param>
        /// <returns></returns>
        public List<SearchResult> GetSearchResult(long searchHeaderID)
        {
            return Context.SearchResult.Where(srs => srs.SearchHeaderID == searchHeaderID).ToList();
        }

        /// <summary>
        ///  For unit test purpose only
        /// </summary>
        /// <param name="searchHeaderID"></param>
        /// <returns></returns>
        public List<SearchResultSpatial> GetSearchResultSpatial(long searchHeaderID)
        {
            return Context.SearchResultSpatial.Where(srs => srs.SearchHeaderID == searchHeaderID).ToList();
        }

        #endregion
    }
}
