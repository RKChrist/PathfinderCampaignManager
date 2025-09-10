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
        var baseAddress = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://localhost:7082";
        var hubUrl = $"{baseAddress}/combathub";
        
        Console.WriteLine($"Initializing SignalR connection to: {hubUrl}");
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                if (!string.IsNullOrEmpty(accessToken))
                {
                    options.AccessTokenProvider = () => Task.FromResult(accessToken);
                }
                // Configure for CORS and Blazor WebAssembly - allow fallback transports
                options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets | 
                                    Microsoft.AspNetCore.Http.Connections.HttpTransportType.LongPolling;
            })
            .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10) })
            .Build();

        Console.WriteLine("Starting SignalR connection...");
        
        // Add connection state event handlers
        _hubConnection.Closed += async (error) =>
        {
            Console.WriteLine($"SignalR connection closed. Error: {error?.Message}");
        };

        _hubConnection.Reconnecting += async (error) =>
        {
            Console.WriteLine($"SignalR reconnecting. Error: {error?.Message}");
        };

        _hubConnection.Reconnected += async (connectionId) =>
        {
            Console.WriteLine($"SignalR reconnected. Connection ID: {connectionId}");
        };

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine($"SignalR connection state: {_hubConnection.State}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to start SignalR connection: {ex.Message}");
            Console.WriteLine($"Full exception: {ex}");
            throw;
        }
    }

    public async Task PingAsync()
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("Ping");
        }
    }

    public async Task JoinCombatAsync(string combatId, string? campaignId = null)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("JoinCombat", combatId, campaignId);
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

    public async Task UpdateArmorClassAsync(string combatId, string participantId, int armorClass)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("UpdateArmorClass", combatId, participantId, armorClass);
        }
    }

    public async Task UpdateSaveAsync(string combatId, string participantId, string saveType, int value)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.InvokeAsync("UpdateSave", combatId, participantId, saveType, value);
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
    public event Action<CombatSession>? OnCombatUpdated;
    public event Action<string, int>? OnInitiativeUpdated;
    public event Action<string, int, int>? OnHitPointsUpdated;
    public event Action<string, int>? OnArmorClassUpdated;
    public event Action<string, string, int>? OnSaveUpdated;
    public event Action<CombatParticipant>? OnParticipantAdded;
    public event Action<CombatParticipant>? OnParticipantUpdated;
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

        _hubConnection.On<CombatSession>("CombatUpdated", (session) =>
        {
            OnCombatUpdated?.Invoke(session);
        });

        _hubConnection.On<string, int>("InitiativeUpdated", (participantId, initiative) =>
        {
            OnInitiativeUpdated?.Invoke(participantId, initiative);
        });

        _hubConnection.On<string, int, int>("HitPointsUpdated", (participantId, currentHp, maxHp) =>
        {
            OnHitPointsUpdated?.Invoke(participantId, currentHp, maxHp);
        });

        _hubConnection.On<string, int>("ArmorClassUpdated", (participantId, armorClass) =>
        {
            OnArmorClassUpdated?.Invoke(participantId, armorClass);
        });

        _hubConnection.On<string, string, int>("SaveUpdated", (participantId, saveType, value) =>
        {
            OnSaveUpdated?.Invoke(participantId, saveType, value);
        });

        _hubConnection.On<CombatParticipant>("ParticipantAdded", (participant) =>
        {
            OnParticipantAdded?.Invoke(participant);
        });

        _hubConnection.On<CombatParticipant>("ParticipantUpdated", (participant) =>
        {
            OnParticipantUpdated?.Invoke(participant);
        });

        _hubConnection.On<string>("ParticipantRemoved", (participantId) =>
        {
            OnParticipantRemoved?.Invoke(participantId);
        });

        _hubConnection.On<List<PathfinderCampaignManager.Presentation.Shared.Models.CombatParticipant>>("ParticipantsAdded", (participants) =>
        {
            foreach (var participant in participants)
            {
                OnParticipantAdded?.Invoke(participant);
            }
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

        _hubConnection.On<string>("Pong", (message) =>
        {
            Console.WriteLine($"Received Pong: {message}");
        });

        _hubConnection.On<string>("Error", (errorMessage) =>
        {
            Console.WriteLine($"SignalR Error: {errorMessage}");
        });
    }

    public void UnsubscribeFromEvents()
    {
        OnCombatStateUpdated = null;
        OnCombatUpdated = null;
        OnInitiativeUpdated = null;
        OnHitPointsUpdated = null;
        OnArmorClassUpdated = null;
        OnSaveUpdated = null;
        OnParticipantAdded = null;
        OnParticipantUpdated = null;
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