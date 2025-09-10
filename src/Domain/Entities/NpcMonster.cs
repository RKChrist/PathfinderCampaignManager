using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Entities;

public class NpcMonster : BaseEntity
{
    public Guid? OwnerUserId { get; private set; } // Null for library NPCs/Monsters
    public Guid? SessionId { get; private set; }   // Null for personal library
    public string Name { get; private set; } = string.Empty;
    public NpcMonsterType Type { get; private set; } = NpcMonsterType.Humanoid;
    public int Level { get; private set; } = 1;
    public string Size { get; private set; } = "Medium";
    public string CreatureType { get; private set; } = "Humanoid";
    public string Alignment { get; private set; } = "Neutral";
    
    // Core Stats
    public int ArmorClass { get; private set; } = 10;
    public int HitPoints { get; private set; } = 10;
    public int Speed { get; private set; } = 25;
    
    // Ability Scores JSON
    public string AbilityScoresJson { get; private set; } = "{}";
    
    // Skills and Saves JSON
    public string SavingThrowsJson { get; private set; } = "{}";
    public string SkillsJson { get; private set; } = "{}";
    
    // Combat Data
    public string AttacksJson { get; private set; } = "[]";
    public string ActionsJson { get; private set; } = "[]";
    public string ReactionsJson { get; private set; } = "[]";
    public string SpellsJson { get; private set; } = "{}";
    
    // Traits and Features
    public string TraitsJson { get; private set; } = "[]";
    public string SpecialAbilitiesJson { get; private set; } = "[]";
    
    // Additional Data
    public string NotesJson { get; private set; } = "{}";
    public bool IsTemplate { get; private set; } = false;
    public string? Source { get; private set; }
    
    // Navigation
    public Session? Session { get; private set; }

    private NpcMonster() { } // For EF Core

    public static NpcMonster Create(
        string name,
        NpcMonsterType type,
        int level,
        Guid? ownerUserId = null,
        Guid? sessionId = null)
    {
        var npcMonster = new NpcMonster
        {
            Name = name,
            Type = type,
            Level = level,
            OwnerUserId = ownerUserId,
            SessionId = sessionId
        };

        npcMonster.RaiseDomainEvent(new NpcMonsterCreatedEvent(npcMonster.Id, name, type, ownerUserId, sessionId));
        return npcMonster;
    }

    public void UpdateBasicInfo(string name, string size, string creatureType, string alignment, Guid updatedBy)
    {
        Name = name;
        Size = size;
        CreatureType = creatureType;
        Alignment = alignment;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "BasicInfo", updatedBy));
    }

    public void UpdateCombatStats(int armorClass, int hitPoints, int speed, Guid updatedBy)
    {
        ArmorClass = armorClass;
        HitPoints = hitPoints;
        Speed = speed;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "CombatStats", updatedBy));
    }

    public void UpdateAbilityScores(string abilityScoresJson, Guid updatedBy)
    {
        AbilityScoresJson = abilityScoresJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "AbilityScores", updatedBy));
    }

    public void UpdateSavingThrows(string savingThrowsJson, Guid updatedBy)
    {
        SavingThrowsJson = savingThrowsJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "SavingThrows", updatedBy));
    }

    public void UpdateSkills(string skillsJson, Guid updatedBy)
    {
        SkillsJson = skillsJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "Skills", updatedBy));
    }

    public void UpdateAttacks(string attacksJson, Guid updatedBy)
    {
        AttacksJson = attacksJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "Attacks", updatedBy));
    }

    public void UpdateActions(string actionsJson, Guid updatedBy)
    {
        ActionsJson = actionsJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "Actions", updatedBy));
    }

    public void UpdateReactions(string reactionsJson, Guid updatedBy)
    {
        ReactionsJson = reactionsJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "Reactions", updatedBy));
    }

    public void UpdateSpells(string spellsJson, Guid updatedBy)
    {
        SpellsJson = spellsJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "Spells", updatedBy));
    }

    public void UpdateTraits(string traitsJson, Guid updatedBy)
    {
        TraitsJson = traitsJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "Traits", updatedBy));
    }

    public void UpdateSpecialAbilities(string specialAbilitiesJson, Guid updatedBy)
    {
        SpecialAbilitiesJson = specialAbilitiesJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "SpecialAbilities", updatedBy));
    }

    public void UpdateNotes(string notesJson, Guid updatedBy)
    {
        NotesJson = notesJson;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "Notes", updatedBy));
    }

    public void AssignToSession(Guid sessionId, Guid assignedBy)
    {
        if (SessionId.HasValue)
            throw new InvalidOperationException("NPC/Monster is already assigned to a session");

        SessionId = sessionId;
        Touch();
        RaiseDomainEvent(new NpcMonsterAssignedToSessionEvent(Id, sessionId, assignedBy));
    }

    public void RemoveFromSession(Guid removedBy)
    {
        if (!SessionId.HasValue)
            return;

        var previousSessionId = SessionId.Value;
        SessionId = null;
        Touch();
        RaiseDomainEvent(new NpcMonsterRemovedFromSessionEvent(Id, previousSessionId, removedBy));
    }

    public void MarkAsTemplate(Guid updatedBy)
    {
        IsTemplate = true;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "MarkedAsTemplate", updatedBy));
    }

    public void SetSource(string source, Guid updatedBy)
    {
        Source = source;
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "Source", updatedBy));
    }

    public void UpdateDetails(
        string name,
        NpcMonsterType type,
        int level,
        string? description = null,
        int? armorClass = null,
        int? hitPoints = null,
        int? speed = null,
        bool? isTemplate = null)
    {
        Name = name;
        Type = type;
        Level = level;
        
        if (armorClass.HasValue)
            ArmorClass = armorClass.Value;
        if (hitPoints.HasValue)
            HitPoints = hitPoints.Value;
        if (speed.HasValue)
            Speed = speed.Value;
        if (isTemplate.HasValue)
            IsTemplate = isTemplate.Value;
            
        Touch();
        RaiseDomainEvent(new NpcMonsterUpdatedEvent(Id, "Details", Guid.Empty));
    }
}

// Domain Events
public sealed record NpcMonsterCreatedEvent(
    Guid NpcMonsterId, 
    string Name, 
    NpcMonsterType Type, 
    Guid? OwnerUserId, 
    Guid? SessionId) : DomainEvent;

public sealed record NpcMonsterUpdatedEvent(
    Guid NpcMonsterId, 
    string UpdateType, 
    Guid UpdatedBy) : DomainEvent;

public sealed record NpcMonsterAssignedToSessionEvent(
    Guid NpcMonsterId, 
    Guid SessionId, 
    Guid AssignedBy) : DomainEvent;

public sealed record NpcMonsterRemovedFromSessionEvent(
    Guid NpcMonsterId, 
    Guid SessionId, 
    Guid RemovedBy) : DomainEvent;