namespace Deploy.DTOs;

public class QuizChoiceDto
{
    public char ChoiceLabel { get; set; }
    public string ChoiceText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class QuizQuestionDto
{
    public int QuestionId { get; set; }
    public int? AnimalId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public IEnumerable<QuizChoiceDto> Choices { get; set; } = [];
}
