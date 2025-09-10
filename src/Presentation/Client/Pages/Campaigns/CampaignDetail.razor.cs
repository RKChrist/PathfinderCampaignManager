using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Presentation.Client.Services;
using PathfinderCampaignManager.Presentation.Client.Store.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Campaigns;

public partial class CampaignDetail : ComponentBase, IAsyncDisposable
{
    [Parameter] public Guid CampaignId { get; set; }
    
    private CampaignDto? _campaign;
    private PathfinderCampaignManager.Presentation.Client.Store.Auth.UserInfo? _currentUser;
    private List<CharacterDto> _campaignCharacters = new();
    private List<CharacterDto> _userCharacters = new();
    private List<PremadeEncounterResponse> _premadeEncounters = new();
    private List<ActivityItem> _recentActivity = new();
    private List<VariantRuleInfo> _variantRules = new();
    private bool _isLoading = true;
    private bool _isDM = false;
    private bool _isStartingEncounter = false;
    private string _errorMessage = string.Empty;
    
    // Combat setup modal
    private bool _showCombatSetupModal = false;
    private List<CharacterDto> _availableCharacters = new();
    private List<MonsterResponse> _availableMonsters = new();
    private HashSet<Guid> _selectedCharacters = new();
    private HashSet<Guid> _selectedMonsters = new();

    protected override async Task OnInitializedAsync()
    {
        await LoadCurrentUser();
        await LoadCampaign();
        await LoadCampaignCharacters();
        await LoadUserCharacters();
        await LoadPremadeEncounters();
        await LoadRecentActivity();
    }

    private async Task LoadCurrentUser()
    {
        try
        {
            _currentUser = await AuthService.GetCurrentUserAsync();
            if (_currentUser == null)
            {
                Navigation.NavigateTo("/login");
            }
        }
        catch (Exception)
        {
            Navigation.NavigateTo("/login");
        }
    }

    private async Task LoadCampaign()
    {
        try
        {
            _isLoading = true;
            _errorMessage = string.Empty;

            var response = await Http.GetAsync($"api/campaign/{CampaignId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _campaign = JsonSerializer.Deserialize<CampaignDto>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (_campaign != null && _currentUser != null)
                {
                    _isDM = _campaign.DMUserId == _currentUser.Id;
                    LoadVariantRules();
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Navigation.NavigateTo("/login");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _errorMessage = "You don't have access to this campaign.";
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _errorMessage = "Campaign not found.";
            }
            else
            {
                _errorMessage = "Failed to load campaign details.";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error loading campaign: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task StartSession()
    {
        if (_campaign == null || _currentUser == null || !_isDM)
            return;

        try
        {
            // TODO: Implement session start logic
            // For now, show a message that combat encounters will be available
            await JSRuntime.InvokeAsync<object>("alert", "Session management and combat encounters are being implemented. Use the Quick Actions to create characters and monsters for now.");
        }
        catch (Exception ex)
        {
            _errorMessage = $"Error starting session: {ex.Message}";
        }
    }

    private async Task ShowInviteModal()
    {
        if (_campaign == null || !_isDM)
            return;

        // Show the join token
        var joinUrl = $"{Navigation.BaseUri}campaigns/join?token={_campaign.JoinToken}";
        await JSRuntime.InvokeAsync<object>("navigator.clipboard.writeText", joinUrl);
        await JSRuntime.InvokeAsync<object>("alert", $"Join URL copied to clipboard!\n\nShare this URL with players:\n{joinUrl}");
    }

    private int GetDaysSince(DateTime dateTime)
    {
        return Math.Max(0, (int)(DateTime.UtcNow - dateTime).TotalDays);
    }

    private string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.UtcNow - dateTime;
        
        if (timeSpan.TotalDays > 30)
            return dateTime.ToString("MMM dd, yyyy");
        else if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays}d ago";
        else if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours}h ago";
        else if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes}m ago";
        else
            return "Just now";
    }

