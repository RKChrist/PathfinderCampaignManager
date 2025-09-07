using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PathfinderCampaignManager.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using static PathfinderCampaignManager.Presentation.Client.Components.Notes.NotesPanel;

namespace PathfinderCampaignManager.Presentation.Client.Components.Notes;

public partial class NoteCard : ComponentBase
{
    [Parameter] public CharacterNoteDto Note { get; set; } = null!;
    [Parameter] public bool IsEditing { get; set; }
    [Parameter] public bool CanEdit { get; set; }
    [Parameter] public EventCallback<CharacterNoteDto> OnEdit { get; set; }
    [Parameter] public EventCallback<CharacterNoteDto> OnSave { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public EventCallback<CharacterNoteDto> OnDelete { get; set; }
    [Parameter] public EventCallback<(CharacterNoteDto Note, NoteVisibility Visibility)> OnVisibilityChange { get; set; }
    [Parameter] public EventCallback<CharacterNoteDto> OnPin { get; set; }

    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;

    private ElementReference _editTitleRef;
    private string _editTitle = string.Empty;
    private string _editContent = string.Empty;
    private bool _showVisibilityMenu = false;

    // Computed properties for template binding
    private string NoteCardClass => IsEditing ? "note-card editing" : "note-card";
    private string PinButtonClass => Note.IsPinned ? "action-button pin-button pinned" : "action-button pin-button";
    private string PinTitle => Note.IsPinned ? "Unpin note" : "Pin note";
    
    private (string Icon, string Title) VisibilityInfo => Note.Visibility switch
    {
        NoteVisibility.Private => ("fas fa-user", "Private - Only visible to you"),
        NoteVisibility.Shared => ("fas fa-users", "Shared - Visible to character owner and DM"),
        NoteVisibility.DMOnly => ("fas fa-crown", "DM Only - Visible only to DMs"),
        _ => ("fas fa-question", "Unknown")
    };

    private string GetVisibilityOptionClass(NoteVisibility visibility) => 
        Note.Visibility == visibility ? "visibility-option active" : "visibility-option";

    protected override void OnParametersSet()
    {
        if (IsEditing)
        {
            _editTitle = Note.Title;
            _editContent = Note.Content;
            
            // Focus title input after render
            _ = Task.Delay(100).ContinueWith(async _ =>
            {
                await InvokeAsync(async () =>
                {
                    await _editTitleRef.FocusAsync();
                });
            });
        }
    }

    private void StartEdit()
    {
        OnEdit.InvokeAsync(Note);
    }

    private async Task SaveEdit()
    {
        var updatedNote = new CharacterNoteDto
        {
            Id = Note.Id,
            CharacterId = Note.CharacterId,
            AuthorId = Note.AuthorId,
            AuthorName = Note.AuthorName,
            Title = _editTitle?.Trim() ?? string.Empty,
            Content = _editContent?.Trim() ?? string.Empty,
            Visibility = Note.Visibility,
            Color = Note.Color,
            IsPinned = Note.IsPinned,
            Tags = ParseTags(_editContent),
            CreatedAt = Note.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        await OnSave.InvokeAsync(updatedNote);
    }

    private void CancelEdit()
    {
        _editTitle = string.Empty;
        _editContent = string.Empty;
        OnCancel.InvokeAsync();
    }

    private async Task DeleteNote()
    {
        if (await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure you want to delete this note?"))
        {
            await OnDelete.InvokeAsync(Note);
        }
    }

    private void ShowVisibilityMenu()
    {
        _showVisibilityMenu = true;
    }

    private void HideVisibilityMenu()
    {
        _showVisibilityMenu = false;
    }

    private async Task ChangeVisibility(NoteVisibility newVisibility)
    {
        if (newVisibility != Note.Visibility)
        {
            await OnVisibilityChange.InvokeAsync((Note, newVisibility));
        }
        HideVisibilityMenu();
    }

    private string GetFormattedDate()
    {
        var date = Note.UpdatedAt ?? Note.CreatedAt;
        var now = DateTime.UtcNow;
        var diff = now - date;

        return diff.TotalDays switch
        {
            < 1 when diff.TotalHours < 1 => "Just now",
            < 1 when diff.TotalHours < 2 => "1 hour ago",
            < 1 => $"{(int)diff.TotalHours} hours ago",
            < 2 => "Yesterday",
            < 7 => $"{(int)diff.TotalDays} days ago",
            < 30 => $"{(int)(diff.TotalDays / 7)} weeks ago",
            _ => date.ToString("MMM dd, yyyy")
        };
    }

    private string FormatContent(string content)
    {
        if (string.IsNullOrEmpty(content)) return string.Empty;

        // Simple markdown-like formatting
        var formatted = content
            .Replace("\r\n", "\n")
            .Replace("\n", "<br>")
            .Replace("**", "</strong>")
            .Replace("**", "<strong>")
            .Replace("*", "</em>")
            .Replace("*", "<em>");

        // Highlight hashtags
        var words = formatted.Split(' ');
        for (int i = 0; i < words.Length; i++)
        {
            if (words[i].StartsWith("#") && words[i].Length > 1)
            {
                words[i] = $"<span class=\"hashtag\">{words[i]}</span>";
            }
        }

        return string.Join(" ", words);
    }

    private List<string> ParseTags(string? content)
    {
        if (string.IsNullOrEmpty(content)) return new List<string>();
        
        var tags = new List<string>();
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (var word in words)
        {
            if (word.StartsWith("#") && word.Length > 1)
            {
                var tag = word.Substring(1).ToLowerInvariant();
                // Remove punctuation from end of tag
                tag = System.Text.RegularExpressions.Regex.Replace(tag, @"[^\w]$", "");
                if (!string.IsNullOrEmpty(tag))
                {
                    tags.Add(tag);
                }
            }
        }
        
        return tags.Distinct().ToList();
    }

}