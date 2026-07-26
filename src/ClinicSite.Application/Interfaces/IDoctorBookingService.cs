using ClinicSite.Application.Common;
using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Domain.Enums;

namespace ClinicSite.Application.Interfaces;

/// <summary>
/// The doctor's view of, and actions on, the bookings for their own slots. Every method takes the
/// doctorId (from the JWT) and only ever touches data belonging to that doctor.
/// </summary>
public interface IDoctorBookingService
{
    /// <summary>Filtered, paged list of the doctor's bookings (List view).</summary>
    Task<PagedResult<DoctorBookingDto>> GetBookingsAsync(Guid doctorId, DoctorBookingFilterDto filter, CancellationToken cancellationToken = default);

    /// <summary>Summary tiles + today's schedule for the doctor home page.</summary>
    Task<DoctorDashboardDto> GetDashboardAsync(Guid doctorId, CancellationToken cancellationToken = default);

    /// <summary>Slot-centric schedule between two UTC instants (Today / Week views).</summary>
    Task<List<DoctorScheduleItemDto>> GetScheduleAsync(Guid doctorId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    /// <summary>Sets the appointment (visit) status of one of the doctor's bookings.</summary>
    Task UpdateStatusAsync(Guid doctorId, Guid bookingId, AppointmentStatus status, CancellationToken cancellationToken = default);

    /// <summary>Saves the doctor's private note. Returns the stored value (null when cleared).</summary>
    Task<string?> UpdateNoteAsync(Guid doctorId, Guid bookingId, string? note, CancellationToken cancellationToken = default);

    /// <summary>Other bookings for the same patient (matched by e-mail, case-insensitive), newest first.</summary>
    Task<List<PatientHistoryItemDto>> GetPatientHistoryAsync(Guid doctorId, Guid bookingId, CancellationToken cancellationToken = default);

    /// <summary>Sends a free-form message to the booking's patient e-mail.</summary>
    Task SendMessageAsync(Guid doctorId, Guid bookingId, string subject, string message, CancellationToken cancellationToken = default);
}
