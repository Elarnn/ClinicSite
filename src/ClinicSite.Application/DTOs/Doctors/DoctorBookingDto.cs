namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>A booking as shown to the doctor who owns the slot.</summary>
public class DoctorBookingDto
{
    public string PatientName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;
    public string? Comment { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string Status { get; set; } = string.Empty;
}
