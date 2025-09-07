using System.Text.Json;
using System.Text.Json.Serialization;

namespace PathfinderCampaignManager.Scripts;

/// <summary>
/// Pathfinder 2e Data Scraper using Archives of Nethys Elasticsearch backend
/// Retrieves classes, ancestries, skills, feats, spells, and other game data
/// </summary>
public class PathfinderDataScraper
{
    private readonly HttpClient _httpClient;
    private readonly string _elasticsearchUrl = "https://elasticsearch.aonprd.com/";
    private readonly string _indexName = "aon";
    private readonly JsonSerializerOptions _jsonOptions;

    // Available data categories from Archives of Nethys
    private readonly string[] _dataCategories = 
    {
        "class",
        "ancestry", 
        "background",
        "feat",
        "skill",
        "spell",
        "action",
        "archetype",
        "armor",
        "weapon",
        "equipment",
        "creature",
        "deity",
        "hazard",
        "trait",
        "rules"
    };

    public PathfinderDataScraper()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PathfinderCampaignManager/1.0");
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true
        };
    }

    /// <summary>
    /// Scrape all Pathfinder 2e data and save to JSON files
    /// </summary>
    public async Task ScrapeAllDataAsync(string outputDirectory = "./pathfinder_data")
    {
        Directory.CreateDirectory(outputDirectory);
        
        Console.WriteLine("Starting Pathfinder 2e data scraping...");
        
        var allData = new Dictionary<string, List<PathfinderDataItem>>();
        
        foreach (var category in _dataCategories)
        {
            Console.WriteLine($"Scraping {category} data...");
            
            try
            {
                var items = await ScrapeDataCategoryAsync(category);
                allData[category] = items;
                
                // Save individual category file
                var categoryFile = Path.Combine(outputDirectory, $"{category}.json");
                await SaveDataToFileAsync(items, categoryFile);
                
                Console.WriteLine($"Successfully scraped {items.Count} {category} items");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scraping {category}: {ex.Message}");
            }
            
            // Be respectful - add delay between requests
            await Task.Delay(1000);
        }
        
        // Save combined data file
        var combinedFile = Path.Combine(outputDirectory, "pathfinder_2e_complete.json");
        await SaveDataToFileAsync(allData, combinedFile);
        
        Console.WriteLine($"Data scraping complete! Files saved to: {outputDirectory}");
    }

    /// <summary>
    /// Scrape data for a specific category (class, ancestry, feat, etc.)
    /// </summary>
    public async Task<List<PathfinderDataItem>> ScrapeDataCategoryAsync(string category)
    {
        var searchQuery = new
        {
            query = new
            {
                match = new
                {
                    category = category
                }
            },
            from = 0,
            size = 10000
        };

        var requestUrl = $"{_elasticsearchUrl}{_indexName}/_search";
        var jsonContent = JsonSerializer.Serialize(searchQuery, _jsonOptions);
        var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(requestUrl, content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<ElasticsearchResponse>(responseJson, _jsonOptions);

        var items = new List<PathfinderDataItem>();
        
        if (searchResult?.Hits?.Hits != null)
        {
            foreach (var hit in searchResult.Hits.Hits)
            {
                if (hit.Source != null)
                {
                    items.Add(hit.Source);
                }
            }
        }

        return items.OrderBy(x => x.Name).ToList();
    }

    /// <summary>
    /// Get filtered data for specific use cases
    /// </summary>
    public async Task<PathfinderGameData> GetFilteredGameDataAsync()
    {
        var gameData = new PathfinderGameData();

        // Get core character creation data
        gameData.Classes = await ScrapeDataCategoryAsync("class");
        gameData.Ancestries = await ScrapeDataCategoryAsync("ancestry");
        gameData.Backgrounds = await ScrapeDataCategoryAsync("background");
        gameData.Skills = await ScrapeDataCategoryAsync("skill");
        
        // Get feats categorized by type
        var allFeats = await ScrapeDataCategoryAsync("feat");
        gameData.Feats = CategorizeFeats(allFeats);
        
        // Get spells
        gameData.Spells = await ScrapeDataCategoryAsync("spell");
        
        // Get equipment
        gameData.Weapons = await ScrapeDataCategoryAsync("weapon");
        gameData.Armor = await ScrapeDataCategoryAsync("armor");
        gameData.Equipment = await ScrapeDataCategoryAsync("equipment");
        
        // Get creatures for NPCs
        gameData.Creatures = await ScrapeDataCategoryAsync("creature");
        
        // Get other useful data
        gameData.Actions = await ScrapeDataCategoryAsync("action");
        gameData.Traits = await ScrapeDataCategoryAsync("trait");

        return gameData;
    }

    /// <summary>
    /// Categorize feats by type for easier use
    /// </summary>
    private Dictionary<string, List<PathfinderDataItem>> CategorizeFeats(List<PathfinderDataItem> feats)
    {
        var categorizedFeats = new Dictionary<string, List<PathfinderDataItem>>();

        foreach (var feat in feats)
        {
            var featTypes = ExtractFeatTypes(feat);
            
            foreach (var featType in featTypes)
            {
                if (!categorizedFeats.ContainsKey(featType))
                {
                    categorizedFeats[featType] = new List<PathfinderDataItem>();
                }
                
                categorizedFeats[featType].Add(feat);
            }
        }

        return categorizedFeats;
    }

    /// <summary>
    /// Extract feat types from feat traits
    /// </summary>
    private List<string> ExtractFeatTypes(PathfinderDataItem feat)
    {
        var featTypes = new List<string>();

        // Default categorization based on common patterns
        if (feat.Traits?.Any(t => t.ToLower().Contains("class")) == true)
            featTypes.Add("Class");
        
        if (feat.Traits?.Any(t => t.ToLower().Contains("ancestry")) == true)
            featTypes.Add("Ancestry");
        
        if (feat.Traits?.Any(t => t.ToLower().Contains("skill")) == true)
            featTypes.Add("Skill");
        
        if (feat.Traits?.Any(t => t.ToLower().Contains("general")) == true)
            featTypes.Add("General");

        if (feat.Traits?.Any(t => t.ToLower().Contains("archetype")) == true)
            featTypes.Add("Archetype");

        // If no specific type found, add to general
        if (!featTypes.Any())
            featTypes.Add("General");

        return featTypes;
    }

    /// <summary>
    /// Save data to JSON file
    /// </summary>
    private async Task SaveDataToFileAsync<T>(T data, string filePath)
    {
        var json = JsonSerializer.Serialize(data, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Generate summary statistics about the scraped data
    /// </summary>
    public async Task GenerateDataSummaryAsync(string dataDirectory = "./pathfinder_data")
    {
        var summaryFile = Path.Combine(dataDirectory, "data_summary.json");
        var summary = new Dictionary<string, object>();

        foreach (var category in _dataCategories)
        {
            var categoryFile = Path.Combine(dataDirectory, $"{category}.json");
            
            if (File.Exists(categoryFile))
            {
                var json = await File.ReadAllTextAsync(categoryFile);
                var items = JsonSerializer.Deserialize<List<PathfinderDataItem>>(json);
                
                summary[category] = new
                {
                    Count = items?.Count ?? 0,
                    LastUpdated = File.GetLastWriteTime(categoryFile),
                    SampleItems = items?.Take(5).Select(x => x.Name).ToList() ?? new List<string>()
                };
            }
        }

        await SaveDataToFileAsync(summary, summaryFile);
        Console.WriteLine($"Data summary saved to: {summaryFile}");
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}

// Data Models
public class ElasticsearchResponse
{
    [JsonPropertyName("hits")]
    public HitsContainer? Hits { get; set; }
}

public class HitsContainer
{
    [JsonPropertyName("hits")]
    public Hit[]? Hits { get; set; }
}

public class Hit
{
    [JsonPropertyName("_source")]
    public PathfinderDataItem? Source { get; set; }
}

public class PathfinderDataItem
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("category")]
    public string Category { get; set; } = string.Empty;

    [JsonPropertyName("level")]
    public int? Level { get; set; }

    [JsonPropertyName("traits")]
    public string[]? Traits { get; set; }

    [JsonPropertyName("rarity")]
    public string? Rarity { get; set; }

    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("hp")]
    public int? HP { get; set; }

    [JsonPropertyName("ac")]
    public int? AC { get; set; }

    [JsonPropertyName("speed")]
    public object? Speed { get; set; }

    [JsonPropertyName("ability_modifier")]
    public object? AbilityModifier { get; set; }

    [JsonPropertyName("skill")]
    public string[]? Skill { get; set; }

    [JsonPropertyName("price")]
    public object? Price { get; set; }

    [JsonPropertyName("bulk")]
    public object? Bulk { get; set; }

    [JsonPropertyName("damage")]
    public object? Damage { get; set; }

    [JsonPropertyName("range")]
    public object? Range { get; set; }

    [JsonPropertyName("spell_type")]
    public string[]? SpellType { get; set; }

    [JsonPropertyName("school")]
    public string? School { get; set; }

    [JsonPropertyName("cast")]
    public object? Cast { get; set; }

    [JsonPropertyName("components")]
    public string[]? Components { get; set; }

    [JsonPropertyName("targets")]
    public string? Targets { get; set; }

    [JsonPropertyName("area")]
    public string? Area { get; set; }

    [JsonPropertyName("duration")]
    public string? Duration { get; set; }

    // Additional properties for extensibility
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}

public class PathfinderGameData
{
    public List<PathfinderDataItem> Classes { get; set; } = new();
    public List<PathfinderDataItem> Ancestries { get; set; } = new();
    public List<PathfinderDataItem> Backgrounds { get; set; } = new();
    public List<PathfinderDataItem> Skills { get; set; } = new();
    public Dictionary<string, List<PathfinderDataItem>> Feats { get; set; } = new();
    public List<PathfinderDataItem> Spells { get; set; } = new();
    public List<PathfinderDataItem> Weapons { get; set; } = new();
    public List<PathfinderDataItem> Armor { get; set; } = new();
    public List<PathfinderDataItem> Equipment { get; set; } = new();
    public List<PathfinderDataItem> Creatures { get; set; } = new();
    public List<PathfinderDataItem> Actions { get; set; } = new();
    public List<PathfinderDataItem> Traits { get; set; } = new();
}