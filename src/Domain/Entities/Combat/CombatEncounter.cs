namespace PathfinderCampaignManager.Domain.Entities.Combat;

// CombatSession is a simplified version of CombatEncounter for campaign-specific combat
public class CombatSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPaused { get; set; } = false;
    public int Round { get; set; } = 0;
    public int CurrentTurn { get; set; } = 0;
    public List<CombatParticipant> Participants { get; set; } = new();
}

public class CombatEncounter
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsPaused { get; set; }
    public int CurrentRound { get; set; } = 1;
    public int CurrentTurn { get; set; } = 0; // Index in InitiativeOrder
    
    public List<CombatParticipant> Participants { get; set; } = new();
    public List<CombatParticipant> InitiativeOrder => 
        Participants.OrderByDescending(p => p.Initiative)
                   .ThenByDescending(p => p.InitiativeModifier)
                   .ToList();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CombatParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public CombatParticipantType Type { get; set; }
    public bool IsPlayerCharacter { get; set; }
    
    // Core Stats
    public int Initiative { get; set; }
    public int InitiativeModifier { get; set; }
    public int ArmorClass { get; set; }
    public int HitPoints { get; set; }
    public int CurrentHitPoints { get; set; }
    public int TemporaryHitPoints { get; set; }
    
    // Passive Scores
    public int PassivePerception { get; set; }
    public int Perception { get; set; }
    
    // Saving Throws
    public int FortitudeSave { get; set; }
    public int ReflexSave { get; set; }
    public int WillSave { get; set; }
    public int Fortitude { get; set; }
    public int Reflex { get; set; }
    public int Will { get; set; }
    
    // Combat State
    public bool IsDefeated => CurrentHitPoints <= 0;
    public bool IsDying => CurrentHitPoints < 0 && !IsPlayerCharacter;
    public int DyingValue { get; set; } = 0; // PF2e dying condition
    public int WoundedValue { get; set; } = 0; // PF2e wounded condition
    public List<string> Conditions { get; set; } = new();
    
    // Additional Info
    public string Notes { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    
    // For NPCs/Monsters
    public Guid? CharacterId { get; set; } // Link to player character if applicable
    public Guid? PlayerId { get; set; } // Link to the player who owns this character
    public string CreatureType { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public string? Class { get; set; }
    public string? Ancestry { get; set; }
}

public enum CombatParticipantType
{
    PlayerCharacter,
    NonPlayerCharacter,
    Monster,
    Hazard
}