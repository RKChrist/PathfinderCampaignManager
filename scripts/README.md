# Pathfinder 2e Data Scraper

A comprehensive tool for scraping Pathfinder 2nd Edition game data from the Archives of Nethys (AON) Elasticsearch backend. This scraper retrieves classes, ancestries, feats, spells, equipment, and other game data in structured JSON format.

## 🎯 Features

- **Comprehensive Data Collection**: Scrapes all major Pathfinder 2e categories
- **Multiple Output Formats**: Raw JSON or filtered data optimized for character creation
- **Category Filtering**: Scrape specific categories (classes, feats, spells, etc.)
- **Data Validation**: Built-in error handling and data validation
- **Performance Optimized**: Respectful scraping with rate limiting
- **Cross-Platform**: Works on Windows, macOS, and Linux

## 📊 Available Data Categories

- **Character Creation**: Classes, Ancestries, Backgrounds, Skills
- **Character Advancement**: Feats (by type), Spells (by class/level)
- **Equipment**: Weapons, Armor, Equipment, Items
- **Game Content**: Actions, Creatures, Hazards, Traits
- **Rules**: Archetypes, Deities, Rules Text

## 🚀 Quick Start

### Option 1: PowerShell Script (Recommended)
```powershell
# Scrape all data
.\ScrapePathfinderData.ps1

# Scrape specific category
.\ScrapePathfinderData.ps1 -Category "class"

# Get character creation optimized data
.\ScrapePathfinderData.ps1 -Format "filtered"

# Custom output directory
.\ScrapePathfinderData.ps1 -OutputDir "./my_pf2e_data"
```

### Option 2: Batch File (Windows)
```cmd
REM Scrape all data
ScrapePathfinderData.bat

REM Scrape specific category
ScrapePathfinderData.bat --category class

REM Show help
ScrapePathfinderData.bat --help
```

### Option 3: Direct C# Execution
```bash
# Build and run the scraper directly
dotnet build PathfinderDataScraper.csproj
dotnet run -- --output "./pathfinder_data" --verbose
```

## 📋 Command Line Options

| Option | Description | Default |
|--------|-------------|---------|
| `--output` | Output directory for scraped data | `./pathfinder_data` |
| `--category` | Specific category to scrape | All categories |
| `--format` | Output format (`json` or `filtered`) | `json` |
| `--summary-only` | Generate only data summary | `false` |
| `--verbose` | Enable detailed logging | `false` |
| `--help` | Show help information | - |

## 📁 Output Files

### Standard Format (`--format json`)
- `class.json` - All character classes
- `ancestry.json` - All ancestries/races  
- `background.json` - All character backgrounds
- `feat.json` - All feats (uncategorized)
- `skill.json` - All skills
- `spell.json` - All spells
- `weapon.json` - All weapons
- `armor.json` - All armor
- `equipment.json` - All equipment
- `creature.json` - All creatures/NPCs
- `action.json` - All actions
- `trait.json` - All traits
- `pathfinder_2e_complete.json` - All data combined
- `data_summary.json` - Statistics and metadata

### Filtered Format (`--format filtered`)
- `pathfinder_2e_filtered.json` - Optimized for character creation with:
  - Classes organized by role
  - Ancestries with related backgrounds
  - Feats categorized by type (Class, Ancestry, Skill, General, Archetype)
  - Spells organized by class and level
  - Equipment categorized by type

## 🔧 Integration Examples

### Load Classes for Character Creation
```csharp
using var scraper = new PathfinderDataScraper();
var classes = await scraper.ScrapeDataCategoryAsync("class");

// Filter to core player classes
var playerClasses = classes
    .Where(c => c.Rarity != "Uncommon" && c.Rarity != "Rare")
    .OrderBy(c => c.Name)
    .ToList();
```

