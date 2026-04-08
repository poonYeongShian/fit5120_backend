namespace Deploy.DTOs;

public class WeatherMissionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Step1 { get; set; } = string.Empty;
    public string Step2 { get; set; } = string.Empty;
    public string Step3 { get; set; } = string.Empty;
    public int TimeLimitMin { get; set; }
    public bool IsOutdoor { get; set; }
    public bool IsDayOnly { get; set; }
    public int PointsReward { get; set; }
    public string? ImageUrl { get; set; }
    public string TypeName { get; set; } = string.Empty;
}
