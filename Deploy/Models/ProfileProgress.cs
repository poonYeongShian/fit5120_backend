namespace Deploy.Models;

public class ProfileProgress
{
    public int Id { get; set; }
    public Guid ProfileId { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public int TotalPoints { get; set; } = 0;
    public int TotalQuizzes { get; set; } = 0;
    public int TotalCorrect { get; set; } = 0;
    public int TotalMissions { get; set; } = 0;
    public int StreakDays { get; set; } = 0;
    public DateTimeOffset? LastActiveAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
