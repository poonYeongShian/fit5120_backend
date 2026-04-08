using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface IProfileService
{
    Task<CreateProfileResponseDto> CreateProfileAsync(CreateProfileRequestDto request);
    Task<ProfileAutoLoginDto?> AutoLoginAsync(string sessionToken);
    Task<bool> LogoutAsync(string sessionToken);
    Task<RestoreProfileResponseDto?> RestoreProfileAsync(RestoreProfileRequestDto request);
}
