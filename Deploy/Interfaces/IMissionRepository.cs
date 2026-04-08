using Deploy.DTOs;

namespace Deploy.Interfaces;

public interface IMissionRepository
{
    Task<WeatherMissionDto?> GetWeatherAdaptiveMissionAsync(int weatherCode, bool isDay);
    Task<int?> AssignMissionAsync(Guid profileId, int missionId, int? weatherCode, bool? isDay, decimal? weatherTemp, decimal? locationLat, decimal? locationLon);
    Task<bool> StartMissionAsync(int profileMissionId);
    Task<CompleteMissionResponseDto?> CompleteMissionAsync(int profileMissionId);
}
