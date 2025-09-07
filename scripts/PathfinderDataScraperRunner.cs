using PathfinderCampaignManager.Scripts;
using System.CommandLine;
using System.Text.Json;

namespace PathfinderCampaignManager.Scripts;

/// <summary>
/// Console application for running the Pathfinder 2e data scraper
/// Usage: dotnet run -- [options]
/// </summary>
public class PathfinderDataScraperRunner
{
    public static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("Pathfinder 2e Data Scraper - Retrieve game data from Archives of Nethys");

        // Add command options
        var outputOption = new Option<DirectoryInfo>(
            name: "--output",
            description: "Output directory for scraped data",
            getDefaultValue: () => new DirectoryInfo("./pathfinder_data"));

        var categoryOption = new Option<string>(
            name: "--category",
            description: "Specific category to scrape (class, ancestry, feat, skill, spell, etc.). If not specified, all categories will be scraped.");

        var formatOption = new Option<string>(
            name: "--format",
            description: "Output format: json (default) or filtered",
            getDefaultValue: () => "json");

        var summaryOption = new Option<bool>(
            name: "--summary-only",
            description: "Generate only a summary of existing data without scraping");

        var verboseOption = new Option<bool>(
            name: "--verbose",
            description: "Enable verbose logging");

        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(categoryOption);
        rootCommand.AddOption(formatOption);
        rootCommand.AddOption(summaryOption);
        rootCommand.AddOption(verboseOption);

