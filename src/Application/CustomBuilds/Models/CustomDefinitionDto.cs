using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Application.CustomBuilds.Models;

public class CustomDefinitionDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public CustomDefinitionType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string JsonData { get; set; } = "{}";
    public int Version { get; set; }
    public string Rarity { get; set; } = "Common";
    public List<string> Traits { get; set; } = new();
    public string Source { get; set; } = "Custom";
    public bool IsPublic { get; set; }
    public bool IsApproved { get; set; }
    public string? ImageUrl { get; set; }
    public int Level { get; set; }
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<ModifierDto> Modifiers { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ModifierDto
{
    public Guid Id { get; set; }
    public ModifierTarget Target { get; set; }
    public int Value { get; set; }
    public ModifierType ModifierType { get; set; }
    public string? Condition { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}