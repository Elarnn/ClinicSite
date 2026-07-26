namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>Summary tiles + today's schedule for the doctor home page.</summary>
public class DoctorDashboardDto
{
    /// <summary>Booked appointments scheduled for today.</summary>
    public int TodayCount { get; set; }

    /// <summary>Today's appointments still ahead (not yet completed / no-show / cancelled, start in the future).</summary>
    public int RemainingCount { get; set; }

    /// <summary>Start time of the next upcoming patient today, if any.</summary>
    public DateTime? NextPatientStartUtc { get; set; }

    /// <summary>Free (open) slots remaining today.</summary>
    public int FreeWindowsCount { get; set; }

    /// <summary>Today's slots (booked, free and blocked), ordered by start time.</summary>
    public List<DoctorScheduleItemDto> Today { get; set; } = new();
}
