namespace ClinicSite.Application.DTOs.Bookings
{
    /// <summary>
    /// Returned to the public site after a booking form is submitted. The booking is only
    /// <c>PendingConfirmation</c> at this point — no confirmation or cancellation token is ever
    /// included here.
    /// </summary>
    public class BookingResultDto
    {
        public Guid BookingId { get; set; }
        public Guid AppointmentSlotId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>Current lifecycle status, e.g. "PendingConfirmation".</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>When the confirmation link expires (UTC).</summary>
        public DateTime? ConfirmationExpiresAtUtc { get; set; }
    }
}
