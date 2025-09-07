using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.Entities;

public class Encounter : BaseEntity
{
    private readonly List<Combatant> _combatants = new();

    public Guid SessionId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Round { get; private set; } = 0;
    public int CurrentRound => Round;
    public int CurrentTurn { get; private set; } = 0;
    public Guid? ActiveCombatantId { get; private set; }
    public string LogJson { get; private set; } = "[]";
    public bool IsActive { get; private set; } = false;
    public bool IsCompleted { get; private set; } = false;

    public IReadOnlyCollection<Combatant> Combatants => _combatants.AsReadOnly();
    public Session Session { get; private set; } = null!;
    public Combatant? ActiveCombatant => ActiveCombatantId.HasValue 
        ? _combatants.FirstOrDefault(c => c.Id == ActiveCombatantId.Value) 
        : null;

    private Encounter() { } // For EF Core

    public static Encounter Create(Guid sessionId, string name, string? description = null)
    {
        var encounter = new Encounter
        {
            SessionId = sessionId,
            Name = name,
            Description = description
        };

        encounter.RaiseDomainEvent(new EncounterCreatedEvent(encounter.Id, sessionId, name));
        return encounter;
    }

    public void StartEncounter()
    {
        if (IsActive)
            throw new InvalidOperationException("Encounter is already active");
        
        if (!_combatants.Any())
            throw new InvalidOperationException("Cannot start encounter without combatants");

        IsActive = true;
        Round = 1;
        CurrentTurn = 0;
        
        // Order combatants by initiative and assign turn order
        var orderedCombatants = _combatants.OrderByDescending(c => c.Initiative).ToList();
        for (int i = 0; i < orderedCombatants.Count; i++)
        {
            orderedCombatants[i].TurnOrder = i;
        }
        
        // Set first combatant as active (highest initiative)
        var firstCombatant = orderedCombatants.FirstOrDefault();
        ActiveCombatantId = firstCombatant?.Id;

        Touch();
        RaiseDomainEvent(new EncounterStartedEvent(Id));
    }

    public void AddCombatant(CombatantType type, Guid? refId, string name, int? initiative = null)
    {
        if (IsCompleted)
            throw new InvalidOperationException("Cannot add combatants to a completed encounter");

        var combatant = new Combatant
        {
            EncounterId = Id,
            Type = type,
            RefId = refId,
            Name = name,
            Initiative = initiative ?? 0,
            CurrentHP = GetMaxHPForCombatant(type, refId),
            MaxHP = GetMaxHPForCombatant(type, refId),
            ConditionsJson = "[]",
            TurnOrder = _combatants.Count // Temporary, will be reordered when encounter starts
        };

        _combatants.Add(combatant);
        Touch();
        RaiseDomainEvent(new CombatantAddedEvent(Id, combatant.Id, name, type));
    }

    public void AddCombatant(string name, int initiative, Guid? characterId = null, Guid? npcMonsterId = null)
    {
        var type = characterId.HasValue ? CombatantType.PC : CombatantType.NPC;
        var refId = characterId ?? npcMonsterId;
        AddCombatant(type, refId, name, initiative);
    }

    public void RemoveCombatant(Guid combatantId)
    {
        var combatant = _combatants.FirstOrDefault(c => c.Id == combatantId);
        if (combatant == null)
            return;

        // If removing the active combatant, move to next
        if (ActiveCombatantId == combatantId)
        {
            NextTurn();
        }

        _combatants.Remove(combatant);
        Touch();
        RaiseDomainEvent(new CombatantRemovedEvent(Id, combatantId));
    }


    public void NextTurn()
    {
        if (!IsActive)
            throw new InvalidOperationException("Encounter is not active");

        var orderedCombatants = _combatants
            .Where(c => c.CurrentHP > 0) // Only alive combatants
            .OrderBy(c => c.TurnOrder)
            .ToList();

        if (!orderedCombatants.Any())
        {
            EndEncounter();
            return;
        }

        var currentTurn = CurrentTurn;
        
        // Advance to next turn
        if (currentTurn < orderedCombatants.Count - 1)
        {
            CurrentTurn++;
        }
        else
        {
            // Start new round
            Round++;
            CurrentTurn = 0;
            RaiseDomainEvent(new EncounterRoundAdvancedEvent(Id, Round));
        }
        
        // Set active combatant based on turn order
        if (orderedCombatants.Count > CurrentTurn)
        {
            ActiveCombatantId = orderedCombatants[CurrentTurn].Id;
        }

        Touch();
        RaiseDomainEvent(new EncounterTurnChangedEvent(Id, ActiveCombatantId));
    }

