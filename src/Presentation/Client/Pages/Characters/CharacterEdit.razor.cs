using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Presentation.Shared.Models;
using System.Net.Http.Json;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Characters;

public partial class CharacterEdit : ComponentBase
{
    [Parameter] public Guid CharacterId { get; set; }
    
    private CharacterDto? Character { get; set; }
    private UpdateCharacterRequest EditModel { get; set; } = new();
    private bool IsLoading { get; set; } = true;
    private bool IsSaving { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadCharacter();
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
                if (Character != null)
                {
                    EditModel = new UpdateCharacterRequest
                    {
                        Name = Character.Name,
                        Level = Character.Level,
                        AbilityScores = new Dictionary<string, int>(Character.AbilityScores),
                        Skills = new Dictionary<string, string>(Character.Skills),
                        Feats = new List<string>(Character.Feats),
                        Equipment = new List<string>(Character.Equipment)
                    };
                }
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

    private async Task SaveCharacter()
    {
        if (Character == null || IsSaving) return;

        IsSaving = true;
        StateHasChanged();

        try
        {
            var response = await Http.PutAsJsonAsync($"api/characters/{CharacterId}", EditModel);
            if (response.IsSuccessStatusCode)
            {
                // Navigate back to character details
                Navigation.NavigateTo($"/characters/{CharacterId}");
            }
            else
            {
                Console.WriteLine($"Error saving character: {response.StatusCode}");
                // TODO: Show error message to user
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving character: {ex.Message}");
            // TODO: Show error message to user
        }
        finally
        {
            IsSaving = false;
            StateHasChanged();
        }
    }

    private void Cancel()
    {
        Navigation.NavigateTo($"/characters/{CharacterId}");
    }

    private void AddFeat()
    {
        EditModel.Feats ??= new List<string>();
        EditModel.Feats.Add("");
        StateHasChanged();
    }

    private void RemoveFeat(int index)
    {
        if (EditModel.Feats != null && index >= 0 && index < EditModel.Feats.Count)
        {
            EditModel.Feats.RemoveAt(index);
            StateHasChanged();
        }
    }

    private void AddEquipment()
    {
        EditModel.Equipment ??= new List<string>();
        EditModel.Equipment.Add("");
        StateHasChanged();
    }

    private void RemoveEquipment(int index)
    {
        if (EditModel.Equipment != null && index >= 0 && index < EditModel.Equipment.Count)
        {
            EditModel.Equipment.RemoveAt(index);
            StateHasChanged();
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