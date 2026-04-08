namespace Deploy.Models;

/// <summary>
/// Maps to the <c>public.weather_conditions</c> table.
/// Reference data that maps WMO weather code ranges to named conditions
/// and indicates whether conditions are safe for outdoor missions.
/// </summary>
public class WeatherCondition
{
    public int Id { get; set; }

    /// <summary>Lower bound of the WMO weather code range (inclusive).</summary>
    public int WmoCodeMin { get; set; }

    /// <summary>Upper bound of the WMO weather code range (inclusive).</summary>
    public int WmoCodeMax { get; set; }

    public string ConditionName { get; set; } = string.Empty;

    /// <summary>Whether this weather condition is considered safe for outdoor missions.</summary>
    public bool IsOutdoorSafe { get; set; } = true;

    public string? Description { get; set; }
}
