using Deploy.Enums;

namespace Deploy.Models;

/// <summary>
/// Maps to the <c>public.profile_badges</c> table.
/// Records every badge that a profile has earned, regardless of source.
/// </summary>
public class ProfileBadge
{
    public int Id { get; set; }

    /// <summary>Foreign key → <c>public.profiles(id)</c>.</summary>
    public Guid ProfileId { get; set; }

    /// <summary>Foreign key → <c>public.badges(id)</c>.</summary>
    public int BadgeId { get; set; }

    /// <summary>Indicates whether the badge was awarded via a quiz, mission, or special event.</summary>
    public BadgeSourceEnum Source { get; set; }

    public DateTimeOffset EarnedAt { get; set; }
}