    public void EndEncounter()
    {
        IsActive = false;
        IsCompleted = true;
        ActiveCombatantId = null;
        Touch();
        RaiseDomainEvent(new EncounterEndedEvent(Id));
    }

    public void SetCombatantInitiative(Guid combatantId, int initiative)
    {
        var combatant = _combatants.FirstOrDefault(c => c.Id == combatantId);
        if (combatant == null)
            throw new ArgumentException("Combatant not found");

        combatant.Initiative = initiative;
        Touch();
    }

    public void DamageCombatant(Guid combatantId, int damage)
    {
        var combatant = _combatants.FirstOrDefault(c => c.Id == combatantId);
        if (combatant == null)
            throw new ArgumentException("Combatant not found");

        combatant.TakeDamage(damage);
        Touch();
        RaiseDomainEvent(new CombatantDamagedEvent(Id, combatantId, damage, combatant.CurrentHP));
    }

    public void HealCombatant(Guid combatantId, int healing)
    {
        var combatant = _combatants.FirstOrDefault(c => c.Id == combatantId);
        if (combatant == null)
            throw new ArgumentException("Combatant not found");

        combatant.Heal(healing);
        Touch();
        RaiseDomainEvent(new CombatantHealedEvent(Id, combatantId, healing, combatant.CurrentHP));
    }

    private static int GetMaxHPForCombatant(CombatantType type, Guid? refId)
    {
        // Placeholder logic - in real implementation, this would lookup from character/monster data
        return type switch
        {
            CombatantType.PC => 20, // Would lookup from Character entity
            CombatantType.NPC => 15, // Would lookup from NPC entity
            CombatantType.Monster => 25, // Would lookup from Monster entity
            _ => 10
        };
    }
}

public class Combatant : BaseEntity
{
    public Guid EncounterId { get; set; }
    public CombatantType Type { get; set; }
    public Guid? RefId { get; set; } // References Character, NPC, or Monster ID
    public string Name { get; set; } = string.Empty;
    public int Initiative { get; set; } = 0;
    public int CurrentHP { get; set; }
    public int MaxHP { get; set; }
    public string ConditionsJson { get; set; } = "[]";
    public string NotesJson { get; set; } = "[]";
    public int TurnOrder { get; set; } = 0;
    
    // Convenience properties for API responses
    public Guid? CharacterId => Type == CombatantType.PC ? RefId : null;
    public Guid? NpcMonsterId => Type == CombatantType.NPC || Type == CombatantType.Monster ? RefId : null;

    public Encounter Encounter { get; set; } = null!;

    public void TakeDamage(int damage)
    {
        CurrentHP = Math.Max(0, CurrentHP - damage);
        Touch();
    }

    public void Heal(int healing)
    {
        CurrentHP = Math.Min(MaxHP, CurrentHP + healing);
        Touch();
    }

    public void SetConditions(string conditionsJson)
    {
        ConditionsJson = conditionsJson;
        Touch();
    }
}

// Domain Events
public sealed record EncounterCreatedEvent(Guid EncounterId, Guid SessionId, string Name) : DomainEvent;
public sealed record EncounterStartedEvent(Guid EncounterId) : DomainEvent;
public sealed record EncounterEndedEvent(Guid EncounterId) : DomainEvent;
public sealed record EncounterRoundAdvancedEvent(Guid EncounterId, int Round) : DomainEvent;
public sealed record EncounterTurnChangedEvent(Guid EncounterId, Guid? ActiveCombatantId) : DomainEvent;
public sealed record CombatantAddedEvent(Guid EncounterId, Guid CombatantId, string Name, CombatantType Type) : DomainEvent;
public sealed record CombatantRemovedEvent(Guid EncounterId, Guid CombatantId) : DomainEvent;
public sealed record CombatantDamagedEvent(Guid EncounterId, Guid CombatantId, int Damage, int CurrentHP) : DomainEvent;
public sealed record CombatantHealedEvent(Guid EncounterId, Guid CombatantId, int Healing, int CurrentHP) : DomainEvent;