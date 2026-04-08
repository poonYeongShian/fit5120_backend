namespace Deploy.Enums;

/// <summary>
/// Maps to the PostgreSQL <c>badge_source_enum</c> type.
/// Tracks the origin of a badge earned by a profile.
/// </summary>
public enum BadgeSourceEnum
{
    /// <summary>Badge was awarded via a quiz / level-up flow.</summary>
    Quiz,

    /// <summary>Badge was awarded via a mission milestone.</summary>
    Mission,

    /// <summary>Badge was awarded via a special event.</summary>
    Special
}
