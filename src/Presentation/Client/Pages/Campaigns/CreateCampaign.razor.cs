using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Campaigns;

public partial class CreateCampaign : ComponentBase
{
    private readonly CreateCampaignRequest _createRequest = new();
    private bool _isLoading = false;
    private string _errorMessage = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        await SetupAuthentication();
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

    private async Task CreateCampaignAsync()
    {
        try
        {
            _isLoading = true;
            _errorMessage = string.Empty;
            StateHasChanged();

            var response = await Http.PostAsJsonAsync("api/campaign", _createRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var campaign = JsonSerializer.Deserialize<CampaignDto>(content, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                if (campaign != null)
                {
                    // Navigate to the new campaign
                    Navigation.NavigateTo($"/campaigns/{campaign.Id}");
                }
                else
                {
                    // Fallback to campaigns overview
                    Navigation.NavigateTo("/campaigns");
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Navigation.NavigateTo("/login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _errorMessage = $"Failed to create campaign: {response.StatusCode}";
                
                // Try to parse error details
                try
                {
                    var errorObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errorObj.TryGetProperty("message", out var messageElement))
                    {
                        _errorMessage = messageElement.GetString() ?? _errorMessage;
                    }
                    else if (errorObj.TryGetProperty("title", out var titleElement))
                    {
                        _errorMessage = titleElement.GetString() ?? _errorMessage;
                    }
                }
                catch
                {
                    // Keep the default error message
                }
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"An error occurred while creating the campaign: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo("/campaigns");
    }

    public class CreateCampaignRequest
    {
        [Required(ErrorMessage = "Campaign name is required")]
        [StringLength(100, ErrorMessage = "Campaign name cannot be longer than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot be longer than 1000 characters")]
        public string? Description { get; set; }

        // Variant Rules
        public bool UseFreeArchetype { get; set; } = false;
        public bool UseDualClass { get; set; } = false;
        public bool UseProficiencyWithoutLevel { get; set; } = false;
        public bool UseAutomaticBonusProgression { get; set; } = false;
        public bool UseGradualAbilityBoosts { get; set; } = false;
        public bool UseStaminaVariant { get; set; } = false;
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
}