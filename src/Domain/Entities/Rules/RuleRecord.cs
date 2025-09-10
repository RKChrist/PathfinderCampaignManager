namespace PathfinderCampaignManager.Domain.Entities.Rules;

public enum RuleCategory
{
    CoreMechanics,
    Combat,
    Defenses,
    Saves,
    Conditions,
    Senses,
    Environment,
    Spellcasting,
    Exploration,
    Items,
    GMTools,
    Subsystems
}

public enum RuleContentType
{
    Article,
    Table,
    Formula,
    Glossary,
    Example
}

public sealed record RuleId(string Value)
{
    public static implicit operator string(RuleId id) => id.Value;
    public static implicit operator RuleId(string value) => new(value);
    
    public override string ToString() => Value;
}

public sealed record RuleRecord(
    RuleId Id,
    string Name,
    RuleCategory Category,
    RuleContentType ContentType,
    string Summary,
    string? DetailsMarkdown,
    IReadOnlyList<string> Traits,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> References
)
{
    public static RuleRecord Create(
        string id,
        string name,
        RuleCategory category,
        RuleContentType contentType,
        string summary,
        string? detailsMarkdown = null,
        IReadOnlyList<string>? traits = null,
        IReadOnlyList<string>? tags = null,
        IReadOnlyList<string>? references = null)
    {
        return new RuleRecord(
            new RuleId(id),
            name,
            category,
            contentType,
            summary,
            detailsMarkdown,
            traits ?? Array.Empty<string>(),
            tags ?? Array.Empty<string>(),
            references ?? Array.Empty<string>()
        );
    }
}

public sealed record RuleTable(
    RuleId RuleId,
    string Caption,
    IReadOnlyList<string> Columns,
    IReadOnlyList<IReadOnlyList<string>> Rows
)
{
    public static RuleTable Create(
        string ruleId,
        string caption,
        IReadOnlyList<string> columns,
        IReadOnlyList<IReadOnlyList<string>> rows)
    {
        return new RuleTable(new RuleId(ruleId), caption, columns, rows);
    }
}

public sealed record RuleFormula(
    RuleId RuleId,
    string Expression,
    string HumanReadable
)
{
    public static RuleFormula Create(
        string ruleId,
        string expression,
        string humanReadable)
    {
        return new RuleFormula(new RuleId(ruleId), expression, humanReadable);
    }
}

// Extensions for better usability
public static class RuleCategoryExtensions
{
    public static string GetDisplayName(this RuleCategory category)
    {
        return category switch
        {
            RuleCategory.CoreMechanics => "Core Mechanics",
            RuleCategory.Combat => "Combat & Movement",
            RuleCategory.Defenses => "Defenses & Damage",
            RuleCategory.Saves => "Saving Throws",
            RuleCategory.Conditions => "Conditions",
            RuleCategory.Senses => "Senses & Vision",
            RuleCategory.Environment => "Environment & Hazards",
            RuleCategory.Spellcasting => "Spellcasting Rules",
            RuleCategory.Exploration => "Exploration & Downtime",
            RuleCategory.Items => "Items & Equipment Rules",
            _ => category.ToString()
        };
    }
    
    public static string GetDescription(this RuleCategory category)
    {
        return category switch
        {
            RuleCategory.CoreMechanics => "Action Economy, Degrees of Success, Proficiency, DCs & Checks, Initiative, Hero Points, Rest & Recovery",
            RuleCategory.Combat => "Strides/Strikes, Grab/Trip/Shove, Grapple rules, Reactions/Triggers, Cover & Concealment, Flanking, Reach, Movement Types",
            RuleCategory.Defenses => "Damage Types, Resistance/Weakness/Immunity, Persistent Damage, Splash, Precision",
            RuleCategory.Saves => "Basic Saves, Nonbasic Saves, Counteract rules, Ongoing effects, Recovery",
            RuleCategory.Conditions => "All SRD conditions (Blinded, Clumsy, Dazzled, Dying, Flat-Footed, Frightened, etc.)",
            RuleCategory.Senses => "Perception, Seek, Hidden/Undetected/Unnoticed, Scent, Darkvision, Low-Light Vision, Tremorsense",
            RuleCategory.Environment => "Environmental Damage categories, Falling, Suffocation, Extreme Cold/Heat, Weather, Hazard rules",
            RuleCategory.Spellcasting => "Components, Casting/Concentrating, Sustaining, Heightening, Counteract, Line of Effect/Sight, AOE templates",
            RuleCategory.Exploration => "Exploration activities, Crafting basics, Treat Wounds mechanics",
            RuleCategory.Items => "Bulk & Encumbrance, Hardness/HP/BT for objects, Interact, Draw/Stow, Consumables usage",
            _ => string.Empty
        };
    }
    
    public static string GetIconClass(this RuleCategory category)
    {
        return category switch
        {
            RuleCategory.CoreMechanics => "fas fa-cog",
            RuleCategory.Combat => "fas fa-sword",
            RuleCategory.Defenses => "fas fa-shield-alt",
            RuleCategory.Saves => "fas fa-dice-d20",
            RuleCategory.Conditions => "fas fa-exclamation-triangle",
            RuleCategory.Senses => "fas fa-eye",
            RuleCategory.Environment => "fas fa-mountain",
            RuleCategory.Spellcasting => "fas fa-magic",
            RuleCategory.Exploration => "fas fa-compass",
            RuleCategory.Items => "fas fa-box",
            _ => "fas fa-book"
        };
    }
}