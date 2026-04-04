namespace Deploy.DTOs;

public class AnimalOccurrenceDto
{
    public string? LocationName { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public DateOnly? ObservedAt { get; set; }
    public string? Notes { get; set; }
}
