using ClinicSite.Domain.Common;
using ClinicSite.Domain.Enums;


namespace ClinicSite.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public Guid AppointmentSlotId { get; set; }

        public AppointmentSlot AppointmentSlot { get; set; } = null!;

        public string PatientName { get; set; } = string.Empty;

        public string PatientEmail { get; set; } = string.Empty;

        public string? Comment { get; set; }

        // Lifecycle state of the booking. Replaces the old boolean IsCancelled flag.
        public BookingStatus Status { get; set; } = BookingStatus.PendingConfirmation;

        // --- Email confirmation ---
        // Only the SHA-256 hash of the confirmation token is ever stored (never the raw token).
        public string? ConfirmationTokenHash { get; set; }

        // The booking must be confirmed before this moment or it is expired and the slot is freed.
        public DateTime? ConfirmationExpiresAtUtc { get; set; }

        public DateTime? ConfirmedAtUtc { get; set; }

        // --- Cancellation ---
        // A separate token (and hash) so a confirmation link can never be used to cancel and vice versa.
        public string? CancellationTokenHash { get; set; }

        public DateTime? CancelledAtUtc { get; set; }

        // --- Confirmation e-mail delivery bookkeeping (used for resend throttling) ---
        public DateTime? ConfirmationEmailSentAtUtc { get; set; }

        public int ConfirmationEmailAttempts { get; set; }

        public DateTime? LastConfirmationEmailSentAtUtc { get; set; }

        // Optimistic-concurrency token. Mapped as a SQL Server rowversion in production and
        // ignored on providers that do not support it (e.g. SQLite used in tests).
        public byte[]? RowVersion { get; set; }
    }
}
