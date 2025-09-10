using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace PathfinderCampaignManager.Presentation.Client.Services;

public class CampaignSignalRService : IAsyncDisposable
{
    private readonly ILogger<CampaignSignalRService> _logger;
    private readonly HttpClient _httpClient;
    private HubConnection? _hubConnection;
    private bool _isConnected;

    public CampaignSignalRService(ILogger<CampaignSignalRService> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
    }

    // Events for campaign-level updates
    public event Func<string, string, DateTime, Task>? UserJoined;
    public event Func<string, string, DateTime, Task>? UserLeft;
    public event Func<List<object>, Task>? ConnectedUsersUpdated;
    public event Func<string, object, string, Task>? CharacterSheetUpdated;
    public event Func<string, object, Task>? CharacterSheetReceived;
    public event Func<string, Task>? CharacterSheetNotFound;
    public event Func<object, Task>? MessageReceived;
    public event Func<string, string, Task>? ChatMessageReceived;
    public event Action<string, string>? OnChatMessageReceived;
    public event Func<string, DateTime, Task>? CombatSessionStarted;
    public event Func<string, DateTime, Task>? CombatSessionEnded;
    public event Func<string, string, string, DateTime, Task>? UserStatusUpdated;
    public event Func<object, Task>? ContentShared;
    public event Func<object, Task>? CampaignStateUpdated;

    public bool IsConnected => _isConnected;

