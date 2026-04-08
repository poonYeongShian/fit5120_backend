namespace Deploy.Enums;

/// <summary>
/// Maps to the PostgreSQL <c>badge_type_enum</c> type.
/// </summary>
public enum BadgeTypeEnum
{
    /// <summary>Earned by leveling up through quizzes.</summary>
    Level,

    /// <summary>Earned by reaching mission milestones.</summary>
    Mission,

    /// <summary>Reserved for future special events.</summary>
    Special
}
