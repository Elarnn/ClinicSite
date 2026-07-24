namespace ClinicSite.Application.DTOs.Bookings
{
    /// <summary>Result of a successful (or idempotent) cancellation.</summary>
    public class CancelBookingResultDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
