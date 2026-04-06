using Deploy.DTOs;
using Deploy.Interfaces;
using Deploy.Mappers;

namespace Deploy.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _repository;

    public QuizService(IQuizRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<QuizDto>> GetAllQuizzesAsync()
    {
        var quizzes = await _repository.GetAllQuizzesAsync();
        return QuizMapper.ToQuizDtoList(quizzes);
    }

    public async Task<IEnumerable<QuizQuestionDto>?> GetQuestionsByQuizIdAsync(int quizId)
    {
        var exists = await _repository.QuizExistsAsync(quizId);

        if (!exists)
            return null;

        var rows = await _repository.GetQuestionsWithChoicesByQuizIdAsync(quizId);
        return QuizMapper.ToQuizQuestionDtoList(rows);
    }

    public async Task<IEnumerable<QuizQuestionDto>> GetRandomQuestionsAsync(int count)
    {
        var rows = await _repository.GetRandomQuestionsWithChoicesAsync(count);
        return QuizMapper.ToQuizQuestionDtoList(rows);
    }

    public async Task<IEnumerable<QuizQuestionDto>> GetRandomQuestionsByAnimalIdAsync(int animalId, int count)
    {
        var rows = await _repository.GetRandomQuestionsWithChoicesByAnimalIdAsync(animalId, count);
        return QuizMapper.ToQuizQuestionDtoList(rows);
    }
}
