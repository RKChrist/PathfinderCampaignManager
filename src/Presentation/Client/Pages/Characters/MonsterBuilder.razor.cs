using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Entities.Pathfinder;
using System.Net.Http.Json;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Characters;

public partial class MonsterBuilder : ComponentBase
{
    [Parameter] public Guid? CampaignId { get; set; }
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private NavigationManager Navigation { get; set; } = default!;

    private PfMonster monster = new();
    private bool isLoading = false;

    // Form inputs for arrays/lists
    private string creatureType = "Humanoid";
    private string traitsInput = string.Empty;
    private string immunitiesInput = string.Empty;
    private string resistancesInput = string.Empty;
    private string weaknessesInput = string.Empty;
    private string languagesInput = string.Empty;
    private string sensesInput = string.Empty;
    private Dictionary<PfStrike, string> strikeTraitsInputs = new();

    private readonly string[] skillNames = 
    {
        "Acrobatics", "Arcana", "Athletics", "Crafting", "Deception", "Diplomacy",
        "Intimidation", "Medicine", "Nature", "Occultism", "Performance", "Religion",
        "Society", "Stealth", "Survival", "Thievery"
    };

    private bool IsValid => !string.IsNullOrWhiteSpace(monster.Name) && 
                           monster.Level >= -1 && 
                           monster.ArmorClass > 0 && 
                           monster.HitPoints > 0;

    protected override void OnInitialized()
    {
        // Initialize default monster
        monster = new PfMonster
        {
            Id = Guid.NewGuid().ToString(),
            Name = string.Empty,
            Level = 0,
            Size = "Medium",
            Alignment = "N",
            Rarity = "Common",
            ArmorClass = 10,
            HitPoints = 10,
            AbilityScores = new Dictionary<string, int>
            {
                ["Strength"] = 10,
                ["Dexterity"] = 10,
                ["Constitution"] = 10,
                ["Intelligence"] = 10,
                ["Wisdom"] = 10,
                ["Charisma"] = 10
            },
            Skills = new Dictionary<string, int>(),
            Speeds = new Dictionary<string, int> { ["land"] = 25 }
        };

        // Initialize all skills to 0
        foreach (var skill in skillNames)
        {
            monster.Skills[skill] = 0;
        }
    }

    private void AddStrike()
    {
        var strike = new PfStrike
        {
            Name = $"Strike {monster.Strikes.Count + 1}",
            AttackBonus = 0,
            DamageFormula = "1d6",
            DamageType = "bludgeoning",
            Range = "melee"
        };
        
        monster.Strikes.Add(strike);
        strikeTraitsInputs[strike] = string.Empty;
        StateHasChanged();
    }

    private void RemoveStrike(PfStrike strike)
    {
        monster.Strikes.Remove(strike);
        strikeTraitsInputs.Remove(strike);
        StateHasChanged();
    }

    private async Task SaveMonster()
    {
        if (!IsValid || isLoading) return;

        isLoading = true;
        
        try
        {
            // Process form inputs into monster properties
            ProcessFormInputs();

            // Create the request object in the format the server expects
            var request = new CreateMonsterRequest
            {
                Name = monster.Name,
                Type = creatureType,
                Level = monster.Level,
                Description = monster.Description,
                ArmorClass = monster.ArmorClass,
                HitPoints = monster.HitPoints,
                Speed = monster.Speeds.TryGetValue("land", out var landSpeed) ? $"{landSpeed} feet" : "25 feet",
                SessionId = CampaignId,
                IsTemplate = false
            };

            Console.WriteLine($"Sending monster request: Name='{request.Name}', Type='{request.Type}', Level={request.Level}, AC={request.ArmorClass}, HP={request.HitPoints}, Speed='{request.Speed}'");

            // Save monster via API
            var response = await Http.PostAsJsonAsync("api/npcmonster", request);
            
            if (response.IsSuccessStatusCode)
            {
                var monsterId = await response.Content.ReadFromJsonAsync<Guid>();
                
                if (CampaignId.HasValue)
                {
                    Navigation.NavigateTo($"/campaigns/{CampaignId}");
                }
                else
                {
                    Navigation.NavigateTo($"/npc-monsters");
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to save monster: {response.StatusCode} - {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving monster: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ProcessFormInputs()
    {
        // Process traits
        if (!string.IsNullOrWhiteSpace(traitsInput))
        {
            monster.Traits = traitsInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList();
        }

        // Process immunities
        if (!string.IsNullOrWhiteSpace(immunitiesInput))
        {
            monster.Immunities = immunitiesInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(i => i.Trim())
                .ToList();
        }

        // Process resistances
        if (!string.IsNullOrWhiteSpace(resistancesInput))
        {
            monster.Resistances = resistancesInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(r => r.Trim())
                .ToList();
        }

        // Process weaknesses
        if (!string.IsNullOrWhiteSpace(weaknessesInput))
        {
            monster.Weaknesses = weaknessesInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim())
                .ToList();
        }

        // Process languages
        if (!string.IsNullOrWhiteSpace(languagesInput))
        {
            monster.Languages = languagesInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .ToList();
        }

        // Process senses
        if (!string.IsNullOrWhiteSpace(sensesInput))
        {
            monster.Senses = sensesInput.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .ToList();
        }

        // Process strike traits
        foreach (var kvp in strikeTraitsInputs)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value))
            {
                kvp.Key.Traits = kvp.Value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => t.Trim())
                    .ToList();
            }
        }
    }

    private void LoadTemplate()
    {
        // Show template selection modal or implement template loading logic
        Console.WriteLine("Load template clicked");
    }

    private string FormatModifier(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    public class CreateMonsterRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Level { get; set; }
        public string? Description { get; set; }
        public int ArmorClass { get; set; }
        public int HitPoints { get; set; }
        public string? Speed { get; set; }
        public Guid? SessionId { get; set; }
        public bool IsTemplate { get; set; }
    }
}