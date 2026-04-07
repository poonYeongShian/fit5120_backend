namespace Deploy.Models;

public class AnimalFunFact
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public string? Emoji { get; set; }
    public string FactText { get; set; } = string.Empty;
    public string? FactImageUrl { get; set; }
    public int FactOrder { get; set; } = 1;
    public int UnlockLevel { get; set; } = 1;
    public bool IsLocked { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; }
}
