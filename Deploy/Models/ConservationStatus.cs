namespace Deploy.Models;

public class ConservationStatus
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public short SeverityOrder { get; set; }
}
