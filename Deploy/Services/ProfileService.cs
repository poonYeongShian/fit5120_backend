using Deploy.DTOs;
using Deploy.Helpers;
using Deploy.Interfaces;

namespace Deploy.Services;

public class ProfileService : IProfileService
{
    private readonly IProfileRepository _repository;

    public ProfileService(IProfileRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateProfileResponseDto> CreateProfileAsync(CreateProfileRequestDto request)
    {
        // Step 1: Generate a unique profile code and insert the profile row
        var profileCode = ProfileHelpers.GenerateProfileCode(request.AnimalId);
        var profile = await _repository.CreateProfileAsync(
            profileCode,
            request.Pin,
            request.DisplayName,
            request.AnimalId);

        // Step 2: Initialise the profile's progress row (level 1, 0 points)
        await _repository.CreateProfileProgressAsync(profile.Id);

        // Step 3: Create a session token for the requesting device
        var sessionToken = ProfileHelpers.GenerateSessionToken();
        await _repository.CreateProfileSessionAsync(profile.Id, sessionToken, request.DeviceInfo);

        return new CreateProfileResponseDto
        {
            ProfileId    = profile.Id,
            ProfileCode  = profile.ProfileCode,
            Pin          = profile.Pin,
            DisplayName  = profile.DisplayName,
            SessionToken = sessionToken
        };
    }

    public async Task<ProfileAutoLoginDto?> AutoLoginAsync(string sessionToken)
    {
        // Step 1: Validate session token and fetch joined profile + progress + level data
        var row = await _repository.GetProfileBySessionTokenAsync(sessionToken);

        if (row is null)
            return null;

        // Step 2: Refresh last_used_at on the session row
        await _repository.TouchSessionLastUsedAsync(sessionToken);

        return new ProfileAutoLoginDto
        {
            ProfileId    = row.ProfileId,
            DisplayName  = row.DisplayName,
            AnimalId     = row.AnimalId,
            ProfileCode  = row.ProfileCode,
            CurrentLevel = row.CurrentLevel,
            TotalPoints  = row.TotalPoints,
            TotalQuizzes = row.TotalQuizzes,
            StreakDays   = row.StreakDays,
            LevelName    = row.LevelName,
            BadgeEmoji   = row.BadgeEmoji
        };
    }

    public async Task<bool> LogoutAsync(string sessionToken)
    {
        // Invalidate the session token — returns false if token was not found or already inactive
        return await _repository.InvalidateSessionAsync(sessionToken);
    }

    public async Task<RestoreProfileResponseDto?> RestoreProfileAsync(RestoreProfileRequestDto request)
    {
        // Step 1: Validate profile_code + PIN and load profile + progress + level
        var row = await _repository.GetProfileByCodeAndPinAsync(request.ProfileCode, request.Pin);

        if (row is null)
            return null;

        // Step 2: Issue a brand-new session token for this device
        var sessionToken = ProfileHelpers.GenerateSessionToken();
        await _repository.CreateProfileSessionAsync(row.ProfileId, sessionToken, request.DeviceInfo);

        return new RestoreProfileResponseDto
        {
            ProfileId    = row.ProfileId,
            DisplayName  = row.DisplayName,
            AnimalId     = row.AnimalId,
            ProfileCode  = row.ProfileCode,
            CurrentLevel = row.CurrentLevel,
            TotalPoints  = row.TotalPoints,
            StreakDays   = row.StreakDays,
            LevelName    = row.LevelName,
            BadgeEmoji   = row.BadgeEmoji,
            SessionToken = sessionToken
        };
    }

    public async Task<SaveProgressResponseDto?> SaveProgressAsync(SaveProgressRequestDto request)
    {
        // Capture current level before any changes so we can detect a level-up later
        var levelBefore = await _repository.GetCurrentLevelAsync(request.ProfileId);

        if (levelBefore is null)
            return null; // profile does not exist

        // Step 1: Record the quiz attempt in history
        var historyId = await _repository.InsertQuizHistoryAsync(
            request.ProfileId,
            request.Score,
            request.TotalQuestions,
            request.CorrectAnswers,
            request.PointsEarned,
            levelBefore.Value);

        // Step 2: Add points, increment counters
        await _repository.AddPointsAsync(request.ProfileId, request.PointsEarned, request.CorrectAnswers);

        // Step 3: Resolve and apply the new level
        var progress = await _repository.ApplyLevelUpAsync(request.ProfileId);

        // Step 4: Unlock any fun facts the profile has now earned
        var newFactsUnlocked = await _repository.UnlockNewFactsAsync(request.ProfileId);

        // Step 5: Stamp level_after + leveled_up on the history row
        await _repository.FinaliseHistoryAsync(request.ProfileId);

        return new SaveProgressResponseDto
        {
            HistoryId        = historyId,
            TotalPoints      = progress.TotalPoints,
            CurrentLevel     = progress.CurrentLevel,
            LevelName        = progress.LevelName,
            BadgeEmoji       = progress.BadgeEmoji,
            LeveledUp        = progress.CurrentLevel > levelBefore.Value,
            LevelBefore      = levelBefore.Value,
            LevelAfter       = progress.CurrentLevel,
            NewFactsUnlocked = newFactsUnlocked
        };
    }
}
