using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Application.CharacterCreation.Models;
using PathfinderCampaignManager.Application.CharacterCreation.Services;

namespace PathfinderCampaignManager.Presentation.Client.Components.CharacterCreation.Steps;

public partial class ClassSelectionStep : ComponentBase
{
    [Parameter] public CharacterBuilder Character { get; set; } = null!;
    [Parameter] public EventCallback OnSelectionChanged { get; set; }

    [Inject] private ICharacterCreationService CreationService { get; set; } = null!;

    private List<PathfinderClass> _availableClasses = new();
    private List<PathfinderClass> _filteredClasses = new();
    private PathfinderClass? _selectedClass;
    private PathfinderClass? _hoveredClass;
    private bool _isLoading = true;
    private string? _errorMessage;
    private string _searchQuery = string.Empty;
    private string _selectedRole = "all";
    private bool _showOnlyCoreClasses = true;

    // Class role categories for filtering
    private readonly Dictionary<string, List<string>> _classRoles = new()
    {
        ["defender"] = new() { "champion", "monk", "fighter" },
        ["striker"] = new() { "barbarian", "ranger", "rogue", "gunslinger", "swashbuckler" },
        ["controller"] = new() { "wizard", "witch", "kineticist", "psychic" },
        ["leader"] = new() { "bard", "cleric", "oracle" },
        ["support"] = new() { "alchemist", "investigator", "thaumaturge" },
        ["hybrid"] = new() { "druid", "magus", "summoner", "inventor" }
    };

    // Computed properties
    private PathfinderClass? SelectedClass
    {
        get => _selectedClass ?? Character?.SelectedClass;
        set
        {
            _selectedClass = value;
            if (Character != null)
            {
                Character.SelectedClass = value;
            }
        }
    }

    private List<PathfinderClass> FilteredClasses => _filteredClasses;
    private bool IsLoading => _isLoading;
    private string? ErrorMessage => _errorMessage;
    private string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            FilterClasses();
        }
    }
    
    private string SelectedRole
    {
        get => _selectedRole;
        set
        {
            _selectedRole = value;
            FilterClasses();
        }
    }

    private bool ShowOnlyCoreClasses
    {
        get => _showOnlyCoreClasses;
        set
        {
            _showOnlyCoreClasses = value;
            FilterClasses();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadAvailableClasses();
    }

    private async Task LoadAvailableClasses()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            _availableClasses = await CreationService.GetAvailableClassesAsync();
            FilterClasses();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load classes: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void FilterClasses()
    {
        var filtered = _availableClasses.AsEnumerable();

        // Filter by search query
        if (!string.IsNullOrWhiteSpace(_searchQuery))
        {
            var query = _searchQuery.ToLowerInvariant();
            filtered = filtered.Where(c => 
                c.Name.ToLowerInvariant().Contains(query) ||
                c.Description.ToLowerInvariant().Contains(query) ||
                c.Traits.Any(t => t.ToLowerInvariant().Contains(query))
            );
        }

        // Filter by role
        if (_selectedRole != "all" && _classRoles.ContainsKey(_selectedRole))
        {
            var roleClasses = _classRoles[_selectedRole];
            filtered = filtered.Where(c => roleClasses.Contains(c.Id.ToLowerInvariant()));
        }

        // Filter by core classes only
        if (_showOnlyCoreClasses)
        {
            filtered = filtered.Where(c => c.Rarity == "Common" || c.Source == "Core Rulebook");
        }

        _filteredClasses = filtered.OrderBy(c => c.Name).ToList();
    }

    private async Task SelectClass(PathfinderClass selectedClass)
    {
        SelectedClass = selectedClass;
        await OnSelectionChanged.InvokeAsync();
    }

    private void SetHoveredClass(PathfinderClass? pathfinderClass)
    {
        _hoveredClass = pathfinderClass;
    }

    private string GetClassCardClass(PathfinderClass pathfinderClass)
    {
        var baseClass = "class-card";
        
        if (SelectedClass?.Id == pathfinderClass.Id)
        {
            baseClass += " selected";
        }
        
        if (_hoveredClass?.Id == pathfinderClass.Id)
        {
            baseClass += " hovered";
        }

        return baseClass;
    }

    private string GetRoleDisplayName(string role)
    {
        return role switch
        {
            "defender" => "Defender",
            "striker" => "Striker", 
            "controller" => "Controller",
            "leader" => "Leader",
            "support" => "Support",
            "hybrid" => "Hybrid",
            _ => "All Roles"
        };
    }

    private string GetClassRole(PathfinderClass pathfinderClass)
    {
        foreach (var role in _classRoles)
        {
            if (role.Value.Contains(pathfinderClass.Id.ToLowerInvariant()))
            {
                return GetRoleDisplayName(role.Key);
            }
        }
        return "Hybrid";
    }

    private List<string> GetClassKeyAbilities(PathfinderClass pathfinderClass)
    {
        return pathfinderClass.KeyAbilities?.Select(a => a.ToString()).ToList() ?? new List<string>();
    }

    private string GetSpellcastingInfo(PathfinderClass pathfinderClass)
    {
        if (!pathfinderClass.IsSpellcaster)
            return "Non-spellcaster";

        return $"{pathfinderClass.SpellcastingTradition} ({pathfinderClass.SpellcastingAbility})";
    }
}