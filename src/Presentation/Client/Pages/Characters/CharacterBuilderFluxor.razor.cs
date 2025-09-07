using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using PathfinderCampaignManager.Presentation.Client.Store.Pathfinder;
using Fluxor.Blazor.Web.Components;
using System.Net.Http.Json;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Characters;

public partial class CharacterBuilderFluxor : FluxorComponent
{
    [Parameter] public Guid? CharacterId { get; set; }
    
    protected PfCharacter Character { get; set; } = new();
    protected int ActiveStep { get; set; } = 0;
    protected string AbilityMethod { get; set; } = "array";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        if (CharacterId.HasValue)
        {
            // Load existing character
            await LoadCharacter(CharacterId.Value);
        }
        else
        {
            // Initialize new character
            InitializeNewCharacter();
        }

        // Check if we need to load Pathfinder data
        if (!PathfinderState.Value.AllDataLoaded && !PathfinderState.Value.IsLoading)
        {
            LoadData();
        }
    }

    private async Task LoadCharacter(Guid characterId)
    {
        try
        {
            var character = await Http.GetFromJsonAsync<PfCharacter>($"api/characters/{characterId}");
            if (character != null)
            {
                Character = character;
            }
        }
        catch (Exception)
        {
            // Handle error - character not found or other issues
            InitializeNewCharacter();
        }
    }

    private void InitializeNewCharacter()
    {
        Character = new PfCharacter
        {
            Level = 1,
            AbilityScores = new PfAbilityScores
            {
                Strength = 10,
                Dexterity = 10,
                Constitution = 10,
                Intelligence = 10,
                Wisdom = 10,
                Charisma = 10
            }
        };
        
        // Initialize skills when skills are loaded
        if (PathfinderState.Value.SkillsLoaded)
        {
            InitializeCharacterSkills();
        }
    }

    private void InitializeCharacterSkills()
    {
        Character.Skills.Clear();
        foreach (var skill in PathfinderState.Value.Skills)
        {
            Character.Skills.Add(new PfCharacterSkill
            {
                SkillName = skill.Name,
                Proficiency = new PfProficiency { Rank = ProficiencyRank.Untrained }
            });
        }
    }

    protected void LoadData()
    {
        Dispatcher.Dispatch(new LoadAllPathfinderDataAction());
    }

    protected void SetActiveStep(int step)
    {
        ActiveStep = step;
    }

    protected void NextStep()
    {
        if (ActiveStep < 5)
        {
            ActiveStep++;
        }
    }

    protected void PreviousStep()
    {
        if (ActiveStep > 0)
        {
            ActiveStep--;
        }
    }

    protected void SelectAncestry(PathfinderCampaignManager.Domain.Entities.Pathfinder.PfAncestry ancestry)
    {
        Character.Ancestry = ancestry.Name;
        if (ancestry.Heritages.Any())
        {
            Character.Heritage = ancestry.Heritages.First().Name;
        }
        StateHasChanged();
    }

    protected void SelectBackground(PathfinderCampaignManager.Domain.Entities.Pathfinder.PfBackground background)
    {
        Character.Background = background.Name;
        
        // Apply background skill training if skills are loaded
        if (PathfinderState.Value.SkillsLoaded)
        {
            foreach (var skillName in background.SkillProficiencies)
            {
                var characterSkill = Character.Skills.FirstOrDefault(s => s.SkillName == skillName);
                if (characterSkill != null && characterSkill.Proficiency.Rank == ProficiencyRank.Untrained)
                {
                    characterSkill.Proficiency.Rank = ProficiencyRank.Trained;
                }
            }
        }
        
        StateHasChanged();
    }

    protected void SelectClass(PathfinderCampaignManager.Domain.Entities.Pathfinder.PfClass pfClass)
    {
        Character.ClassName = pfClass.Name;
        
        // Set class features
        Character.ClassFeatures = pfClass.ClassFeaturesByLevel
            .Where(kvp => kvp.Key <= Character.Level)
            .SelectMany(kvp => kvp.Value)
            .Select(cf => cf.Name)
            .ToList();
        
        StateHasChanged();
    }

    protected void ToggleFeat(PfFeat feat)
    {
        if (Character.SelectedFeats.Contains(feat.Name))
        {
            Character.SelectedFeats.Remove(feat.Name);
        }
        else
        {
            Character.SelectedFeats.Add(feat.Name);
        }
        StateHasChanged();
    }

    protected PfCharacterSkill GetCharacterSkill(string skillName)
    {
        var skill = Character.Skills.FirstOrDefault(s => s.SkillName == skillName);
        if (skill == null)
        {
            skill = new PfCharacterSkill
            {
                SkillName = skillName,
                Proficiency = new PfProficiency { Rank = ProficiencyRank.Untrained }
            };
            Character.Skills.Add(skill);
        }
        return skill;
    }

    protected async Task SaveCharacter()
    {
        try
        {
            // Ensure skills are initialized if they weren't already
            if (!Character.Skills.Any() && PathfinderState.Value.SkillsLoaded)
            {
                InitializeCharacterSkills();
            }

            // Calculate final stats before saving
            Character.ArmorClass = CalculateAC();
            Character.HitPoints = CalculateHitPoints();
            Character.CurrentHitPoints = Character.HitPoints;
            Character.UpdatedAt = DateTime.UtcNow;

            if (CharacterId.HasValue)
            {
                // Update existing character
                await Http.PutAsJsonAsync($"api/characters/{CharacterId}", Character);
            }
            else
            {
                // Create new character
                var response = await Http.PostAsJsonAsync("api/characters", Character);
                if (response.IsSuccessStatusCode)
                {
                    var createdCharacter = await response.Content.ReadFromJsonAsync<PfCharacter>();
                    if (createdCharacter != null)
                    {
                        Character = createdCharacter;
                        CharacterId = createdCharacter.Id;
                    }
                }
            }

            // Navigate to character sheet or characters list
            Navigation.NavigateTo("/characters");
        }
        catch (Exception ex)
        {
            // Handle error - show notification
            Console.WriteLine($"Error saving character: {ex.Message}");
        }
    }

    #region Calculations

    protected int CalculateAC()
    {
        var dexMod = Character.AbilityScores.GetModifier(Character.AbilityScores.Dexterity);
        return 10 + dexMod; // Base AC calculation, would need armor bonuses, etc.
    }

    protected int CalculateHitPoints()
    {
        var conMod = Character.AbilityScores.GetModifier(Character.AbilityScores.Constitution);
        var ancestryHP = GetAncestryHitPoints();
        var classHP = GetClassHitPoints();
        
        return ancestryHP + classHP + conMod + ((Character.Level - 1) * (classHP + conMod));
    }

    private int GetAncestryHitPoints()
    {
        if (string.IsNullOrEmpty(Character.Ancestry))
            return 8; // Default
            
        var ancestry = PathfinderState.Value.Ancestries.FirstOrDefault(a => a.Name == Character.Ancestry);
        return ancestry?.HitPoints ?? 8;
    }

    protected int CalculatePerception()
    {
        var wisMod = Character.AbilityScores.GetModifier(Character.AbilityScores.Wisdom);
        var profBonus = Character.Level + Character.Perception.Rank.GetBonus();
        return wisMod + profBonus;
    }

    protected int CalculateFortitude()
    {
        var conMod = Character.AbilityScores.GetModifier(Character.AbilityScores.Constitution);
        var profBonus = Character.Level + Character.FortitudeSave.Rank.GetBonus();
        return conMod + profBonus;
    }

    protected int CalculateReflex()
    {
        var dexMod = Character.AbilityScores.GetModifier(Character.AbilityScores.Dexterity);
        var profBonus = Character.Level + Character.ReflexSave.Rank.GetBonus();
        return dexMod + profBonus;
    }

    protected int CalculateWill()
    {
        var wisMod = Character.AbilityScores.GetModifier(Character.AbilityScores.Wisdom);
        var profBonus = Character.Level + Character.WillSave.Rank.GetBonus();
        return wisMod + profBonus;
    }

    private int GetClassHitPoints()
    {
        if (string.IsNullOrEmpty(Character.ClassName))
            return 8; // Default
            
        var pfClass = PathfinderState.Value.Classes.FirstOrDefault(c => c.Name == Character.ClassName);
        return pfClass?.HitPoints ?? 8;
    }

    #endregion
}