using ClinicSite.Domain.Enums;

namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>A booking as shown to the doctor who owns the slot.</summary>
public class DoctorBookingDto
{
    public Guid BookingId { get; set; }
    public Guid AppointmentSlotId { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string PatientEmail { get; set; } = string.Empty;

    /// <summary>The comment the patient left when booking (distinct from the doctor's note).</summary>
    public string? PatientComment { get; set; }

    /// <summary>The doctor's private note.</summary>
    public string? DoctorNote { get; set; }

    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    public AppointmentStatus Status { get; set; }
}
