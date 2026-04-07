namespace Deploy.DTOs;

public class SaveProgressRequestDto
{
    public Guid ProfileId { get; set; }
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int PointsEarned { get; set; }
}