        rootCommand.SetHandler(async (DirectoryInfo output, string category, string format, bool summaryOnly, bool verbose) =>
        {
            await ExecuteScrapingAsync(output, category, format, summaryOnly, verbose);
        }, outputOption, categoryOption, formatOption, summaryOption, verboseOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task ExecuteScrapingAsync(DirectoryInfo output, string category, string format, bool summaryOnly, bool verbose)
    {
        var scraper = new PathfinderDataScraper();
        
        try
        {
            Console.WriteLine("🎲 Pathfinder 2e Data Scraper");
            Console.WriteLine("=============================");
            
            if (verbose)
            {
                Console.WriteLine($"Output Directory: {output.FullName}");
                Console.WriteLine($"Format: {format}");
                Console.WriteLine($"Category Filter: {category ?? "All categories"}");
                Console.WriteLine();
            }

            // Create output directory
            output.Create();

            if (summaryOnly)
            {
                Console.WriteLine("Generating data summary...");
                await scraper.GenerateDataSummaryAsync(output.FullName);
                return;
            }

            if (!string.IsNullOrEmpty(category))
            {
                // Scrape specific category
                await ScrapeSingleCategoryAsync(scraper, category, output.FullName, verbose);
            }
            else if (format == "filtered")
            {
                // Get filtered game data optimized for character creation
                await ScrapeFilteredDataAsync(scraper, output.FullName, verbose);
            }
            else
            {
                // Scrape all data
                await scraper.ScrapeAllDataAsync(output.FullName);
            }

            // Always generate summary after scraping
            await scraper.GenerateDataSummaryAsync(output.FullName);

            Console.WriteLine();
            Console.WriteLine("✅ Scraping completed successfully!");
            Console.WriteLine($"📁 Data saved to: {output.FullName}");
            
            // Show sample of what was scraped
            await DisplayDataSampleAsync(output.FullName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error during scraping: {ex.Message}");
            if (verbose)
            {
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
        finally
        {
            scraper.Dispose();
        }
    }

    private static async Task ScrapeSingleCategoryAsync(PathfinderDataScraper scraper, string category, string outputDir, bool verbose)
    {
        Console.WriteLine($"Scraping {category} data...");
        
        var items = await scraper.ScrapeDataCategoryAsync(category);
        var categoryFile = Path.Combine(outputDir, $"{category}.json");
        
        var json = JsonSerializer.Serialize(items, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        
        await File.WriteAllTextAsync(categoryFile, json);
        
        Console.WriteLine($"✅ {items.Count} {category} items saved to {categoryFile}");
        
        if (verbose && items.Any())
        {
            Console.WriteLine($"Sample items: {string.Join(", ", items.Take(5).Select(x => x.Name))}");
        }
    }

    private static async Task ScrapeFilteredDataAsync(PathfinderDataScraper scraper, string outputDir, bool verbose)
    {
        Console.WriteLine("Scraping filtered game data optimized for character creation...");
        
        var gameData = await scraper.GetFilteredGameDataAsync();
        var filteredFile = Path.Combine(outputDir, "pathfinder_2e_filtered.json");
        
        var json = JsonSerializer.Serialize(gameData, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
        });
        
        await File.WriteAllTextAsync(filteredFile, json);
        
        Console.WriteLine($"✅ Filtered game data saved to {filteredFile}");
        
        if (verbose)
        {
            Console.WriteLine($"Classes: {gameData.Classes.Count}");
            Console.WriteLine($"Ancestries: {gameData.Ancestries.Count}");
            Console.WriteLine($"Backgrounds: {gameData.Backgrounds.Count}");
            Console.WriteLine($"Skills: {gameData.Skills.Count}");
            Console.WriteLine($"Feat Categories: {gameData.Feats.Count}");
            Console.WriteLine($"Spells: {gameData.Spells.Count}");
            Console.WriteLine($"Equipment Items: {gameData.Weapons.Count + gameData.Armor.Count + gameData.Equipment.Count}");
        }
    }

    private static async Task DisplayDataSampleAsync(string dataDir)
    {
        try
        {
            var summaryFile = Path.Combine(dataDir, "data_summary.json");
            if (!File.Exists(summaryFile))
                return;

            var summaryJson = await File.ReadAllTextAsync(summaryFile);
            var summary = JsonSerializer.Deserialize<Dictionary<string, object>>(summaryJson);

            if (summary == null)
                return;

            Console.WriteLine();
            Console.WriteLine("📊 Data Summary:");
            Console.WriteLine("===============");

            foreach (var kvp in summary.Take(10)) // Show first 10 categories
            {
                var categoryData = JsonSerializer.Deserialize<dynamic>(kvp.Value.ToString() ?? "{}");
                if (categoryData != null)
                {
                    var count = categoryData.GetProperty("Count").GetInt32();
                    Console.WriteLine($"{kvp.Key.PadRight(12)} : {count,4} items");
                }
            }

            if (summary.Count > 10)
            {
                Console.WriteLine($"... and {summary.Count - 10} more categories");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Could not display data summary: {ex.Message}");
        }
    }
}

/// <summary>
/// Example usage class showing how to integrate the scraper into your application
/// </summary>
public static class PathfinderDataScraperExamples
{
    /// <summary>
    /// Example: Get all classes for character creation
    /// </summary>
    public static async Task<List<PathfinderDataItem>> GetClassesForCharacterCreationAsync()
    {
        using var scraper = new PathfinderDataScraper();
        
        var classes = await scraper.ScrapeDataCategoryAsync("class");
        
        // Filter to core classes suitable for player characters
        return classes
            .Where(c => c.Rarity != "Uncommon" && c.Rarity != "Rare")
            .OrderBy(c => c.Name)
            .ToList();
    }

    /// <summary>
    /// Example: Get ancestries with their heritages
    /// </summary>
    public static async Task<Dictionary<string, List<PathfinderDataItem>>> GetAncestriesWithHeritagesAsync()
    {
        using var scraper = new PathfinderDataScraper();
        
        var ancestries = await scraper.ScrapeDataCategoryAsync("ancestry");
        var backgrounds = await scraper.ScrapeDataCategoryAsync("background");
        
        // Group related ancestries and backgrounds
        var ancestryData = new Dictionary<string, List<PathfinderDataItem>>();
        
        foreach (var ancestry in ancestries)
        {
            var relatedBackgrounds = backgrounds
                .Where(b => b.Traits?.Any(t => t.Contains(ancestry.Name, StringComparison.OrdinalIgnoreCase)) == true)
                .ToList();
                
            ancestryData[ancestry.Name] = relatedBackgrounds;
        }
        
        return ancestryData;
    }

    /// <summary>
    /// Example: Get feats by level and type for character advancement
    /// </summary>
    public static async Task<Dictionary<string, Dictionary<int, List<PathfinderDataItem>>>> GetFeatsForCharacterAdvancementAsync()
    {
        using var scraper = new PathfinderDataScraper();
        
        var feats = await scraper.ScrapeDataCategoryAsync("feat");
        
        var featsByTypeAndLevel = new Dictionary<string, Dictionary<int, List<PathfinderDataItem>>>();
        
        foreach (var feat in feats)
        {
            var featType = GetPrimaryFeatType(feat);
            var level = feat.Level ?? 1;
            
            if (!featsByTypeAndLevel.ContainsKey(featType))
                featsByTypeAndLevel[featType] = new Dictionary<int, List<PathfinderDataItem>>();
            
            if (!featsByTypeAndLevel[featType].ContainsKey(level))
                featsByTypeAndLevel[featType][level] = new List<PathfinderDataItem>();
            
            featsByTypeAndLevel[featType][level].Add(feat);
        }
        
        return featsByTypeAndLevel;
    }

    /// <summary>
    /// Example: Get spells filtered by class and level
    /// </summary>
    public static async Task<Dictionary<string, Dictionary<int, List<PathfinderDataItem>>>> GetSpellsByClassAndLevelAsync()
    {
        using var scraper = new PathfinderDataScraper();
        
        var spells = await scraper.ScrapeDataCategoryAsync("spell");
        
        var spellsByClassAndLevel = new Dictionary<string, Dictionary<int, List<PathfinderDataItem>>>();
        
        var casterClasses = new[] { "Wizard", "Sorcerer", "Cleric", "Druid", "Bard", "Oracle", "Witch" };
        
        foreach (var spell in spells)
        {
            var level = spell.Level ?? 0;
            
            foreach (var casterClass in casterClasses)
            {
                if (spell.SpellType?.Any(st => st.Contains(casterClass, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    if (!spellsByClassAndLevel.ContainsKey(casterClass))
                        spellsByClassAndLevel[casterClass] = new Dictionary<int, List<PathfinderDataItem>>();
                    
                    if (!spellsByClassAndLevel[casterClass].ContainsKey(level))
                        spellsByClassAndLevel[casterClass][level] = new List<PathfinderDataItem>();
                    
                    spellsByClassAndLevel[casterClass][level].Add(spell);
                }
            }
        }
        
        return spellsByClassAndLevel;
    }

    private static string GetPrimaryFeatType(PathfinderDataItem feat)
    {
        if (feat.Traits == null)
            return "General";
            
        if (feat.Traits.Any(t => t.Contains("Class", StringComparison.OrdinalIgnoreCase)))
            return "Class";
        if (feat.Traits.Any(t => t.Contains("Ancestry", StringComparison.OrdinalIgnoreCase)))
            return "Ancestry";
        if (feat.Traits.Any(t => t.Contains("Skill", StringComparison.OrdinalIgnoreCase)))
            return "Skill";
        if (feat.Traits.Any(t => t.Contains("Archetype", StringComparison.OrdinalIgnoreCase)))
            return "Archetype";
            
        return "General";
    }
}