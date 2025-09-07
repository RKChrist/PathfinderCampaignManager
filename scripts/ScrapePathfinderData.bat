@echo off
REM Pathfinder 2e Data Scraper Batch Script
REM Simple wrapper around the PowerShell script

setlocal enabledelayedexpansion

echo.
echo 🎲 Pathfinder 2e Data Scraper
echo =============================
echo.

REM Check if PowerShell is available
powershell -Command "Write-Host 'PowerShell available'" >nul 2>&1
if errorlevel 1 (
    echo ❌ PowerShell not found. Please install PowerShell or use the C# files directly.
    pause
    exit /b 1
)

REM Get the directory of this batch file
set "SCRIPT_DIR=%~dp0"

REM Default parameters
set "OUTPUT_DIR=./pathfinder_data"
set "CATEGORY="
set "FORMAT=json"
set "SUMMARY_ONLY="
set "VERBOSE="
set "SHOW_HELP="

REM Parse command line arguments
:parse_args
if "%~1"=="" goto :run_scraper
if /I "%~1"=="--help" set "SHOW_HELP=1" & goto :next_arg
if /I "%~1"=="-h" set "SHOW_HELP=1" & goto :next_arg
if /I "%~1"=="--output" set "OUTPUT_DIR=%~2" & shift & goto :next_arg
if /I "%~1"=="--category" set "CATEGORY=%~2" & shift & goto :next_arg
if /I "%~1"=="--format" set "FORMAT=%~2" & shift & goto :next_arg
if /I "%~1"=="--summary-only" set "SUMMARY_ONLY=1" & goto :next_arg
if /I "%~1"=="--verbose" set "VERBOSE=1" & goto :next_arg

:next_arg
shift
goto :parse_args

:run_scraper
if defined SHOW_HELP (
    echo USAGE:
    echo   ScrapePathfinderData.bat [options]
    echo.
    echo OPTIONS:
    echo   --help              Show this help message
    echo   --output DIR        Output directory default: ./pathfinder_data
    echo   --category NAME     Specific category to scrape
    echo   --format TYPE       Output format: json, filtered default: json
    echo   --summary-only      Generate only data summary
    echo   --verbose           Enable detailed logging
    echo.
    echo EXAMPLES:
    echo   ScrapePathfinderData.bat
    echo   ScrapePathfinderData.bat --category class
    echo   ScrapePathfinderData.bat --format filtered
    echo   ScrapePathfinderData.bat --output ./my_pf2e_data
    echo.
    pause
    exit /b 0
)

REM Build PowerShell command
set "PS_CMD=& '%SCRIPT_DIR%ScrapePathfinderData.ps1'"

if defined OUTPUT_DIR (
    set "PS_CMD=!PS_CMD! -OutputDir '%OUTPUT_DIR%'"
)

if defined CATEGORY (
    set "PS_CMD=!PS_CMD! -Category '%CATEGORY%'"
)

if defined FORMAT (
    if not "%FORMAT%"=="json" (
        set "PS_CMD=!PS_CMD! -Format '%FORMAT%'"
    )
)

if defined SUMMARY_ONLY (
    set "PS_CMD=!PS_CMD! -SummaryOnly"
)

if defined VERBOSE (
    set "PS_CMD=!PS_CMD! -Verbose"
)

echo 🚀 Running Pathfinder 2e Data Scraper...
echo Command: !PS_CMD!
echo.

REM Execute PowerShell script
powershell -ExecutionPolicy Bypass -Command "!PS_CMD!"

if errorlevel 1 (
    echo.
    echo ❌ Scraping failed. Check the output above for details.
    pause
    exit /b 1
)

echo.
echo 🎉 Scraping completed successfully!
echo.
echo You can now use the JSON files in your Pathfinder Campaign Manager application.
echo.
pause