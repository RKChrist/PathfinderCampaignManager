using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using System.Net.Http.Json;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Characters;

public partial class CharacterBuilder : ComponentBase
{
    [Parameter] public Guid? CharacterId { get; set; }
    
    protected PfCharacter Character { get; set; } = new();
    protected int ActiveStep { get; set; } = 0;
    protected string AbilityMethod { get; set; } = "array";
    
    protected List<PfClass> Classes { get; set; } = new();
    protected List<PfFeat> AvailableFeats { get; set; } = new();
    protected List<PfSkill> AvailableSkills { get; set; } = new();
    protected List<PfAncestry> Ancestries { get; set; } = new();
    protected List<PfBackground> Backgrounds { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadData();
        
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
    }

    private async Task LoadData()
    {
        try
        {
            var classesResponse = await Http.GetFromJsonAsync<List<PfClass>>("api/pathfinder/classes");
            Classes = classesResponse ?? new();

            var featsResponse = await Http.GetFromJsonAsync<List<PfFeat>>("api/pathfinder/feats");
            AvailableFeats = featsResponse ?? new();

            var skillsResponse = await Http.GetFromJsonAsync<List<PfSkill>>("api/pathfinder/skills");
            AvailableSkills = skillsResponse ?? new();

            Ancestries = GetAncestries();
            Backgrounds = GetBackgrounds();
        }
        catch (Exception ex)
        {
            // Handle error - for now just initialize empty lists
            Classes = new();
            AvailableFeats = new();
            AvailableSkills = new();
            Ancestries = GetAncestries();
            Backgrounds = GetBackgrounds();
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
        
        // Initialize skills with all available skills as untrained
        foreach (var skill in AvailableSkills)
        {
            Character.Skills.Add(new PfCharacterSkill
            {
                SkillName = skill.Name,
                Proficiency = new PfProficiency { Rank = ProficiencyRank.Untrained }
            });
        }
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

    protected void SelectAncestry(PfAncestry ancestry)
    {
        Character.Ancestry = ancestry.Name;
        Character.Heritage = ancestry.Heritages.FirstOrDefault() ?? "";
        StateHasChanged();
    }

    protected void SelectBackground(PfBackground background)
    {
        Character.Background = background.Name;
        
        // Apply background skill training
        foreach (var skillName in background.SkillProficiencies)
        {
            var characterSkill = Character.Skills.FirstOrDefault(s => s.SkillName == skillName);
            if (characterSkill != null && characterSkill.Proficiency.Rank == ProficiencyRank.Untrained)
            {
                characterSkill.Proficiency.Rank = ProficiencyRank.Trained;
            }
        }
        
        StateHasChanged();
    }

    protected void SelectClass(PfClass pfClass)
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
        var ancestryHP = 8; // Default, would vary by ancestry
        var classHP = GetClassHitPoints();
        
        return ancestryHP + classHP + conMod + ((Character.Level - 1) * (classHP + conMod));
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
        return Character.ClassName switch
        {
            "Fighter" => 10,
            "Wizard" => 6,
            "Rogue" => 8,
            "Cleric" => 8,
            _ => 8
        };
    }

    #endregion

    #region Data

    private List<PfAncestry> GetAncestries()
    {
        return new List<PfAncestry>
        {
            new()
            {
                Name = "Human",
                Description = "Ambitious, adaptable, and diverse.",
                HitPoints = 8,
                Size = "Medium",
                Speed = 25,
                AbilityBoosts = new List<string> { "Free", "Free" },
                Languages = new List<string> { "Common" },
                Heritages = new List<string> { "Versatile Human", "Half-Elf", "Half-Orc" }
            },
            new()
            {
                Name = "Elf",
                Description = "Tall, slender, and graceful with pointed ears.",
                HitPoints = 6,
                Size = "Medium", 
                Speed = 30,
                AbilityBoosts = new List<string> { "Dexterity", "Intelligence", "Free" },
                AbilityFlaws = new List<string> { "Constitution" },
                Languages = new List<string> { "Common", "Elven" },
                Heritages = new List<string> { "Arctic Elf", "Cavern Elf", "Seer Elf", "Whisper Elf", "Woodland Elf" }
            },
            new()
            {
                Name = "Dwarf",
                Description = "Short, stocky, and hardy folk who live underground.",
                HitPoints = 10,
                Size = "Medium",
                Speed = 20,
                AbilityBoosts = new List<string> { "Constitution", "Wisdom", "Free" },
                AbilityFlaws = new List<string> { "Charisma" },
                Languages = new List<string> { "Common", "Dwarven" },
                Heritages = new List<string> { "Ancient-Blooded Dwarf", "Death Warden Dwarf", "Forge Dwarf", "Rock Dwarf", "Strong-Blooded Dwarf" }
            },
            new()
            {
                Name = "Halfling",
                Description = "Small folk with big hearts and incredible luck.",
                HitPoints = 6,
                Size = "Small",
                Speed = 25,
                AbilityBoosts = new List<string> { "Dexterity", "Wisdom", "Free" },
                AbilityFlaws = new List<string> { "Strength" },
                Languages = new List<string> { "Common", "Halfling" },
                Heritages = new List<string> { "Gutsy Halfling", "Hillock Halfling", "Nomadic Halfling", "Twilight Halfling", "Wildwood Halfling" }
            }
        };
    }

    private List<PfBackground> GetBackgrounds()
    {
        return new List<PfBackground>
        {
            new()
            {
                Name = "Acolyte",
                Description = "You served as an acolyte in a temple, either studying to become a full priest or as a lay worshipper.",
                SkillProficiencies = new List<string> { "Religion" },
                FeatGranted = "Student of the Canon"
            },
            new()
            {
                Name = "Criminal",
                Description = "You lived outside the law, perhaps as a smuggler, highway robber, or gang member.",
                SkillProficiencies = new List<string> { "Stealth" },
                FeatGranted = "Experienced Smuggler"
            },
            new()
            {
                Name = "Folk Hero",
                Description = "You're a common person who performed an uncommon act of heroism, earning you fame and admiration.",
                SkillProficiencies = new List<string> { "Animal Handling", "Survival" },
                FeatGranted = "Train Animal"
            },
            new()
            {
                Name = "Noble",
                Description = "You were born into privilege, whether in a noble family or wealthy merchant dynasty.",
                SkillProficiencies = new List<string> { "Society" },
                FeatGranted = "Courtly Graces"
            },
            new()
            {
                Name = "Scholar",
                Description = "You have spent years studying ancient texts and documents.",
                SkillProficiencies = new List<string> { "Arcana", "Nature", "Occultism", "Religion" },
                FeatGranted = "Assurance"
            },
            new()
            {
                Name = "Soldier",
                Description = "You served in an army, whether on the front lines or in a supporting role.",
                SkillProficiencies = new List<string> { "Athletics", "Intimidation" },
                FeatGranted = "Armor Assist"
            }
        };
    }

    #endregion
}

public class PfAncestry
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int HitPoints { get; set; }
    public string Size { get; set; } = string.Empty;
    public int Speed { get; set; }
    public List<string> AbilityBoosts { get; set; } = new();
    public List<string> AbilityFlaws { get; set; } = new();
    public List<string> Languages { get; set; } = new();
    public List<string> Heritages { get; set; } = new();
}

public class PfBackground
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> SkillProficiencies { get; set; } = new();
    public string FeatGranted { get; set; } = string.Empty;
}