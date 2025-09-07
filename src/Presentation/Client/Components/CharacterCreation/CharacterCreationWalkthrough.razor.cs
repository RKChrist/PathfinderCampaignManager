using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Application.CharacterCreation.Models;
using PathfinderCampaignManager.Application.CharacterCreation.Services;

namespace PathfinderCampaignManager.Presentation.Client.Components.CharacterCreation;

public partial class CharacterCreationWalkthrough : ComponentBase, IDisposable
{
    [Parameter] public Guid CampaignId { get; set; }
    [Parameter] public Guid UserId { get; set; }
    [Parameter] public EventCallback<Guid> OnCharacterCreated { get; set; }
    [Parameter] public EventCallback OnCancelled { get; set; }

    [Inject] private ICharacterCreationService CharacterCreationService { get; set; } = null!;
    [Inject] private ICharacterCreationStateService StateService { get; set; } = null!;
    [Inject] private ICharacterValidationService ValidationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private CharacterCreationSession? _session;
    private CharacterCreationUIState _uiState = new();
    private bool _isLoading = true;
    private bool _isNavigating = false;
    private string? _errorMessage;

    // Step Configuration
    private readonly Dictionary<CharacterCreationStep, StepConfiguration> _stepConfiguration = new()
    {
        {
            CharacterCreationStep.ClassSelection,
            new StepConfiguration
            {
                Title = "Choose Your Class",
                Description = "Select the class that defines your character's role and abilities.",
                // ComponentType removed - using Razor component directly
                IsRequired = true
            }
        },
        {
            CharacterCreationStep.AncestrySelection,
            new StepConfiguration
            {
                Title = "Choose Your Ancestry",
                Description = "Select your character's ancestry and heritage.",
                // ComponentType removed - using Razor component directly
                IsRequired = true
            }
        },
        {
            CharacterCreationStep.BackgroundSelection,
            new StepConfiguration
            {
                Title = "Choose Your Background",
                Description = "Select your character's background and early life experiences.",
                // ComponentType removed - using Razor component directlyBackgroundSelectionStep),
                IsRequired = true
            }
        },
        {
            CharacterCreationStep.AbilityScores,
            new StepConfiguration
            {
                Title = "Assign Ability Scores",
                Description = "Determine your character's core attributes.",
                // ComponentType removed - using Razor component directlyAbilityScoreStep),
                IsRequired = true
            }
        },
        {
            CharacterCreationStep.SkillSelection,
            new StepConfiguration
            {
                Title = "Select Skills",
                Description = "Choose your character's trained skills and expertise.",
                // ComponentType removed - using Razor component directlySkillSelectionStep),
                IsRequired = true
            }
        },
        {
            CharacterCreationStep.FeatSelection,
            new StepConfiguration
            {
                Title = "Select Feats",
                Description = "Choose feats that customize your character's abilities.",
                // ComponentType removed - using Razor component directlyFeatSelectionStep),
                IsRequired = true
            }
        },
        {
            CharacterCreationStep.EquipmentSelection,
            new StepConfiguration
            {
                Title = "Starting Equipment",
                Description = "Select your character's starting equipment and gear.",
                // ComponentType removed - using Razor component directlyEquipmentSelectionStep),
                IsRequired = false
            }
        },
        {
            CharacterCreationStep.SpellSelection,
            new StepConfiguration
            {
                Title = "Select Spells",
                Description = "Choose your character's starting spells (if applicable).",
                // ComponentType removed - using Razor component directlySpellSelectionStep),
                IsRequired = false
            }
        },
        {
            CharacterCreationStep.Finalization,
            new StepConfiguration
            {
                Title = "Character Details",
                Description = "Add final touches to your character.",
                // ComponentType removed - using Razor component directlyCharacterFinalizationStep),
                IsRequired = true
            }
        },
        {
            CharacterCreationStep.Review,
            new StepConfiguration
            {
                Title = "Review & Create",
                Description = "Review your character and finalize creation.",
                // ComponentType removed - using Razor component directlyCharacterReviewStep),
                IsRequired = true
            }
        }
    };

    // Computed Properties
    private CharacterCreationStep CurrentStep => _session?.CurrentStep ?? CharacterCreationStep.ClassSelection;
    private StepConfiguration CurrentStepConfig => _stepConfiguration[CurrentStep];
    private CharacterBuilder CharacterData => _session?.CharacterData ?? new CharacterBuilder();
    
