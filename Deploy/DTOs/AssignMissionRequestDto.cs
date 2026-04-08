namespace Deploy.DTOs;

public class AssignMissionRequestDto
{
    public Guid ProfileId { get; set; }
    public int MissionId { get; set; }
    public int? WeatherCode { get; set; }
    public bool? IsDay { get; set; }
    public decimal? WeatherTemp { get; set; }
    public decimal? LocationLat { get; set; }
    public decimal? LocationLon { get; set; }
}
