using Microsoft.Extensions.Logging;
using PathfinderCampaignManager.Application.Search.Models;
using PathfinderCampaignManager.Application.Search.Services;
using PathfinderCampaignManager.Application.RulesSync.Models;
using PathfinderCampaignManager.Domain.Interfaces;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace PathfinderCampaignManager.Infrastructure.Search;

public class OmniSearchService : IOmniSearchService
{
    private readonly ISearchIndexService _indexService;
    private readonly IFacetedSearchEngine _facetedSearchEngine;
    private readonly ISearchQueryParser _queryParser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OmniSearchService> _logger;
    
    // In-memory search index (in production, this would be Elasticsearch, Solr, or similar)
    private readonly ConcurrentDictionary<string, SearchableContent> _searchIndex = new();
    private readonly ConcurrentDictionary<string, List<string>> _termIndex = new(); // Term -> DocumentIds
    private readonly List<string> _searchHistory = new();
    private SearchStats _searchStats = new();

    public OmniSearchService(
        ISearchIndexService indexService,
        IFacetedSearchEngine facetedSearchEngine,
        ISearchQueryParser queryParser,
        IUnitOfWork unitOfWork,
        ILogger<OmniSearchService> logger)
    {
        _indexService = indexService;
        _facetedSearchEngine = facetedSearchEngine;
        _queryParser = queryParser;
        _unitOfWork = unitOfWork;
        _logger = logger;
        
        // Initialize with some sample data
        InitializeSampleData();
    }

    public async Task<SearchResult> SearchAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogDebug("Executing search query: {Query}", query.Query);
            
            var parsedQuery = _queryParser.ParseQuery(query.Query);
            var results = await ExecuteSearchAsync(parsedQuery, query, cancellationToken);
            
            var searchTime = DateTime.UtcNow - startTime;
            results.SearchTime = searchTime;
            
            // Update search statistics
            UpdateSearchStats(query.Query, searchTime);
            
