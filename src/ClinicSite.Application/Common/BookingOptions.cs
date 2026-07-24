namespace ClinicSite.Application.Common;

/// <summary>
/// Non-secret booking-confirmation settings consumed by the Application layer. Bound from the
/// "Email" configuration section in the API host.
/// </summary>
public class BookingOptions
{
    public const string SectionName = "Email";

    /// <summary>How long, in minutes, a patient has to confirm a booking before it expires.</summary>
    public int ConfirmationLifetimeMinutes { get; set; } = 30;

    /// <summary>Minimum delay between confirmation-email resends for the same booking.</summary>
    public int ResendCooldownMinutes { get; set; } = 1;

    /// <summary>Maximum number of confirmation e-mails (including the first) that may be sent.</summary>
    public int MaxConfirmationEmailAttempts { get; set; } = 5;
}
