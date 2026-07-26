using ClinicSite.Domain.Enums;

namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>Query filter for the doctor's booking list (bound from the query string).</summary>
public class DoctorBookingFilterDto
{
    /// <summary>Only bookings whose slot starts at/after this UTC instant.</summary>
    public DateTime? From { get; set; }

    /// <summary>Only bookings whose slot starts at/before this UTC instant.</summary>
    public DateTime? To { get; set; }

    public AppointmentStatus? Status { get; set; }

    /// <summary>Case-insensitive match on patient name or e-mail.</summary>
    public string? Search { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
