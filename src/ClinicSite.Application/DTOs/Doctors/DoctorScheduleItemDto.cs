using ClinicSite.Domain.Enums;

namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>
/// One slot on the doctor's calendar (Today / Week views). Carries the slot's own state plus, when
/// the slot is booked, the booking details. Booking-related fields are null for free/blocked slots.
/// </summary>
public class DoctorScheduleItemDto
{
    public Guid SlotId { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTimeUtc { get; set; }

    /// <summary>Slot state as a readable string: "Free", "Reserved", "Booked", "Blocked".</summary>
    public string SlotStatus { get; set; } = string.Empty;

    /// <summary>True when this time-of-day is blocked on a future day (i.e. a recurring/daily block is active).</summary>
    public bool RecurringBlocked { get; set; }

    public Guid? BookingId { get; set; }
    public string? PatientName { get; set; }
    public string? PatientEmail { get; set; }
    public string? PatientComment { get; set; }
    public string? DoctorNote { get; set; }
    public AppointmentStatus? Status { get; set; }
}
