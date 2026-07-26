using ClinicSite.Domain.Enums;

namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>One booking of the same patient (matched by e-mail), across any doctor. The booking the
/// history was opened from is included too, so the caller can highlight it via <see cref="BookingId"/>.</summary>
public class PatientHistoryItemDto
{
    public Guid BookingId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }
    public string DoctorName { get; set; } = string.Empty;
    public string SpecialtyName { get; set; } = string.Empty;
    public AppointmentStatus Status { get; set; }
}
