namespace ClinicSite.Application.DTOs.Bookings
{
    /// <summary>
    /// Minimal, privacy-safe view of a booking shown on the cancellation page before the patient
    /// confirms the cancellation. Deliberately omits the e-mail address and all internal ids.
    /// </summary>
    public class BookingSummaryDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string Status { get; set; } = string.Empty;

        /// <summary>True when the booking is still in a state where it can be cancelled.</summary>
        public bool Cancellable { get; set; }
    }
}
