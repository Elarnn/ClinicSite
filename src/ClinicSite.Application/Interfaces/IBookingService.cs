using ClinicSite.Application.DTOs.Bookings;

namespace ClinicSite.Application.Interfaces
{
    public interface IBookingService
    {
        /// <summary>
        /// Creates a booking in <c>PendingConfirmation</c> state, marks the slot Booked and sends the
        /// first confirmation e-mail. If that e-mail cannot be delivered the change is compensated
        /// (booking expired, slot freed) and an <see cref="Exceptions.EmailDeliveryException"/> is thrown.
        /// </summary>
        Task<BookingResultDto> CreateBookingAsync(CreateBookingDto dto, CancellationToken cancellationToken = default);

        /// <summary>Confirms a pending booking from a raw confirmation token. Idempotent.</summary>
        Task<ConfirmBookingResultDto> ConfirmBookingAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>Privacy-safe lookup for the cancellation page (no e-mail / ids returned).</summary>
        Task<BookingSummaryDto> GetCancellableBookingAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>Cancels a confirmed booking from a raw cancellation token. Idempotent.</summary>
        Task<CancelBookingResultDto> CancelBookingAsync(string token, CancellationToken cancellationToken = default);

        /// <summary>
        /// Expires every pending booking whose confirmation window has elapsed and frees its slot.
        /// Returns the number of bookings expired. Safe to call repeatedly (used by the background sweep).
        /// </summary>
        Task<int> ExpirePendingBookingsAsync(CancellationToken cancellationToken = default);
    }
}
