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
    Aberration = 1,
    Animal = 2,
    Construct = 3,
    Dragon = 4,
    Elemental = 5,
    Fey = 6,
    Fiend = 7,
    Humanoid = 8,
    Plant = 9,
    Undead = 10,
    Beast = 11,
    Celestial = 12,
    Monitor = 13,
    Fungus = 14,
    Giant = 15,
    Spirit = 16,
    Time = 17,
    Astral = 18,
    Dream = 19,
    Ethereal = 20,
    Petitioner = 21,
    Hazard = 22
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

// Removed ProficiencyLevel enum - using ProficiencyRank from PathfinderCampaignManager.Domain.Entities.Pathfinder instead

public enum NoteVisibility
{
    Private = 1,    // Only visible to the author
    Shared = 2,     // Visible to character owner, DM, and author
    DMOnly = 3      // Visible only to DMs and author (if author is DM)
}

