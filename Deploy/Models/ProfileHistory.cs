namespace Deploy.Models;

public class ProfileHistory
{
    public int Id { get; set; }
    public Guid ProfileId { get; set; }
    public int Score { get; set; } = 0;
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; } = 0;
    public int PointsEarned { get; set; } = 0;
    public bool LeveledUp { get; set; } = false;
    public int? LevelBefore { get; set; }
    public int? LevelAfter { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
}
