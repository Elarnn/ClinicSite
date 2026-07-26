using ClinicSite.Application.Exceptions;
using ClinicSite.Domain.Enums;
using ClinicSite.Tests.TestSupport;

namespace ClinicSite.Tests;

/// <summary>
/// The doctor-portal patient history. There is no Patient entity — a "patient" is the e-mail on a
/// booking, compared trimmed and case-insensitively. Access is gated by the booking the history is
/// opened from (it must belong to the calling doctor); past that gate the history is clinic-wide.
///
/// Every test gets its own <see cref="TestDatabase"/> (a private SQLite in-memory connection), so the
/// tests share no state and do not depend on execution order. Dates are fixed constants inside each
/// test rather than offsets from "now", so they never go stale.
/// </summary>
public class DoctorPatientHistoryTests
{
    // A fixed, far-future anchor: the service never compares against DateTime.UtcNow, so the exact
    // instants are irrelevant — only their relative order matters.
    private static readonly DateTime Anchor = new(2030, 3, 1, 9, 0, 0, DateTimeKind.Utc);

    private const string PatientEmail = "patient@example.com";
    private const string OtherEmail = "someone.else@example.com";

    [Fact]
    public async Task GetPatientHistoryAsync_ReturnsBookingsOfTheSamePatientOnly()
    {
        using var db = new TestDatabase();
        var doctorId = db.SeedDoctorWithSpecialty("Dr. House", "Cardiology");

        var first = db.AddBooking(doctorId, Anchor, PatientEmail, "Pat Patient", appointmentStatus: AppointmentStatus.Completed);
        db.AddBooking(doctorId, Anchor.AddDays(7), PatientEmail, "Pat Patient", appointmentStatus: AppointmentStatus.NoShow);
        db.AddBooking(doctorId, Anchor.AddDays(14), PatientEmail, "Pat Patient");
        var stranger = db.AddBooking(doctorId, Anchor.AddDays(21), OtherEmail, "Sam Stranger");

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        var history = await service.GetPatientHistoryAsync(doctorId, first);

        Assert.Equal(3, history.Count);
        Assert.DoesNotContain(history, h => h.BookingId == stranger);

        // The booking the history was opened from is part of the result, with its own data.
        var opened = Assert.Single(history, h => h.BookingId == first);
        Assert.Equal(Anchor, opened.StartTimeUtc);
        Assert.Equal(Anchor.AddMinutes(30), opened.EndTimeUtc);
        Assert.Equal("Dr. House", opened.DoctorName);
        Assert.Equal("Cardiology", opened.SpecialtyName);
        Assert.Equal(AppointmentStatus.Completed, opened.Status);
    }

    [Fact]
    public async Task GetPatientHistoryAsync_ReturnsBookingsWithSameNormalizedEmail()
    {
        using var db = new TestDatabase();
        var doctorId = db.SeedDoctorWithSpecialty("Dr. House", "Cardiology");

        var lower = db.AddBooking(doctorId, Anchor, "patient@example.com");
        var upper = db.AddBooking(doctorId, Anchor.AddDays(1), "PATIENT@EXAMPLE.COM");
        var padded = db.AddBooking(doctorId, Anchor.AddDays(2), " patient@example.com ");

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        // Opening the history from the padded booking also proves the *source* e-mail is normalized.
        var history = await service.GetPatientHistoryAsync(doctorId, padded);

        Assert.Equal(3, history.Count);
        Assert.Equal(
            new[] { lower, upper, padded }.OrderBy(id => id),
            history.Select(h => h.BookingId).OrderBy(id => id));
    }

