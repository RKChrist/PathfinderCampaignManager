namespace PathfinderCampaignManager.Presentation.Shared.Models;

public class CombatSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<CombatParticipant> Participants { get; set; } = new();
    public bool IsActive { get; set; } = false;
    public bool IsPaused { get; set; } = false;
    public int CurrentTurn { get; set; } = 0;
    public int Round { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CombatParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "PC"; // PC, NPC, Monster
    public int Initiative { get; set; } = 0;
    public int HitPoints { get; set; } = 0;
    public int CurrentHitPoints { get; set; } = 0;
    public int ArmorClass { get; set; } = 10;
    public int Perception { get; set; } = 0;
    public int Fortitude { get; set; } = 0;
    public int Reflex { get; set; } = 0;
    public int Will { get; set; } = 0;
    public List<string> Conditions { get; set; } = new();
    public bool IsPlayerCharacter { get; set; } = true;
    public string? CharacterId { get; set; }
}