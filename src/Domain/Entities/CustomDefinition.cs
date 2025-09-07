using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Entities;

/// <summary>
/// Base entity for all custom user-created content (classes, items, feats, spells, operations)
/// </summary>
public class CustomDefinition : BaseEntity
{
    public Guid OwnerUserId { get; private set; }
    public CustomDefinitionType Type { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string JsonData { get; private set; } = "{}";
    public int Version { get; private set; } = 1;
    public string Rarity { get; private set; } = "Common";
    public List<string> Traits { get; private set; } = new();
    public string Source { get; private set; } = "Custom";
    public bool IsPublic { get; private set; } = false;
    public bool IsApproved { get; private set; } = false;
    public string? ImageUrl { get; private set; }
    public int Level { get; private set; } = 1;
    public string Category { get; private set; } = string.Empty;
    public string Tags { get; private set; } = "[]"; // JSON array of tags

    // Navigation properties
    public User Owner { get; private set; } = null!;
    public List<CustomDefinitionModifier> Modifiers { get; private set; } = new();
    public List<CustomBuildSetItem> BuildSetItems { get; private set; } = new();

    private CustomDefinition() { } // For EF Core

    public static CustomDefinition Create(
        Guid ownerUserId,
        CustomDefinitionType type,
        string name,
        string description,
        string jsonData,
        string category = "")
    {
        var definition = new CustomDefinition
        {
            OwnerUserId = ownerUserId,
            Type = type,
            Name = name,
            Description = description,
            JsonData = jsonData,
            Category = category
        };

        definition.RaiseDomainEvent(new CustomDefinitionCreatedEvent(definition.Id, ownerUserId, type, name));
        return definition;
    }

    public void UpdateContent(string name, string description, string jsonData, Guid updatedBy)
    {
        Name = name;
        Description = description;
        JsonData = jsonData;
        Version++;
        Touch();
        RaiseDomainEvent(new CustomDefinitionUpdatedEvent(Id, updatedBy, Version));
    }

    public void UpdateMetadata(string rarity, List<string> traits, int level, string category, Guid updatedBy)
    {
        Rarity = rarity;
        Traits = traits;
        Level = level;
        Category = category;
        Touch();
        RaiseDomainEvent(new CustomDefinitionUpdatedEvent(Id, updatedBy, Version));
    }

    public void SetTags(List<string> tags, Guid updatedBy)
    {
        Tags = System.Text.Json.JsonSerializer.Serialize(tags);
        Touch();
        RaiseDomainEvent(new CustomDefinitionUpdatedEvent(Id, updatedBy, Version));
    }

    public List<string> GetTags()
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(Tags) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public void SetImageUrl(string? imageUrl, Guid updatedBy)
    {
        ImageUrl = imageUrl;
        Touch();
        RaiseDomainEvent(new CustomDefinitionUpdatedEvent(Id, updatedBy, Version));
    }

    public void SetPublicStatus(bool isPublic, Guid updatedBy)
    {
        IsPublic = isPublic;
        Touch();
        RaiseDomainEvent(new CustomDefinitionVisibilityChangedEvent(Id, isPublic, updatedBy));
    }

    public void Approve(Guid approvedBy)
    {
        IsApproved = true;
        Touch();
        RaiseDomainEvent(new CustomDefinitionApprovedEvent(Id, approvedBy));
    }

    public void AddModifier(ModifierTarget target, int value, ModifierType modifierType, string? condition = null)
    {
        var modifier = new CustomDefinitionModifier
        {
            CustomDefinitionId = Id,
            Target = target,
            Value = value,
            ModifierType = modifierType,
            Condition = condition
        };

        Modifiers.Add(modifier);
        Touch();
    }

    public void RemoveModifier(Guid modifierId)
    {
        var modifier = Modifiers.FirstOrDefault(m => m.Id == modifierId);
        if (modifier != null)
        {
            Modifiers.Remove(modifier);
            Touch();
        }
    }

    public bool CanBeEditedBy(Guid userId)
    {
        return OwnerUserId == userId;
    }

    public bool CanBeViewedBy(Guid userId)
    {
        return IsPublic || OwnerUserId == userId;
    }
}

/// <summary>
/// Modifiers that can be applied by custom items, spells, or effects
/// </summary>
public class CustomDefinitionModifier : BaseEntity
{
    public Guid CustomDefinitionId { get; set; }
    public ModifierTarget Target { get; set; }
    public int Value { get; set; }
    public ModifierType ModifierType { get; set; }
    public string? Condition { get; set; }
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 0;

