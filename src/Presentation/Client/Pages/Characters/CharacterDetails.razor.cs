using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Presentation.Shared.Models;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Characters;

public partial class CharacterDetails : ComponentBase
{
    [Parameter] public Guid CharacterId { get; set; }
    
    private CharacterDto? Character { get; set; }
    private bool IsLoading { get; set; } = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadCharacter();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (Character?.Id != CharacterId)
        {
            await LoadCharacter();
        }
    }

    private async Task LoadCharacter()
    {
        IsLoading = true;
        StateHasChanged();

        try
        {
            var response = await Http.GetAsync($"api/characters/{CharacterId}");
            if (response.IsSuccessStatusCode)
            {
                Character = await response.Content.ReadFromJsonAsync<CharacterDto>();
            }
            else
            {
                Character = null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading character: {ex.Message}");
            Character = null;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private async Task EditCharacter()
    {
        if (Character != null)
        {
            await JSRuntime.InvokeVoidAsync("history.pushState", null, "", $"/characters/{Character.Id}/edit");
        }
    }

    private string FormatModifier(int modifier)
    {
        return modifier >= 0 ? $"+{modifier}" : modifier.ToString();
    }

    private int GetModifier(int abilityScore)
    {
        return (abilityScore - 10) / 2;
    }
}