### Get Feats by Level and Type
```csharp
using var scraper = new PathfinderDataScraper();
var gameData = await scraper.GetFilteredGameDataAsync();

// Access categorized feats
var classFeatsByLevel = gameData.Feats["Class"]
    .GroupBy(f => f.Level ?? 1)
    .ToDictionary(g => g.Key, g => g.ToList());
```

### Load Spells for Spellcasters
```csharp
using var scraper = new PathfinderDataScraper();
var spells = await scraper.ScrapeDataCategoryAsync("spell");

// Get wizard spells by level
var wizardSpells = spells
    .Where(s => s.SpellType?.Contains("Wizard") == true)
    .GroupBy(s => s.Level ?? 0)
    .ToDictionary(g => g.Key, g => g.ToList());
```

## 🏗️ Data Structure

Each scraped item follows this structure:

```csharp
public class PathfinderDataItem
{
    public string Name { get; set; }           // Item name
    public string Category { get; set; }       // Data category
    public int? Level { get; set; }           // Level requirement
    public string[] Traits { get; set; }      // Associated traits
    public string Rarity { get; set; }        // Rarity (Common, Uncommon, Rare)
    public string Source { get; set; }        // Source book
    public string Description { get; set; }   // Description text
    public string Url { get; set; }          // AON URL
    
    // Category-specific fields
    public int? HP { get; set; }              // Hit points (creatures)
    public int? AC { get; set; }              // Armor class
    public object Price { get; set; }         // Item price
    public object Damage { get; set; }        // Weapon damage
    public string School { get; set; }        // Spell school
    public string Duration { get; set; }      // Spell duration
    
    // Extensible additional data
    public Dictionary<string, JsonElement> AdditionalData { get; set; }
}
```

## 🔄 Update Frequency

- **Manual Updates**: Run the scraper whenever you need fresh data
- **Automated Updates**: Set up a scheduled task to run daily/weekly
- **Cache Duration**: AON data is cached for performance (20 minutes server-side)

## ⚡ Performance Tips

1. **Use Category Filtering**: Scrape only needed categories for faster execution
2. **Filtered Format**: Use for character creation apps (smaller file size)
3. **Rate Limiting**: Built-in delays prevent server overload
4. **Local Caching**: Re-use scraped data files when possible

## 🛠️ Prerequisites

- **.NET 8.0 or later** - [Download here](https://dotnet.microsoft.com/download)
- **Internet Connection** - Required to access Archives of Nethys
- **PowerShell** - For running the PowerShell script (Windows/macOS/Linux)

## 📜 Legal & Attribution

This scraper accesses publicly available data from the [Archives of Nethys](https://2e.aonprd.com/), which hosts the official Pathfinder 2nd Edition System Reference Document (SRD).

**Important Notes:**
- All Pathfinder content is owned by Paizo Inc.
- Use scraped data in accordance with the [Open Gaming License](https://paizo.com/community/communityuse)
- This tool is for personal and community use
- Be respectful with scraping frequency

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Add new data categories or improve existing ones
4. Submit a pull request

## 🐛 Troubleshooting

### Common Issues

**"Could not connect to Archives of Nethys"**
- Check internet connection
- Verify AON website is accessible
- Try again later (server may be busy)

**"Build failed" errors**
- Ensure .NET 8.0+ is installed
- Check NuGet package restoration
- Run `dotnet restore` manually

**"No data returned"**
- Verify category name spelling
- Check if AON has updated their structure
- Try different category names

### Getting Help

1. Check the generated `data_summary.json` for statistics
2. Run with `--verbose` flag for detailed logging
3. Verify output files are being created
4. Check file permissions in output directory

## 📈 Roadmap

- [ ] Add support for custom data filtering
- [ ] Implement incremental updates
- [ ] Add data validation and cleanup
- [ ] Create GUI interface
- [ ] Add support for homebrew content
- [ ] Integration with character sheet applications

---

*Happy gaming with your Pathfinder 2e data! 🎲*