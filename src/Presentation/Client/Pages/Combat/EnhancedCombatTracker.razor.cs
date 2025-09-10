using Microsoft.AspNetCore.Components;
using PathfinderCampaignManager.Presentation.Client.Services;
using PathfinderCampaignManager.Domain.Entities.Combat;
using PathfinderCampaignManager.Presentation.Shared.Models;
using System.Net.Http.Json;
using Microsoft.JSInterop;

namespace PathfinderCampaignManager.Presentation.Client.Pages.Combat;

public partial class EnhancedCombatTracker : ComponentBase, IAsyncDisposable
{
    [Parameter] public Guid CampaignId { get; set; }

    private PathfinderCampaignManager.Presentation.Shared.Models.CombatSession? _combatSession;
    private bool _isConnected;
    private bool _isDM;
    private Guid _currentUserId;
    private string CampaignName = string.Empty;
    
    // Modal states
    private bool _showAddParticipantModal;
    private bool _showCharacterSheetModal;
    private bool _showNotesModal;
    private bool _showAddConditionModal;
    
    // Form data
    private Dictionary<Guid, int> _hpInputs = new();
    private PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant? _selectedParticipant;
    private string _newParticipantName = string.Empty;
    private int _newParticipantInitiative = 10;
    private int _newParticipantHP = 20;
    private int _newParticipantAC = 15;
    private CombatParticipantType _newParticipantType = CombatParticipantType.Monster;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Get current user info
            var token = await AuthService.GetTokenAsync();
            _currentUserId = await GetCurrentUserIdFromToken(token);
            
            // Check if user is DM
            await LoadCampaignInfo();
            
            // Initialize HP inputs for all participants
            if (_combatSession?.Participants != null)
            {
                foreach (var participant in _combatSession.Participants)
                {
                    _hpInputs[participant.Id] = 0;
                }
            }

            // Connect to SignalR
            await ConnectToSignalR();
            
