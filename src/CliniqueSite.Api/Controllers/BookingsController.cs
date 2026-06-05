using CliniqueSite.Application.DTOs.Bookings;
using CliniqueSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CliniqueSite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking(CreateBookingDto dto)
    {
        var result = await _bookingService.CreateBookingAsync(dto);
        return Ok(result);
    }
}
