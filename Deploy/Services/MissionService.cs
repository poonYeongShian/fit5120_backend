using Deploy.DTOs;
using Deploy.Interfaces;

namespace Deploy.Services;

public class MissionService : IMissionService
{
    private readonly IMissionRepository _repository;

    public MissionService(IMissionRepository repository)
    {
        _repository = repository;
    }

    public async Task<WeatherMissionDto?> GetWeatherAdaptiveMissionAsync(int weatherCode, bool isDay)
    {
        return await _repository.GetWeatherAdaptiveMissionAsync(weatherCode, isDay);
    }

    public async Task<int?> AssignMissionAsync(
        Guid profileId, int missionId, int? weatherCode, bool? isDay,
        decimal? weatherTemp, decimal? locationLat, decimal? locationLon)
    {
        return await _repository.AssignMissionAsync(profileId, missionId, weatherCode, isDay, weatherTemp, locationLat, locationLon);
    }

    public async Task<bool> StartMissionAsync(int profileMissionId)
    {
        return await _repository.StartMissionAsync(profileMissionId);
    }

    public async Task<CompleteMissionResponseDto?> CompleteMissionAsync(int profileMissionId)
    {
        return await _repository.CompleteMissionAsync(profileMissionId);
    }
}
