namespace Deploy.Models;

public class Animal
{
    public int Id { get; set; }
    public string CommonName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Habitat { get; set; }
    public string? Diet { get; set; }
    public string? Lifespan { get; set; }
    public int ConservationStatusId { get; set; }
    public string? ConservationReason { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public ConservationStatus? ConservationStatus { get; set; }
}
