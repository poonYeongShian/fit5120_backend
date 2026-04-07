namespace Deploy.DTOs;

public class AnimalCardDetailDto
{
    public string CommonName { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string ConservationCode { get; set; } = string.Empty;
    public string ConservationLabel { get; set; } = string.Empty;
    public string ConservationDescription { get; set; } = string.Empty;
    public string? ConservationReason { get; set; }
    public string? ImageUrl { get; set; }
    public string? AvatarPath { get; set; }
    public short SeverityOrder { get; set; }
    public string? Habitat { get; set; }
    public string? Diet { get; set; }
    public string? Lifespan { get; set; }
    public string? Description { get; set; }
}
