using ClinicSite.Domain.Enums;
using ClinicSite.Infrastructure.Seed;
using ClinicSite.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Tests;

/// <summary>
/// The Development-only fixtures. These run on every app start in Development, so re-running the
/// seeder must never duplicate data — that is what these tests pin down. They exercise the seeder
/// against SQLite in-memory; the real Development database is never touched.
/// </summary>
public class DevSeederTests
{
    private static async Task SeedDoctorsAsync(TestDatabase db)
    {
        using var context = db.CreateContext();
        await DbSeeder.SeedSpecialtiesAsync(context);
        await DbSeeder.SeedDoctorsAsync(context);
    }

    [Fact]
    public async Task SeedAsync_CreatesThePatientHistoryFixture()
    {
        using var db = new TestDatabase();
        await SeedDoctorsAsync(db);

        using (var context = db.CreateContext())
        {
            await DevSeeder.SeedAsync(context);
        }

        using var verify = db.CreateContext();
        var bookings = await verify.Bookings
            .Include(b => b.AppointmentSlot)
            .Where(b => b.PatientEmail == DevSeeder.HistoryPatientEmail)
            .ToListAsync();

        Assert.Equal(3, bookings.Count);
        Assert.Contains(bookings, b => b.AppointmentStatus == AppointmentStatus.Completed);
        Assert.Contains(bookings, b => b.AppointmentStatus == AppointmentStatus.NoShow);
        Assert.Contains(bookings, b => b.AppointmentStatus == AppointmentStatus.Scheduled);

        // Distinct dates, and spread over more than one doctor so the history shows a cross-doctor view.
        Assert.Equal(3, bookings.Select(b => b.AppointmentSlot.StartTimeUtc.Date).Distinct().Count());
        Assert.True(bookings.Select(b => b.AppointmentSlot.DoctorId).Distinct().Count() > 1);
    }

    [Fact]
    public async Task SeedAsync_IsIdempotent()
    {
        using var db = new TestDatabase();
        await SeedDoctorsAsync(db);

        for (var run = 0; run < 3; run++)
        {
            using var context = db.CreateContext();
            await DevSeeder.SeedAsync(context);
        }

        using var verify = db.CreateContext();
        Assert.Equal(3, await verify.Bookings.CountAsync(b => b.PatientEmail == DevSeeder.HistoryPatientEmail));
        Assert.Equal(1, await verify.Doctors.CountAsync(d => d.Email == DevSeeder.DemoDoctorEmail));
    }

    [Fact]
    public async Task SeedAsync_ReanchorsTheUpcomingVisitOnceItHasGoneStale()
    {
        using var db = new TestDatabase();
        await SeedDoctorsAsync(db);

        using (var context = db.CreateContext())
        {
            await DevSeeder.SeedAsync(context);
        }

        // Simulate the app being started again days later: the "upcoming" visit is now in the past.
        using (var context = db.CreateContext())
        {
            var upcoming = await context.Bookings
                .Include(b => b.AppointmentSlot)
                .SingleAsync(b => b.PatientEmail == DevSeeder.HistoryPatientEmail
                    && b.AppointmentStatus == AppointmentStatus.Scheduled);
            upcoming.AppointmentSlot.StartTimeUtc = DateTime.UtcNow.Date.AddDays(-3).AddHours(10);
            upcoming.AppointmentSlot.EndTimeUtc = upcoming.AppointmentSlot.StartTimeUtc.AddMinutes(30);
            await context.SaveChangesAsync();
        }

        using (var context = db.CreateContext())
        {
            await DevSeeder.SeedAsync(context);
        }

        using var verify = db.CreateContext();
        var bookings = await verify.Bookings
            .Include(b => b.AppointmentSlot)
            .Where(b => b.PatientEmail == DevSeeder.HistoryPatientEmail)
            .ToListAsync();

        Assert.Equal(3, bookings.Count); // re-anchored, not re-created
        var scheduled = bookings.Single(b => b.AppointmentStatus == AppointmentStatus.Scheduled);
        Assert.Equal(DateTime.UtcNow.Date, scheduled.AppointmentSlot.StartTimeUtc.Date);
    }

    [Fact]
    public async Task SeedAsync_DoesNothingWhenThereAreNoDoctors()
    {
        using var db = new TestDatabase();

        using (var context = db.CreateContext())
        {
            await DevSeeder.SeedAsync(context);
        }

        using var verify = db.CreateContext();
        Assert.Equal(0, await verify.Bookings.CountAsync());
    }

    [Fact]
    public async Task SeedAsync_LeavesAnExistingDoctorAccountAlone()
    {
        using var db = new TestDatabase();
        await SeedDoctorsAsync(db);

        // An admin already invited the first doctor — the fixture must not clobber that invite.
        Guid invitedId;
        using (var context = db.CreateContext())
        {
            var doctor = await context.Doctors.OrderBy(d => d.FullName).FirstAsync();
            invitedId = doctor.Id;
            doctor.Email = "real.doctor@clinic.example";
            doctor.AccountStatus = DoctorAccountStatus.Invited;
            doctor.InviteTokenHash = "existing-invite-hash";
            await context.SaveChangesAsync();
        }

        using (var context = db.CreateContext())
        {
            await DevSeeder.SeedAsync(context);
        }

        using var verify = db.CreateContext();
        var invited = await verify.Doctors.SingleAsync(d => d.Id == invitedId);
        Assert.Equal("real.doctor@clinic.example", invited.Email);
        Assert.Equal(DoctorAccountStatus.Invited, invited.AccountStatus);
        Assert.Equal("existing-invite-hash", invited.InviteTokenHash);

        // The history fixture itself is unaffected by the login step.
        Assert.Equal(3, await verify.Bookings.CountAsync(b => b.PatientEmail == DevSeeder.HistoryPatientEmail));
    }
}
