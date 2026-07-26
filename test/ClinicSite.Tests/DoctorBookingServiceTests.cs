using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Exceptions;
using ClinicSite.Domain.Enums;
using ClinicSite.Tests.TestSupport;

namespace ClinicSite.Tests;

public class DoctorBookingServiceTests
{
    private static DoctorBookingFilterDto Filter() => new() { Page = 1, PageSize = 20 };

    [Fact]
    public async Task GetBookings_returns_only_the_doctors_own()
    {
        using var db = new TestDatabase();
        var (doctorA, _, bookingA) = db.SeedBooking(patientEmail: "a@example.com");
        db.SeedBooking(patientEmail: "b@example.com"); // a different doctor's booking

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        var result = await service.GetBookingsAsync(doctorA, Filter());

        Assert.Equal(1, result.TotalCount);
        Assert.Equal(bookingA, result.Items.Single().BookingId);
    }

    [Fact]
    public async Task UpdateStatus_changes_the_appointment_status()
    {
        using var db = new TestDatabase();
        var (doctorId, _, bookingId) = db.SeedBooking();

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorBookingService(ctx, new FakeEmailService())
                .UpdateStatusAsync(doctorId, bookingId, AppointmentStatus.Completed);
        }

        using var verify = db.CreateContext();
        var booking = verify.Bookings.Single(b => b.Id == bookingId);
        Assert.Equal(AppointmentStatus.Completed, booking.AppointmentStatus);
    }

    [Fact]
    public async Task Cannot_change_another_doctors_booking()
    {
        using var db = new TestDatabase();
        var (_, _, bookingId) = db.SeedBooking();
        var (otherDoctor, _) = db.SeedFreeSlot();

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateStatusAsync(otherDoctor, bookingId, AppointmentStatus.Completed));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.UpdateNoteAsync(otherDoctor, bookingId, "hi"));
    }

    [Fact]
    public async Task UpdateNote_saves_and_clears()
    {
        using var db = new TestDatabase();
        var (doctorId, _, bookingId) = db.SeedBooking();

        using (var ctx = db.CreateContext())
        {
            var saved = await db.CreateDoctorBookingService(ctx, new FakeEmailService())
                .UpdateNoteAsync(doctorId, bookingId, "  Follow up in 2 weeks  ");
            Assert.Equal("Follow up in 2 weeks", saved);
        }

        using (var ctx = db.CreateContext())
        {
            var cleared = await db.CreateDoctorBookingService(ctx, new FakeEmailService())
                .UpdateNoteAsync(doctorId, bookingId, "   ");
            Assert.Null(cleared);
        }
    }

    // Patient history lives in its own suite — see DoctorPatientHistoryTests.

    [Fact]
    public async Task GetBookings_filters_by_status_and_search()
    {
        using var db = new TestDatabase();
        var (doctorId, _, targetId) = db.SeedBooking(patientName: "Alice Adams", patientEmail: "alice@example.com", appointmentStatus: AppointmentStatus.Completed);
        // Same doctor, another booking with a different status/name — reuse the same doctor by adding to their slot list.
        AddBookingForDoctor(db, doctorId, "Bob Brown", "bob@example.com", AppointmentStatus.Scheduled);

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        var byStatus = await service.GetBookingsAsync(doctorId, new DoctorBookingFilterDto { Page = 1, PageSize = 20, Status = AppointmentStatus.Completed });
        Assert.Equal(targetId, Assert.Single(byStatus.Items).BookingId);

        var bySearch = await service.GetBookingsAsync(doctorId, new DoctorBookingFilterDto { Page = 1, PageSize = 20, Search = "alice" });
        Assert.Equal(targetId, Assert.Single(bySearch.Items).BookingId);
    }

    [Fact]
    public async Task Schedule_marks_recurring_blocked_times()
    {
        using var db = new TestDatabase();
        var thirteenTomorrow = DateTime.UtcNow.Date.AddDays(1).AddHours(13);
        var (doctorId, slotTomorrow) = db.SeedFreeSlot(thirteenTomorrow);
        db.AddFreeSlot(doctorId, thirteenTomorrow.AddDays(1)); // same time, day after

        using (var ctx = db.CreateContext())
        {
            await db.CreateDoctorSlotService(ctx).BlockRecurringAsync(doctorId, slotTomorrow);
        }

        using var sctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(sctx, new FakeEmailService());
        var day2Start = thirteenTomorrow.AddDays(1).Date;
        var schedule = await service.GetScheduleAsync(doctorId, day2Start, day2Start.AddDays(1));

        Assert.True(Assert.Single(schedule).RecurringBlocked);
    }

    [Fact]
    public async Task Dashboard_includes_todays_booking()
    {
        using var db = new TestDatabase();
        var noonToday = DateTime.UtcNow.Date.AddHours(12);
        var (doctorId, _, _) = db.SeedBooking(startTimeUtc: noonToday);

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        var dashboard = await service.GetDashboardAsync(doctorId);

        Assert.Equal(1, dashboard.TodayCount);
        Assert.Single(dashboard.Today);
    }

    [Fact]
    public async Task SendMessage_goes_to_the_bookings_email()
    {
        using var db = new TestDatabase();
        var (doctorId, _, bookingId) = db.SeedBooking(patientEmail: "target@example.com");
        var email = new FakeEmailService();

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, email);

        await service.SendMessageAsync(doctorId, bookingId, "Reminder", "See you tomorrow.");

        Assert.Equal(1, email.PatientMessageCount);
        Assert.Equal("target@example.com", email.LastMessageEmail);

        await Assert.ThrowsAsync<ValidationException>(() =>
            service.SendMessageAsync(doctorId, bookingId, "", "body"));
    }

    // Adds a second booking to an existing doctor by creating another slot under the same doctor id.
    private static void AddBookingForDoctor(TestDatabase db, Guid doctorId, string name, string email, AppointmentStatus status)
    {
        using var context = db.CreateContext();
        var start = DateTime.UtcNow.AddDays(2);
        var slot = new ClinicSite.Domain.Entities.AppointmentSlot
        {
            DoctorId = doctorId,
            StartTimeUtc = start,
            EndTimeUtc = start.AddMinutes(30),
            Status = SlotStatus.Booked
        };
        var booking = new ClinicSite.Domain.Entities.Booking
        {
            AppointmentSlot = slot,
            PatientName = name,
            PatientEmail = email,
            Status = BookingStatus.Confirmed,
            AppointmentStatus = status
        };
        context.AppointmentSlots.Add(slot);
        context.Bookings.Add(booking);
        context.SaveChanges();
    }
}