    public async Task StartAsync(string? accessToken = null)
    {
        try
        {
            var baseAddress = _httpClient.BaseAddress?.ToString().TrimEnd('/') ?? "https://localhost:7082";
            var hubUrl = $"{baseAddress}/campaignhub";

            var hubConnectionBuilder = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        options.AccessTokenProvider = () => Task.FromResult(accessToken);
                    }
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                });

            _hubConnection = hubConnectionBuilder.Build();

            // Register event handlers
            RegisterEventHandlers();

            // Handle connection state changes
            _hubConnection.Closed += OnConnectionClosed;
            _hubConnection.Reconnected += OnReconnected;
            _hubConnection.Reconnecting += OnReconnecting;

            await _hubConnection.StartAsync();
            _isConnected = true;
            
            _logger.LogInformation("Connected to Campaign SignalR Hub");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Campaign SignalR connection");
            throw;
        }
    }

    private void RegisterEventHandlers()
    {
        if (_hubConnection == null) return;

        _hubConnection.On<string, string, DateTime>("UserJoined", (userId, userName, timestamp) =>
        {
            _logger.LogInformation("User {UserName} ({UserId}) joined at {Timestamp}", userName, userId, timestamp);
            return UserJoined?.Invoke(userId, userName, timestamp) ?? Task.CompletedTask;
        });

        _hubConnection.On<string, string, DateTime>("UserLeft", (userId, userName, timestamp) =>
        {
            _logger.LogInformation("User {UserName} ({UserId}) left at {Timestamp}", userName, userId, timestamp);
            return UserLeft?.Invoke(userId, userName, timestamp) ?? Task.CompletedTask;
        });

        _hubConnection.On<List<object>>("ConnectedUsersUpdated", (users) =>
        {
            return ConnectedUsersUpdated?.Invoke(users) ?? Task.CompletedTask;
        });

        _hubConnection.On<string, object, string>("CharacterSheetUpdated", (characterId, characterData, updatedBy) =>
        {
            _logger.LogInformation("Character sheet {CharacterId} updated by {UpdatedBy}", characterId, updatedBy);
            return CharacterSheetUpdated?.Invoke(characterId, characterData, updatedBy) ?? Task.CompletedTask;
        });

        _hubConnection.On<string, object>("CharacterSheetReceived", (characterId, characterData) =>
        {
            return CharacterSheetReceived?.Invoke(characterId, characterData) ?? Task.CompletedTask;
        });

        _hubConnection.On<string>("CharacterSheetNotFound", (characterId) =>
        {
            return CharacterSheetNotFound?.Invoke(characterId) ?? Task.CompletedTask;
        });

        _hubConnection.On<object>("MessageReceived", (message) =>
        {
            return MessageReceived?.Invoke(message) ?? Task.CompletedTask;
        });

        _hubConnection.On<string, string>("ChatMessageReceived", (campaignId, messageJson) =>
        {
            OnChatMessageReceived?.Invoke(campaignId, messageJson);
            return ChatMessageReceived?.Invoke(campaignId, messageJson) ?? Task.CompletedTask;
        });

        _hubConnection.On<string, DateTime>("CombatSessionStarted", (combatId, timestamp) =>
        {
            _logger.LogInformation("Combat session {CombatId} started at {Timestamp}", combatId, timestamp);
            return CombatSessionStarted?.Invoke(combatId, timestamp) ?? Task.CompletedTask;
        });

        _hubConnection.On<string, DateTime>("CombatSessionEnded", (combatId, timestamp) =>
        {
            _logger.LogInformation("Combat session {CombatId} ended at {Timestamp}", combatId, timestamp);
            return CombatSessionEnded?.Invoke(combatId, timestamp) ?? Task.CompletedTask;
        });

        _hubConnection.On<string, string, string, DateTime>("UserStatusUpdated", (userId, status, activity, timestamp) =>
        {
            return UserStatusUpdated?.Invoke(userId, status, activity, timestamp) ?? Task.CompletedTask;
        });

        _hubConnection.On<object>("ContentShared", (content) =>
        {
            return ContentShared?.Invoke(content) ?? Task.CompletedTask;
        });

        _hubConnection.On<object>("CampaignStateUpdated", (campaignState) =>
        {
            return CampaignStateUpdated?.Invoke(campaignState) ?? Task.CompletedTask;
        });
    }

    public async Task JoinCampaignAsync(string campaignId, string userId, string userName)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("JoinCampaign", campaignId, userId, userName);
            _logger.LogInformation("Joined campaign {CampaignId} as {UserName}", campaignId, userName);
        }
    }

    public async Task LeaveCampaignAsync(string campaignId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("LeaveCampaign", campaignId);
            _logger.LogInformation("Left campaign {CampaignId}", campaignId);
        }
    }

    public async Task UpdateCharacterSheetAsync(string campaignId, string characterId, object characterData)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("UpdateCharacterSheet", campaignId, characterId, characterData);
        }
    }

    public async Task RequestCharacterSheetAsync(string campaignId, string characterId)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("RequestCharacterSheet", campaignId, characterId);
        }
    }

    public async Task BroadcastMessageAsync(string campaignId, string message, string messageType = "info")
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("BroadcastMessage", campaignId, message, messageType);
        }
    }

    public async Task UpdateUserStatusAsync(string campaignId, string status, string activity = "")
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("UpdateUserStatus", campaignId, status, activity);
        }
    }

    public async Task ShareContentAsync(string campaignId, string contentType, object content, string title = "")
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("ShareContent", campaignId, contentType, content, title);
        }
    }

    public async Task AddParticipantsToCombatAsync(string campaignId, List<object> participants)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            await _hubConnection.SendAsync("AddParticipantsToCombat", campaignId, participants);
            _logger.LogInformation("Added {Count} participants to combat for campaign {CampaignId}", participants.Count, campaignId);
        }
    }

    private async Task OnConnectionClosed(Exception? exception)
    {
        _isConnected = false;
        if (exception != null)
        {
            _logger.LogError(exception, "Campaign SignalR connection closed with error");
        }
        else
        {
            _logger.LogInformation("Campaign SignalR connection closed");
        }
    }

    private async Task OnReconnected(string? connectionId)
    {
        _isConnected = true;
        _logger.LogInformation("Campaign SignalR reconnected with connection ID: {ConnectionId}", connectionId);
    }

    private async Task OnReconnecting(Exception? exception)
    {
        _isConnected = false;
        _logger.LogWarning(exception, "Campaign SignalR attempting to reconnect");
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}