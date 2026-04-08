namespace Deploy.Enums;

/// <summary>
/// Maps to the PostgreSQL <c>mission_status</c> enum type.
/// Tracks the lifecycle state of a profile's mission attempt.
/// </summary>
public enum MissionStatus
{
    /// <summary>Mission has been assigned to the profile but not yet started.</summary>
    Assigned,

    /// <summary>Mission has been started but not yet completed.</summary>
    InProgress,

    /// <summary>Mission was successfully completed.</summary>
    Completed,

    /// <summary>Mission was skipped by the profile.</summary>
    Skipped
}
