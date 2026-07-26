using ClinicSite.Application.Notifications;

namespace ClinicSite.Application.Interfaces;

/// <summary>
/// Transactional e-mail notifications for the booking flow. Implemented in the Infrastructure layer
/// (currently over SMTP via MailKit). The abstraction is intentionally free of any provider type.
///
/// The raw confirmation / cancellation tokens are passed in so the implementation can build the
/// public links; implementations MUST NOT log the tokens or the resulting URLs.
/// </summary>
public interface IEmailService
{
    /// <summary>First e-mail: asks the patient to confirm a freshly created (pending) booking.</summary>
    Task SendConfirmationRequestAsync(BookingEmailModel model, string confirmationToken, CancellationToken cancellationToken = default);

    /// <summary>Second e-mail: sent after a successful confirmation, includes a cancellation link.</summary>
    Task SendBookingConfirmedAsync(BookingEmailModel model, string cancellationToken, CancellationToken ct = default);

    /// <summary>Optional courtesy e-mail sent when a confirmed booking is cancelled.</summary>
    Task SendBookingCancelledAsync(BookingEmailModel model, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invitation e-mail for a new doctor account, containing a one-time link to set a password.
    /// The raw invite token is passed in so the implementation can build the link; it MUST NOT be logged.
    /// </summary>
    Task SendDoctorInviteAsync(string doctorName, string toEmail, string inviteToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// A free-form message a doctor sends to a patient about their booking. The caller supplies the
    /// recipient from the booking (never from client input); subject/message length is validated upstream.
    /// </summary>
    Task SendPatientMessageAsync(string toEmail, string patientName, string subject, string message, CancellationToken cancellationToken = default);
}
