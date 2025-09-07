namespace PathfinderCampaignManager.Domain.Entities.Pathfinder;

// Unified proficiency system used for skills, weapons, armor, saves, etc.
public class PfProficiency
{
    public string Name { get; set; } = string.Empty;
    public ProficiencyRank Rank { get; set; } = ProficiencyRank.Untrained;
    public ProficiencyType Type { get; set; }
    public string Category { get; set; } = string.Empty; // For weapons: "Simple", "Martial", etc.
    public string? SpecificItem { get; set; } // For specific weapon/armor proficiency
}

public enum ProficiencyRank
{
    Untrained = 0,
    Trained = 2,
    Expert = 4,
    Master = 6,
    Legendary = 8
}

public enum ProficiencyType
{
    Skill,           // Thievery, Nature, etc.
    Weapon,          // Simple weapons, Martial weapons, specific weapons
    Armor,           // Light armor, Medium armor, Heavy armor
    Save,            // Fortitude, Reflex, Will
    Perception,      // Perception
    ClassDC,         // Class DC for class abilities
    SpellAttack,     // Spell attack rolls
    SpellDC          // Spell save DC
}

public static class ProficiencyExtensions
{
    public static int GetBonus(this ProficiencyRank rank)
    {
        return (int)rank;
    }

    public static string GetName(this ProficiencyRank rank)
    {
        return rank switch
        {
            ProficiencyRank.Untrained => "Untrained",
            ProficiencyRank.Trained => "Trained",
            ProficiencyRank.Expert => "Expert",
            ProficiencyRank.Master => "Master",
            ProficiencyRank.Legendary => "Legendary",
            _ => "Unknown"
        };
    }

    public static string GetDescription(this ProficiencyRank rank)
    {
        return rank switch
        {
            ProficiencyRank.Untrained => "No particular training. Cannot attempt some actions.",
            ProficiencyRank.Trained => "Some training through study or practice.",
            ProficiencyRank.Expert => "Quite capable, with significant dedication to mastery.",
            ProficiencyRank.Master => "Achieved mastery, far exceeding most others.",
            ProficiencyRank.Legendary => "Renowned skill that stories might be told of for generations.",
            _ => "Unknown proficiency level."
        };
    }

    public static string GetColorClass(this ProficiencyRank rank)
    {
        return rank switch
        {
            ProficiencyRank.Untrained => "text-secondary",
            ProficiencyRank.Trained => "text-info",
            ProficiencyRank.Expert => "text-primary",
            ProficiencyRank.Master => "text-warning",
            ProficiencyRank.Legendary => "text-success",
            _ => "text-muted"
        };
    }

    public static string GetBadgeClass(this ProficiencyRank rank)
    {
        return rank switch
        {
            ProficiencyRank.Untrained => "badge bg-secondary",
            ProficiencyRank.Trained => "badge bg-info",
            ProficiencyRank.Expert => "badge bg-primary",
            ProficiencyRank.Master => "badge bg-warning text-dark",
            ProficiencyRank.Legendary => "badge bg-success",
            _ => "badge bg-light text-dark"
        };
    }
}

// Character proficiency tracking
public class PfCharacterProficiency
{
    public string Name { get; set; } = string.Empty;
    public ProficiencyType Type { get; set; }
    public ProficiencyRank Rank { get; set; } = ProficiencyRank.Untrained;
    public string Category { get; set; } = string.Empty;
    public string? SpecificItem { get; set; }
    public int Level { get; set; } = 1; // Character level for calculations

    public int GetTotalBonus(int abilityModifier = 0)
    {
        return Level + Rank.GetBonus() + abilityModifier;
    }

    public int GetTotalBonusWithoutAbility()
    {
        return Level + Rank.GetBonus();
    }
}