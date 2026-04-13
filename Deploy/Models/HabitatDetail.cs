namespace Deploy.Models;

public class HabitatDetail
{
    public int HabitatDetailId { get; set; }
    public int AnimalId { get; set; }
    public int HabitatCategoryId { get; set; }
    public string? Priority { get; set; }
    public string? Emoji { get; set; }
}
