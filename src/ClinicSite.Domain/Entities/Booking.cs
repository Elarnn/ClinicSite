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

        // Lifecycle state of the booking (e-mail confirmation flow). Replaces the old IsCancelled flag.
        public BookingStatus Status { get; set; } = BookingStatus.PendingConfirmation;

        // Visit outcome, managed by the doctor in the doctor portal. Separate from Status above.
        public AppointmentStatus AppointmentStatus { get; set; } = AppointmentStatus.Scheduled;

        // Private note written by the doctor (not shown to the patient).
        public string? DoctorNote { get; set; }

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