    // Navigation
    public CustomDefinition CustomDefinition { get; set; } = null!;

    public string GetDisplayName()
    {
        var sign = Value >= 0 ? "+" : "";
        var targetName = Target.ToString().Replace("_", " ");
        var typeName = ModifierType == ModifierType.Untyped ? "" : $" ({ModifierType})";
        
        return $"{sign}{Value} {targetName}{typeName}";
    }

    public bool CanStackWith(CustomDefinitionModifier other)
    {
        // Same target and same non-untyped modifier types don't stack
        if (Target == other.Target && ModifierType != ModifierType.Untyped && ModifierType == other.ModifierType)
        {
            return false;
        }
        
        return true;
    }
}

/// <summary>
/// A collection of custom definitions for a specific campaign or user
/// </summary>
public class CustomBuildSet : BaseEntity
{
    public Guid? CampaignId { get; private set; }
    public Guid UserId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public bool IsShared { get; private set; } = false;
    
    // Navigation
    public Campaign? Campaign { get; private set; }
    public User User { get; private set; } = null!;
    public List<CustomBuildSetItem> Items { get; private set; } = new();

    private CustomBuildSet() { } // For EF Core

    public static CustomBuildSet Create(Guid userId, string name, string description, Guid? campaignId = null)
    {
        var buildSet = new CustomBuildSet
        {
            UserId = userId,
            CampaignId = campaignId,
            Name = name,
            Description = description
        };

        buildSet.RaiseDomainEvent(new CustomBuildSetCreatedEvent(buildSet.Id, userId, campaignId));
        return buildSet;
    }

    public void AddCustomDefinition(Guid customDefinitionId, Guid addedBy)
    {
        if (Items.Any(i => i.CustomDefinitionId == customDefinitionId))
            return;

        var item = new CustomBuildSetItem
        {
            CustomBuildSetId = Id,
            CustomDefinitionId = customDefinitionId,
            AddedAt = DateTime.UtcNow
        };

        Items.Add(item);
        Touch();
        RaiseDomainEvent(new CustomDefinitionAddedToBuildSetEvent(Id, customDefinitionId, addedBy));
    }

    public void RemoveCustomDefinition(Guid customDefinitionId, Guid removedBy)
    {
        var item = Items.FirstOrDefault(i => i.CustomDefinitionId == customDefinitionId);
        if (item != null)
        {
            Items.Remove(item);
            Touch();
            RaiseDomainEvent(new CustomDefinitionRemovedFromBuildSetEvent(Id, customDefinitionId, removedBy));
        }
    }

    public void SetShared(bool isShared, Guid updatedBy)
    {
        IsShared = isShared;
        Touch();
        RaiseDomainEvent(new CustomBuildSetSharedEvent(Id, isShared, updatedBy));
    }
}

public class CustomBuildSetItem : BaseEntity
{
    public Guid CustomBuildSetId { get; set; }
    public Guid CustomDefinitionId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public CustomBuildSet CustomBuildSet { get; set; } = null!;
    public CustomDefinition CustomDefinition { get; set; } = null!;
}

// Domain Events
public sealed record CustomDefinitionCreatedEvent(
    Guid CustomDefinitionId,
    Guid OwnerUserId,
    CustomDefinitionType Type,
    string Name) : DomainEvent;

public sealed record CustomDefinitionUpdatedEvent(
    Guid CustomDefinitionId,
    Guid UpdatedBy,
    int Version) : DomainEvent;

public sealed record CustomDefinitionVisibilityChangedEvent(
    Guid CustomDefinitionId,
    bool IsPublic,
    Guid ChangedBy) : DomainEvent;

public sealed record CustomDefinitionApprovedEvent(
    Guid CustomDefinitionId,
    Guid ApprovedBy) : DomainEvent;

public sealed record CustomBuildSetCreatedEvent(
    Guid BuildSetId,
    Guid UserId,
    Guid? CampaignId) : DomainEvent;

public sealed record CustomDefinitionAddedToBuildSetEvent(
    Guid BuildSetId,
    Guid CustomDefinitionId,
    Guid AddedBy) : DomainEvent;

public sealed record CustomDefinitionRemovedFromBuildSetEvent(
    Guid BuildSetId,
    Guid CustomDefinitionId,
    Guid RemovedBy) : DomainEvent;

public sealed record CustomBuildSetSharedEvent(
    Guid BuildSetId,
    bool IsShared,
    Guid UpdatedBy) : DomainEvent;