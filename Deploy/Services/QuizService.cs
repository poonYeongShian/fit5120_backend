using Deploy.DTOs;
using Deploy.Interfaces;
using Deploy.Mappers;
using Npgsql;

namespace Deploy.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _repository;
    private readonly IProfileRepository _profileRepository;
    private readonly NpgsqlConnection _connection;

    public QuizService(IQuizRepository repository, IProfileRepository profileRepository, NpgsqlConnection connection)
    {
        _repository = repository;
        _profileRepository = profileRepository;
        _connection = connection;
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

    public async Task<SaveQuizProgressResponseDto?> SaveQuizProgressAsync(Guid profileId, SaveQuizProgressRequestDto request)
    {
        var score = (int)Math.Round((double)request.CorrectAnswers / request.TotalQuestions * 100);
        var pointsEarned = request.CorrectAnswers * 5;

        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();
        using var transaction = await connection.BeginTransactionAsync();

        // Step 1: Capture level before update
        var levelBefore = await _profileRepository.GetCurrentLevelAsync(profileId, connection, transaction);

        // Step 2: Insert quiz history
        var historyId = await _profileRepository.InsertQuizHistoryAsync(
            profileId,
            score, request.TotalQuestions, request.CorrectAnswers,
            pointsEarned, levelBefore,
            connection, transaction);

        // Step 3: Add points and increment quiz counters
        await _profileRepository.AddPointsAndIncrementQuizzesAsync(
            profileId, pointsEarned, request.CorrectAnswers,
            connection, transaction);

        // Step 4: Level-up check
        await _profileRepository.UpdateLevelAsync(profileId, connection, transaction);

        // Step 5: Unlock new fun facts based on new level
        var newFactsUnlocked = await _profileRepository.UnlockNewFunFactsAsync(profileId, connection, transaction);

        // Step 6: Update history with level_after and leveled_up flag
        await _profileRepository.UpdateQuizHistoryLevelAfterAsync(historyId, profileId, connection, transaction);

        // Step 7: Award level badge if leveled up (source = 'quiz')
        await _profileRepository.AwardQuizLevelBadgeAsync(profileId, connection, transaction);

        // Read back updated progress
        var progress = await _profileRepository.GetProgressAsync(profileId, connection, transaction);

        // Read back any newly awarded badges from this transaction
        var newBadges = await _profileRepository.GetRecentBadgesAsync(profileId, connection, transaction);

        await transaction.CommitAsync();

        return new SaveQuizProgressResponseDto
        {
            TotalPoints = progress.TotalPoints,
            NewLevel = progress.CurrentLevel,
            LeveledUp = progress.CurrentLevel > levelBefore,
            NewFactsUnlocked = newFactsUnlocked,
            NewBadges = newBadges
        };
    }
}
