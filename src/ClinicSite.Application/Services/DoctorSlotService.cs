using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Application.Services;

public class DoctorSlotService : IDoctorSlotService
{
    private readonly IApplicationDbContext _context;

    public DoctorSlotService(IApplicationDbContext context)
    {
        _context = context;
    }

    // --- Single slot (Week view): block/unblock one specific slot -----------------------------

    public async Task BlockSlotAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken = default)
    {
        var slot = await LoadOwnedSlotAsync(doctorId, slotId, cancellationToken);

        if (slot.StartTimeUtc <= DateTime.UtcNow)
        {
            throw new ConflictException("Cannot change a slot that has already started.");
        }
        if (slot.Status == SlotStatus.Booked || slot.Status == SlotStatus.Reserved)
        {
            throw new ConflictException("Cannot block a slot that is booked.");
        }
        if (slot.Status == SlotStatus.Blocked)
        {
            return; // idempotent
        }

        slot.Status = SlotStatus.Blocked;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UnblockSlotAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken = default)
    {
        var slot = await LoadOwnedSlotAsync(doctorId, slotId, cancellationToken);

        if (slot.StartTimeUtc <= DateTime.UtcNow)
        {
            throw new ConflictException("Cannot change a slot that has already started.");
        }
        if (slot.Status != SlotStatus.Blocked)
        {
            throw new ConflictException("Slot is not blocked.");
        }

        slot.Status = SlotStatus.Free;
        await _context.SaveChangesAsync(cancellationToken);
    }

    // --- Recurring (Today view): block/unblock a time-of-day on every (future) day -------------

    public async Task<int> BlockRecurringAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken = default)
    {
        var slot = await LoadOwnedSlotAsync(doctorId, slotId, cancellationToken);
        var minutes = MinutesOfDay(slot.StartTimeUtc);
        var now = DateTime.UtcNow;

        // Every future free slot of this doctor at the same time-of-day becomes blocked. Because no
        // new slots are ever generated at this time, that is effectively "blocked on every day".
        var candidates = await _context.AppointmentSlots
            .Where(s => s.DoctorId == doctorId && s.Status == SlotStatus.Free && s.StartTimeUtc > now)
            .ToListAsync(cancellationToken);

        var matched = candidates.Where(s => MinutesOfDay(s.StartTimeUtc) == minutes).ToList();
        foreach (var s in matched)
        {
            s.Status = SlotStatus.Blocked;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return matched.Count;
    }

    public async Task<int> UnblockRecurringAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken = default)
    {
        var slot = await LoadOwnedSlotAsync(doctorId, slotId, cancellationToken);
        var minutes = MinutesOfDay(slot.StartTimeUtc);
        var now = DateTime.UtcNow;

        var candidates = await _context.AppointmentSlots
            .Where(s => s.DoctorId == doctorId && s.Status == SlotStatus.Blocked && s.StartTimeUtc > now)
            .ToListAsync(cancellationToken);

        var matched = candidates.Where(s => MinutesOfDay(s.StartTimeUtc) == minutes).ToList();
        foreach (var s in matched)
        {
            s.Status = SlotStatus.Free;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return matched.Count;
    }

    // --- helpers -----------------------------------------------------------------------------

    private static int MinutesOfDay(DateTime utc) => utc.Hour * 60 + utc.Minute;

    private async Task<AppointmentSlot> LoadOwnedSlotAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken)
    {
        var slot = await _context.AppointmentSlots.FirstOrDefaultAsync(s => s.Id == slotId, cancellationToken);

        if (slot is null || slot.DoctorId != doctorId)
        {
            throw new NotFoundException("Slot not found.");
        }

        return slot;
    }
}
