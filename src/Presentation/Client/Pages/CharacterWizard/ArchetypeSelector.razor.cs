using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Enums;
using System.Net.Http.Json;
using System.Text.Json;

namespace PathfinderCampaignManager.Presentation.Client.Pages.CharacterWizard;

public partial class ArchetypeSelector : ComponentBase
{
    [Parameter] public int CurrentLevel { get; set; } = 2;
    [Parameter] public bool IsFreeArchetype { get; set; } = false;
    [Parameter] public ICalculatedCharacter? Character { get; set; }
    [Parameter] public PfArchetype? SelectedArchetype { get; set; }
    [Parameter] public EventCallback<PfArchetype?> SelectedArchetypeChanged { get; set; }
    [Parameter] public EventCallback<PfArchetype> OnArchetypeSelected { get; set; }

    private List<PfArchetype> _allArchetypes = new();
    private IEnumerable<PfArchetype> _multiclassArchetypes = Enumerable.Empty<PfArchetype>();
    private IEnumerable<PfArchetype> _generalArchetypes = Enumerable.Empty<PfArchetype>();
    private IEnumerable<PfArchetype> _classArchetypes = Enumerable.Empty<PfArchetype>();
    
    private string _selectedTab = "multiclass";
    private string _searchTerm = string.Empty;
    private string _selectedSource = string.Empty;
    private bool _isLoading = true;
    private string _errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadArchetypes();
    }

    private async Task LoadArchetypes()
    {
        try
        {
            _isLoading = true;
            _errorMessage = string.Empty;
            StateHasChanged();

            var response = await Http.GetAsync("api/archetypes");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _allArchetypes = JsonSerializer.Deserialize<List<PfArchetype>>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new List<PfArchetype>();

                // Group archetypes by type
                _multiclassArchetypes = _allArchetypes.Where(a => a.Type == ArchetypeType.Multiclass);
                _generalArchetypes = _allArchetypes.Where(a => a.Type == ArchetypeType.General);
                _classArchetypes = _allArchetypes.Where(a => a.Type == ArchetypeType.Class);
            }
            else
            {
                _errorMessage = "Failed to load archetypes. Please try again.";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error loading archetypes: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void SetActiveTab(string tab)
    {
        _selectedTab = tab;
        StateHasChanged();
    }

    private IEnumerable<PfArchetype> GetFilteredArchetypes()
    {
        var archetypes = _selectedTab switch
        {
            "multiclass" => _multiclassArchetypes,
            "general" => _generalArchetypes,
            "class" => _classArchetypes,
            _ => _multiclassArchetypes
        };

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(_searchTerm))
        {
            archetypes = archetypes.Where(a => 
                a.Name.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                a.Description.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                a.Traits.Any(t => t.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)));
        }

        // Apply source filter
        if (!string.IsNullOrWhiteSpace(_selectedSource))
        {
            archetypes = archetypes.Where(a => a.Source.Equals(_selectedSource, StringComparison.OrdinalIgnoreCase));
        }

        // Filter out archetypes the character can't take
        if (Character != null)
        {
            archetypes = archetypes.Where(CanSelect);
        }

        return archetypes.OrderBy(a => a.Name);
    }

    private async Task OnSearchChanged(ChangeEventArgs e)
    {
        _searchTerm = e.Value?.ToString() ?? string.Empty;
        StateHasChanged();
    }

    private async Task OnSourceChangedAsync()
    {
        StateHasChanged();
    }

    private bool IsSelected(PfArchetype archetype)
    {
        return SelectedArchetype?.Id == archetype.Id;
    }

    private bool CanSelect(PfArchetype archetype)
    {
        if (Character == null) return true;

        // Check if character already has this archetype
        var hasArchetype = Character.AvailableFeats.Any(f => 
            f == archetype.DedicationFeatId || 
            archetype.ArchetypeFeatIds.Contains(f));

        if (hasArchetype) return false;

        // Check prerequisites
        foreach (var prerequisite in archetype.Prerequisites)
        {
            if (!ValidatePrerequisite(prerequisite))
            {
                return false;
            }
        }

        // Check if character can take a new archetype (two feat rule)
        if (IsFreeArchetype)
        {
            return true; // Free archetype bypasses the two feat rule
        }

        return CanTakeNewArchetype();
    }

    private bool ValidatePrerequisite(PfPrerequisite prerequisite)
    {
        if (Character == null) return true;

        return prerequisite.Type switch
        {
            "AbilityScore" => ValidateAbilityScorePrerequisite(prerequisite),
            "Skill" => ValidateSkillPrerequisite(prerequisite),
            "Level" => Character.Level >= int.Parse(prerequisite.Value),
            "Feat" => Character.AvailableFeats.Contains(prerequisite.Target),
            _ => true
        };
    }

    private bool ValidateAbilityScorePrerequisite(PfPrerequisite prerequisite)
    {
        if (Character == null) return true;

        if (!Character.AbilityScores.TryGetValue(prerequisite.Target, out var score))
            return false;

        var requiredScore = int.Parse(prerequisite.Value);
        return prerequisite.Operator switch
        {
            ">=" => score >= requiredScore,
            ">" => score > requiredScore,
            "=" => score == requiredScore,
            "<=" => score <= requiredScore,
            "<" => score < requiredScore,
            _ => false
        };
    }

    private bool ValidateSkillPrerequisite(PfPrerequisite prerequisite)
    {
        if (Character == null) return true;
        
        if (!Character.Proficiencies.TryGetValue(prerequisite.Target, out var proficiency))
            return false;

        var requiredRank = prerequisite.Value switch
        {
            "Trained" => ProficiencyLevel.Trained,
            "Expert" => ProficiencyLevel.Expert,
            "Master" => ProficiencyLevel.Master,
            "Legendary" => ProficiencyLevel.Legendary,
            _ => ProficiencyLevel.Untrained
        };

        return proficiency >= requiredRank;
    }

    private bool CanTakeNewArchetype()
    {
        if (Character == null) return true;

        // Get archetype feats the character has
        var archetypeFeats = Character.AvailableFeats.Where(f => 
            f.Contains("archetype", StringComparison.OrdinalIgnoreCase) ||
            f.Contains("dedication", StringComparison.OrdinalIgnoreCase) ||
            f.Contains("multiclass", StringComparison.OrdinalIgnoreCase));

        // Group by archetype
        var archetypeGroups = archetypeFeats.GroupBy(f => GetArchetypeFromFeatId(f));

        foreach (var group in archetypeGroups)
        {
            if (group.Count() < 2)
            {
                // Need at least 2 feats from an archetype before taking another
                return false;
            }
        }

        return true;
    }

    private string GetArchetypeFromFeatId(string featId)
    {
        // Simple heuristic to extract archetype from feat ID
        if (featId.Contains("barbarian")) return "barbarian";
        if (featId.Contains("fighter")) return "fighter";
        if (featId.Contains("wizard")) return "wizard";
        return "unknown";
    }

    private async Task SelectArchetype(PfArchetype archetype)
    {
        if (!CanSelect(archetype)) return;

        SelectedArchetype = archetype;
        await SelectedArchetypeChanged.InvokeAsync(archetype);
        await OnArchetypeSelected.InvokeAsync(archetype);
        StateHasChanged();
    }

    private async Task ClearSelection()
    {
        SelectedArchetype = null;
        await SelectedArchetypeChanged.InvokeAsync(null);
        StateHasChanged();
    }

    private void ViewDetails(PfArchetype archetype)
    {
        // Navigate to archetype details page or show modal
        // For now, just log the archetype details
        Console.WriteLine($"Viewing details for archetype: {archetype.Name}");
    }

    private string GetArchetypeIcon(PfArchetype archetype)
    {
        return archetype.Type switch
        {
            ArchetypeType.Multiclass => GetMulticlassIcon(archetype.AssociatedClassId),
            ArchetypeType.Class => "fas fa-shield-alt",
            ArchetypeType.General => "fas fa-star",
            _ => "fas fa-question"
        };
    }

    private string GetMulticlassIcon(string? classId)
    {
        return classId?.ToLowerInvariant() switch
        {
            "barbarian" => "fas fa-hammer",
            "fighter" => "fas fa-sword",
            "wizard" => "fas fa-hat-wizard",
            "rogue" => "fas fa-mask",
            "ranger" => "fas fa-bow-arrow",
            "cleric" => "fas fa-cross",
            _ => "fas fa-users"
        };
    }

    private string GetArchetypeTypeBadgeColor(ArchetypeType type)
    {
        return type switch
        {
            ArchetypeType.Multiclass => "primary",
            ArchetypeType.Class => "success",
            ArchetypeType.General => "warning",
            _ => "secondary"
        };
    }

    private string GetClassName(string? classId)
    {
        return classId?.ToLowerInvariant() switch
        {
            "barbarian" => "Barbarian",
            "fighter" => "Fighter",
            "wizard" => "Wizard",
            "rogue" => "Rogue",
            "ranger" => "Ranger",
            "cleric" => "Cleric",
            _ => classId ?? "Unknown"
        };
    }

    private string FormatPrerequisite(PfPrerequisite prerequisite)
    {
        return prerequisite.Type switch
        {
            "AbilityScore" => $"{prerequisite.Target} {prerequisite.Value}+",
            "Skill" => $"{prerequisite.Target} ({prerequisite.Value})",
            "Level" => $"Level {prerequisite.Value}+",
            "Feat" => prerequisite.Target,
            _ => $"{prerequisite.Type}: {prerequisite.Target}"
        };
    }
}