            _logger.LogDebug("Search completed in {SearchTime}ms. Found {TotalHits} results", 
                searchTime.TotalMilliseconds, results.TotalHits);
                
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search failed for query: {Query}", query.Query);
            throw;
        }
    }

    public async Task<SearchResult> SearchWithFacetsAsync(SearchQuery query, List<SearchFacet> facets, CancellationToken cancellationToken = default)
    {
        var facetedQuery = new FacetedSearchQuery
        {
            Query = query.Query,
            ContentTypes = query.ContentTypes,
            SortBy = query.SortBy,
            PageSize = query.PageSize,
            PageNumber = query.PageNumber,
            MinLevel = query.MinLevel,
            MaxLevel = query.MaxLevel
        };

        // Convert facets to selected facets format
        foreach (var facet in facets)
        {
            var selectedValues = facet.Values.Where(v => v.IsSelected).Select(v => v.Value).ToList();
            if (selectedValues.Any())
            {
                facetedQuery.SelectedFacets[facet.Name] = selectedValues;
            }
        }

        var facetedResult = await _facetedSearchEngine.ExecuteFacetedSearchAsync(facetedQuery, cancellationToken);
        
        return new SearchResult
        {
            Results = facetedResult.Results,
            TotalHits = facetedResult.TotalHits,
            PageNumber = facetedResult.PageNumber,
            PageSize = facetedResult.PageSize,
            SearchTime = facetedResult.SearchTime,
            Facets = facetedResult.Facets.Values.ToList()
        };
    }

    public async Task<AutocompleteResult> GetAutocompleteSuggestionsAsync(string partialQuery, int maxSuggestions = 10, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var suggestions = new List<AutocompleteSuggestion>();
            var lowerQuery = partialQuery.ToLowerInvariant().Trim();
            
            if (string.IsNullOrWhiteSpace(lowerQuery) || lowerQuery.Length < 2)
            {
                return new AutocompleteResult { SearchTime = DateTime.UtcNow - startTime };
            }

            // Search through indexed content for matches
            var matchingContent = _searchIndex.Values
                .Where(content => 
                    content.Title.ToLowerInvariant().Contains(lowerQuery) ||
                    content.Description.ToLowerInvariant().Contains(lowerQuery) ||
                    content.Keywords.Any(k => k.ToLowerInvariant().Contains(lowerQuery)) ||
                    content.Traits.Any(t => t.ToLowerInvariant().Contains(lowerQuery)))
                .OrderByDescending(content => CalculateAutocompletScore(content, lowerQuery))
                .Take(maxSuggestions)
                .ToList();

            foreach (var content in matchingContent)
            {
                suggestions.Add(new AutocompleteSuggestion
                {
                    Text = content.Title,
                    DisplayText = FormatAutocompletDisplay(content),
                    ContentType = content.ContentType,
                    Category = content.Category,
                    Score = CalculateAutocompletScore(content, lowerQuery),
                    Metadata = new Dictionary<string, object>
                    {
                        ["level"] = content.Level,
                        ["traits"] = content.Traits,
                        ["source"] = content.Source
                    }
                });
            }

            // Add common search terms if we have room
            if (suggestions.Count < maxSuggestions)
            {
                var commonTerms = GetCommonSearchTerms()
                    .Where(term => term.ToLowerInvariant().StartsWith(lowerQuery))
                    .Take(maxSuggestions - suggestions.Count)
                    .Select(term => new AutocompleteSuggestion
                    {
                        Text = term,
                        DisplayText = term,
                        ContentType = ContentType.Rules,
                        Category = "Search Term",
                        Score = 0.5
                    });
                
                suggestions.AddRange(commonTerms);
            }

            await Task.CompletedTask;
            
            return new AutocompleteResult
            {
                Suggestions = suggestions,
                SearchTime = DateTime.UtcNow - startTime
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Autocomplete failed for query: {Query}", partialQuery);
            return new AutocompleteResult { SearchTime = DateTime.UtcNow - startTime };
        }
    }

    public async Task<List<SearchFacet>> GetAvailableFacetsAsync(SearchQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var allFacets = await _facetedSearchEngine.GetAllFacetsAsync(query.Query, cancellationToken);
            
            var facets = new List<SearchFacet>();
            
            foreach (var facetGroup in allFacets)
            {
                var facet = new SearchFacet
                {
                    Name = facetGroup.Key,
                    DisplayName = FormatFacetDisplayName(facetGroup.Key),
                    Type = GetFacetType(facetGroup.Key),
                    Values = facetGroup.Value,
                    TotalValues = facetGroup.Value.Count,
                    IsMultiSelect = IsMultiSelectFacet(facetGroup.Key)
                };
                
                facets.Add(facet);
            }
            
            return facets.OrderBy(f => GetFacetSortOrder(f.Name)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get available facets for query: {Query}", query.Query);
            return new List<SearchFacet>();
        }
    }

    public Task<SearchStats> GetSearchStatsAsync(CancellationToken cancellationToken = default)
    {
        _searchStats.TotalDocuments = _searchIndex.Count;
        _searchStats.DocumentsByType = _searchIndex.Values
            .GroupBy(c => c.ContentType)
            .ToDictionary(g => g.Key, g => (long)g.Count());
        _searchStats.DocumentsBySource = _searchIndex.Values
            .GroupBy(c => c.Source)
            .ToDictionary(g => g.Key, g => (long)g.Count());
        _searchStats.PopularQueries = GetPopularQueries();
        
        return Task.FromResult(_searchStats);
    }

    public async Task<bool> RebuildIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting search index rebuild");
            
            // Clear existing index
            _searchIndex.Clear();
            _termIndex.Clear();
            
            // Rebuild from database
            var customDefinitions = await _unitOfWork.Repository<Domain.Entities.CustomDefinition>().GetAllAsync(cancellationToken);
            
            foreach (var definition in customDefinitions)
            {
                var searchableContent = ConvertToSearchableContent(definition);
                await _indexService.IndexContentAsync(searchableContent, cancellationToken);
                
                // Add to in-memory index
                _searchIndex[searchableContent.Id] = searchableContent;
                IndexTerms(searchableContent);
            }
            
            _logger.LogInformation("Search index rebuild completed. Indexed {Count} documents", _searchIndex.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Search index rebuild failed");
            return false;
        }
    }

    private async Task<SearchResult> ExecuteSearchAsync(ParsedSearchQuery parsedQuery, SearchQuery originalQuery, CancellationToken cancellationToken)
    {
        var results = new List<SearchHit>();
        
        // If no search terms, return all content with filters applied
        if (string.IsNullOrWhiteSpace(parsedQuery.MainQuery))
        {
            results = _searchIndex.Values
                .Where(content => MatchesFilters(content, originalQuery))
                .Select(content => ConvertToSearchHit(content, 1.0))
                .ToList();
        }
        else
        {
            // Execute term-based search
            var matchingDocuments = FindMatchingDocuments(parsedQuery);
            
            results = matchingDocuments
                .Where(kvp => MatchesFilters(_searchIndex[kvp.Key], originalQuery))
                .Select(kvp => ConvertToSearchHit(_searchIndex[kvp.Key], kvp.Value))
                .ToList();
        }
        
        // Apply sorting
        results = ApplySorting(results, originalQuery.SortBy);
        
        // Calculate pagination
        var totalHits = results.Count;
        var skip = (originalQuery.PageNumber - 1) * originalQuery.PageSize;
        var pagedResults = results.Skip(skip).Take(originalQuery.PageSize).ToList();
        
        // Get facets if requested
        var facets = new List<SearchFacet>();
        if (originalQuery.IncludeFacets)
        {
            facets = await GetAvailableFacetsAsync(originalQuery, cancellationToken);
        }
        
        return new SearchResult
        {
            Results = pagedResults,
            TotalHits = totalHits,
            PageNumber = originalQuery.PageNumber,
            PageSize = originalQuery.PageSize,
            Facets = facets,
            DidYouMean = GenerateDidYouMeanSuggestion(parsedQuery.MainQuery)
        };
    }

    private Dictionary<string, double> FindMatchingDocuments(ParsedSearchQuery parsedQuery)
    {
        var documentScores = new Dictionary<string, double>();
        
        foreach (var term in parsedQuery.Terms)
        {
            if (_termIndex.TryGetValue(term.ToLowerInvariant(), out var documentIds))
            {
                foreach (var docId in documentIds)
                {
                    if (!documentScores.ContainsKey(docId))
                        documentScores[docId] = 0;
                    
                    // Simple TF-IDF-like scoring
                    var termFrequency = CalculateTermFrequency(docId, term);
                    var inverseDocumentFrequency = Math.Log((double)_searchIndex.Count / documentIds.Count);
                    
                    documentScores[docId] += termFrequency * inverseDocumentFrequency;
                }
            }
        }
        
        // Boost exact phrase matches
        foreach (var phrase in parsedQuery.PhraseQueries)
        {
            foreach (var kvp in _searchIndex)
            {
                if (kvp.Value.Content.ToLowerInvariant().Contains(phrase.ToLowerInvariant()) ||
                    kvp.Value.Title.ToLowerInvariant().Contains(phrase.ToLowerInvariant()))
                {
                    if (!documentScores.ContainsKey(kvp.Key))
                        documentScores[kvp.Key] = 0;
                    
                    documentScores[kvp.Key] += 2.0; // Phrase match boost
                }
            }
        }
        
        return documentScores.OrderByDescending(kvp => kvp.Value).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private bool MatchesFilters(SearchableContent content, SearchQuery query)
    {
        // Content type filter
        if (query.ContentTypes.Any() && !query.ContentTypes.Contains(content.ContentType))
            return false;
        
        // Level range filter
        if (query.MinLevel.HasValue && content.Level < query.MinLevel.Value)
            return false;
        if (query.MaxLevel.HasValue && content.Level > query.MaxLevel.Value)
            return false;
        
        // Required traits
        if (query.RequiredTraits.Any() && !query.RequiredTraits.All(trait => 
            content.Traits.Any(t => t.Equals(trait, StringComparison.OrdinalIgnoreCase))))
            return false;
        
        // Excluded traits
        if (query.ExcludedTraits.Any() && query.ExcludedTraits.Any(trait =>
            content.Traits.Any(t => t.Equals(trait, StringComparison.OrdinalIgnoreCase))))
            return false;
        
        // Source filter
        if (!string.IsNullOrWhiteSpace(query.Source) && 
            !content.Source.Equals(query.Source, StringComparison.OrdinalIgnoreCase))
            return false;
        
        // Custom content filter
        if (!query.IncludeCustomContent && content.IsCustomContent)
            return false;
        
        return true;
    }

    private SearchHit ConvertToSearchHit(SearchableContent content, double score)
    {
        return new SearchHit
        {
            Id = content.Id,
            Title = content.Title,
            Description = content.Description,
            ContentType = content.ContentType,
            Category = content.Category,
            Level = content.Level,
            Traits = content.Traits,
            Source = content.Source,
            Url = content.Url,
            Score = score,
            IsCustomContent = content.IsCustomContent,
            Fields = content.CustomFields
        };
    }

    private List<SearchHit> ApplySorting(List<SearchHit> results, SearchSortOrder sortOrder)
    {
        return sortOrder switch
        {
            SearchSortOrder.Relevance => results.OrderByDescending(r => r.Score).ToList(),
            SearchSortOrder.Alphabetical => results.OrderBy(r => r.Title).ToList(),
            SearchSortOrder.Level => results.OrderBy(r => r.Level).ThenBy(r => r.Title).ToList(),
            SearchSortOrder.ContentType => results.OrderBy(r => r.ContentType).ThenBy(r => r.Title).ToList(),
            SearchSortOrder.Source => results.OrderBy(r => r.Source).ThenBy(r => r.Title).ToList(),
            _ => results
        };
    }

    private double CalculateTermFrequency(string documentId, string term)
    {
        if (!_searchIndex.TryGetValue(documentId, out var content))
            return 0;
        
        var lowerTerm = term.ToLowerInvariant();
        var allText = $"{content.Title} {content.Content} {content.Description} {string.Join(" ", content.Keywords)}".ToLowerInvariant();
        
        return Regex.Matches(allText, Regex.Escape(lowerTerm), RegexOptions.IgnoreCase).Count;
    }

    private double CalculateAutocompletScore(SearchableContent content, string query)
    {
        var score = 0.0;
        
        // Exact title match gets highest score
        if (content.Title.Equals(query, StringComparison.OrdinalIgnoreCase))
            score += 10.0;
        else if (content.Title.ToLowerInvariant().StartsWith(query))
            score += 5.0;
        else if (content.Title.ToLowerInvariant().Contains(query))
            score += 2.0;
        
        // Keyword matches
        if (content.Keywords.Any(k => k.ToLowerInvariant().StartsWith(query)))
            score += 3.0;
        
        // Trait matches
        if (content.Traits.Any(t => t.ToLowerInvariant().StartsWith(query)))
            score += 1.0;
        
        // Boost common content types
        if (content.ContentType == ContentType.Classes || content.ContentType == ContentType.Spells)
            score += 0.5;
        
        return score;
    }

    private string FormatAutocompletDisplay(SearchableContent content)
    {
        var parts = new List<string> { content.Title };
        
        if (!string.IsNullOrEmpty(content.Category))
            parts.Add($"({content.Category})");
        
        if (content.Level > 0)
            parts.Add($"Level {content.Level}");
        
        return string.Join(" ", parts);
    }

    private void IndexTerms(SearchableContent content)
    {
        var allText = $"{content.Title} {content.Content} {content.Description} {string.Join(" ", content.Keywords)}";
        var terms = ExtractTerms(allText);
        
        foreach (var term in terms)
        {
            var lowerTerm = term.ToLowerInvariant();
            if (!_termIndex.ContainsKey(lowerTerm))
                _termIndex[lowerTerm] = new List<string>();
            
            if (!_termIndex[lowerTerm].Contains(content.Id))
                _termIndex[lowerTerm].Add(content.Id);
        }
    }

    private List<string> ExtractTerms(string text)
    {
        // Simple tokenization - in production would use more sophisticated NLP
        return Regex.Matches(text, @"\b\w{2,}\b")
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(term => term.Length >= 2)
            .Distinct()
            .ToList();
    }

    private void UpdateSearchStats(string query, TimeSpan searchTime)
    {
        _searchHistory.Add(query);
        if (_searchHistory.Count > 1000) // Keep last 1000 searches
            _searchHistory.RemoveAt(0);
        
        _searchStats.TotalSearches++;
        _searchStats.AverageSearchTime = TimeSpan.FromMilliseconds(
            (_searchStats.AverageSearchTime.TotalMilliseconds * (_searchStats.TotalSearches - 1) + searchTime.TotalMilliseconds) / _searchStats.TotalSearches);
    }

    private List<string> GetPopularQueries()
    {
        return _searchHistory
            .GroupBy(q => q.ToLowerInvariant())
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();
    }

    private List<string> GetCommonSearchTerms()
    {
        return new List<string>
        {
            "fighter", "wizard", "cleric", "rogue", "barbarian", "ranger",
            "fireball", "heal", "magic missile", "shield",
            "sword", "armor", "potion", "ring",
            "human", "elf", "dwarf", "halfling",
            "noble", "criminal", "scholar", "warrior"
        };
    }

    private string? GenerateDidYouMeanSuggestion(string query)
    {
        // Simple spell checking - in production would use Levenshtein distance or similar
        var commonTerms = GetCommonSearchTerms();
        var lowerQuery = query.ToLowerInvariant();
        
        var closestMatch = commonTerms
            .FirstOrDefault(term => Math.Abs(term.Length - query.Length) <= 2 && 
                                   CalculateLevenshteinDistance(lowerQuery, term) <= 2);
        
        return closestMatch != query ? closestMatch : null;
    }

    private int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var distance = new int[source.Length + 1, target.Length + 1];

        for (int i = 0; i <= source.Length; distance[i, 0] = i++) { }
        for (int j = 0; j <= target.Length; distance[0, j] = j++) { }

        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                var cost = target[j - 1] == source[i - 1] ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        return distance[source.Length, target.Length];
    }

    private SearchableContent ConvertToSearchableContent(Domain.Entities.CustomDefinition definition)
    {
        return new SearchableContent
        {
            Id = definition.Id.ToString(),
            Title = definition.Name,
            Content = definition.JsonData,
            Description = definition.Description,
            ContentType = (ContentType)(int)definition.Type,
            Category = definition.Category,
            Level = definition.Level,
            Traits = definition.Traits,
            Source = definition.Source,
            Rarity = definition.Rarity,
            Keywords = definition.GetTags(),
            CustomFields = new Dictionary<string, object>
            {
                ["version"] = definition.Version,
                ["isPublic"] = definition.IsPublic,
                ["isApproved"] = definition.IsApproved
            },
            LastModified = definition.UpdatedAt,
            IsCustomContent = true,
            Url = $"/custom/{definition.Type.ToString().ToLowerInvariant()}/{definition.Id}",
            Tags = definition.GetTags()
        };
    }

    private void InitializeSampleData()
    {
        // Add some sample searchable content for testing
        var sampleContent = new List<SearchableContent>
        {
            new()
            {
                Id = "fighter",
                Title = "Fighter",
                Content = "Core martial class with weapon proficiency and combat flexibility",
                Description = "Masters of weapons and armor, fighters are versatile combatants",
                ContentType = ContentType.Classes,
                Category = "Core Class",
                Level = 1,
                Traits = new List<string>(),
                Source = "Core Rulebook",
                Keywords = new List<string> { "martial", "weapons", "armor", "combat" }
            },
            new()
            {
                Id = "fireball",
                Title = "Fireball",
                Content = "3rd-level evocation spell that creates a fiery explosion",
                Description = "A classic blast spell that deals fire damage in an area",
                ContentType = ContentType.Spells,
                Category = "Evocation",
                Level = 3,
                Traits = new List<string> { "Evocation", "Fire" },
                Source = "Core Rulebook",
                Keywords = new List<string> { "fire", "damage", "area", "explosion" }
            },
            new()
            {
                Id = "longsword",
                Title = "Longsword",
                Content = "Versatile martial melee weapon",
                Description = "A classic one-handed sword that can be used with two hands",
                ContentType = ContentType.Weapons,
                Category = "Martial Melee",
                Level = 0,
                Traits = new List<string> { "Versatile P" },
                Source = "Core Rulebook",
                Keywords = new List<string> { "sword", "versatile", "martial", "melee" }
            }
        };

        foreach (var content in sampleContent)
        {
            _searchIndex[content.Id] = content;
            IndexTerms(content);
        }
    }

    private string FormatFacetDisplayName(string facetName) => facetName switch
    {
        "contentType" => "Content Type",
        "level" => "Level",
        "traits" => "Traits",
        "source" => "Source",
        "rarity" => "Rarity",
        "category" => "Category",
        _ => facetName
    };

    private FacetType GetFacetType(string facetName) => facetName switch
    {
        "level" => FacetType.Range,
        _ => FacetType.Terms
    };

    private bool IsMultiSelectFacet(string facetName) => facetName switch
    {
        "contentType" => true,
        "traits" => true,
        "source" => false,
        "rarity" => true,
        "category" => true,
        _ => true
    };

    private int GetFacetSortOrder(string facetName) => facetName switch
    {
        "contentType" => 1,
        "level" => 2,
        "traits" => 3,
        "source" => 4,
        "rarity" => 5,
        "category" => 6,
        _ => 99
    };
}