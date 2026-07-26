using ClinicSite.Domain.Enums;

namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>One past/other booking for the same patient (matched by e-mail), across any doctor.</summary>
public class PatientHistoryItemDto
{
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
}
