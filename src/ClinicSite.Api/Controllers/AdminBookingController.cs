using ClinicSite.Application.DTOs.Admin;
using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers;

[ApiController]
[Route("api/admin/bookings")]
public class AdminBookingsController : ControllerBase
{
    private readonly IAdminBookingService _adminBookingService;

    public AdminBookingsController(IAdminBookingService adminBookingService)
    {
        _adminBookingService = adminBookingService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminBookingDto>>> GetAll()
    {
        var bookings = await _adminBookingService.GetAllAsync();
        return Ok(bookings);
    }

    [HttpPost("{bookingId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid bookingId)
    {
        await _adminBookingService.CancelAsync(bookingId);
        return NoContent();
    }
}