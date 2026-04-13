namespace Deploy.Models;

/// <summary>
/// Maps to the <c>public.mission</c> table.
/// A mission card with three ordered steps and an optional time limit,
/// filtered by weather / time-of-day conditions.
/// </summary>
public class Mission
{
    public int Id { get; set; }

    /// <summary>Foreign key ? <c>public.mission_type(mission_type_id)</c>.</summary>
    public int MissionTypeId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Step1 { get; set; } = string.Empty;
    public string Step2 { get; set; } = string.Empty;
    public string Step3 { get; set; } = string.Empty;

    /// <summary>Time limit in minutes. Constrained between 5 and 15 at the DB level.</summary>
    public int TimeLimitMin { get; set; } = 10;

    public bool IsOutdoor { get; set; } = false;

    /// <summary>When <c>true</c> the mission may only be undertaken during daylight hours.</summary>
    public bool IsDayOnly { get; set; } = false;

    public decimal? MinTemperature { get; set; }
    public decimal? MaxTemperature { get; set; }

    public int PointsReward { get; set; } = 20;
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
