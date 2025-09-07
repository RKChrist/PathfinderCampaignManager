using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PathfinderCampaignManager.Presentation.Client.Services;
using PathfinderCampaignManager.Presentation.Client.Store.Auth;
using System.Net.Http.Headers;
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
    private bool _isLoading = true;
    private bool _isDM = false;
    private bool _isStartingEncounter = false;
    private string _errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await LoadCurrentUser();
        await LoadCampaign();
        await LoadCampaignCharacters();
        await LoadUserCharacters();
        await LoadPremadeEncounters();
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
}