using Deploy.DTOs;
using Deploy.Models;

namespace Deploy.Mappers;

public static class QuizMapper
{
    public static QuizDto ToQuizDto(Quiz quiz)
    {
        return new QuizDto
        {
            Id = quiz.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            Topic = quiz.Topic
        };
    }

    public static IEnumerable<QuizDto> ToQuizDtoList(IEnumerable<Quiz> quizzes)
    {
        return quizzes.Select(ToQuizDto);
    }

    public static IEnumerable<QuizQuestionDto> ToQuizQuestionDtoList(
        IEnumerable<(Question Question, Choice Choice)> rows)
    {
        return rows
            .GroupBy(r => r.Question.Id)
            .Select(g =>
            {
                var question = g.First().Question;
                return new QuizQuestionDto
                {
                    QuestionId = question.Id,
                    AnimalId = question.AnimalId,
                    QuestionText = question.QuestionText,
                    Explanation = question.Explanation,
                    Difficulty = question.Difficulty.ToString().ToLowerInvariant(),
                    Choices = g.Select(r => new QuizChoiceDto
                    {
                        ChoiceLabel = r.Choice.ChoiceLabel,
                        ChoiceText = r.Choice.ChoiceText,
                        IsCorrect = r.Choice.IsCorrect
                    })
                };
            });
    }
}
