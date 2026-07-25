using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Services;
using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using ClinicSite.Tests.TestSupport;

namespace ClinicSite.Tests;

public class AppointmentSlotServiceTests
{
    // Seeds one doctor with a past Free slot and a future Free slot.
    private static (Guid DoctorId, Guid PastSlotId, Guid FutureSlotId) SeedPastAndFutureSlots(TestDatabase db)
    {
        using var context = db.CreateContext();

        var specialty = new Specialty { Name = "Cardiology" };
        var doctor = new Doctor { FullName = "Dr. Smith", Specialty = specialty };

        var past = new AppointmentSlot
        {
            Doctor = doctor,
            StartTimeUtc = DateTime.UtcNow.AddHours(-2),
            EndTimeUtc = DateTime.UtcNow.AddHours(-2).AddMinutes(30),
            Status = SlotStatus.Free
        };
        var future = new AppointmentSlot
        {
            Doctor = doctor,
            StartTimeUtc = DateTime.UtcNow.AddDays(1),
            EndTimeUtc = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            Status = SlotStatus.Free
        };

        context.AppointmentSlots.AddRange(past, future);
        context.SaveChanges();

        return (doctor.Id, past.Id, future.Id);
    }

    [Fact]
    public async Task GetAllByDoctor_excludes_past_slots()
    {
        using var db = new TestDatabase();
        var (doctorId, _, futureId) = SeedPastAndFutureSlots(db);

        using var context = db.CreateContext();
        var service = new AppointmentSlotService(context);

        var slots = await service.GetAllByDoctorAsync(doctorId);

        Assert.Single(slots);
        Assert.Equal(futureId, slots[0].SlotId);
    }

    [Fact]
    public async Task GetFreeByDoctor_excludes_past_slots()
    {
        using var db = new TestDatabase();
        var (doctorId, _, futureId) = SeedPastAndFutureSlots(db);

        using var context = db.CreateContext();
        var service = new AppointmentSlotService(context);

        var slots = await service.GetFreeByDoctorAsync(doctorId);

        Assert.Single(slots);
        Assert.Equal(futureId, slots[0].SlotId);
    }

    [Fact]
    public async Task ReserveSlot_rejects_a_past_slot()
    {
        using var db = new TestDatabase();
        var (_, pastId, _) = SeedPastAndFutureSlots(db);

        using var context = db.CreateContext();
        var service = new AppointmentSlotService(context);

        await Assert.ThrowsAsync<ConflictException>(() => service.ReserveSlotAsync(pastId));
    }

    [Fact]
    public async Task ReserveSlot_allows_a_future_slot()
    {
        using var db = new TestDatabase();
        var (_, _, futureId) = SeedPastAndFutureSlots(db);

        using var context = db.CreateContext();
        var service = new AppointmentSlotService(context);

        var result = await service.ReserveSlotAsync(futureId);

        Assert.Equal(futureId, result.SlotId);
        Assert.False(string.IsNullOrWhiteSpace(result.ReservationToken));
    }
}
