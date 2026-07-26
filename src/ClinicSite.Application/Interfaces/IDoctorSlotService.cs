namespace ClinicSite.Application.Interfaces;

/// <summary>
/// A doctor's slot blocking. Two kinds:
/// <list type="bullet">
///   <item>single — block/unblock one specific slot (Week view);</item>
///   <item>recurring — block/unblock a time-of-day on every (future) day (Today view).</item>
/// </list>
/// All operations are scoped to the given doctorId and only affect that doctor's own slots.
/// </summary>
public interface IDoctorSlotService
{
    Task BlockSlotAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken = default);

    Task UnblockSlotAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken = default);

    /// <summary>Blocks every future free slot at the same time-of-day as the given slot. Returns the count.</summary>
    Task<int> BlockRecurringAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken = default);

    /// <summary>Unblocks every future blocked slot at the same time-of-day as the given slot. Returns the count.</summary>
    Task<int> UnblockRecurringAsync(Guid doctorId, Guid slotId, CancellationToken cancellationToken = default);
}
