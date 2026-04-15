namespace Deploy.Models;

public class Animal
{
    public int Id { get; set; }
    public string CommonName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public int GroupId { get; set; }
    public string? Diet { get; set; }
    public string? Lifespan { get; set; }
    public string? Description { get; set; }
    public int ConservationStatusId { get; set; }
    public string? ImageUrl { get; set; }
    public string? AvatarPath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public AnimalGroup? AnimalGroup { get; set; }
    public ConservationStatus? ConservationStatus { get; set; }
}
