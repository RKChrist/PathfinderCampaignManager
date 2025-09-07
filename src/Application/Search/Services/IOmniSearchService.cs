using PathfinderCampaignManager.Application.Search.Models;

namespace PathfinderCampaignManager.Application.Search.Services;

public interface IOmniSearchService
{
    Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default);
    Task<SearchResult> SearchWithFacetsAsync(SearchQuery query, List<SearchFacet> facets, CancellationToken cancellationToken = default);
    Task<AutocompleteResult> GetAutocompleteSuggestionsAsync(string partialQuery, int maxSuggestions = 10, CancellationToken cancellationToken = default);
    Task<List<SearchFacet>> GetAvailableFacetsAsync(SearchQuery query, CancellationToken cancellationToken = default);
    Task<SearchStats> GetSearchStatsAsync(CancellationToken cancellationToken = default);
    Task<bool> RebuildIndexAsync(CancellationToken cancellationToken = default);
}

public interface ISearchIndexService
{
    Task IndexContentAsync(SearchableContent content, CancellationToken cancellationToken = default);
    Task IndexBatchAsync(List<SearchableContent> contents, CancellationToken cancellationToken = default);
    Task RemoveFromIndexAsync(string contentId, CancellationToken cancellationToken = default);
    Task UpdateIndexAsync(SearchableContent content, CancellationToken cancellationToken = default);
    Task<bool> OptimizeIndexAsync(CancellationToken cancellationToken = default);
    Task<SearchIndexStats> GetIndexStatsAsync(CancellationToken cancellationToken = default);
}

public interface IFacetedSearchEngine
{
    Task<FacetedSearchResult> ExecuteFacetedSearchAsync(FacetedSearchQuery query, CancellationToken cancellationToken = default);
    Task<List<FacetValue>> GetFacetValuesAsync(string facetName, string? filterQuery = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, List<FacetValue>>> GetAllFacetsAsync(string? filterQuery = null, CancellationToken cancellationToken = default);
}

public interface ISearchQueryParser
{
    ParsedSearchQuery ParseQuery(string query);
    SearchFilter ParseFilter(string filterExpression);
    List<string> ExtractSearchTerms(string query);
    bool ValidateQuery(string query, out List<string> errors);
}