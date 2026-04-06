using Deploy.DTOs;
using Deploy.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Deploy.Endpoints;

public static class QuizEndpoints
{
    public static void MapQuizEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/quizzes");

        group.MapGet("/", GetAllQuizzes)
            .WithName("GetAllQuizzes")
            .WithDescription("Returns a list of all available quizzes.")
            .Produces<IEnumerable<QuizDto>>(StatusCodes.Status200OK)
            .WithOpenApi(operation =>
            {
                operation.Responses["200"].Description = "A list of all quizzes.";
                return operation;
            });

        group.MapGet("/questions/random", GetRandomQuestions)
            .WithName("GetRandomQuestions")
            .WithDescription("Returns a specified number of random, non-repeated questions with their choices from all quizzes.")
            .Produces<IEnumerable<QuizQuestionDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                var countParam = operation.Parameters.FirstOrDefault(p => p.Name == "count");
                if (countParam is not null)
                {
                    countParam.Description = "Number of random questions to return. Must be at least 1.";
                    countParam.Required = true;
                }

                operation.Responses["200"].Description = "A list of random questions with choices.";
                operation.Responses["400"].Description = "Invalid count. Error code: INVALID_COUNT.";
                return operation;
            });

        group.MapGet("/animals/{animalId:int}/questions", GetRandomQuestionsByAnimalId)
            .WithName("GetRandomQuestionsByAnimalId")
            .WithDescription("Returns a specified number of random, non-repeated questions for a specific animal.")
            .Produces<IEnumerable<QuizQuestionDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status400BadRequest)
            .WithOpenApi(operation =>
            {
                var countParam = operation.Parameters.FirstOrDefault(p => p.Name == "count");
                if (countParam is not null)
                {
                    countParam.Description = "Number of random questions to return. Must be at least 1.";
                    countParam.Required = true;
                }

                operation.Responses["200"].Description = "A list of random questions with choices for the given animal.";
                operation.Responses["400"].Description = "Invalid count. Error code: INVALID_COUNT.";
                return operation;
            });

        group.MapGet("/{quizId:int}/questions", GetQuestionsByQuizId)
            .WithName("GetQuizQuestions")
            .WithDescription("Returns all questions with their A/B/C/D choices and hint for a specific quiz.")
            .Produces<IEnumerable<QuizQuestionDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponseDto>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.Responses["200"].Description = "A list of questions with choices for the given quiz.";
                operation.Responses["404"].Description = "Quiz not found. Error code: QUIZ_NOT_FOUND.";
                return operation;
            });
    }

    private static async Task<Ok<IEnumerable<QuizDto>>> GetAllQuizzes(IQuizService service)
    {
        var quizzes = await service.GetAllQuizzesAsync();
        return TypedResults.Ok(quizzes);
    }

    private static async Task<Results<Ok<IEnumerable<QuizQuestionDto>>, BadRequest<ErrorResponseDto>>> GetRandomQuestions(
        int count, IQuizService service)
    {
        if (count < 1)
            return TypedResults.BadRequest(new ErrorResponseDto { ErrorCode = "INVALID_COUNT" });

        var questions = await service.GetRandomQuestionsAsync(count);
        return TypedResults.Ok(questions);
    }

    private static async Task<Results<Ok<IEnumerable<QuizQuestionDto>>, BadRequest<ErrorResponseDto>>> GetRandomQuestionsByAnimalId(
        int animalId, int count, IQuizService service)
    {
        if (count < 1)
            return TypedResults.BadRequest(new ErrorResponseDto { ErrorCode = "INVALID_COUNT" });

        var questions = await service.GetRandomQuestionsByAnimalIdAsync(animalId, count);
        return TypedResults.Ok(questions);
    }

    private static async Task<Results<Ok<IEnumerable<QuizQuestionDto>>, NotFound<ErrorResponseDto>>> GetQuestionsByQuizId(
        int quizId, IQuizService service)
    {
        var questions = await service.GetQuestionsByQuizIdAsync(quizId);

        if (questions is null)
            return TypedResults.NotFound(new ErrorResponseDto { ErrorCode = "QUIZ_NOT_FOUND" });

        return TypedResults.Ok(questions);
    }
}
