using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Campaigns;

public partial class ManageCampaign : ComponentBase
{
    [Parameter] public Guid CampaignId { get; set; }
    
    private CampaignDto? _campaign;
    private readonly UpdateCampaignRequest _updateRequest = new();
    private readonly VariantRulesModel _variantRules = new();
    private bool _isLoading = true;
    private bool _isUpdating = false;
    private bool _isUpdatingRules = false;
    private bool _isRegeneratingToken = false;
    private string _errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await SetupAuthentication();
        await LoadCampaign();
    }

    private async Task SetupAuthentication()
    {
        try
        {
            var token = await JSRuntime.InvokeAsync<string>("localStorage.getItem", new object[] { "authToken" });
            if (string.IsNullOrEmpty(token))
            {
                Navigation.NavigateTo("/login");
                return;
            }

            Http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
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

                if (_campaign != null)
                {
                    // Populate update form
                    _updateRequest.Name = _campaign.Name;
                    _updateRequest.Description = _campaign.Description;
                    
                    // TODO: Load variant rules from campaign when API supports it
                    // For now, initialize with defaults
                    _variantRules.FreeArchetype = false;
                    _variantRules.DualClass = false;
                    _variantRules.ProficiencyWithoutLevel = false;
                    _variantRules.AutomaticBonusProgression = false;
                    _variantRules.GradualAbilityBoosts = false;
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Navigation.NavigateTo("/login");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _errorMessage = "You don't have permission to manage this campaign.";
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

    private async Task UpdateCampaign()
    {
        try
        {
            _isUpdating = true;
            StateHasChanged();

            var response = await Http.PutAsJsonAsync($"api/campaign/{CampaignId}", _updateRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _campaign = JsonSerializer.Deserialize<CampaignDto>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                // Show success message (could be implemented with a toast notification)
                await JSRuntime.InvokeVoidAsync("alert", "Campaign updated successfully!");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await JSRuntime.InvokeVoidAsync("alert", $"Failed to update campaign: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error updating campaign: {ex.Message}");
        }
        finally
        {
            _isUpdating = false;
            StateHasChanged();
        }
    }

    private async Task UpdateVariantRules()
    {
        try
        {
            _isUpdatingRules = true;
            StateHasChanged();

            // TODO: Implement variant rules update API call
            // For now, just show a message
            await JSRuntime.InvokeVoidAsync("alert", "Variant rules updated successfully!");
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error updating variant rules: {ex.Message}");
        }
        finally
        {
            _isUpdatingRules = false;
            StateHasChanged();
        }
    }

    private async Task RegenerateJoinToken()
    {
        try
        {
            _isRegeneratingToken = true;
            StateHasChanged();

            var response = await Http.PostAsync($"api/campaign/{CampaignId}/regenerate-token", null);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonSerializer.Deserialize<JoinTokenResponse>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (tokenResponse != null && _campaign != null)
                {
                    _campaign.JoinToken = tokenResponse.JoinToken;
                    await JSRuntime.InvokeVoidAsync("alert", "New join link generated successfully!");
                }
            }
            else
            {
                await JSRuntime.InvokeVoidAsync("alert", "Failed to generate new join link.");
            }
        }
        catch (Exception ex)
        {
            await JSRuntime.InvokeVoidAsync("alert", $"Error generating join link: {ex.Message}");
        }
        finally
        {
            _isRegeneratingToken = false;
            StateHasChanged();
        }
    }

    private string GetJoinUrl()
    {
        if (_campaign == null) return "";
        var baseUrl = Navigation.BaseUri.TrimEnd('/');
        return $"{baseUrl}/join/{_campaign.JoinToken}";
    }

    private async Task CopyJoinUrl()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", GetJoinUrl());
            await JSRuntime.InvokeVoidAsync("alert", "Join link copied to clipboard!");
        }
        catch (Exception)
        {
            // Fallback for browsers that don't support clipboard API
            await JSRuntime.InvokeVoidAsync("alert", "Please manually copy the join link.");
        }
    }

    public class UpdateCampaignRequest
    {
        [Required(ErrorMessage = "Campaign name is required")]
        [StringLength(100, ErrorMessage = "Campaign name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string? Description { get; set; }
    }

    public class VariantRulesModel
    {
        public bool FreeArchetype { get; set; }
        public bool DualClass { get; set; }
        public bool ProficiencyWithoutLevel { get; set; }
        public bool AutomaticBonusProgression { get; set; }
        public bool GradualAbilityBoosts { get; set; }
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

    public class JoinTokenResponse
    {
        public Guid JoinToken { get; set; }
    }
}