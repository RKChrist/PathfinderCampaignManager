using PathfinderCampaignManager.Application.CharacterCreation.Models;

namespace PathfinderCampaignManager.Application.CharacterCreation.Services;

/// <summary>
/// Service for managing character creation sessions and validation
/// </summary>
public interface ICharacterCreationService
{
    // Session Management
    Task<CharacterCreationSession> CreateSessionAsync(Guid userId, Guid campaignId);
    Task<CharacterCreationSession?> GetSessionAsync(Guid sessionId);
    Task<CharacterCreationSession> UpdateSessionAsync(CharacterCreationSession session);
    Task DeleteSessionAsync(Guid sessionId);
    Task<bool> ValidateStepAsync(CharacterCreationSession session, CharacterCreationStep step);
    
    // Navigation
    Task<CharacterCreationStep?> GetNextStepAsync(CharacterCreationStep currentStep, CharacterBuilder character);
    Task<CharacterCreationStep?> GetPreviousStepAsync(CharacterCreationStep currentStep);
    bool CanNavigateToStep(CharacterCreationStep targetStep, CharacterBuilder character);
    
    // Data Loading
    Task<List<PathfinderClass>> GetAvailableClassesAsync();
    Task<PathfinderClass?> GetClassByIdAsync(string classId);
    Task<List<PathfinderAncestry>> GetAvailableAncestriesAsync();
    Task<PathfinderAncestry?> GetAncestryByIdAsync(string ancestryId);
    Task<List<PathfinderBackground>> GetAvailableBackgroundsAsync();
    Task<PathfinderBackground?> GetBackgroundByIdAsync(string backgroundId);
    
    // Character Building
    Task<List<PathfinderFeat>> GetAvailableFeatsAsync(CharacterBuilder character, string category);
    Task<List<PathfinderSpell>> GetAvailableSpellsAsync(CharacterBuilder character, int level);
    Task<List<PathfinderItem>> GetStartingEquipmentAsync(CharacterBuilder character);
    
    // Validation
    Task<List<string>> ValidateCharacterAsync(CharacterBuilder character);
    Task<bool> IsCharacterCompleteAsync(CharacterBuilder character);
    
    // Finalization
    Task<Guid> FinalizeCharacterAsync(CharacterCreationSession session);
}

/// <summary>
/// Service for loading and caching Pathfinder 2e game data
/// </summary>
public interface IPathfinderDataService
{
    // Data Loading
    Task<List<PathfinderClass>> LoadClassesAsync();
    Task<List<PathfinderAncestry>> LoadAncestriesAsync();
    Task<List<PathfinderBackground>> LoadBackgroundsAsync();
    Task<List<PathfinderFeat>> LoadFeatsAsync();
    Task<List<PathfinderSpell>> LoadSpellsAsync();
    Task<List<PathfinderItem>> LoadEquipmentAsync();
    
    // Filtering and Queries
    Task<List<PathfinderFeat>> GetFeatsByCategoryAsync(string category);
    Task<List<PathfinderFeat>> GetFeatsForClassAsync(string classId, int level);
    Task<List<PathfinderSpell>> GetSpellsForClassAsync(string classId, int level);
    Task<List<PathfinderItem>> GetEquipmentByCategoryAsync(string category);
    
    // Cache Management
    Task RefreshDataAsync();
    Task<DateTime> GetLastUpdateAsync();
}

/// <summary>
/// Service for character creation step validation and business rules
/// </summary>
public interface ICharacterValidationService
{
    // Step Validation
    ValidationResult ValidateClassSelection(CharacterBuilder character);
    ValidationResult ValidateAncestrySelection(CharacterBuilder character);
    ValidationResult ValidateBackgroundSelection(CharacterBuilder character);
    ValidationResult ValidateAbilityScores(CharacterBuilder character);
    ValidationResult ValidateSkillSelection(CharacterBuilder character);
    ValidationResult ValidateFeatSelection(CharacterBuilder character);
    ValidationResult ValidateEquipmentSelection(CharacterBuilder character);
    ValidationResult ValidateSpellSelection(CharacterBuilder character);
    
    // Prerequisites
    bool MeetsFeatPrerequisites(PathfinderFeat feat, CharacterBuilder character);
    bool CanSelectSkill(string skill, CharacterBuilder character);
    bool CanSelectSpell(PathfinderSpell spell, CharacterBuilder character);
    
    // Calculations
    int CalculateMaxSkillRanks(CharacterBuilder character);
    int CalculateMaxFeats(CharacterBuilder character, string category);
    int CalculateStartingGold(CharacterBuilder character);
    Dictionary<AbilityScore, int> CalculateFinalAbilityScores(CharacterBuilder character);
}

/// <summary>
/// Service for character creation UI state management
/// </summary>
public interface ICharacterCreationStateService
{
    // State Management
    Task<CharacterCreationUIState> GetUIStateAsync(Guid sessionId);
    Task UpdateUIStateAsync(Guid sessionId, CharacterCreationUIState state);
    Task ResetUIStateAsync(Guid sessionId);
    
    // Progress Tracking
    double GetProgressPercentage(CharacterCreationStep currentStep, CharacterBuilder character);
    List<CharacterCreationStep> GetCompletedSteps(CharacterBuilder character);
    List<CharacterCreationStep> GetAvailableSteps(CharacterBuilder character);
}

// Supporting Models
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    
    public static ValidationResult Success() => new() { IsValid = true };
    public static ValidationResult Failure(params string[] errors) => new() { IsValid = false, Errors = errors.ToList() };
}

public class CharacterCreationUIState
{
    public Guid SessionId { get; set; }
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> ComponentState { get; set; } = new();
    public List<string> ExpandedSections { get; set; } = new();
    public string? SearchQuery { get; set; }
    public Dictionary<string, string> Filters { get; set; } = new();
    
    public T? GetComponentState<T>(string componentKey) where T : class
    {
        if (ComponentState.TryGetValue(componentKey, out var state) && state is T typedState)
        {
            return typedState;
        }
        return null;
    }
    
    public void SetComponentState<T>(string componentKey, T state) where T : class
    {
        ComponentState[componentKey] = state;
    }
}