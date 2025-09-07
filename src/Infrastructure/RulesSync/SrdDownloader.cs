using Microsoft.Extensions.Logging;
using PathfinderCampaignManager.Application.RulesSync.Models;
using PathfinderCampaignManager.Application.RulesSync.Services;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PathfinderCampaignManager.Infrastructure.RulesSync;

public class SrdDownloader : ISrdDownloader
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SrdDownloader> _logger;
    private readonly SrdConfiguration _config;

    // Pathfinder 2e SRD URLs - these are examples, would need real URLs
    private static readonly Dictionary<ContentType, string> SrdUrls = new()
    {
        { ContentType.Classes, "https://2e.aonprd.com/Classes.aspx?ID=" },
        { ContentType.Spells, "https://2e.aonprd.com/Spells.aspx?ID=" },
        { ContentType.Feats, "https://2e.aonprd.com/Feats.aspx?ID=" },
        { ContentType.Equipment, "https://2e.aonprd.com/Equipment.aspx?ID=" },
        { ContentType.Ancestry, "https://2e.aonprd.com/Ancestries.aspx?ID=" },
        { ContentType.Backgrounds, "https://2e.aonprd.com/Backgrounds.aspx?ID=" }
    };

    public SrdDownloader(HttpClient httpClient, ILogger<SrdDownloader> logger, SrdConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
        
        // Configure HttpClient for SRD access
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "PathfinderCampaignManager/1.0");
        _httpClient.Timeout = TimeSpan.FromMinutes(5);
    }

    public async Task<SrdContent> DownloadContentAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading content from {Url}", url);

        try
        {
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var html = await response.Content.ReadAsStringAsync(cancellationToken);
            var contentType = DetectContentType(url, html);
            
            var content = new SrdContent
            {
                Id = ExtractIdFromUrl(url),
                Name = ExtractNameFromHtml(html),
                Type = contentType,
                SourceUrl = url,
                RawContent = html,
                ParsedData = ParseHtmlContent(html, contentType),
                Version = ExtractVersionFromHtml(html),
                LastModified = DateTime.UtcNow,
                Tags = ExtractTraitsFromHtml(html),
                Metadata = ExtractMetadataFromHtml(html)
            };

            _logger.LogInformation("Successfully downloaded {ContentType} '{Name}' from {Url}", 
                contentType, content.Name, url);

            return content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download content from {Url}", url);
            throw;
        }
    }

    public async Task<List<SrdIndex>> GetContentIndexAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching content index from SRD");

        var index = new List<SrdIndex>();

        try
        {
            // Download index pages for each content type
            foreach (var contentType in Enum.GetValues<ContentType>().Where(ct => ct != ContentType.All))
            {
                if (!SrdUrls.ContainsKey(contentType))
                    continue;

                var indexUrl = GetIndexUrlForContentType(contentType);
                var indexItems = await DownloadContentIndexAsync(indexUrl, contentType, cancellationToken);
                index.AddRange(indexItems);
            }

            _logger.LogInformation("Successfully fetched {Count} items from content index", index.Count);
            return index;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch content index from SRD");
            throw;
        }
    }

    public async Task<SrdValidationResult> ValidateContentAsync(SrdContent content, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Validating content: {Name} ({Type})", content.Name, content.Type);

        var result = new SrdValidationResult
        {
            DetectedType = content.Type,
            IsValid = true
        };

        // Basic validation
        if (string.IsNullOrWhiteSpace(content.Name))
        {
            result.Errors.Add(new ValidationError
            {
                Field = "Name",
                Message = "Content name is required",
                ExpectedValue = "Non-empty string",
                ActualValue = content.Name
            });
            result.IsValid = false;
        }

        if (string.IsNullOrWhiteSpace(content.RawContent))
        {
            result.Errors.Add(new ValidationError
            {
                Field = "RawContent",
                Message = "Raw content cannot be empty",
                ExpectedValue = "Non-empty HTML content",
                ActualValue = "Empty or null"
            });
            result.IsValid = false;
        }

        // Content type specific validation
        switch (content.Type)
        {
            case ContentType.Classes:
                ValidateClassContent(content, result);
                break;
            case ContentType.Spells:
                ValidateSpellContent(content, result);
                break;
            case ContentType.Feats:
                ValidateFeatContent(content, result);
                break;
            case ContentType.Equipment:
                ValidateEquipmentContent(content, result);
                break;
        }

        _logger.LogDebug("Content validation completed. Valid: {IsValid}, Errors: {ErrorCount}, Warnings: {WarningCount}",
            result.IsValid, result.Errors.Count, result.Warnings.Count);

        return result;
    }

    private async Task<List<SrdIndex>> DownloadContentIndexAsync(string indexUrl, ContentType contentType, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(indexUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync(cancellationToken);
        return ParseIndexHtml(html, contentType);
    }

    private string GetIndexUrlForContentType(ContentType contentType)
    {
        return contentType switch
        {
            ContentType.Classes => "https://2e.aonprd.com/Classes.aspx",
            ContentType.Spells => "https://2e.aonprd.com/Spells.aspx",
            ContentType.Feats => "https://2e.aonprd.com/Feats.aspx",
            ContentType.Equipment => "https://2e.aonprd.com/Equipment.aspx",
            ContentType.Ancestry => "https://2e.aonprd.com/Ancestries.aspx",
            ContentType.Backgrounds => "https://2e.aonprd.com/Backgrounds.aspx",
            _ => throw new ArgumentException($"No index URL configured for content type: {contentType}")
        };
    }

    private ContentType DetectContentType(string url, string html)
    {
        // Check URL patterns first
        if (url.Contains("Classes.aspx")) return ContentType.Classes;
        if (url.Contains("Spells.aspx")) return ContentType.Spells;
        if (url.Contains("Feats.aspx")) return ContentType.Feats;
        if (url.Contains("Equipment.aspx")) return ContentType.Equipment;
        if (url.Contains("Ancestries.aspx")) return ContentType.Ancestry;
        if (url.Contains("Backgrounds.aspx")) return ContentType.Backgrounds;

        // Fallback to HTML content analysis
        return AnalyzeHtmlForContentType(html);
    }

    private ContentType AnalyzeHtmlForContentType(string html)
    {
        // Simple heuristics based on common HTML patterns in PF2e SRD
        if (html.Contains("Hit Points") && html.Contains("Key Ability")) return ContentType.Classes;
        if (html.Contains("Traditions") && html.Contains("Cast")) return ContentType.Spells;
        if (html.Contains("Prerequisites") && html.Contains("Frequency")) return ContentType.Feats;
        if (html.Contains("Price") && (html.Contains("Bulk") || html.Contains("Usage"))) return ContentType.Equipment;
        if (html.Contains("Ability Boosts") && html.Contains("Languages")) return ContentType.Ancestry;
        if (html.Contains("Skill Feats") && html.Contains("Lore")) return ContentType.Backgrounds;

        return ContentType.Rules; // Default fallback
    }

    private string ExtractIdFromUrl(string url)
    {
        var match = Regex.Match(url, @"ID=(\d+)");
        return match.Success ? match.Groups[1].Value : Guid.NewGuid().ToString();
    }

    private string ExtractNameFromHtml(string html)
    {
        // Extract title from common SRD patterns
        var titleMatch = Regex.Match(html, @"<h1[^>]*>(.+?)</h1>", RegexOptions.IgnoreCase);
        if (titleMatch.Success)
        {
            return System.Web.HttpUtility.HtmlDecode(titleMatch.Groups[1].Value.Trim());
        }

        var titleTagMatch = Regex.Match(html, @"<title[^>]*>(.+?)</title>", RegexOptions.IgnoreCase);
        if (titleTagMatch.Success)
        {
            var title = System.Web.HttpUtility.HtmlDecode(titleTagMatch.Groups[1].Value.Trim());
            // Remove common suffixes like " - Archives of Nethys: Pathfinder 2nd Edition Database"
            return title.Split(" - ")[0].Trim();
        }

        return "Unknown";
    }

    private Dictionary<string, object> ParseHtmlContent(string html, ContentType contentType)
    {
        var data = new Dictionary<string, object>();

        switch (contentType)
        {
            case ContentType.Classes:
                ParseClassData(html, data);
                break;
            case ContentType.Spells:
                ParseSpellData(html, data);
                break;
            case ContentType.Feats:
                ParseFeatData(html, data);
                break;
            case ContentType.Equipment:
                ParseEquipmentData(html, data);
                break;
            case ContentType.Ancestry:
                ParseAncestryData(html, data);
                break;
            case ContentType.Backgrounds:
                ParseBackgroundData(html, data);
                break;
        }

        return data;
    }

    private void ParseClassData(string html, Dictionary<string, object> data)
    {
        data["hitPoints"] = ExtractValueFromPattern(html, @"Hit Points[:\s]*(\d+)", "8");
        data["keyAbility"] = ExtractValueFromPattern(html, @"Key Ability[:\s]*([^<\n]+)", "");
        data["proficiencies"] = ExtractSectionContent(html, "Proficiencies");
        data["classDC"] = ExtractValueFromPattern(html, @"Class DC[:\s]*([^<\n]+)", "");
        data["skills"] = ExtractValueFromPattern(html, @"Skills[:\s]*(\d+)", "2");
    }

    private void ParseSpellData(string html, Dictionary<string, object> data)
    {
        data["traditions"] = ExtractValueFromPattern(html, @"Traditions[:\s]*([^<\n]+)", "");
        data["cast"] = ExtractValueFromPattern(html, @"Cast[:\s]*([^<\n]+)", "");
        data["range"] = ExtractValueFromPattern(html, @"Range[:\s]*([^<\n]+)", "");
        data["area"] = ExtractValueFromPattern(html, @"Area[:\s]*([^<\n]+)", "");
        data["duration"] = ExtractValueFromPattern(html, @"Duration[:\s]*([^<\n]+)", "");
        data["level"] = ExtractValueFromPattern(html, @"Spell (\d+)", "1");
    }

    private void ParseFeatData(string html, Dictionary<string, object> data)
    {
        data["prerequisites"] = ExtractValueFromPattern(html, @"Prerequisites[:\s]*([^<\n]+)", "");
        data["frequency"] = ExtractValueFromPattern(html, @"Frequency[:\s]*([^<\n]+)", "");
        data["trigger"] = ExtractValueFromPattern(html, @"Trigger[:\s]*([^<\n]+)", "");
        data["requirements"] = ExtractValueFromPattern(html, @"Requirements[:\s]*([^<\n]+)", "");
        data["level"] = ExtractValueFromPattern(html, @"Level (\d+)", "1");
    }

    private void ParseEquipmentData(string html, Dictionary<string, object> data)
    {
        data["price"] = ExtractValueFromPattern(html, @"Price[:\s]*([^<\n]+)", "");
        data["bulk"] = ExtractValueFromPattern(html, @"Bulk[:\s]*([^<\n]+)", "");
        data["usage"] = ExtractValueFromPattern(html, @"Usage[:\s]*([^<\n]+)", "");
        data["category"] = ExtractValueFromPattern(html, @"Category[:\s]*([^<\n]+)", "");
        data["level"] = ExtractValueFromPattern(html, @"Level (\d+)", "0");
    }

    private void ParseAncestryData(string html, Dictionary<string, object> data)
    {
        data["abilityBoosts"] = ExtractValueFromPattern(html, @"Ability Boosts[:\s]*([^<\n]+)", "");
        data["abilityFlaw"] = ExtractValueFromPattern(html, @"Ability Flaw[:\s]*([^<\n]+)", "");
        data["hitPoints"] = ExtractValueFromPattern(html, @"Hit Points[:\s]*(\d+)", "8");
        data["size"] = ExtractValueFromPattern(html, @"Size[:\s]*([^<\n]+)", "Medium");
        data["speed"] = ExtractValueFromPattern(html, @"Speed[:\s]*([^<\n]+)", "25 feet");
        data["languages"] = ExtractValueFromPattern(html, @"Languages[:\s]*([^<\n]+)", "");
    }

    private void ParseBackgroundData(string html, Dictionary<string, object> data)
    {
        data["abilityBoosts"] = ExtractValueFromPattern(html, @"Ability Boosts[:\s]*([^<\n]+)", "");
        data["skillFeats"] = ExtractValueFromPattern(html, @"Skill Feats[:\s]*([^<\n]+)", "");
        data["lore"] = ExtractValueFromPattern(html, @"Lore[:\s]*([^<\n]+)", "");
        data["feat"] = ExtractValueFromPattern(html, @"Feat[:\s]*([^<\n]+)", "");
    }

    private string ExtractValueFromPattern(string html, string pattern, string defaultValue)
    {
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        return match.Success ? System.Web.HttpUtility.HtmlDecode(match.Groups[1].Value.Trim()) : defaultValue;
    }

    private string ExtractSectionContent(string html, string sectionName)
    {
        var pattern = $@"<[^>]*>{sectionName}[:\s]*</[^>]*>\s*<[^>]*>([^<]+)</[^>]*>";
        var match = Regex.Match(html, pattern, RegexOptions.IgnoreCase);
        return match.Success ? System.Web.HttpUtility.HtmlDecode(match.Groups[1].Value.Trim()) : "";
    }

    private string ExtractVersionFromHtml(string html)
    {
        // Try to extract version information, fallback to current date
        return DateTime.UtcNow.ToString("yyyy.MM.dd");
    }

    private List<string> ExtractTraitsFromHtml(string html)
    {
        var traits = new List<string>();
        var traitMatches = Regex.Matches(html, @"<span[^>]*class=['""]trait['""][^>]*>([^<]+)</span>", RegexOptions.IgnoreCase);
        
        foreach (Match match in traitMatches)
        {
            traits.Add(System.Web.HttpUtility.HtmlDecode(match.Groups[1].Value.Trim()));
        }

        return traits;
    }

    private Dictionary<string, string> ExtractMetadataFromHtml(string html)
    {
        var metadata = new Dictionary<string, string>
        {
            ["source"] = "Archives of Nethys",
            ["extractedAt"] = DateTime.UtcNow.ToString("O")
        };

        // Try to extract source book information
        var sourceMatch = Regex.Match(html, @"Source[:\s]*([^<\n]+)", RegexOptions.IgnoreCase);
        if (sourceMatch.Success)
        {
            metadata["sourceBook"] = System.Web.HttpUtility.HtmlDecode(sourceMatch.Groups[1].Value.Trim());
        }

        return metadata;
    }

    private List<SrdIndex> ParseIndexHtml(string html, ContentType contentType)
    {
        var items = new List<SrdIndex>();
        
        // Parse index page links - this would need to be customized based on actual SRD structure
        var linkPattern = contentType switch
        {
            ContentType.Classes => @"<a[^>]*href=['""]([^'""]*Classes\.aspx\?ID=\d+)['""][^>]*>([^<]+)</a>",
            ContentType.Spells => @"<a[^>]*href=['""]([^'""]*Spells\.aspx\?ID=\d+)['""][^>]*>([^<]+)</a>",
            ContentType.Feats => @"<a[^>]*href=['""]([^'""]*Feats\.aspx\?ID=\d+)['""][^>]*>([^<]+)</a>",
            ContentType.Equipment => @"<a[^>]*href=['""]([^'""]*Equipment\.aspx\?ID=\d+)['""][^>]*>([^<]+)</a>",
            ContentType.Ancestry => @"<a[^>]*href=['""]([^'""]*Ancestries\.aspx\?ID=\d+)['""][^>]*>([^<]+)</a>",
            ContentType.Backgrounds => @"<a[^>]*href=['""]([^'""]*Backgrounds\.aspx\?ID=\d+)['""][^>]*>([^<]+)</a>",
            _ => @"<a[^>]*href=['""]([^'""]*\?ID=\d+)['""][^>]*>([^<]+)</a>"
        };

        var matches = Regex.Matches(html, linkPattern, RegexOptions.IgnoreCase);
        
        foreach (Match match in matches)
        {
            var url = match.Groups[1].Value;
            var name = System.Web.HttpUtility.HtmlDecode(match.Groups[2].Value.Trim());
            
            items.Add(new SrdIndex
            {
                Name = name,
                Url = url.StartsWith("http") ? url : $"https://2e.aonprd.com{url}",
                Type = contentType,
                Category = contentType.ToString(),
                LastUpdated = DateTime.UtcNow,
                Checksum = GenerateChecksum(name + url)
            });
        }

        return items;
    }

    private string GenerateChecksum(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    private void ValidateClassContent(SrdContent content, SrdValidationResult result)
    {
        if (!content.ParsedData.ContainsKey("hitPoints"))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = "hitPoints",
                Message = "Hit Points not found in class data",
                Suggestion = "Verify the HTML parsing for Hit Points extraction"
            });
        }

        if (!content.ParsedData.ContainsKey("keyAbility"))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = "keyAbility",
                Message = "Key Ability not found in class data",
                Suggestion = "Verify the HTML parsing for Key Ability extraction"
            });
        }
    }

    private void ValidateSpellContent(SrdContent content, SrdValidationResult result)
    {
        if (!content.ParsedData.ContainsKey("traditions"))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = "traditions",
                Message = "Spell traditions not found",
                Suggestion = "Verify the HTML parsing for traditions extraction"
            });
        }
    }

    private void ValidateFeatContent(SrdContent content, SrdValidationResult result)
    {
        if (!content.ParsedData.ContainsKey("level"))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = "level",
                Message = "Feat level not found",
                Suggestion = "Verify the HTML parsing for level extraction"
            });
        }
    }

    private void ValidateEquipmentContent(SrdContent content, SrdValidationResult result)
    {
        if (!content.ParsedData.ContainsKey("price"))
        {
            result.Warnings.Add(new ValidationWarning
            {
                Field = "price",
                Message = "Equipment price not found",
                Suggestion = "Verify the HTML parsing for price extraction"
            });
        }
    }
}

public class SrdConfiguration
{
    public string BaseUrl { get; set; } = "https://2e.aonprd.com";
    public int RequestDelayMs { get; set; } = 1000; // Be respectful to the server
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromMinutes(5);
    public bool EnableCaching { get; set; } = true;
    public TimeSpan CacheExpiry { get; set; } = TimeSpan.FromHours(24);
}