            // Load combat session
            await LoadCombatSession();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing combat tracker: {ex.Message}");
        }
    }

    private async Task LoadCampaignInfo()
    {
        try
        {
            var response = await Http.GetAsync($"api/campaign/{CampaignId}");
            if (response.IsSuccessStatusCode)
            {
                var campaignDto = await response.Content.ReadFromJsonAsync<CampaignDto>();
                if (campaignDto != null)
                {
                    CampaignName = campaignDto.Name;
                    _isDM = campaignDto.DMUserId == _currentUserId;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading campaign info: {ex.Message}");
        }
    }

    private async Task ConnectToSignalR()
    {
        try
        {
            var token = await AuthService.GetTokenAsync();
            await SignalRService.InitializeAsync(token);
            SignalRService.SubscribeToEvents();
            
            // Subscribe to combat events
            SignalRService.OnCombatUpdated += OnCombatUpdated;
            SignalRService.OnParticipantUpdated += OnParticipantUpdated;
            SignalRService.OnTurnChanged += OnTurnChanged;
            SignalRService.OnParticipantAdded += OnParticipantAdded;
            
            await SignalRService.JoinCombatAsync(CampaignId.ToString());
            _isConnected = SignalRService.IsConnected;
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to SignalR: {ex.Message}");
        }
    }

    private async Task LoadCombatSession()
    {
        try
        {
            var response = await Http.GetAsync($"api/combat/campaigns/{CampaignId}");
            if (response.IsSuccessStatusCode)
            {
                _combatSession = await response.Content.ReadFromJsonAsync<PathfinderCampaignManager.Presentation.Shared.Models.CombatSession>();
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading combat session: {ex.Message}");
        }
    }

    // Combat Control Methods
    private async Task StartCombat()
    {
        if (!_isDM || _combatSession == null) return;

        try
        {
            var response = await Http.PostAsync($"api/combat/campaigns/{CampaignId}/start", null);
            if (response.IsSuccessStatusCode)
            {
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error starting combat: {ex.Message}");
        }
    }

    private async Task NextTurn()
    {
        if (!_isDM || _combatSession == null) return;

        try
        {
            var response = await Http.PostAsync($"api/combat/campaigns/{CampaignId}/next-turn", null);
            if (response.IsSuccessStatusCode)
            {
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error advancing turn: {ex.Message}");
        }
    }

    private async Task PauseCombat()
    {
        if (!_isDM || _combatSession == null) return;

        try
        {
            var response = await Http.PostAsync($"api/campaigns/{CampaignId}/combat/pause", null);
            if (response.IsSuccessStatusCode)
            {
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error pausing combat: {ex.Message}");
        }
    }

    private async Task EndCombat()
    {
        if (!_isDM || _combatSession == null) return;

        try
        {
            var response = await Http.PostAsync($"api/campaigns/{CampaignId}/combat/end", null);
            if (response.IsSuccessStatusCode)
            {
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ending combat: {ex.Message}");
        }
    }

    // Participant Management
    private async Task UpdateInitiative(Guid participantId, int newInitiative)
    {
        if (!_isDM) return;

        try
        {
            var request = new { participantId, initiative = newInitiative };
            var response = await Http.PutAsJsonAsync($"api/campaigns/{CampaignId}/combat/initiative", request);
            if (response.IsSuccessStatusCode)
            {
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating initiative: {ex.Message}");
        }
    }

    private async Task ApplyHealing(Guid participantId)
    {
        if (!_isDM || !_hpInputs.ContainsKey(participantId)) return;

        var amount = Math.Abs(_hpInputs[participantId]);
        if (amount <= 0) return;

        await ApplyHPChange(participantId, amount);
        _hpInputs[participantId] = 0;
    }

    private async Task ApplyDamage(Guid participantId)
    {
        if (!_isDM || !_hpInputs.ContainsKey(participantId)) return;

        var amount = -Math.Abs(_hpInputs[participantId]);
        if (amount >= 0) return;

        await ApplyHPChange(participantId, amount);
        _hpInputs[participantId] = 0;
    }

    private async Task ApplyHPChange(Guid participantId, int change)
    {
        try
        {
            var request = new { participantId, change };
            var response = await Http.PutAsJsonAsync($"api/combat/campaigns/{CampaignId}/hp", request);
            if (response.IsSuccessStatusCode)
            {
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error applying HP change: {ex.Message}");
        }
    }

    private async Task RemoveCondition(Guid participantId, string condition)
    {
        if (!_isDM) return;

        try
        {
            var response = await Http.DeleteAsync($"api/campaigns/{CampaignId}/combat/condition/{participantId}/{condition}");
            if (response.IsSuccessStatusCode)
            {
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing condition: {ex.Message}");
        }
    }

    private async Task RemoveParticipant(Guid participantId)
    {
        if (!_isDM) return;

        try
        {
            var response = await Http.DeleteAsync($"api/campaigns/{CampaignId}/combat/participant/{participantId}");
            if (response.IsSuccessStatusCode)
            {
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing participant: {ex.Message}");
        }
    }

    // UI Helper Methods
    private bool ShouldShowParticipant(PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant participant)
    {
        // DMs see everything
        if (_isDM) return true;
        
        // Players see their own characters
        if (participant.IsPlayerCharacter && !string.IsNullOrEmpty(participant.CharacterId))
        {
            // In a real implementation, check if this character belongs to the current user
            return true;
        }

        // Players see visible NPCs/monsters (but with limited info)
        return !participant.IsHidden;
    }

    private string GetParticipantTypeDisplay(string type)
    {
        return type.ToUpperInvariant() switch
        {
            "PC" or "PLAYERCHARACTER" => "PC",
            "NPC" or "NONPLAYERCHARACTER" => "NPC", 
            "MONSTER" => "Monster",
            "HAZARD" => "Hazard",
            _ => type // Return the original if no match
        };
    }

    private string GetHPStatus(PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant participant)
    {
        if (participant.CurrentHitPoints <= 0)
            return "unconscious";
        if (participant.CurrentHitPoints <= participant.HitPoints * 0.25)
            return "critical";
        if (participant.CurrentHitPoints <= participant.HitPoints * 0.5)
            return "wounded";
        return "healthy";
    }

    private string FormatModifier(int modifier)
    {
        return modifier >= 0 ? $"+{modifier}" : modifier.ToString();
    }

    private string GetConditionDescription(string condition)
    {
        // In a real implementation, this would look up condition descriptions
        return $"Condition: {condition}";
    }

    // Modal Methods
    private void ShowAddParticipantModal()
    {
        if (!_isDM) return;
        _showAddParticipantModal = true;
        StateHasChanged();
    }

    private void ShowCharacterSheet(PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant participant)
    {
        _selectedParticipant = participant;
        _showCharacterSheetModal = true;
        StateHasChanged();
    }

    private void ShowNotesModal(Guid participantId)
    {
        if (!_isDM) return;
        var participant = _combatSession?.Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant != null)
        {
            _selectedParticipant = participant;
            _showNotesModal = true;
            StateHasChanged();
        }
    }

    private void ShowAddConditionModal(Guid participantId)
    {
        if (!_isDM) return;
        var participant = _combatSession?.Participants.FirstOrDefault(p => p.Id == participantId);
        if (participant != null)
        {
            _selectedParticipant = participant;
            _showAddConditionModal = true;
            StateHasChanged();
        }
    }

    // SignalR Event Handlers
    private void OnCombatUpdated(PathfinderCampaignManager.Presentation.Shared.Models.CombatSession combatSession)
    {
        InvokeAsync(async () =>
        {
            await LoadCombatSession();
            StateHasChanged();
        });
    }

    private void OnParticipantUpdated(PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant participant)
    {
        InvokeAsync(async () =>
        {
            await LoadCombatSession();
            StateHasChanged();
        });
    }

    private void OnTurnChanged(int currentTurn, int round)
    {
        InvokeAsync(async () =>
        {
            await LoadCombatSession();
            StateHasChanged();
        });
    }

    private void OnParticipantAdded(PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant participant)
    {
        InvokeAsync(async () =>
        {
            Console.WriteLine($"New participant added: {participant.Name}");
            await LoadCombatSession();
            
            // Initialize HP input for the new participant
            _hpInputs[participant.Id] = 0;
            
            StateHasChanged();
        });
    }

    private async Task AddParticipant()
    {
        if (!_isDM || string.IsNullOrWhiteSpace(_newParticipantName)) return;

        try
        {
            var request = new
            {
                name = _newParticipantName,
                type = _newParticipantType,
                initiative = _newParticipantInitiative,
                hitPoints = _newParticipantHP,
                armorClass = _newParticipantAC
            };

            var response = await Http.PostAsJsonAsync($"api/combat/campaigns/{CampaignId}/participant", request);
            if (response.IsSuccessStatusCode)
            {
                _showAddParticipantModal = false;
                _newParticipantName = string.Empty;
                _newParticipantInitiative = 10;
                _newParticipantHP = 20;
                _newParticipantAC = 15;
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding participant: {ex.Message}");
        }
    }

    private async Task AddCondition(string condition)
    {
        if (!_isDM || _selectedParticipant == null) return;

        try
        {
            var request = new
            {
                participantId = _selectedParticipant.Id,
                condition = condition
            };

            var response = await Http.PostAsJsonAsync($"api/combat/campaigns/{CampaignId}/condition", request);
            if (response.IsSuccessStatusCode)
            {
                _showAddConditionModal = false;
                _selectedParticipant = null;
                await LoadCombatSession();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding condition: {ex.Message}");
        }
    }

    private async Task EditCharacter()
    {
        // Navigate to character edit page or show edit modal
        Console.WriteLine($"Edit character: {_selectedParticipant?.Name}");
    }

    // Utility Methods
    private async Task<Guid> GetCurrentUserIdFromToken(string? token)
    {
        // In a real implementation, decode JWT token to get user ID
        // For now, return a placeholder
        return Guid.NewGuid();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (SignalRService != null)
            {
                SignalRService.OnCombatUpdated -= OnCombatUpdated;
                SignalRService.OnParticipantUpdated -= OnParticipantUpdated;
                SignalRService.OnTurnChanged -= OnTurnChanged;
                SignalRService.OnParticipantAdded -= OnParticipantAdded;
                await SignalRService.DisposeAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing combat tracker: {ex.Message}");
        }
    }
}

// Supporting DTOs

public class CampaignDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid DMUserId { get; set; }
}