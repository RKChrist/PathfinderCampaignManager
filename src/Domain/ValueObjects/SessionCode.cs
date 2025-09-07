namespace PathfinderCampaignManager.Domain.ValueObjects;

public sealed record SessionCode
{
    public string Value { get; }

    private SessionCode(string value)
    {
        Value = value;
    }

    public static SessionCode Generate()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        return new SessionCode(code);
    }

    public static SessionCode FromString(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 6)
            throw new ArgumentException("Session code must be exactly 6 characters");

        return new SessionCode(value.ToUpper());
    }

    public static implicit operator string(SessionCode sessionCode) => sessionCode.Value;
    public override string ToString() => Value;
}