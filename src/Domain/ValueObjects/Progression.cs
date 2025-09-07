using PathfinderCampaignManager.Domain.Enums;

namespace PathfinderCampaignManager.Domain.ValueObjects;

public enum Proficiency
{
    Untrained = 0,
    Trained = 2,
    Expert = 4,
    Master = 6,
    Legendary = 8
}

public interface IProgressionTarget
{
    string Id { get; }
}

public sealed record ProgressionStep(int Level, Proficiency Proficiency);

public sealed record Progression<T>(string Id, IReadOnlyList<ProgressionStep> Steps)
    where T : IProgressionTarget
{
    public Proficiency GetProficiencyAtLevel(int level)
    {
        if (level < 1) return Proficiency.Untrained;
        
        var applicableStep = Steps
            .Where(s => s.Level <= level)
            .OrderByDescending(s => s.Level)
            .FirstOrDefault();
            
        return applicableStep?.Proficiency ?? Proficiency.Untrained;
    }
}

public sealed record SaveProgression : IProgressionTarget
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty; // "Fortitude", "Reflex", "Will"
}

public sealed record SkillProgression : IProgressionTarget
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string KeyAbility { get; init; } = string.Empty;
}

public sealed record WeaponProgression : IProgressionTarget
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty; // "Simple", "Martial", "Advanced"
}

public sealed record ArmorProgression : IProgressionTarget
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty; // "Unarmored", "Light", "Medium", "Heavy"
}

public sealed record PerceptionProgression : IProgressionTarget
{
    public string Id { get; init; } = "perception";
    public string Name { get; init; } = "Perception";
}

public sealed record SpellcastingProgression : IProgressionTarget
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty; // "Attack", "DC", "ClassDC"
}