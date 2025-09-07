using Microsoft.Extensions.Logging;
using Moq;
using PathfinderCampaignManager.Application.Search.Models;
using PathfinderCampaignManager.Application.Search.Services;
using PathfinderCampaignManager.Application.RulesSync.Models;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Infrastructure.Search;
using Xunit;

namespace PathfinderCampaignManager.Infrastructure.Tests.Search;

public class OmniSearchServiceTests
{
    private readonly Mock<ISearchIndexService> _mockIndexService;
    private readonly Mock<IFacetedSearchEngine> _mockFacetedEngine;
    private readonly Mock<ISearchQueryParser> _mockQueryParser;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<ILogger<OmniSearchService>> _mockLogger;
    private readonly OmniSearchService _searchService;

    public OmniSearchServiceTests()
    {
        _mockIndexService = new Mock<ISearchIndexService>();
        _mockFacetedEngine = new Mock<IFacetedSearchEngine>();
        _mockQueryParser = new Mock<ISearchQueryParser>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<OmniSearchService>>();

        _searchService = new OmniSearchService(
            _mockIndexService.Object,
            _mockFacetedEngine.Object,
            _mockQueryParser.Object,
            _mockUnitOfWork.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SearchAsync_WithSimpleQuery_ReturnsResults()
    {
        // Arrange
        var query = new SearchQuery { Query = "fighter" };
        var parsedQuery = new ParsedSearchQuery 
        { 
            MainQuery = "fighter",
            Terms = new List<string> { "fighter" }
        };

        _mockQueryParser.Setup(x => x.ParseQuery("fighter"))
            .Returns(parsedQuery);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.SearchTime.TotalMilliseconds >= 0);
        Assert.Contains(result.Results, r => r.Title.Contains("Fighter"));
    }

    [Fact]
    public async Task SearchAsync_WithContentTypeFilter_FiltersResults()
    {
        // Arrange
        var query = new SearchQuery 
        { 
            Query = "magic",
            ContentTypes = new List<ContentType> { ContentType.Spells }
        };
        var parsedQuery = new ParsedSearchQuery 
        { 
            MainQuery = "magic",
            Terms = new List<string> { "magic" }
        };

        _mockQueryParser.Setup(x => x.ParseQuery("magic"))
            .Returns(parsedQuery);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Results, r => Assert.Equal(ContentType.Spells, r.ContentType));
    }

