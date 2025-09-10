namespace PathfinderCampaignManager.Presentation.Shared.Models;

public class CharacterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; }
    public string Ancestry { get; set; } = string.Empty;
    public string Heritage { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public Dictionary<string, int> AbilityScores { get; set; } = new();
    public Dictionary<string, string> Skills { get; set; } = new();
    public List<string> Feats { get; set; } = new();
    public List<string> Equipment { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateCharacterRequest
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public string Ancestry { get; set; } = string.Empty;
    public string Heritage { get; set; } = string.Empty;
    public string Background { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public Dictionary<string, int>? AbilityScores { get; set; }
    public Dictionary<string, string>? Skills { get; set; }
    public List<string>? Feats { get; set; }
    public List<string>? Equipment { get; set; }
    public Guid? CampaignId { get; set; }
}

public class UpdateCharacterRequest
{
    public string Name { get; set; } = string.Empty;
    public int Level { get; set; } = 1;
    public Dictionary<string, int>? AbilityScores { get; set; }
    public Dictionary<string, string>? Skills { get; set; }
    public List<string>? Feats { get; set; }
    public List<string>? Equipment { get; set; }
}