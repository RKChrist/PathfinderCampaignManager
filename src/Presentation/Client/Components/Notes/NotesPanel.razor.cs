using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PathfinderCampaignManager.Domain.Entities;
using PathfinderCampaignManager.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Text.Json;

namespace PathfinderCampaignManager.Presentation.Client.Components.Notes;

public partial class NotesPanel : ComponentBase
{
    [Parameter] public Guid CharacterId { get; set; }
    [Parameter] public bool IsCurrentUserDM { get; set; }
    [Parameter] public Guid CurrentUserId { get; set; }

    [Inject] private HttpClient Http { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    private ElementReference _notesListRef;
    private ElementReference _newNoteTitleRef;

    private List<CharacterNoteDto> _allNotes = new();
    private List<CharacterNoteDto> _visibleNotes = new();
    private string _searchQuery = "";
    private string _viewMode = "all";
    private bool _isLoading = true;
    private bool _isCreatingNote = false;
    private bool _isSubmitting = false;
    private Guid? _editingNoteId;

    private CreateNoteModel _newNoteModel = new();

    private readonly List<(string Name, string Value)> _noteColors = new()
    {
        ("Yellow", "#fef3c7"),
        ("Blue", "#dbeafe"),
        ("Green", "#d1fae5"),
        ("Pink", "#fce7f3"),
        ("Purple", "#e9d5ff"),
        ("Orange", "#fed7aa"),
        ("Gray", "#f3f4f6")
    };

    private Timer? _searchTimer;

    // Computed properties for razor template
    private List<CharacterNoteDto> VisibleNotes => _visibleNotes;
    private List<CharacterNoteDto> AllNotes => _allNotes;
    private string SearchQuery
    {
        get => _searchQuery;
        set
        {
            _searchQuery = value;
            HandleSearchChange();
        }
    }
    
    private bool IsLoading => _isLoading;
    private bool IsCreatingNote => _isCreatingNote;
    private bool IsSubmitting => _isSubmitting;
    private Guid? EditingNoteId => _editingNoteId;
    private CreateNoteModel NewNoteModel => _newNoteModel;
    private List<(string Name, string Value)> NoteColors => _noteColors;
    
    private string GetViewModeClass(string mode) => _viewMode == mode ? "active" : "";
    private bool ShouldShowSearch => _allNotes.Count > 5;
    private bool HasPinnedNotes => _visibleNotes.Any(n => n.IsPinned);
    private bool ShowAddFirstNoteButton => _viewMode == "all" || _viewMode == "private";

    protected override async Task OnInitializedAsync()
    {
        await LoadNotes();
        await SetupNotesHotkeys();
    }

    protected override async Task OnParametersSetAsync()
    {
        if (CharacterId != Guid.Empty)
        {
            await LoadNotes();
        }
    }

    private async Task LoadNotes()
    {
        _isLoading = true;
        
        try
        {
            var response = await Http.GetAsync($"api/notes/character/{CharacterId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                _allNotes = JsonSerializer.Deserialize<List<CharacterNoteDto>>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                }) ?? new List<CharacterNoteDto>();
            }
            else
            {
                _allNotes = new List<CharacterNoteDto>();
                await ShowToast($"Failed to load notes: {response.ReasonPhrase}", "error");
            }
            FilterNotes();
        }
        catch (Exception ex)
        {
            _allNotes = new List<CharacterNoteDto>();
            await ShowToast($"Error loading notes: {ex.Message}", "error");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task SetupNotesHotkeys()
    {
        await JSRuntime.InvokeVoidAsync("setupNotesHotkeys", DotNetObjectReference.Create(this));
    }

    [JSInvokable]
    public void TriggerAddNote()
    {
        StartCreatingNote();
        InvokeAsync(StateHasChanged);
    }

    [JSInvokable]
    public void HandleEscapeKey()
    {
        if (_isCreatingNote)
        {
            CancelCreateNote();
        }
        else if (_editingNoteId.HasValue)
        {
            CancelEdit();
        }
        InvokeAsync(StateHasChanged);
    }

    private void SetViewMode(string mode)
    {
        _viewMode = mode;
        FilterNotes();
    }

    private void FilterNotes()
    {
        var filtered = _allNotes.AsEnumerable();

        // Apply visibility filter
        filtered = _viewMode switch
        {
            "private" => filtered.Where(n => n.AuthorId == CurrentUserId && n.Visibility == NoteVisibility.Private),
            "shared" => filtered.Where(n => n.Visibility == NoteVisibility.Shared),
            "dm" => filtered.Where(n => n.Visibility == NoteVisibility.DMOnly),
            _ => filtered.Where(n => CanViewNote(n))
        };

        // Apply search filter
        if (!string.IsNullOrEmpty(_searchQuery))
        {
            var query = _searchQuery.ToLowerInvariant();
            filtered = filtered.Where(n =>
                n.Title.ToLowerInvariant().Contains(query) ||
                n.Content.ToLowerInvariant().Contains(query) ||
                n.Tags.Any(t => t.ToLowerInvariant().Contains(query))
            );
        }

        _visibleNotes = filtered.ToList();
    }

    private void HandleSearchChange()
    {
        _searchTimer?.Dispose();
        _searchTimer = new Timer(_ => InvokeAsync(() =>
        {
            FilterNotes();
            StateHasChanged();
        }), null, 300, Timeout.Infinite);
    }

    private void HandleSearchKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        HandleSearchChange();
    }

    private void ClearSearch()
    {
        _searchQuery = "";
        FilterNotes();
    }

    private void StartCreatingNote()
    {
        _newNoteModel = new CreateNoteModel();
        _isCreatingNote = true;
        
        // Focus title input after render
        _ = Task.Delay(100).ContinueWith(async _ =>
        {
            await InvokeAsync(async () =>
            {
                await _newNoteTitleRef.FocusAsync();
            });
        });
    }

    private void CancelCreateNote()
    {
        _isCreatingNote = false;
        _newNoteModel = new CreateNoteModel();
    }

    private async Task CreateNote()
    {
        _isSubmitting = true;
        
        try
        {
            var request = new CreateNoteRequest
            {
                CharacterId = CharacterId,
                Title = _newNoteModel.Title?.Trim() ?? "",
                Content = _newNoteModel.Content?.Trim() ?? "",
                Visibility = _newNoteModel.Visibility,
                Color = _newNoteModel.Color,
                Tags = ParseTags(_newNoteModel.Content)
            };

            var response = await Http.PostAsJsonAsync("api/notes", request);
            if (response.IsSuccessStatusCode)
            {
                await LoadNotes();
                CancelCreateNote();
                await ShowToast("Note created successfully", "success");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await ShowToast($"Failed to create note: {errorContent}", "error");
            }
        }
        catch (Exception ex)
        {
            await ShowToast($"Failed to create note: {ex.Message}", "error");
        }
        finally
        {
            _isSubmitting = false;
        }
    }

    private async Task StartEditingNote(CharacterNoteDto note)
    {
        _editingNoteId = note.Id;
    }

    private async Task SaveNote(CharacterNoteDto note)
    {
        try
        {
            var request = new UpdateNoteRequest
            {
                Title = note.Title,
                Content = note.Content,
                Tags = note.Tags
            };

            var response = await Http.PutAsJsonAsync($"api/notes/{note.Id}", request);
            if (response.IsSuccessStatusCode)
            {
                _editingNoteId = null;
                await LoadNotes(); // Refresh to get updated data
                await ShowToast("Note updated successfully", "success");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await ShowToast($"Failed to save note: {errorContent}", "error");
            }
        }
        catch (Exception ex)
        {
            await ShowToast($"Failed to save note: {ex.Message}", "error");
        }
    }

    private void CancelEdit()
    {
        _editingNoteId = null;
    }

    private async Task DeleteNote(CharacterNoteDto note)
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this note?"))
        {
            try
            {
                var response = await Http.DeleteAsync($"api/notes/{note.Id}");
                if (response.IsSuccessStatusCode)
                {
                    await LoadNotes(); // Refresh to remove deleted note
                    await ShowToast("Note deleted successfully", "success");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await ShowToast($"Failed to delete note: {errorContent}", "error");
                }
            }
            catch (Exception ex)
            {
                await ShowToast($"Failed to delete note: {ex.Message}", "error");
            }
        }
    }

    private async Task ChangeNoteVisibility(CharacterNoteDto note, NoteVisibility newVisibility)
    {
        try
        {
            var request = new ChangeVisibilityRequest { Visibility = newVisibility };
            var response = await Http.PutAsJsonAsync($"api/notes/{note.Id}/visibility", request);
            
            if (response.IsSuccessStatusCode)
            {
                await LoadNotes(); // Refresh to get updated visibility
                await ShowToast("Note visibility updated successfully", "success");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await ShowToast($"Failed to update visibility: {errorContent}", "error");
            }
        }
        catch (Exception ex)
        {
            await ShowToast($"Failed to update visibility: {ex.Message}", "error");
        }
    }

    private async Task ToggleNotePin(CharacterNoteDto note)
    {
        try
        {
            var response = await Http.PutAsync($"api/notes/{note.Id}/pin", null);
            
            if (response.IsSuccessStatusCode)
            {
                await LoadNotes(); // Refresh to get updated pin status
                await ShowToast(note.IsPinned ? "Note unpinned" : "Note pinned", "success");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await ShowToast($"Failed to update pin status: {errorContent}", "error");
            }
        }
        catch (Exception ex)
        {
            await ShowToast($"Failed to update pin status: {ex.Message}", "error");
        }
    }

    private bool CanViewNote(CharacterNoteDto note)
    {
        return note.Visibility switch
        {
            NoteVisibility.Private => note.AuthorId == CurrentUserId,
            NoteVisibility.Shared => IsCurrentUserDM || note.AuthorId == CurrentUserId, // In real app, check if owner
            NoteVisibility.DMOnly => IsCurrentUserDM,
            _ => false
        };
    }

    private bool CanEditNote(CharacterNoteDto note)
    {
        return note.AuthorId == CurrentUserId || (IsCurrentUserDM && note.Visibility != NoteVisibility.Private);
    }

    private string GetEmptyMessage()
    {
        return _viewMode switch
        {
            "private" => "You haven't created any private notes yet",
            "shared" => "No shared notes for this character",
            "dm" => "No DM-only notes for this character",
            _ => "No notes for this character yet"
        };
    }

    private List<string> ParseTags(string? content)
    {
        if (string.IsNullOrEmpty(content)) return new List<string>();
        
        // Simple tag parsing - look for #hashtags
        var tags = new List<string>();
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            if (word.StartsWith("#") && word.Length > 1)
            {
                tags.Add(word.Substring(1).ToLowerInvariant());
            }
        }
        
        return tags.Distinct().ToList();
    }

    private async Task ShowToast(string message, string type = "info")
    {
        await JSRuntime.InvokeVoidAsync("showToast", message, type);
    }

    // DTOs and models
    public class CreateNoteModel
    {
        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        public NoteVisibility Visibility { get; set; } = NoteVisibility.Private;
        public string Color { get; set; } = "#fef3c7";
    }

    public class CharacterNoteDto
    {
        public Guid Id { get; set; }
        public Guid CharacterId { get; set; }
        public Guid AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NoteVisibility Visibility { get; set; }
        public string Color { get; set; } = string.Empty;
        public bool IsPinned { get; set; }
        public List<string> Tags { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int SortOrder { get; set; }
    }

    public class CreateNoteRequest
    {
        public Guid CharacterId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public NoteVisibility Visibility { get; set; }
        public string Color { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class UpdateNoteRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
    }

    public class ChangeVisibilityRequest
    {
        public NoteVisibility Visibility { get; set; }
    }
}