using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Entities;

public class RulesVersion : BaseEntity
{
    private readonly List<ClassDef> _classes = new();
    
    public string Version { get; private set; } = string.Empty;
    public RulesVersionStatus Status { get; private set; } = RulesVersionStatus.Draft;
    public Guid CreatedBy { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public string? Description { get; private set; }

    public IReadOnlyCollection<ClassDef> Classes => _classes.AsReadOnly();

    private RulesVersion() { } // For EF Core

    public static RulesVersion CreateDraft(string version, Guid createdBy, string? description = null)
    {
        var rulesVersion = new RulesVersion
        {
            Version = version,
            CreatedBy = createdBy,
            Description = description
        };

        rulesVersion.RaiseDomainEvent(new RulesVersionCreatedEvent(rulesVersion.Id, version, createdBy));
        return rulesVersion;
    }

    public void Publish(Guid publishedBy)
    {
        if (Status != RulesVersionStatus.Draft)
            throw new InvalidOperationException("Only draft versions can be published");

        Status = RulesVersionStatus.Published;
        PublishedAt = DateTime.UtcNow;
        Touch();
        RaiseDomainEvent(new RulesVersionPublishedEvent(Id, Version, publishedBy));
    }

    public void Archive(Guid archivedBy)
    {
        Status = RulesVersionStatus.Archived;
        Touch();
        RaiseDomainEvent(new RulesVersionArchivedEvent(Id, Version, archivedBy));
    }

    public void AddClass(ClassDef classDef)
    {
        if (Status != RulesVersionStatus.Draft)
            throw new InvalidOperationException("Can only add classes to draft versions");

        if (_classes.Any(c => c.Name == classDef.Name))
            throw new InvalidOperationException($"Class '{classDef.Name}' already exists in this version");

        classDef.RulesVersionId = Id;
        _classes.Add(classDef);
        Touch();
    }

    public void UpdateClass(Guid classId, string name, string keyAbility, string progressionsJson, string featuresJson)
    {
        if (Status != RulesVersionStatus.Draft)
            throw new InvalidOperationException("Can only update classes in draft versions");

        var classDef = _classes.FirstOrDefault(c => c.Id == classId);
        if (classDef == null)
            throw new ArgumentException("Class not found");

        classDef.Update(name, keyAbility, progressionsJson, featuresJson);
        Touch();
    }

    public void RemoveClass(Guid classId)
    {
        if (Status != RulesVersionStatus.Draft)
            throw new InvalidOperationException("Can only remove classes from draft versions");

        var classDef = _classes.FirstOrDefault(c => c.Id == classId);
        if (classDef != null)
        {
            _classes.Remove(classDef);
            Touch();
        }
    }
}

public class ClassDef : BaseEntity
{
    public Guid RulesVersionId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string KeyAbility { get; private set; } = string.Empty;
    public string ProgressionsJson { get; private set; } = "{}"; // Level progression data
    public string FeaturesJson { get; private set; } = "{}"; // Class features by level
    public string MetadataJson { get; private set; } = "{}"; // Additional metadata

    public RulesVersion RulesVersion { get; set; } = null!;

    private ClassDef() { } // For EF Core

    public static ClassDef Create(string name, string keyAbility, string progressionsJson, string featuresJson, string? metadataJson = null)
    {
        var classDef = new ClassDef
        {
            Name = name,
            KeyAbility = keyAbility,
            ProgressionsJson = progressionsJson,
            FeaturesJson = featuresJson,
            MetadataJson = metadataJson ?? "{}"
        };

        classDef.RaiseDomainEvent(new ClassDefinitionCreatedEvent(classDef.Id, name));
        return classDef;
    }

    public void Update(string name, string keyAbility, string progressionsJson, string featuresJson, string? metadataJson = null)
    {
        Name = name;
        KeyAbility = keyAbility;
        ProgressionsJson = progressionsJson;
        FeaturesJson = featuresJson;
        if (metadataJson != null)
            MetadataJson = metadataJson;
        
        Touch();
        RaiseDomainEvent(new ClassDefinitionUpdatedEvent(Id, name));
    }
}

// Additional rules entities for future expansion
public class AncestryDef : BaseEntity
{
    public Guid RulesVersionId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public string TraitsJson { get; private set; } = "[]";
    public string FeaturesJson { get; private set; } = "{}";
    public string MetadataJson { get; private set; } = "{}";

    public RulesVersion RulesVersion { get; set; } = null!;

    private AncestryDef() { } // For EF Core
}

public class FeatDef : BaseEntity
{
    public Guid RulesVersionId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public int Level { get; private set; }
    public string Type { get; private set; } = string.Empty; // General, Class, Ancestry, etc.
    public string PrerequisitesJson { get; private set; } = "[]";
    public string EffectJson { get; private set; } = "{}";
    public string MetadataJson { get; private set; } = "{}";

    public RulesVersion RulesVersion { get; set; } = null!;

    private FeatDef() { } // For EF Core
}

public class SpellDef : BaseEntity
{
    public Guid RulesVersionId { get; set; }
    public string Name { get; private set; } = string.Empty;
    public int Level { get; private set; }
    public string School { get; private set; } = string.Empty;
    public string TraitsJson { get; private set; } = "[]";
    public string EffectJson { get; private set; } = "{}";
    public string MetadataJson { get; private set; } = "{}";

    public RulesVersion RulesVersion { get; set; } = null!;

    private SpellDef() { } // For EF Core
}

// Domain Events
public sealed record RulesVersionCreatedEvent(Guid RulesVersionId, string Version, Guid CreatedBy) : DomainEvent;
public sealed record RulesVersionPublishedEvent(Guid RulesVersionId, string Version, Guid PublishedBy) : DomainEvent;
public sealed record RulesVersionArchivedEvent(Guid RulesVersionId, string Version, Guid ArchivedBy) : DomainEvent;
public sealed record ClassDefinitionCreatedEvent(Guid ClassDefId, string Name) : DomainEvent;
public sealed record ClassDefinitionUpdatedEvent(Guid ClassDefId, string Name) : DomainEvent;