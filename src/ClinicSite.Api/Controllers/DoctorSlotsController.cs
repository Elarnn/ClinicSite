using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers;

/// <summary>
/// A doctor's slot blocking — a single slot (Week view) or a recurring time-of-day (Today view).
/// All operations are scoped to the JWT's doctor and only affect that doctor's own slots.
/// </summary>
[Route("api/doctor/slots")]
public sealed class DoctorSlotsController : DoctorControllerBase
{
    private readonly IDoctorSlotService _slots;

    public DoctorSlotsController(IDoctorSlotService slots)
    {
        _slots = slots;
    }

    /// <summary>Block one specific slot.</summary>
    [HttpPatch("{slotId:guid}/block")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Block(Guid slotId, CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        await _slots.BlockSlotAsync(doctorId, slotId, cancellationToken);
        return NoContent();
    }

    /// <summary>Re-open one specific blocked slot.</summary>
    [HttpPatch("{slotId:guid}/unblock")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Unblock(Guid slotId, CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        await _slots.UnblockSlotAsync(doctorId, slotId, cancellationToken);
        return NoContent();
    }

    /// <summary>Block this time-of-day on every future day (until unblocked).</summary>
    [HttpPost("{slotId:guid}/block-recurring")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BlockRecurring(Guid slotId, CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        var blocked = await _slots.BlockRecurringAsync(doctorId, slotId, cancellationToken);
        return Ok(new { blocked });
    }

    /// <summary>Re-open this time-of-day on every future day.</summary>
    [HttpPost("{slotId:guid}/unblock-recurring")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnblockRecurring(Guid slotId, CancellationToken cancellationToken)
    {
        if (!TryGetDoctorId(out var doctorId)) return Unauthorized();
        var unblocked = await _slots.UnblockRecurringAsync(doctorId, slotId, cancellationToken);
        return Ok(new { unblocked });
    }
}
