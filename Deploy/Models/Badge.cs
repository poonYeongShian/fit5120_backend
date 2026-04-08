using Deploy.Enums;

namespace Deploy.Models;

/// <summary>
/// Maps to the <c>public.badges</c> table.
/// Shared badge definitions used by both quiz (level-up) and mission (milestone) flows.
/// </summary>
public class Badge
{
    public int Id { get; set; }
    public string BadgeName { get; set; } = string.Empty;
    public string? BadgeImageUrl { get; set; }
    public string? Description { get; set; }

    /// <summary>Discriminates between level, mission, and special badges.</summary>
    public BadgeTypeEnum BadgeType { get; set; }

    /// <summary>
    /// The level number a profile must reach to earn this badge.
    /// Only relevant when <see cref="BadgeType"/> is <see cref="BadgeTypeEnum.Level"/>.
    /// Foreign key → <c>public.levels(level_number)</c>.
    /// </summary>
    public int? LevelRequired { get; set; }

    /// <summary>
    /// The total missions a profile must complete to earn this badge.
    /// Only relevant when <see cref="BadgeType"/> is <see cref="BadgeTypeEnum.Mission"/>.
    /// </summary>
    public int? MissionsRequired { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}
