using PathfinderCampaignManager.Application.RulesSync.Models;

namespace PathfinderCampaignManager.Application.Search.Models;

public class SearchQuery
{
    public string Query { get; set; } = string.Empty;
    public List<ContentType> ContentTypes { get; set; } = new();
    public List<SearchFilter> Filters { get; set; } = new();
    public SearchSortOrder SortBy { get; set; } = SearchSortOrder.Relevance;
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 1;
    public bool IncludeHighlights { get; set; } = true;
    public bool IncludeFacets { get; set; } = true;
    public List<string> RequiredTraits { get; set; } = new();
    public List<string> ExcludedTraits { get; set; } = new();
    public int? MinLevel { get; set; }
    public int? MaxLevel { get; set; }
    public string? Source { get; set; }
    public bool IncludeCustomContent { get; set; } = true;
}

public class SearchResult
{
    public List<SearchHit> Results { get; set; } = new();
    public long TotalHits { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalHits / PageSize);
    public TimeSpan SearchTime { get; set; }
    public List<SearchFacet> Facets { get; set; } = new();
    public string? DidYouMean { get; set; }
    public List<string> Suggestions { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SearchHit
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<string> Traits { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, List<string>> Highlights { get; set; } = new();
    public Dictionary<string, object> Fields { get; set; } = new();
    public bool IsCustomContent { get; set; }
}

public class SearchFacet
{
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public FacetType Type { get; set; } = FacetType.Terms;
    public List<FacetValue> Values { get; set; } = new();
    public int TotalValues { get; set; }
    public bool IsMultiSelect { get; set; } = true;
}

public class FacetValue
{
    public string Value { get; set; } = string.Empty;
    public string DisplayValue { get; set; } = string.Empty;
    public long Count { get; set; }
    public bool IsSelected { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SearchFilter
{
    public string Field { get; set; } = string.Empty;
    public FilterOperator Operator { get; set; } = FilterOperator.Equals;
    public object Value { get; set; } = string.Empty;
    public List<object> Values { get; set; } = new(); // For IN operations
}

public class AutocompleteResult
{
    public List<AutocompleteSuggestion> Suggestions { get; set; } = new();
    public TimeSpan SearchTime { get; set; }
}

public class AutocompleteSuggestion
{
    public string Text { get; set; } = string.Empty;
    public string DisplayText { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string Category { get; set; } = string.Empty;
    public double Score { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SearchStats
{
    public long TotalDocuments { get; set; }
    public Dictionary<ContentType, long> DocumentsByType { get; set; } = new();
    public Dictionary<string, long> DocumentsBySource { get; set; } = new();
    public DateTime LastIndexUpdate { get; set; }
    public TimeSpan AverageSearchTime { get; set; }
    public long TotalSearches { get; set; }
    public List<string> PopularQueries { get; set; } = new();
}

public class SearchableContent
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ContentType ContentType { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Level { get; set; }
    public List<string> Traits { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common";
    public List<string> Keywords { get; set; } = new();
    public Dictionary<string, object> CustomFields { get; set; } = new();
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public bool IsCustomContent { get; set; }
    public string Url { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

public class ParsedSearchQuery
{
    public string MainQuery { get; set; } = string.Empty;
    public List<string> Terms { get; set; } = new();
    public List<string> PhraseQueries { get; set; } = new();
    public List<SearchFilter> Filters { get; set; } = new();
    public List<string> RequiredTerms { get; set; } = new(); // Terms with +
    public List<string> ExcludedTerms { get; set; } = new(); // Terms with -
    public bool HasWildcards { get; set; }
    public bool IsFuzzySearch { get; set; }
}

public class FacetedSearchQuery
{
    public string Query { get; set; } = string.Empty;
    public Dictionary<string, List<string>> SelectedFacets { get; set; } = new();
    public List<ContentType> ContentTypes { get; set; } = new();
    public SearchSortOrder SortBy { get; set; } = SearchSortOrder.Relevance;
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 1;
    public int? MinLevel { get; set; }
    public int? MaxLevel { get; set; }
}

public class FacetedSearchResult
{
    public List<SearchHit> Results { get; set; } = new();
    public long TotalHits { get; set; }
    public Dictionary<string, SearchFacet> Facets { get; set; } = new();
    public TimeSpan SearchTime { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalHits / PageSize);
}

public class SearchIndexStats
{
    public long TotalDocuments { get; set; }
    public long IndexSize { get; set; }
    public DateTime LastOptimized { get; set; }
    public Dictionary<string, long> FieldCounts { get; set; } = new();
    public TimeSpan AverageIndexTime { get; set; }
    public bool IsOptimized { get; set; }
}

public enum SearchSortOrder
{
    Relevance = 1,
    Alphabetical = 2,
    Level = 3,
    ContentType = 4,
    Source = 5,
    DateModified = 6
}

public enum FilterOperator
{
    Equals = 1,
    NotEquals = 2,
    Contains = 3,
    StartsWith = 4,
    EndsWith = 5,
    GreaterThan = 6,
    LessThan = 7,
    GreaterThanOrEqual = 8,
    LessThanOrEqual = 9,
    In = 10,
    NotIn = 11,
    Range = 12
}

public enum FacetType
{
    Terms = 1,
    Range = 2,
    Hierarchy = 3,
    Date = 4
}