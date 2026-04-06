namespace Deploy.Models;

public class Quiz
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Topic { get; set; }
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Question> Questions { get; set; } = [];
}
