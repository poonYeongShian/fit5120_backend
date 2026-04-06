using Deploy.Enums;

namespace Deploy.Models;

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public int? AnimalId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;
    public string? ImageUrl { get; set; }

    public Quiz? Quiz { get; set; }
    public ICollection<Choice> Choices { get; set; } = [];
}
