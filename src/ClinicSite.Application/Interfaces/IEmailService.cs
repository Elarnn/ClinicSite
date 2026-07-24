using ClinicSite.Application.Notifications;

namespace ClinicSite.Application.Interfaces;

/// <summary>
/// Transactional e-mail notifications for the booking flow. Implemented in the Infrastructure layer
/// (currently via the Brevo HTTP API). The abstraction is intentionally free of any provider type.
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
}
