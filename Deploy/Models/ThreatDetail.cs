namespace Deploy.Models;

public class ThreatDetail
{
    public int ThreatDetailId { get; set; }
    public int AnimalId { get; set; }
    public int ThreatCategoryId { get; set; }
    public string? Explanation { get; set; }
    public string? Priority { get; set; }
}