    [Fact]
    public async Task SearchAsync_WithLevelRange_FiltersCorrectly()
    {
        // Arrange
        var query = new SearchQuery 
        { 
            Query = "spell",
            MinLevel = 1,
            MaxLevel = 3
        };
        var parsedQuery = new ParsedSearchQuery 
        { 
            MainQuery = "spell",
            Terms = new List<string> { "spell" }
        };

        _mockQueryParser.Setup(x => x.ParseQuery("spell"))
            .Returns(parsedQuery);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Results, r => Assert.InRange(r.Level, 1, 3));
    }

    [Fact]
    public async Task GetAutocompleteSuggestionsAsync_WithPartialQuery_ReturnsSuggestions()
    {
        // Arrange
        var partialQuery = "figh";

        // Act
        var result = await _searchService.GetAutocompleteSuggestionsAsync(partialQuery);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Suggestions);
        Assert.Contains(result.Suggestions, s => s.Text.ToLowerInvariant().Contains("fighter"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("a")] // Too short
    public async Task GetAutocompleteSuggestionsAsync_WithInvalidQuery_ReturnsEmpty(string invalidQuery)
    {
        // Act
        var result = await _searchService.GetAutocompleteSuggestionsAsync(invalidQuery);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Suggestions);
    }

    [Fact]
    public async Task SearchAsync_WithRequiredTraits_FiltersCorrectly()
    {
        // Arrange
        var query = new SearchQuery 
        { 
            Query = "damage",
            RequiredTraits = new List<string> { "Fire" }
        };
        var parsedQuery = new ParsedSearchQuery 
        { 
            MainQuery = "damage",
            Terms = new List<string> { "damage" }
        };

        _mockQueryParser.Setup(x => x.ParseQuery("damage"))
            .Returns(parsedQuery);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Results, r => Assert.Contains("Fire", r.Traits));
    }

    [Fact]
    public async Task SearchAsync_WithExcludedTraits_FiltersCorrectly()
    {
        // Arrange
        var query = new SearchQuery 
        { 
            Query = "spell",
            ExcludedTraits = new List<string> { "Fire" }
        };
        var parsedQuery = new ParsedSearchQuery 
        { 
            MainQuery = "spell",
            Terms = new List<string> { "spell" }
        };

        _mockQueryParser.Setup(x => x.ParseQuery("spell"))
            .Returns(parsedQuery);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.All(result.Results, r => Assert.DoesNotContain("Fire", r.Traits));
    }

    [Fact]
    public async Task SearchAsync_WithPagination_ReturnsCorrectPage()
    {
        // Arrange
        var query = new SearchQuery 
        { 
            Query = "*", // Match all
            PageSize = 2,
            PageNumber = 1
        };
        var parsedQuery = new ParsedSearchQuery 
        { 
            MainQuery = "*",
            Terms = new List<string>()
        };

        _mockQueryParser.Setup(x => x.ParseQuery("*"))
            .Returns(parsedQuery);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(1, result.PageNumber);
        Assert.True(result.Results.Count <= 2);
    }

    [Fact]
    public async Task GetSearchStatsAsync_ReturnsValidStats()
    {
        // Act
        var stats = await _searchService.GetSearchStatsAsync();

        // Assert
        Assert.NotNull(stats);
        Assert.True(stats.TotalDocuments >= 0);
        Assert.NotNull(stats.DocumentsByType);
        Assert.NotNull(stats.DocumentsBySource);
        Assert.NotNull(stats.PopularQueries);
    }

    [Fact]
    public async Task RebuildIndexAsync_WithValidData_ReturnsTrue()
    {
        // Arrange
        var mockRepository = new Mock<IRepository<PathfinderCampaignManager.Domain.Entities.CustomDefinition>>();
        mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<PathfinderCampaignManager.Domain.Entities.CustomDefinition>());

        _mockUnitOfWork.Setup(x => x.Repository<PathfinderCampaignManager.Domain.Entities.CustomDefinition>())
            .Returns(mockRepository.Object);

        // Act
        var result = await _searchService.RebuildIndexAsync();

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData(SearchSortOrder.Alphabetical)]
    [InlineData(SearchSortOrder.Level)]
    [InlineData(SearchSortOrder.ContentType)]
    [InlineData(SearchSortOrder.Source)]
    public async Task SearchAsync_WithDifferentSortOrders_AppliesCorrectSorting(SearchSortOrder sortOrder)
    {
        // Arrange
        var query = new SearchQuery 
        { 
            Query = "*",
            SortBy = sortOrder
        };
        var parsedQuery = new ParsedSearchQuery 
        { 
            MainQuery = "*",
            Terms = new List<string>()
        };

        _mockQueryParser.Setup(x => x.ParseQuery("*"))
            .Returns(parsedQuery);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.NotNull(result);
        
        if (result.Results.Count > 1)
        {
            switch (sortOrder)
            {
                case SearchSortOrder.Alphabetical:
                    Assert.True(result.Results.Zip(result.Results.Skip(1), (a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase) <= 0).All(x => x));
                    break;
                case SearchSortOrder.Level:
                    Assert.True(result.Results.Zip(result.Results.Skip(1), (a, b) => a.Level <= b.Level).All(x => x));
                    break;
                case SearchSortOrder.ContentType:
                    Assert.True(result.Results.Zip(result.Results.Skip(1), (a, b) => (int)a.ContentType <= (int)b.ContentType).All(x => x));
                    break;
                case SearchSortOrder.Source:
                    Assert.True(result.Results.Zip(result.Results.Skip(1), (a, b) => string.Compare(a.Source, b.Source, StringComparison.OrdinalIgnoreCase) <= 0).All(x => x));
                    break;
            }
        }
    }
}