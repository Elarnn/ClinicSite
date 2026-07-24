namespace ClinicSite.Application.Notifications;

/// <summary>
/// The data needed to render a booking e-mail. Contains no tokens, ids or persistence concerns —
/// the infrastructure e-mail sender receives the raw token separately and builds the link itself.
/// </summary>
public sealed record BookingEmailModel
{
    public required string PatientName { get; init; }

    public required string PatientEmail { get; init; }

    public required string DoctorName { get; init; }

    public required string SpecialtyName { get; init; }

    public required DateTime StartTimeUtc { get; init; }

    public required DateTime EndTimeUtc { get; init; }

    public string? Comment { get; init; }

    /// <summary>Minutes the confirmation link stays valid (used in the first e-mail text only).</summary>
    public int ConfirmationLifetimeMinutes { get; init; } = 30;
}
