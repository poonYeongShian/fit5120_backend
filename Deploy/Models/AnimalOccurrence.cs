namespace Deploy.Models;

public class AnimalOccurrence
{
    public int Id { get; set; }
    public int AnimalId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string? LocationName { get; set; }
    public DateOnly? ObservedAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation property
    public Animal? Animal { get; set; }
}
