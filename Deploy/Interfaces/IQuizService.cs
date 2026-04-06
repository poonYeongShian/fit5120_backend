using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface IQuizService
{
    Task<IEnumerable<QuizDto>> GetAllQuizzesAsync();
    Task<IEnumerable<QuizQuestionDto>?> GetQuestionsByQuizIdAsync(int quizId);
    Task<IEnumerable<QuizQuestionDto>> GetRandomQuestionsAsync(int count);
    Task<IEnumerable<QuizQuestionDto>> GetRandomQuestionsByAnimalIdAsync(int animalId, int count);
}
