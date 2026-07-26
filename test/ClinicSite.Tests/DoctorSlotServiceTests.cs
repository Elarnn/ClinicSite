using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Services;
using ClinicSite.Domain.Enums;
using ClinicSite.Tests.TestSupport;

namespace ClinicSite.Tests;

public class DoctorSlotServiceTests
{
    // --- single slot (Week view) --------------------------------------------------------------

    [Fact]
    public async Task Cannot_block_a_booked_slot()
    {
        using var db = new TestDatabase();
        var (doctorId, slotId, _) = db.SeedBooking();

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorSlotService(ctx);

        await Assert.ThrowsAsync<ConflictException>(() => service.BlockSlotAsync(doctorId, slotId));
    }

    [Fact]
    public async Task Block_then_unblock_a_single_free_slot()
    {
        using var db = new TestDatabase();
        var (doctorId, slotId) = db.SeedFreeSlot();

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorSlotService(ctx).BlockSlotAsync(doctorId, slotId);
        }
        Assert.Equal(SlotStatus.Blocked, db.GetSlot(slotId).Status);

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorSlotService(ctx).UnblockSlotAsync(doctorId, slotId);
        }
        Assert.Equal(SlotStatus.Free, db.GetSlot(slotId).Status);
    }

    [Fact]
    public async Task Blocked_slot_is_hidden_from_patients()
    {
        using var db = new TestDatabase();
        var (doctorId, slotId) = db.SeedFreeSlot();

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorSlotService(ctx).BlockSlotAsync(doctorId, slotId);
        }

        using var verify = db.CreateContext();
        var slotService = new AppointmentSlotService(verify);

        Assert.Empty(await slotService.GetFreeByDoctorAsync(doctorId));
        Assert.Empty(await slotService.GetAllByDoctorAsync(doctorId));
    }

    [Fact]
    public async Task Cannot_change_another_doctors_slot()
    {
        using var db = new TestDatabase();
        var (_, slotId) = db.SeedFreeSlot();
        var (otherDoctor, _) = db.SeedFreeSlot();

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorSlotService(ctx);

        await Assert.ThrowsAsync<NotFoundException>(() => service.BlockSlotAsync(otherDoctor, slotId));
    }

    // --- recurring (Today view) ---------------------------------------------------------------

    [Fact]
    public async Task Recurring_block_blocks_the_same_time_on_all_future_days()
    {
        using var db = new TestDatabase();
        var thirteen = DateTime.UtcNow.Date.AddDays(1).AddHours(13);
        var (doctorId, slotDay1) = db.SeedFreeSlot(thirteen);
        var slotDay2 = db.AddFreeSlot(doctorId, thirteen.AddDays(1));       // same time, next day
        var slotOtherTime = db.AddFreeSlot(doctorId, thirteen.AddHours(1)); // different time, same day

        int blocked;
        using (var ctx = db.CreateContext())
        {
            blocked = await db.CreateDoctorSlotService(ctx).BlockRecurringAsync(doctorId, slotDay1);
        }

        Assert.Equal(2, blocked);
        Assert.Equal(SlotStatus.Blocked, db.GetSlot(slotDay1).Status);
        Assert.Equal(SlotStatus.Blocked, db.GetSlot(slotDay2).Status);
        Assert.Equal(SlotStatus.Free, db.GetSlot(slotOtherTime).Status);
    }

    [Fact]
    public async Task Recurring_unblock_frees_the_same_time_on_all_future_days()
    {
        using var db = new TestDatabase();
        var thirteen = DateTime.UtcNow.Date.AddDays(1).AddHours(13);
        var (doctorId, slotDay1) = db.SeedFreeSlot(thirteen);
        var slotDay2 = db.AddFreeSlot(doctorId, thirteen.AddDays(1));

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorSlotService(ctx).BlockRecurringAsync(doctorId, slotDay1);
        }

        int unblocked;
        using (var ctx = db.CreateContext())
        {
            unblocked = await db.CreateDoctorSlotService(ctx).UnblockRecurringAsync(doctorId, slotDay1);
        }

        Assert.Equal(2, unblocked);
        Assert.Equal(SlotStatus.Free, db.GetSlot(slotDay1).Status);
        Assert.Equal(SlotStatus.Free, db.GetSlot(slotDay2).Status);
    }
}
