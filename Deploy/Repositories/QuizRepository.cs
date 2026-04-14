using Npgsql;
using Dapper;
using Deploy.Models;
using Deploy.Interfaces;

namespace Deploy.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly NpgsqlConnection _connection;

    public QuizRepository(NpgsqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<IEnumerable<Quiz>> GetAllQuizzesAsync()
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.QueryAsync<Quiz>(
            """
            SELECT quiz_id     AS Id,
                   title       AS Title,
                   description AS Description,
                   topic       AS Topic
            FROM quiz
            ORDER BY quiz_id
            """);
    }

    public async Task<bool> QuizExistsAsync(int quizId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        return await connection.ExecuteScalarAsync<bool>(
            "SELECT EXISTS(SELECT 1 FROM quiz WHERE quiz_id = @QuizId)",
            new { QuizId = quizId });
    }

    public async Task<IEnumerable<(Question Question, Choice Choice)>> GetQuestionsWithChoicesByQuizIdAsync(int quizId)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        const string sql = """
            SELECT 
                q.question_id          AS Id,
                q.animal_id            AS AnimalId,
                q.question_text        AS QuestionText,
                q.explanation          AS Explanation,
                q.difficulty::text     AS Difficulty,
                q.image_url            AS ImageUrl,
                c.choice_label         AS ChoiceLabel,
                c.choice_text          AS ChoiceText,
                c.is_correct           AS IsCorrect
            FROM question q
            JOIN choice c ON q.question_id = c.question_id
            WHERE q.quiz_id = @QuizId
            ORDER BY q.question_id, c.choice_label
            """;

        var rows = await connection.QueryAsync<Question, Choice, (Question, Choice)>(
            sql,
            (question, choice) => (question, choice),
            new { QuizId = quizId },
            splitOn: "ChoiceLabel");

        return rows;
    }

    public async Task<IEnumerable<(Question Question, Choice Choice)>> GetRandomQuestionsWithChoicesAsync(int count)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        const string sql = """
            WITH random_questions AS (
                SELECT question_id
                FROM question
                ORDER BY RANDOM()
                LIMIT @Count
            )
            SELECT 
                q.question_id          AS Id,
                q.animal_id            AS AnimalId,
                q.question_text        AS QuestionText,
                q.explanation          AS Explanation,
                q.difficulty::text     AS Difficulty,
                q.image_url            AS ImageUrl,
                c.choice_label         AS ChoiceLabel,
                c.choice_text          AS ChoiceText,
                c.is_correct           AS IsCorrect
            FROM question q
            JOIN random_questions rq ON q.question_id = rq.question_id
            JOIN choice c ON q.question_id = c.question_id
            ORDER BY q.question_id, c.choice_label
            """;

        var rows = await connection.QueryAsync<Question, Choice, (Question, Choice)>(
            sql,
            (question, choice) => (question, choice),
            new { Count = count },
            splitOn: "ChoiceLabel");

        return rows;
    }

    public async Task<IEnumerable<(Question Question, Choice Choice)>> GetRandomQuestionsWithChoicesByAnimalIdAsync(int animalId, int count)
    {
        using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        const string sql = """
            WITH random_questions AS (
                SELECT question_id
                FROM question
                WHERE animal_id = @AnimalId
                ORDER BY RANDOM()
                LIMIT @Count
            )
            SELECT 
                q.question_id          AS Id,
                q.animal_id            AS AnimalId,
                q.question_text        AS QuestionText,
                q.explanation          AS Explanation,
                q.difficulty::text     AS Difficulty,
                q.image_url            AS ImageUrl,
                c.choice_label         AS ChoiceLabel,
                c.choice_text          AS ChoiceText,
                c.is_correct           AS IsCorrect
            FROM question q
            JOIN random_questions rq ON q.question_id = rq.question_id
            JOIN choice c ON q.question_id = c.question_id
            ORDER BY q.question_id, c.choice_label
            """;

        var rows = await connection.QueryAsync<Question, Choice, (Question, Choice)>(
            sql,
            (question, choice) => (question, choice),
            new { AnimalId = animalId, Count = count },
            splitOn: "ChoiceLabel");

        return rows;
    }
}
