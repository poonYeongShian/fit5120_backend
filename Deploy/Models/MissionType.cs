namespace Deploy.Models;

/// <summary>
/// Maps to the <c>public.mission_types</c> table.
/// Categorises missions (e.g. indoor / outdoor) and defines the default points reward.
/// </summary>
public class MissionType
{
    public int Id { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PointsReward { get; set; } = 20;
}
