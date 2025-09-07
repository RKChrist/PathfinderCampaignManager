namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

public class PfSkill
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Ability { get; set; } = string.Empty; // STR, DEX, CON, INT, WIS, CHA
    public List<string> Actions { get; set; } = new();
    public bool ArmorCheckPenalty { get; set; }
    public List<PfSkillAction> SkillActions { get; set; } = new();
}

public class PfSkillAction
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "Free", "1", "2", "3", "Reaction", etc.
    public string Requirements { get; set; } = string.Empty;
    public List<string> Traits { get; set; } = new();
    public int DC { get; set; } = 0; // 0 means variable DC
    public string CriticalSuccess { get; set; } = string.Empty;
    public string Success { get; set; } = string.Empty;
    public string Failure { get; set; } = string.Empty;
    public string CriticalFailure { get; set; } = string.Empty;
}