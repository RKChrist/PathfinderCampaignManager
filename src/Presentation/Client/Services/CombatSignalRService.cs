using Microsoft.AspNetCore.SignalR.Client;
using PathfinderCampaignManager.Presentation.Shared.Models;

namespace PathfinderCampaignManager.Presentation.Client.Services;

public class CombatSignalRService : IAsyncDisposable
{
    private HubConnection? _hubConnection;
    private readonly HttpClient _httpClient;

    public CombatSignalRService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task InitializeAsync(string? accessToken = null)
    {
        var baseAddress = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:7082";
        var hubUrl = $"{baseAddress}/combathub";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                }
                // Important: Allow credentials for CORS
                options.UseDefaultCredentials = true;
            })
            .WithAutomaticReconnect()
            .Build();

        await _hubConnection.StartAsync();
    }

    public async Task JoinCombatAsync(string combatId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("JoinCombat", combatId);
        }
    }

    public async Task LeaveCombatAsync(string combatId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("LeaveCombat", combatId);
        }
    }

    public async Task UpdateInitiativeAsync(string combatId, string participantId, int initiative)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("UpdateInitiative", combatId, participantId, initiative);
        }
    }

    public async Task UpdateHitPointsAsync(string combatId, string participantId, int currentHp, int maxHp)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("UpdateHitPoints", combatId, participantId, currentHp, maxHp);
        }
    }

    public async Task AddParticipantAsync(string combatId, CombatParticipant participant)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("AddParticipant", combatId, participant);
        }
    }

    public async Task RemoveParticipantAsync(string combatId, string participantId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("RemoveParticipant", combatId, participantId);
        }
    }

    public async Task StartCombatAsync(string combatId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("StartCombat", combatId);
        }
    }

    public async Task EndCombatAsync(string combatId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("EndCombat", combatId);
        }
    }

    public async Task NextTurnAsync(string combatId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("NextTurn", combatId);
        }
    }

    public async Task UpdateParticipantConditionsAsync(string combatId, string participantId, List<string> conditions)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("UpdateParticipantConditions", combatId, participantId, conditions);
        }
    }

    // New enhanced methods
    public async Task RollInitiativeAsync(string combatId, string participantId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("RollInitiative", combatId, participantId);
        }
    }

    public async Task AddTemporaryEffectAsync(string combatId, string participantId, object effect)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("AddTemporaryEffect", combatId, participantId, effect);
        }
    }

    public async Task RemoveTemporaryEffectAsync(string combatId, string participantId, string effectId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("RemoveTemporaryEffect", combatId, participantId, effectId);
        }
    }

    public async Task UpdateParticipantPositionAsync(string combatId, string participantId, int x, int y)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("UpdateParticipantPosition", combatId, participantId, x, y);
        }
    }

    public async Task SendCombatMessageAsync(string combatId, string message, string messageType = "action")
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("SendCombatMessage", combatId, message, messageType);
        }
    }

    public async Task RequestRerollAsync(string combatId, string rollType, string reason)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("RequestReroll", combatId, rollType, reason);
        }
    }

    public async Task ShareDiceRollAsync(string combatId, object rollResult)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("ShareDiceRoll", combatId, rollResult);
        }
    }

    public async Task UpdateCombatMapAsync(string combatId, object mapData)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("UpdateCombatMap", combatId, mapData);
        }
    }

    public async Task PauseCombatAsync(string combatId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("PauseCombat", combatId);
        }
    }

    public async Task ResumeCombatAsync(string combatId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("ResumeCombat", combatId);
        }
    }

    // Event subscriptions
    public event Action<CombatSession>? OnCombatStateUpdated;
    public event Action<string, int>? OnInitiativeUpdated;
    public event Action<string, int, int>? OnHitPointsUpdated;
    public event Action<CombatParticipant>? OnParticipantAdded;
    public event Action<string>? OnParticipantRemoved;
    public event Action<CombatSession>? OnCombatStarted;
    public event Action<CombatSession>? OnCombatEnded;
    public event Action<int, int>? OnTurnChanged;
    public event Action<string, List<string>>? OnConditionsUpdated;

    // New enhanced events
    public event Action<string, int, int, int>? OnInitiativeRolled; // participantId, roll, modifier, total
    public event Action<string, object>? OnTemporaryEffectAdded;
    public event Action<string, string>? OnTemporaryEffectRemoved;
    public event Action<string, int, int>? OnParticipantPositionUpdated;
    public event Action<object>? OnCombatMessageReceived;
    public event Action<object>? OnRerollRequested;
    public event Action<object>? OnDiceRollShared;
    public event Action<object>? OnCombatMapUpdated;
    public event Action<CombatSession>? OnCombatPaused;
    public event Action<CombatSession>? OnCombatResumed;

    public void SubscribeToEvents()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<CombatSession>("CombatStateUpdated", (session) =>
        {
            OnCombatStateUpdated?.Invoke(session);
        });

        _hubConnection.On<string, int>("InitiativeUpdated", (participantId, initiative) =>
        {
            OnInitiativeUpdated?.Invoke(participantId, initiative);
        });

        _hubConnection.On<string, int, int>("HitPointsUpdated", (participantId, currentHp, maxHp) =>
        {
            OnHitPointsUpdated?.Invoke(participantId, currentHp, maxHp);
        });

        _hubConnection.On<CombatParticipant>("ParticipantAdded", (participant) =>
        {
            OnParticipantAdded?.Invoke(participant);
        });

        _hubConnection.On<string>("ParticipantRemoved", (participantId) =>
        {
            OnParticipantRemoved?.Invoke(participantId);
        });

        _hubConnection.On<CombatSession>("CombatStarted", (session) =>
        {
            OnCombatStarted?.Invoke(session);
        });

        _hubConnection.On<CombatSession>("CombatEnded", (session) =>
        {
            OnCombatEnded?.Invoke(session);
        });

        _hubConnection.On<int, int>("TurnChanged", (currentTurn, round) =>
        {
            OnTurnChanged?.Invoke(currentTurn, round);
        });

        _hubConnection.On<string, List<string>>("ConditionsUpdated", (participantId, conditions) =>
        {
            OnConditionsUpdated?.Invoke(participantId, conditions);
        });

        // New enhanced event handlers
        _hubConnection.On<string, int, int, int>("InitiativeRolled", (participantId, roll, modifier, total) =>
        {
            OnInitiativeRolled?.Invoke(participantId, roll, modifier, total);
        });

        _hubConnection.On<string, object>("TemporaryEffectAdded", (participantId, effect) =>
        {
            OnTemporaryEffectAdded?.Invoke(participantId, effect);
        });

        _hubConnection.On<string, string>("TemporaryEffectRemoved", (participantId, effectId) =>
        {
            OnTemporaryEffectRemoved?.Invoke(participantId, effectId);
        });

        _hubConnection.On<string, int, int>("ParticipantPositionUpdated", (participantId, x, y) =>
        {
            OnParticipantPositionUpdated?.Invoke(participantId, x, y);
        });

        _hubConnection.On<object>("CombatMessageReceived", (message) =>
        {
            OnCombatMessageReceived?.Invoke(message);
        });

        _hubConnection.On<object>("RerollRequested", (request) =>
        {
            OnRerollRequested?.Invoke(request);
        });

        _hubConnection.On<object>("DiceRollShared", (rollResult) =>
        {
            OnDiceRollShared?.Invoke(rollResult);
        });

        _hubConnection.On<object>("CombatMapUpdated", (mapData) =>
        {
            OnCombatMapUpdated?.Invoke(mapData);
        });

        _hubConnection.On<CombatSession>("CombatPaused", (session) =>
        {
            OnCombatPaused?.Invoke(session);
        });

        _hubConnection.On<CombatSession>("CombatResumed", (session) =>
        {
            OnCombatResumed?.Invoke(session);
        });
    }

    public void UnsubscribeFromEvents()
    {
        OnCombatStateUpdated = null;
        OnInitiativeUpdated = null;
        OnHitPointsUpdated = null;
        OnParticipantAdded = null;
        OnParticipantRemoved = null;
        OnCombatStarted = null;
        OnCombatEnded = null;
        OnTurnChanged = null;
        OnConditionsUpdated = null;
        
        // New enhanced events
        OnInitiativeRolled = null;
        OnTemporaryEffectAdded = null;
        OnTemporaryEffectRemoved = null;
        OnParticipantPositionUpdated = null;
        OnCombatMessageReceived = null;
        OnRerollRequested = null;
        OnDiceRollShared = null;
        OnCombatMapUpdated = null;
        OnCombatPaused = null;
        OnCombatResumed = null;
    }

    public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

    public async ValueTask DisposeAsync()
    {
        UnsubscribeFromEvents();
        
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}