    [Fact]
    public async Task GetPatientHistoryAsync_ReturnsBookingsNewestFirst()
    {
        using var db = new TestDatabase();
        var doctorId = db.SeedDoctorWithSpecialty("Dr. House", "Cardiology");

        // Inserted out of order on purpose — the result must be sorted, not insertion-ordered.
        db.AddBooking(doctorId, Anchor.AddDays(10), PatientEmail);
        var newest = db.AddBooking(doctorId, Anchor.AddDays(40), PatientEmail);
        var oldest = db.AddBooking(doctorId, Anchor, PatientEmail);
        db.AddBooking(doctorId, Anchor.AddDays(25), PatientEmail);

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        var history = await service.GetPatientHistoryAsync(doctorId, oldest);

        var starts = history.Select(h => h.StartTimeUtc).ToList();
        Assert.Equal(starts.OrderByDescending(s => s), starts);
        Assert.Equal(newest, history.First().BookingId);
        Assert.Equal(oldest, history.Last().BookingId);
    }

    [Fact]
    public async Task GetPatientHistoryAsync_ExcludesBookingsWithADifferentEmail()
    {
        using var db = new TestDatabase();
        var doctorId = db.SeedDoctorWithSpecialty("Dr. House", "Cardiology");

        var mine = db.AddBooking(doctorId, Anchor, PatientEmail);
        db.AddBooking(doctorId, Anchor.AddDays(3), PatientEmail);
        var stranger = db.AddBooking(doctorId, Anchor.AddDays(5), OtherEmail, "Sam Stranger");

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        var history = await service.GetPatientHistoryAsync(doctorId, mine);

        Assert.Equal(2, history.Count);
        Assert.DoesNotContain(history, h => h.BookingId == stranger);
    }

    [Fact]
    public async Task GetPatientHistoryAsync_WhenBookingBelongsToAnotherDoctor_ThrowsNotFound()
    {
        using var db = new TestDatabase();
        var ownerId = db.SeedDoctorWithSpecialty("Dr. House", "Cardiology");
        var intruderId = db.SeedDoctorWithSpecialty("Dr. Wilson", "Oncology");

        var booking = db.AddBooking(ownerId, Anchor, PatientEmail);
        db.AddBooking(ownerId, Anchor.AddDays(4), PatientEmail); // more of the same patient to leak

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        // Same 404 as a missing booking, so the response never reveals that it exists elsewhere.
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetPatientHistoryAsync(intruderId, booking));

        // The owner can still read it — the rejection is about ownership, not a broken query.
        Assert.Equal(2, (await service.GetPatientHistoryAsync(ownerId, booking)).Count);
    }

    [Fact]
    public async Task GetPatientHistoryAsync_IncludesVisitsToOtherDoctors()
    {
        using var db = new TestDatabase();
        var cardiologistId = db.SeedDoctorWithSpecialty("Dr. House", "Cardiology");
        var dermatologistId = db.SeedDoctorWithSpecialty("Dr. Wilson", "Dermatology");

        var mine = db.AddBooking(cardiologistId, Anchor.AddDays(10), PatientEmail, appointmentStatus: AppointmentStatus.Scheduled);
        var theirs = db.AddBooking(dermatologistId, Anchor, PatientEmail, appointmentStatus: AppointmentStatus.Completed);

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        var history = await service.GetPatientHistoryAsync(cardiologistId, mine);

        Assert.Equal(2, history.Count);

        var own = Assert.Single(history, h => h.BookingId == mine);
        Assert.Equal("Dr. House", own.DoctorName);
        Assert.Equal("Cardiology", own.SpecialtyName);
        Assert.Equal(AppointmentStatus.Scheduled, own.Status);

        var other = Assert.Single(history, h => h.BookingId == theirs);
        Assert.Equal("Dr. Wilson", other.DoctorName);
        Assert.Equal("Dermatology", other.SpecialtyName);
        Assert.Equal(AppointmentStatus.Completed, other.Status);
    }

    [Fact]
    public async Task GetPatientHistoryAsync_WhenBookingDoesNotExist_ThrowsNotFound()
    {
        using var db = new TestDatabase();
        var doctorId = db.SeedDoctorWithSpecialty("Dr. House", "Cardiology");
        db.AddBooking(doctorId, Anchor, PatientEmail);

        using var ctx = db.CreateContext();
        var service = db.CreateDoctorBookingService(ctx, new FakeEmailService());

        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.GetPatientHistoryAsync(doctorId, Guid.NewGuid()));
    }
}
