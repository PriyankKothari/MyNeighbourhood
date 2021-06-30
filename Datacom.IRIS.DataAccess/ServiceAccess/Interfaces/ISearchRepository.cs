using System.Collections.Generic;
using Datacom.IRIS.DataAccess.Attributes;
using Datacom.IRIS.DataAccess.Security;
using Datacom.IRIS.DomainModel.DTO;
using Datacom.IRIS.DomainModel.Domain;
using Datacom.IRIS.DomainModel.DTO.AdvancedSearch;

namespace Datacom.IRIS.DataAccess.ServiceAccess.Interfaces
{
    public interface ISearchRepository : IRepositoryBase
    {
        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        SearchResultsIndices GetSearchResultContent(long searchHeaderID, int rowsPerPage, int startPageIndex, int startRowIndex, string sortExpression, string excludeObjectIDs);

        [DoNotGenerateBusinessWrapper]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        int ExecuteSearch(ISearchTextCriteria searchTextCriteria, long userID, bool isSpatialSearch, string spatialSearchResult, out long searchHeaderID);

        void IndexSearchableObject(long searchableObjectID, string objectType);

        void IndexSearchableObject(long irisObjectID);

        void ReIndexSearchIndexSpatialID(long irisObjectID);

        void ReIndexSearchIndexForCDFQuestionDefintion(long questionDefinitionId);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        SearchIndex GetSearchIndex(long objectID, long objectTypeID);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<SearchIndex> GetSearchIndexByIrisObjectIds(List<long> irisObjectIds);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<SearchKeyword> GetSearchKeyword(long searchIndexID);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        SearchHeader GetSearchHeader(long searchHeaderID);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<SearchTerm> GetSearchTerm(long searchHeaderID);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<SearchResult> GetSearchResult(long searchHeaderID);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<SearchResultSpatial> GetSearchResultSpatial(long searchHeaderID);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        List<SearchSuggestions> GetSearchSuggestions(string searchCriteria, string searchScopeObjectType, long userId);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        int? GetSearchResultsCount(string searchCriteria, string searchScopeObjectType, long userId);

        [EnsureValidIRISUser]
        [SecurityCheck(CheckWhen = CheckWhen.Before, CheckMethod = SecurityCheckMethod.None)]
        [SecurityCheck(CheckWhen = CheckWhen.After, CheckMethod = SecurityCheckMethod.None)]
        void DeleteSearchSuggestion(long searchHeaderId);
    }


}