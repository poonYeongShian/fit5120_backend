using Deploy.Enums;

namespace Deploy.Models;

/// <summary>
/// Maps to the <c>public.profile_missions</c> table.
/// Records each mission attempt made by a profile, including the
/// weather snapshot and location captured at assignment time.
/// </summary>
public class ProfileMission
{
    public int Id { get; set; }

    /// <summary>Foreign key ? <c>public.profiles(id)</c>.</summary>
    public Guid ProfileId { get; set; }

    /// <summary>Foreign key ? <c>public.missions(id)</c>.</summary>
    public int MissionId { get; set; }

    // ?? Weather snapshot at assignment time ??????????????????????????????
    /// <summary>WMO weather code at the time the mission was assigned.</summary>
    public int? WeatherCode { get; set; }

    /// <summary>Ambient temperature (°C) at the time the mission was assigned.</summary>
    public decimal? WeatherTemp { get; set; }

    /// <summary>Whether it was daytime when the mission was assigned.</summary>
    public bool? WeatherIsDay { get; set; }

    // ?? Location snapshot at assignment time ?????????????????????????????
    public decimal? LocationLat { get; set; }
    public decimal? LocationLon { get; set; }

    // ?? Lifecycle ????????????????????????????????????????????????????????
    public MissionStatus Status { get; set; } = MissionStatus.Assigned;

    public int PointsEarned { get; set; } = 0;

    public DateTimeOffset AssignedAt { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}
