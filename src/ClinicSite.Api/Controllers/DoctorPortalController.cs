using ClinicSite.Application.Common;
using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers;

/// <summary>
/// Authenticated doctor endpoints for viewing and acting on the bookings of their own slots.
/// The doctor id always comes from the JWT (see <see cref="DoctorControllerBase"/>).
/// </summary>
[Route("api/doctor")]
public sealed class DoctorPortalController : DoctorControllerBase
{
    private readonly IDoctorBookingService _bookings;

    public DoctorPortalController(IDoctorBookingService bookings)
    {
        _bookings = bookings;
    }

    /// <summary>Summary tiles + today's schedule.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DoctorDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DoctorDashboardDto>> GetDashboard(CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        return Ok(await _bookings.GetDashboardAsync(doctorId, cancellationToken));
    }

    /// <summary>Slot-centric schedule between two UTC instants (Today / Week views).</summary>
    [HttpGet("schedule")]
    [ProducesResponseType(typeof(List<DoctorScheduleItemDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DoctorScheduleItemDto>>> GetSchedule(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        return Ok(await _bookings.GetScheduleAsync(doctorId, from, to, cancellationToken));
    }

    /// <summary>Filtered, paged list of the doctor's bookings (List view).</summary>
    [HttpGet("bookings")]
    [ProducesResponseType(typeof(PagedResult<DoctorBookingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<DoctorBookingDto>>> GetBookings(
        [FromQuery] DoctorBookingFilterDto filter,
        CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        return Ok(await _bookings.GetBookingsAsync(doctorId, filter, cancellationToken));
    }

    /// <summary>Set the appointment (visit) status of one of the doctor's bookings.</summary>
    [HttpPatch("bookings/{bookingId:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid bookingId,
        [FromBody] UpdateBookingStatusDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        await _bookings.UpdateStatusAsync(doctorId, bookingId, request.Status, cancellationToken);
        return NoContent();
    }

    /// <summary>Save the doctor's private note for a booking.</summary>
    [HttpPut("bookings/{bookingId:guid}/note")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNote(
        Guid bookingId,
        [FromBody] UpdateDoctorNoteDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        var saved = await _bookings.UpdateNoteAsync(doctorId, bookingId, request.Note, cancellationToken);
        return Ok(new { note = saved });
    }

    /// <summary>Other bookings for the same patient (by e-mail), newest first.</summary>
    [HttpGet("bookings/{bookingId:guid}/patient-history")]
    [ProducesResponseType(typeof(List<PatientHistoryItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PatientHistoryItemDto>>> GetPatientHistory(
        Guid bookingId,
        CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        return Ok(await _bookings.GetPatientHistoryAsync(doctorId, bookingId, cancellationToken));
    }

    /// <summary>Send a free-form message to the booking's patient e-mail.</summary>
    [HttpPost("bookings/{bookingId:guid}/send-message")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> SendMessage(
        Guid bookingId,
        [FromBody] SendPatientMessageDto request,
        CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        await _bookings.SendMessageAsync(doctorId, bookingId, request.Subject, request.Message, cancellationToken);
        return NoContent();
    }
}
