# Pathfinder 2e Data Scraper PowerShell Script
# This script compiles and runs the C# PathfinderDataScraper

param(
    [string]$OutputDir = "./pathfinder_data",
    [string]$Category = "",
    [string]$Format = "json",
    [switch]$SummaryOnly,
    [switch]$Verbose,
    [switch]$Help
)

function Show-Help {
    Write-Host "🎲 Pathfinder 2e Data Scraper" -ForegroundColor Cyan
    Write-Host "=============================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "DESCRIPTION:" -ForegroundColor Yellow
    Write-Host "  Scrapes Pathfinder 2e game data from Archives of Nethys"
    Write-Host "  Retrieves classes, ancestries, feats, spells, equipment, and more"
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "  .\ScrapePathfinderData.ps1 [parameters]"
    Write-Host ""
    Write-Host "PARAMETERS:" -ForegroundColor Yellow
    Write-Host "  -OutputDir <path>     Output directory (default: ./pathfinder_data)"
    Write-Host "  -Category <name>      Specific category to scrape:"
    Write-Host "                          class, ancestry, background, feat, skill,"
    Write-Host "                          spell, action, weapon, armor, equipment, etc."
    Write-Host "  -Format <type>        Output format: json, filtered (default: json)"
    Write-Host "  -SummaryOnly          Generate only data summary without scraping"
    Write-Host "  -Verbose              Enable detailed logging"
    Write-Host "  -Help                 Show this help message"
    Write-Host ""
    Write-Host "EXAMPLES:" -ForegroundColor Yellow
    Write-Host "  .\ScrapePathfinderData.ps1                           # Scrape all data"
    Write-Host "  .\ScrapePathfinderData.ps1 -Category class           # Scrape only classes"
    Write-Host "  .\ScrapePathfinderData.ps1 -Format filtered          # Get character creation data"
    Write-Host "  .\ScrapePathfinderData.ps1 -SummaryOnly              # Generate summary only"
    Write-Host "  .\ScrapePathfinderData.ps1 -OutputDir ./pf2e_data   # Custom output directory"
    Write-Host ""
    Write-Host "OUTPUT FILES:" -ForegroundColor Yellow
    Write-Host "  - Individual JSON files for each category (class.json, ancestry.json, etc.)"
    Write-Host "  - pathfinder_2e_complete.json (all data combined)"
    Write-Host "  - pathfinder_2e_filtered.json (character creation optimized)"
    Write-Host "  - data_summary.json (statistics and metadata)"
}

if ($Help) {
    Show-Help
    exit 0
}

# Check if .NET is available
try {
    $dotnetVersion = dotnet --version 2>$null
    if (-not $dotnetVersion) {
        Write-Error "❌ .NET SDK not found. Please install .NET 8.0 or later."
        exit 1
    }
    Write-Host "✅ Using .NET version: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Error "❌ .NET SDK not found. Please install .NET 8.0 or later."
    exit 1
}

# Get script directory
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectDir = Split-Path -Parent $scriptDir

Write-Host "🎲 Pathfinder 2e Data Scraper" -ForegroundColor Cyan
Write-Host "=============================" -ForegroundColor Cyan
Write-Host ""

# Create temporary project file for the scraper
$tempProjectFile = Join-Path $scriptDir "PathfinderDataScraper.csproj"

$projectContent = @"
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
    <PackageReference Include="System.Text.Json" Version="8.0.0" />
  </ItemGroup>
</Project>
"@

try {
    # Write project file
    $projectContent | Out-File -FilePath $tempProjectFile -Encoding UTF8
    
    # Create Program.cs that calls our runner
    $programFile = Join-Path $scriptDir "Program.cs"
    $programContent = @"
using PathfinderCampaignManager.Scripts;

namespace PathfinderCampaignManager.Scripts
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            return await PathfinderDataScraperRunner.Main(args);
        }
    }
}
"@
    
    $programContent | Out-File -FilePath $programFile -Encoding UTF8
    
    Write-Host "🔧 Building scraper..." -ForegroundColor Yellow
    
    # Build the project
    $buildResult = & dotnet build $tempProjectFile --configuration Release --verbosity quiet 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "❌ Failed to build scraper. Error: $buildResult"
        exit 1
    }
    
    Write-Host "✅ Build successful" -ForegroundColor Green
    Write-Host ""
    
    # Prepare arguments
    $scraperArgs = @("--output", $OutputDir)
    
    if ($Category) {
        $scraperArgs += @("--category", $Category)
    }
    
    if ($Format -ne "json") {
        $scraperArgs += @("--format", $Format)
    }
    
    if ($SummaryOnly) {
        $scraperArgs += "--summary-only"
    }
    
    if ($Verbose) {
        $scraperArgs += "--verbose"
    }
    
    # Run the scraper
    Write-Host "🚀 Starting data scraping..." -ForegroundColor Yellow
    $runResult = & dotnet run --project $tempProjectFile --configuration Release -- @scraperArgs
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "🎉 Scraping completed successfully!" -ForegroundColor Green
        Write-Host "📁 Data saved to: $(Resolve-Path $OutputDir)" -ForegroundColor Green
        
        # Show what files were created
        if (Test-Path $OutputDir) {
            $files = Get-ChildItem $OutputDir -Filter "*.json" | Sort-Object Name
            if ($files.Count -gt 0) {
                Write-Host ""
                Write-Host "📄 Generated files:" -ForegroundColor Cyan
                foreach ($file in $files) {
                    $size = [math]::Round($file.Length / 1KB, 1)
                    Write-Host "  - $($file.Name) ($size KB)" -ForegroundColor Gray
                }
            }
        }
    } else {
        Write-Error "❌ Scraping failed with exit code: $LASTEXITCODE"
        exit $LASTEXITCODE
    }
    
} catch {
    Write-Error "❌ An error occurred: $($_.Exception.Message)"
    exit 1
} finally {
    # Clean up temporary files
    if (Test-Path $tempProjectFile) {
        Remove-Item $tempProjectFile -Force
    }
    if (Test-Path $programFile) {
        Remove-Item $programFile -Force
    }
    
    # Clean up build artifacts
    $binDir = Join-Path $scriptDir "bin"
    $objDir = Join-Path $scriptDir "obj"
    if (Test-Path $binDir) {
        Remove-Item $binDir -Recurse -Force
    }
    if (Test-Path $objDir) {
        Remove-Item $objDir -Recurse -Force
    }
}

Write-Host ""
Write-Host "🎲 Happy gaming with your Pathfinder 2e data!" -ForegroundColor Cyan