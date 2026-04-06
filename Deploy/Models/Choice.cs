namespace Deploy.Models;

public class Choice
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public char ChoiceLabel { get; set; }
    public string ChoiceText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; } = false;

    // Navigation property
    public Question? Question { get; set; }
}
