using Deploy.Models;

namespace Deploy.Interfaces;

public interface IQuizRepository
{
    Task<bool> QuizExistsAsync(int quizId);
    Task<IEnumerable<(Question Question, Choice Choice)>> GetQuestionsWithChoicesByQuizIdAsync(int quizId);
    Task<IEnumerable<Quiz>> GetAllQuizzesAsync();
    Task<IEnumerable<(Question Question, Choice Choice)>> GetRandomQuestionsWithChoicesAsync(int count);
    Task<IEnumerable<(Question Question, Choice Choice)>> GetRandomQuestionsWithChoicesByAnimalIdAsync(int animalId, int count);
}