    private int StepCount => _stepConfiguration.Count;
    private int CurrentStepIndex => (int)CurrentStep;
    private double ProgressPercentage => StateService.GetProgressPercentage(CurrentStep, CharacterData);
    
    private bool CanGoBack => CurrentStep > CharacterCreationStep.ClassSelection;
    private bool CanGoNext => !_isNavigating && IsCurrentStepValid();
    private bool IsLastStep => CurrentStep == CharacterCreationStep.Review;

    protected override async Task OnInitializedAsync()
    {
        await LoadSession();
    }

    private async Task LoadSession()
    {
        _isLoading = true;
        _errorMessage = null;

        try
        {
            _session = await CharacterCreationService.CreateSessionAsync(UserId, CampaignId);
            _uiState = await StateService.GetUIStateAsync(_session.Id);
            
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to initialize character creation: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task NavigateToStep(CharacterCreationStep targetStep)
    {
        if (_session == null || _isNavigating)
            return;

        // Validate current step before navigation
        if (!await ValidateCurrentStep())
            return;

        _isNavigating = true;

        try
        {
            _session.CurrentStep = targetStep;
            _session.LastModified = DateTime.UtcNow;

            await CharacterCreationService.UpdateSessionAsync(_session);
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to navigate: {ex.Message}";
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async Task<bool> ValidateCurrentStep()
    {
        if (_session == null)
            return false;

        try
        {
            var isValid = await CharacterCreationService.ValidateStepAsync(_session, CurrentStep);
            if (!isValid)
            {
                _errorMessage = "Please complete all required fields before continuing.";
                return false;
            }

            _errorMessage = null;
            return true;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Validation error: {ex.Message}";
            return false;
        }
    }

    private bool IsCurrentStepValid()
    {
        if (_session == null)
            return false;

        return CurrentStep switch
        {
            CharacterCreationStep.ClassSelection => CharacterData.SelectedClass != null,
            CharacterCreationStep.AncestrySelection => CharacterData.SelectedAncestry != null,
            CharacterCreationStep.BackgroundSelection => CharacterData.SelectedBackground != null,
            CharacterCreationStep.AbilityScores => CharacterData.AbilityScores.Count == 6,
            CharacterCreationStep.SkillSelection => CharacterData.SelectedSkills.Any(),
            CharacterCreationStep.FeatSelection => CharacterData.SelectedFeats.Any(),
            CharacterCreationStep.EquipmentSelection => true, // Optional step
            CharacterCreationStep.SpellSelection => !IsSpellcaster() || CharacterData.KnownSpells.Any(),
            CharacterCreationStep.Finalization => !string.IsNullOrWhiteSpace(CharacterData.Name),
            CharacterCreationStep.Review => CharacterData.IsValid,
            _ => false
        };
    }

    private bool IsSpellcaster()
    {
        return CharacterData.SelectedClass?.IsSpellcaster == true;
    }

    private async Task GoNext()
    {
        if (_session == null)
            return;

        var nextStep = await CharacterCreationService.GetNextStepAsync(CurrentStep, CharacterData);
        if (nextStep.HasValue)
        {
            await NavigateToStep(nextStep.Value);
        }
        else if (IsLastStep)
        {
            await CompleteCharacterCreation();
        }
    }

    private async Task GoPrevious()
    {
        if (_session == null)
            return;

        var previousStep = await CharacterCreationService.GetPreviousStepAsync(CurrentStep);
        if (previousStep.HasValue)
        {
            await NavigateToStep(previousStep.Value);
        }
    }

    private async Task CompleteCharacterCreation()
    {
        if (_session == null)
            return;

        _isNavigating = true;

        try
        {
            var characterId = await CharacterCreationService.FinalizeCharacterAsync(_session);
            await OnCharacterCreated.InvokeAsync(characterId);
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to create character: {ex.Message}";
        }
        finally
        {
            _isNavigating = false;
        }
    }

    private async Task CancelCreation()
    {
        if (_session != null)
        {
            await CharacterCreationService.DeleteSessionAsync(_session.Id);
        }

        await OnCancelled.InvokeAsync();
    }

    private async Task OnStepDataChanged()
    {
        if (_session == null)
            return;

        _session.LastModified = DateTime.UtcNow;
        await CharacterCreationService.UpdateSessionAsync(_session);
        
        // Update UI state
        await StateService.UpdateUIStateAsync(_session.Id, _uiState);
        
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        // Clean up any resources if needed
    }

    // Helper class for step configuration
    private class StepConfiguration
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Type ComponentType { get; set; } = typeof(object);
        public bool IsRequired { get; set; } = true;
    }
}