    private async Task LoadCampaignCharacters()
    {
        try
        {
            var response = await Http.GetAsync($"api/characters?campaignId={CampaignId}");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _campaignCharacters = JsonSerializer.Deserialize<List<CharacterDto>>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new List<CharacterDto>();
            }
        }
        catch (Exception)
        {
            _campaignCharacters = new List<CharacterDto>();
        }
    }

    private async Task LoadUserCharacters()
    {
        try
        {
            var response = await Http.GetAsync("api/characters");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _userCharacters = JsonSerializer.Deserialize<List<CharacterDto>>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new List<CharacterDto>();

                // Filter out characters already in the campaign
                var campaignCharacterIds = _campaignCharacters.Select(c => c.Id).ToHashSet();
                _userCharacters = _userCharacters.Where(c => !campaignCharacterIds.Contains(c.Id)).ToList();
            }
        }
        catch (Exception)
        {
            _userCharacters = new List<CharacterDto>();
        }
    }

    private async Task ShowJoinWithCharacterModal()
    {
        if (!_userCharacters.Any())
        {
            await JSRuntime.InvokeAsync<object>("alert", "You don't have any characters to join this campaign with. Create a character first!");
            Navigation.NavigateTo($"/campaigns/{CampaignId}/characters/create");
            return;
        }

        // In a real implementation, show a modal to select character
        // For now, show a simple prompt
        var characterNames = string.Join("\n", _userCharacters.Select((c, i) => $"{i + 1}. {c.Name} (Level {c.Level} {c.Class})"));
        await JSRuntime.InvokeAsync<object>("alert", $"Available characters:\n{characterNames}\n\nThis feature will be enhanced with a proper character selection modal.");
    }

    private async Task RemoveCharacterFromCampaign(Guid characterId)
    {
        try
        {
            var response = await Http.DeleteAsync($"api/characters/{characterId}/leave-campaign/{CampaignId}");
            if (response.IsSuccessStatusCode)
            {
                await LoadCampaignCharacters(); // Refresh the list
                StateHasChanged();
            }
            else
            {
                await JSRuntime.InvokeAsync<object>("alert", "Failed to remove character from campaign.");
            }
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeAsync<object>("alert", $"Error removing character: {ex.Message}");
        }
    }

    private async Task LoadPremadeEncounters()
    {
        try
        {
            var response = await Http.GetAsync("api/npcmonster/premade-encounters");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _premadeEncounters = JsonSerializer.Deserialize<List<PremadeEncounterResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new List<PremadeEncounterResponse>();
            }
        }
        catch (Exception)
        {
            _premadeEncounters = new List<PremadeEncounterResponse>();
        }
    }

    private async Task StartPremadeEncounter(Guid encounterId)
    {
        try
        {
            _isStartingEncounter = true;
            StateHasChanged();

            var response = await Http.PostAsync($"api/npcmonster/premade-encounters/{encounterId}/start/{CampaignId}", null);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var newEncounterId = JsonSerializer.Deserialize<Guid>(content);
                
                await JSRuntime.InvokeAsync<object>("alert", "Encounter started successfully! Redirecting to combat tracker...");
                Navigation.NavigateTo($"/campaigns/{CampaignId}/combat/{newEncounterId}");
            }
            else
            {
                await JSRuntime.InvokeAsync<object>("alert", "Failed to start encounter. Please try again.");
            }
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeAsync<object>("alert", $"Error starting encounter: {ex.Message}");
        }
        finally
        {
            _isStartingEncounter = false;
            StateHasChanged();
        }
    }

    private string GetDifficultyBadgeClass(string difficulty)
    {
        return difficulty.ToLower() switch
        {
            "trivial" => "bg-secondary",
            "low" => "bg-success",
            "moderate" => "bg-warning text-dark",
            "severe" => "bg-danger",
            "extreme" => "bg-dark",
            _ => "bg-secondary"
        };
    }

    // Combat setup modal methods
    private async Task OpenCombatSetup()
    {
        await LoadAvailableCharacters();
        await LoadAvailableMonsters();
        _showCombatSetupModal = true;
        StateHasChanged();
    }

    private void CloseCombatSetup()
    {
        _showCombatSetupModal = false;
        _selectedCharacters.Clear();
        _selectedMonsters.Clear();
        StateHasChanged();
    }

    private async Task LoadAvailableCharacters()
    {
        try
        {
            // Load characters from campaign sessions
            _availableCharacters = _campaignCharacters.ToList();
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeAsync<object>("alert", $"Error loading characters: {ex.Message}");
        }
    }

    private async Task LoadAvailableMonsters()
    {
        try
        {
            Console.WriteLine($"Loading monsters for campaign: {CampaignId}");
            var response = await Http.GetAsync($"api/npcmonster?campaignId={CampaignId}");
            Console.WriteLine($"Monster API response status: {response.StatusCode}");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Monster API response content: {content}");
                
                _availableMonsters = JsonSerializer.Deserialize<List<MonsterResponse>>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }) ?? new List<MonsterResponse>();
                
                Console.WriteLine($"Loaded {_availableMonsters.Count} monsters");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to load monsters: {response.StatusCode} - {errorContent}");
                _availableMonsters = new List<MonsterResponse>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading monsters: {ex.Message}");
            _availableMonsters = new List<MonsterResponse>();
            await JSRuntime.InvokeAsync<object>("alert", $"Error loading monsters: {ex.Message}");
        }
    }

    private void ToggleCharacterSelection(Guid characterId, ChangeEventArgs e)
    {
        bool isChecked = (bool)(e.Value ?? false);
        if (isChecked)
            _selectedCharacters.Add(characterId);
        else
            _selectedCharacters.Remove(characterId);
    }

    private void ToggleMonsterSelection(Guid monsterId, ChangeEventArgs e)
    {
        bool isChecked = (bool)(e.Value ?? false);
        if (isChecked)
            _selectedMonsters.Add(monsterId);
        else
            _selectedMonsters.Remove(monsterId);
    }

    private async Task StartSelectedCombat()
    {
        try
        {
            await SendSelectedParticipantsToCombat();
            CloseCombatSetup();
            Navigation.NavigateTo($"/combat-signalr?campaignId={CampaignId}");
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeAsync<object>("alert", $"Error starting combat: {ex.Message}");
        }
    }

    private async Task SendSelectedParticipantsToCombat()
    {
        if (!_selectedCharacters.Any() && !_selectedMonsters.Any())
            return;

        try
        {
            // Send selected participants to combat tracker via SignalR
            var selectedParticipants = new List<CombatParticipantRequest>();

            // Add selected characters
            foreach (var characterId in _selectedCharacters)
            {
                var character = _availableCharacters.FirstOrDefault(c => c.Id == characterId);
                if (character != null)
                {
                    selectedParticipants.Add(new CombatParticipantRequest
                    {
                        Id = character.Id,
                        Name = character.Name,
                        Type = "PlayerCharacter",
                        Initiative = 0, // Will be set during initiative rolls
                        HitPoints = CalculateCharacterHitPoints(character),
                        ArmorClass = CalculateCharacterAC(character),
                        IsPlayerCharacter = true
                    });
                }
            }

            // Add selected monsters
            foreach (var monsterId in _selectedMonsters)
            {
                var monster = _availableMonsters.FirstOrDefault(m => m.Id == monsterId);
                if (monster != null)
                {
                    selectedParticipants.Add(new CombatParticipantRequest
                    {
                        Id = monster.Id,
                        Name = monster.Name,
                        Type = monster.Type.ToString(),
                        Initiative = 0, // Will be set during initiative rolls
                        HitPoints = monster.HitPoints ?? 10,
                        ArmorClass = monster.ArmorClass ?? 15,
                        IsPlayerCharacter = false
                    });
                }
            }

            // Send participants to campaign hub to distribute to combat tracker
            // Note: In a real implementation, you would inject CampaignSignalRService
            // For now, we'll use a direct HTTP call or implement a different approach
            await NotifyCombatTrackerOfNewParticipants(selectedParticipants.Cast<object>().ToList());
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeAsync<object>("alert", $"Error sending participants to combat: {ex.Message}");
        }
    }

    private int CalculateCharacterHitPoints(CharacterDto character)
    {
        // Simple HP calculation - in a real implementation this would be more complex
        var constitution = character.AbilityScores.GetValueOrDefault("Constitution", 10);
        var conModifier = (constitution - 10) / 2;
        return Math.Max(1, character.Level * 8 + conModifier * character.Level + constitution);
    }

    private int CalculateCharacterAC(CharacterDto character)
    {
        // Simple AC calculation - in a real implementation this would consider armor, dexterity, etc.
        var dexterity = character.AbilityScores.GetValueOrDefault("Dexterity", 10);
        var dexModifier = (dexterity - 10) / 2;
        return 10 + dexModifier; // Base AC calculation
    }

    private async Task NotifyCombatTrackerOfNewParticipants(List<object> participants)
    {
        try
        {
            // Send participants directly to combat endpoint to be added to active combat
            var request = new
            {
                CampaignId = CampaignId,
                Participants = participants
            };

            var response = await Http.PostAsJsonAsync("api/combat/add-participants", request);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Successfully sent {participants.Count} participants to combat tracker");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Failed to send participants to combat tracker: {errorContent}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending participants to combat tracker: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        // Cleanup will be handled by CampaignStatusPanel
        await Task.CompletedTask;
    }

    public class CampaignDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid DMUserId { get; set; }
        public Guid JoinToken { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastActivityAt { get; set; }
        public int MemberCount { get; set; }
        public int SessionCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CharacterDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Ancestry { get; set; } = string.Empty;
        public string Heritage { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public Dictionary<string, int> AbilityScores { get; set; } = new();
        public Dictionary<string, string> Skills { get; set; } = new();
        public List<string> Feats { get; set; } = new();
        public List<string> Equipment { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PremadeEncounterResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PartyLevel { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public List<MonsterTemplate> MonsterTemplates { get; set; } = new();
    }

    public class MonsterTemplate
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Description { get; set; } = string.Empty;
        public int ArmorClass { get; set; }
        public int HitPoints { get; set; }
        public string Speed { get; set; } = string.Empty;
        public int Count { get; set; } = 1;
    }

    public class MonsterResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public NpcMonsterType Type { get; set; }
        public int Level { get; set; }
        public string? Description { get; set; }
        public int? ArmorClass { get; set; }
        public int? HitPoints { get; set; }
        public int? Speed { get; set; }
        public bool IsTemplate { get; set; }
        public Guid? OwnerUserId { get; set; }
        public Guid? SessionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ActivityItem
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Icon { get; set; } = "fas fa-clock";
        public string Color { get; set; } = "text-muted";
    }

    private async Task LoadRecentActivity()
    {
        try
        {
            _recentActivity.Clear();
            
            // Add campaign creation activity
            if (_campaign != null)
            {
                _recentActivity.Add(new ActivityItem
                {
                    Type = "Campaign Created",
                    Description = $"Campaign '{_campaign.Name}' was created",
                    UserName = "DM",
                    Timestamp = _campaign.CreatedAt,
                    Icon = "fas fa-plus",
                    Color = "text-success"
                });
            }

            // Add character join activities
            foreach (var character in _campaignCharacters.Take(10))
            {
                _recentActivity.Add(new ActivityItem
                {
                    Type = "Character Joined",
                    Description = $"{character.Name} (Level {character.Level} {character.Class}) joined the campaign",
                    UserName = "Player",
                    Timestamp = character.CreatedAt,
                    Icon = "fas fa-user-plus",
                    Color = "text-primary"
                });
            }

            // Sort by timestamp descending (most recent first)
            _recentActivity = _recentActivity.OrderByDescending(a => a.Timestamp).Take(10).ToList();
        }
        catch (Exception)
        {
            _recentActivity = new List<ActivityItem>();
        }
    }

    public class VariantRuleInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; }
        public string Icon { get; set; } = "fas fa-magic";
    }

    private void LoadVariantRules()
    {
        _variantRules.Clear();

        // For now, create static variant rules. In a real implementation, 
        // you would parse the VariantRulesJson from the campaign
        var availableRules = new List<VariantRuleInfo>
        {
            new VariantRuleInfo 
            { 
                Name = "Free Archetype", 
                Description = "Characters gain archetype feats automatically", 
                IsEnabled = false, 
                Icon = "fas fa-certificate" 
            },
            new VariantRuleInfo 
            { 
                Name = "Dual Class", 
                Description = "Characters can multiclass more effectively", 
                IsEnabled = false, 
                Icon = "fas fa-layer-group" 
            },
            new VariantRuleInfo 
            { 
                Name = "Automatic Bonus Progression", 
                Description = "Characters get bonuses without magic items", 
                IsEnabled = false, 
                Icon = "fas fa-chart-line" 
            },
            new VariantRuleInfo 
            { 
                Name = "Proficiency Without Level", 
                Description = "Level doesn't add to proficiency bonuses", 
                IsEnabled = false, 
                Icon = "fas fa-equals" 
            }
        };

        _variantRules = availableRules;
    }

    public class CombatParticipantRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Initiative { get; set; }
        public int HitPoints { get; set; }
        public int ArmorClass { get; set; }
        public bool IsPlayerCharacter { get; set; }
    }
}