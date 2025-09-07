using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Domain.Interfaces;
using PathfinderCampaignManager.Domain.Validation;
using PathfinderCampaignManager.Presentation.Client.Services;

namespace PathfinderCampaignManager.Presentation.Client.Components.Validation;

public partial class ValidationPanel : ComponentBase
{
    [Parameter] public string Title { get; set; } = "Validation";
    [Parameter] public Guid? CharacterId { get; set; }
    [Parameter] public ICalculatedCharacter? CalculatedCharacter { get; set; }
    [Parameter] public string? CampaignId { get; set; }
    [Parameter] public bool ShowAutoValidate { get; set; } = true;
    [Parameter] public bool AutoValidateOnLoad { get; set; } = false;
    [Parameter] public EventCallback<ValidationReport> OnValidationCompleted { get; set; }

    private bool IsExpanded = false;
    private bool IsLoading = false;
    private bool AutoValidate = false;
    private ValidationReport? ValidationReport;
    private string? ErrorMessage;
    private Timer? _autoValidateTimer;

    protected override async Task OnInitializedAsync()
    {
        if (AutoValidateOnLoad)
        {
            await RefreshValidation();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (AutoValidate && ValidationReport == null)
        {
            await RefreshValidation();
        }
    }

    private void ToggleExpanded()
    {
        IsExpanded = !IsExpanded;
    }

    private async Task RefreshValidation()
    {
        if (IsLoading) return;

        IsLoading = true;
        ErrorMessage = null;
        StateHasChanged();

        try
        {
            ValidationReport? result = null;

            if (CharacterId.HasValue)
            {
                result = await ValidationService.ValidateCharacterAsync(CharacterId.Value);
            }
            else if (CalculatedCharacter != null)
            {
                result = await ValidationService.ValidateCalculatedCharacterAsync(CalculatedCharacter);
            }
            else if (!string.IsNullOrEmpty(CampaignId))
            {
                result = await ValidationService.ValidateCampaignAsync(CampaignId);
            }

            ValidationReport = result;

            if (ValidationReport != null && OnValidationCompleted.HasDelegate)
            {
                await OnValidationCompleted.InvokeAsync(ValidationReport);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Validation failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }

    private string GetStatusIcon()
    {
        if (ValidationReport == null)
            return "fa-question-circle";

        return ValidationReport.IsValid switch
        {
            true => "fa-check-circle",
            false when ValidationReport.HasCriticalIssues => "fa-times-circle",
            false => "fa-exclamation-triangle"
        };
    }

    private string GetStatusColor()
    {
        if (ValidationReport == null)
            return "text-muted";

        return ValidationReport.IsValid switch
        {
            true => "text-success",
            false when ValidationReport.HasCriticalIssues => "text-danger",
            false => "text-warning"
        };
    }

    private string GetStatusText()
    {
        if (ValidationReport == null)
            return "Unknown";

        return ValidationReport.IsValid switch
        {
            true => "Valid",
            false when ValidationReport.HasCriticalIssues => "Invalid",
            false => "Warnings"
        };
    }

    private void OnAutoValidateChanged()
    {
        if (AutoValidate)
        {
            StartAutoValidation();
        }
        else
        {
            StopAutoValidation();
        }
    }

    private void StartAutoValidation()
    {
        _autoValidateTimer?.Dispose();
        _autoValidateTimer = new Timer(async _ => await RefreshValidation(), null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
    }

    private void StopAutoValidation()
    {
        _autoValidateTimer?.Dispose();
        _autoValidateTimer = null;
    }

    public void Dispose()
    {
        _autoValidateTimer?.Dispose();
    }
}