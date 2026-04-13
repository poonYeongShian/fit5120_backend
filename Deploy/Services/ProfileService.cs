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
        var profileCode = ProfileHelpers.GenerateProfileCode();
        var profile = await _repository.CreateProfileAsync(
            profileCode,
            request.Pin,
            request.DisplayName);

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
            ProfileCode  = row.ProfileCode,
            CurrentLevel = row.CurrentLevel,
            TotalPoints  = row.TotalPoints,
            TotalQuizzes = row.TotalQuizzes,
            TotalMissions = row.TotalMissions,
            StreakDays   = row.StreakDays
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
            ProfileCode  = row.ProfileCode,
            CurrentLevel = row.CurrentLevel,
            TotalPoints  = row.TotalPoints,
            StreakDays   = row.StreakDays,
            SessionToken = sessionToken
        };
    }
}
