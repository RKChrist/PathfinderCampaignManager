namespace PathfinderCampaignManager.Domain.Enums;

public enum UserRole
{
    Player = 1,
    DM = 2,
    Admin = 3
}

public enum SessionMemberRole
{
    Player = 1,
    DM = 2
}

public enum CombatantType
{
    PC = 1,
    NPC = 2,
    Monster = 3
}

public enum RulesVersionStatus
{
    Draft = 1,
    Published = 2,
    Archived = 3
}

public enum CharacterVisibility
{
    Private = 1,
    SessionOnly = 2,
    Public = 3
}

public enum NpcMonsterType
{
    NPC = 1,
    Monster = 2,
    Familiar = 3,
    AnimalCompanion = 4,
    Hazard = 5
}

public enum VariantRuleType
{
    AncestryParagon = 1,
    AutomaticBonusProgression = 2,
    DualClass = 3,
    FreeArchetype = 4,
    GradualAbilityBoosts = 5,
    ProficiencyWithoutLevel = 6,
    VoluntaryFlaws = 7,
    IgnoreBulkLimit = 8
}

public enum ProficiencyLevel
{
    Untrained = 0,
    Trained = 2,
    Expert = 4,
    Master = 6,
    Legendary = 8
}

public enum NoteVisibility
{
    Private = 1,    // Only visible to the author
    Shared = 2,     // Visible to character owner, DM, and author
    DMOnly = 3      // Visible only to DMs and author (if author is DM)
}

