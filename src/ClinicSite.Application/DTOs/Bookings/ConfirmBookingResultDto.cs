namespace ClinicSite.Application.DTOs.Bookings
{
    /// <summary>
    /// Safe booking details returned to the confirmation page. Contains no e-mail address, internal
    /// ids or tokens.
    /// </summary>
    public class ConfirmBookingResultDto
    {
        public string PatientName { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string? Comment { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
