using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers;

/// <summary>
/// Authenticated doctor endpoints. Requires a valid JWT carrying the "Doctor" role; the doctor id is
/// read from the token's <c>doctorId</c> claim, so a doctor can only ever see their own data.
/// </summary>
[ApiController]
[Route("api/doctor")]
[Authorize(Roles = "Doctor")]
public sealed class DoctorPortalController : ControllerBase
{
    // Matches JwtTokenService.DoctorIdClaim.
    private const string DoctorIdClaim = "doctorId";

    private readonly IDoctorBookingService _bookings;

    public DoctorPortalController(IDoctorBookingService bookings)
    {
        _bookings = bookings;
    }

    /// <summary>Bookings for the authenticated doctor's slots, newest first.</summary>
    [HttpGet("bookings")]
    [ProducesResponseType(typeof(List<DoctorBookingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DoctorBookingDto>>> GetMyBookings(CancellationToken cancellationToken)
    {
        var claim = User.FindFirst(DoctorIdClaim)?.Value;
        if (!Guid.TryParse(claim, out var doctorId))
        {
            return Unauthorized();
        }

        var bookings = await _bookings.GetByDoctorAsync(doctorId, cancellationToken);
        return Ok(bookings);
    }
}
