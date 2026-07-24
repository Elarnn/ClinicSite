namespace ClinicSite.Application.DTOs.Admin
{
    public class AdminBookingDto
    {
        public Guid BookingId { get; set; }
        public Guid AppointmentSlotId { get; set; }

        public string PatientName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        public string? Comment { get; set; }

        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;

        public Guid SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;

        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        /// <summary>Lifecycle status name, e.g. "PendingConfirmation", "Confirmed", "Cancelled".</summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>Convenience flag derived from <see cref="Status"/>, kept for the admin UI.</summary>
        public bool IsCancelled { get; set; }

        public DateTime CreatedAtUtc { get; set; }
    }
}
