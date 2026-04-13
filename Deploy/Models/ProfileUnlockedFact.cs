namespace Deploy.Models;

public class ProfileUnlockedFact
{
    public int Id { get; set; }
    public Guid ProfileId { get; set; }
    public int AnimalFunFactId { get; set; }
    public DateTimeOffset UnlockedAt { get; set; }
}
