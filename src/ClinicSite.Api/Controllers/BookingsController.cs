using ClinicSite.Application.DTOs.Bookings;
using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicSite.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Creates a pending booking and sends the confirmation e-mail. The response never contains any
    /// confirmation or cancellation token.
    /// </summary>
    [HttpPost]
    [EnableRateLimiting(RateLimitPolicies.CreateBooking)]
    public async Task<ActionResult<BookingResultDto>> CreateBooking(CreateBookingDto dto, CancellationToken cancellationToken)
    {
        var result = await _bookingService.CreateBookingAsync(dto, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Confirms a booking from the token embedded in the e-mail link. Called by the React
    /// confirmation page (never a GET, so e-mail scanners cannot auto-confirm). Idempotent.
    /// </summary>
    [HttpPost("confirm")]
    [EnableRateLimiting(RateLimitPolicies.ConfirmCancel)]
    public async Task<ActionResult<ConfirmBookingResultDto>> Confirm(ConfirmBookingDto dto, CancellationToken cancellationToken)
    {
        var result = await _bookingService.ConfirmBookingAsync(dto.Token, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns privacy-safe booking details for the cancellation page (no e-mail / ids). POST so the
    /// token stays out of URLs and server logs.
    /// </summary>
    [HttpPost("cancel-info")]
    [EnableRateLimiting(RateLimitPolicies.ConfirmCancel)]
    public async Task<ActionResult<BookingSummaryDto>> CancelInfo(CancelBookingDto dto, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetCancellableBookingAsync(dto.Token, cancellationToken);
        return Ok(result);
    }

    /// <summary>Cancels a confirmed booking from its cancellation token. Idempotent.</summary>
    [HttpPost("cancel")]
    [EnableRateLimiting(RateLimitPolicies.ConfirmCancel)]
    public async Task<ActionResult<CancelBookingResultDto>> Cancel(CancelBookingDto dto, CancellationToken cancellationToken)
    {
        var result = await _bookingService.CancelBookingAsync(dto.Token, cancellationToken);
        return Ok(result);
    }
}
