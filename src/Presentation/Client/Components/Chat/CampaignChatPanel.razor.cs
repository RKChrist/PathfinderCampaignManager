using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using PathfinderCampaignManager.Domain.Enums;
using PathfinderCampaignManager.Presentation.Client.Services;
using PathfinderCampaignManager.Presentation.Shared.Models;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace PathfinderCampaignManager.Presentation.Client.Components.Chat;

public partial class CampaignChatPanel : ComponentBase, IAsyncDisposable
{
    [Parameter] public Guid CampaignId { get; set; }
    [Parameter] public bool IsDM { get; set; }
    [Parameter] public List<CampaignChatDto>? Messages { get; set; }
    [Parameter] public EventCallback<List<CampaignChatDto>> MessagesChanged { get; set; }

    [Inject] private CampaignSignalRService SignalRService { get; set; } = default!;
    [Inject] private IAuthenticationService AuthService { get; set; } = default!;
    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    private ElementReference messagesContainer;
    private string _newMessage = string.Empty;
    private bool _isPrivateMessage = false;
    private bool _showDiceRoller = false;
    private bool _showChatSettings = false;
    private string _diceExpression = string.Empty;
    private string _diceDescription = string.Empty;
    private Guid _currentUserId;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            // Get current user
            var token = await AuthService.GetTokenAsync();
            _currentUserId = await GetCurrentUserIdFromToken(token);

            // Load existing messages
            await LoadMessages();

            // Subscribe to SignalR events
            SignalRService.OnChatMessageReceived += OnChatMessageReceived;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing chat panel: {ex.Message}");
        }
    }

    private async Task LoadMessages()
    {
        try
        {
            var response = await Http.GetAsync($"api/campaigns/{CampaignId}/chat");
            if (response.IsSuccessStatusCode)
            {
                var messages = await response.Content.ReadFromJsonAsync<List<CampaignChatDto>>();
                if (messages != null)
                {
                    Messages = messages;
                    await MessagesChanged.InvokeAsync(Messages);
                    StateHasChanged();
                    await ScrollToBottom();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading chat messages: {ex.Message}");
        }
    }

    private async Task SendMessage()
    {
        if (string.IsNullOrWhiteSpace(_newMessage))
            return;

        try
        {
            var request = new SendChatMessageRequest
            {
                Content = _newMessage.Trim(),
                IsPrivate = _isPrivateMessage && IsDM
            };

            var response = await Http.PostAsJsonAsync($"api/campaigns/{CampaignId}/chat", request);
            if (response.IsSuccessStatusCode)
            {
                _newMessage = string.Empty;
                _isPrivateMessage = false;
                await LoadMessages();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    private async Task RollDice()
    {
        if (string.IsNullOrWhiteSpace(_diceExpression))
            return;

        try
        {
            var request = new RollDiceRequest
            {
                Expression = _diceExpression.Trim(),
                Description = _diceDescription.Trim(),
                IsPrivate = _isPrivateMessage && IsDM
            };

            var response = await Http.PostAsJsonAsync($"api/campaigns/{CampaignId}/chat/dice", request);
            if (response.IsSuccessStatusCode)
            {
                _diceExpression = string.Empty;
                _diceDescription = string.Empty;
                _showDiceRoller = false;
                _isPrivateMessage = false;
                await LoadMessages();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error rolling dice: {ex.Message}");
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !e.ShiftKey)
        {
            await SendMessage();
        }
    }

    private void ShowDiceRoller()
    {
        _showDiceRoller = true;
        StateHasChanged();
    }

    private void CloseDiceRoller()
    {
        _showDiceRoller = false;
        _diceExpression = string.Empty;
        _diceDescription = string.Empty;
        StateHasChanged();
    }

    private void TogglePrivateMessage()
    {
        if (IsDM)
        {
            _isPrivateMessage = !_isPrivateMessage;
            StateHasChanged();
        }
    }

    private void ToggleChatSettings()
    {
        _showChatSettings = !_showChatSettings;
        StateHasChanged();
    }

    private async Task ScrollToBottom()
    {
        try
        {
            await JSRuntime.InvokeVoidAsync("scrollToBottom", messagesContainer);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scrolling to bottom: {ex.Message}");
        }
    }

    private string GetMessageClass(CampaignChatDto message)
    {
        var classes = new List<string>();

        if (message.IsPrivate)
            classes.Add("private");

        switch (message.Type)
        {
            case CampaignChatMessageType.System:
                classes.Add("system");
                break;
            case CampaignChatMessageType.DiceRoll:
                classes.Add("dice-roll");
                break;
        }

        return string.Join(" ", classes);
    }

    // SignalR Event Handlers
    private void OnChatMessageReceived(string campaignId, string messageJson)
    {
        if (campaignId == CampaignId.ToString())
        {
            InvokeAsync(async () =>
            {
                await LoadMessages();
                await ScrollToBottom();
                StateHasChanged();
            });
        }
    }

    // Utility Methods
    private async Task<Guid> GetCurrentUserIdFromToken(string? token)
    {
        // In a real implementation, decode JWT token to get user ID
        return Guid.NewGuid(); // Placeholder
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (SignalRService != null)
            {
                SignalRService.OnChatMessageReceived -= OnChatMessageReceived;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing chat panel: {ex.Message}");
        }
    }
}

// Supporting DTOs
public class SendChatMessageRequest
{
    public string Content { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
}

public class RollDiceRequest
{
    public string Expression { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
}