namespace ClinicSite.Domain.Enums;

/// <summary>
/// Lifecycle of a doctor's login account.
/// </summary>
public enum DoctorAccountStatus
{
    /// <summary>No email bound, no account.</summary>
    None = 0,

    /// <summary>An invitation email was sent; the doctor has not set a password yet.</summary>
    Invited = 1,

    /// <summary>The doctor has set a password and can log in.</summary>
    Active = 2
}
