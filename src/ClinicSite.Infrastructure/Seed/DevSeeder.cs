using ClinicSite.Application.Security;
using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using ClinicSite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Infrastructure.Seed;

/// <summary>
/// Fixtures for manual testing that must never reach a real database. Call sites are responsible for
/// invoking this only when the host environment is Development (see Program.cs).
///
/// Right now it seeds one patient with a visit history spread over several dates and doctors, so the
/// "Patient history" panel in the doctor portal has something to show, plus an activated login for the
/// doctor who owns the upcoming visit.
///
/// Idempotent: the patient's bookings are keyed off <see cref="HistoryPatientEmail"/> and are created
/// once. On later runs the seeder only re-anchors the upcoming visit to today so the fixture stays
/// usable as days pass; it never inserts a second copy.
/// </summary>
public static class DevSeeder
{
    public const string HistoryPatientEmail = "history.patient@clinicsite.local";
    private const string HistoryPatientName = "History Patient";

    /// <summary>Login for the doctor who owns the upcoming visit. Development-only credentials.</summary>
    public const string DemoDoctorEmail = "demo.doctor@clinicsite.local";
    public const string DemoDoctorPassword = "DevPassw0rd!";

    private const int UpcomingVisitHourUtc = 10;

    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        var doctors = await context.Doctors
            .OrderBy(d => d.FullName)
            .Take(2)
            .ToListAsync(cancellationToken);

        if (doctors.Count == 0)
        {
            return; // DbSeeder has not produced any doctors — nothing to attach a history to.
        }

        var primary = doctors[0];
        var secondary = doctors.Count > 1 ? doctors[1] : primary;

        await SeedDemoDoctorLoginAsync(context, primary, cancellationToken);
        await SeedPatientHistoryAsync(context, primary, secondary, cancellationToken);
    }

    /// <summary>Gives the primary doctor an active account so the portal can be logged into locally.</summary>
    private static async Task SeedDemoDoctorLoginAsync(AppDbContext context, Doctor doctor, CancellationToken cancellationToken)
    {
        // Only ever bind a doctor who has no account at all: an already-active login stays as it is,
        // and an in-flight invite must not have its token clobbered by a fixture.
        if (doctor.AccountStatus != DoctorAccountStatus.None || doctor.Email is not null)
        {
            return;
        }

        // Skip rather than collide if the email is already bound to some other doctor (unique index).
        if (await context.Doctors.AnyAsync(d => d.Email == DemoDoctorEmail, cancellationToken))
        {
            return;
        }

        doctor.Email = DemoDoctorEmail;
        doctor.PasswordHash = PasswordHasher.Hash(DemoDoctorPassword);
        doctor.AccountStatus = DoctorAccountStatus.Active;
        doctor.InviteTokenHash = null;
        doctor.InviteTokenExpiresAtUtc = null;

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedPatientHistoryAsync(
        AppDbContext context,
        Doctor primary,
        Doctor secondary,
        CancellationToken cancellationToken)
    {
        var existing = await context.Bookings
            .Include(b => b.AppointmentSlot)
            .Where(b => b.PatientEmail == HistoryPatientEmail)
            .ToListAsync(cancellationToken);

        if (existing.Count > 0)
        {
            await ReanchorUpcomingVisitAsync(context, existing, cancellationToken);
            return;
        }

        var today = DateTime.UtcNow.Date;

        // Three visits on distinct dates: two closed ones in the past and one still ahead. The
        // upcoming visit belongs to the primary doctor, so logging in as them shows it on the
        // dashboard, and opening it reveals the other two in the history panel.
        var visits = new (Doctor Doctor, DateTime StartUtc, AppointmentStatus Status)[]
        {
            (primary,   today.AddDays(-30).AddHours(9),  AppointmentStatus.Completed),
            (secondary, today.AddDays(-10).AddHours(14), AppointmentStatus.NoShow),
            (primary,   today.AddHours(UpcomingVisitHourUtc), AppointmentStatus.Scheduled)
        };

        foreach (var (doctor, startUtc, status) in visits)
        {
            var slot = new AppointmentSlot
            {
                DoctorId = doctor.Id,
                StartTimeUtc = startUtc,
                EndTimeUtc = startUtc.AddMinutes(30),
                Status = SlotStatus.Booked
            };
            var booking = new Booking
            {
                AppointmentSlot = slot,
                PatientName = HistoryPatientName,
                PatientEmail = HistoryPatientEmail,
                Comment = "Development fixture for the patient-history panel.",
                Status = BookingStatus.Confirmed,
                ConfirmedAtUtc = startUtc.AddDays(-1),
                AppointmentStatus = status
            };

            context.AppointmentSlots.Add(slot);
            context.Bookings.Add(booking);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Keeps the fixture's "upcoming" visit upcoming. Without this the seeded appointment silently
    /// slides into the past after the first day and disappears from the Today view.
    /// </summary>
    private static async Task ReanchorUpcomingVisitAsync(
        AppDbContext context,
        List<Booking> bookings,
        CancellationToken cancellationToken)
    {
        var upcoming = bookings
            .Where(b => b.AppointmentStatus == AppointmentStatus.Scheduled)
            .OrderByDescending(b => b.AppointmentSlot.StartTimeUtc)
            .FirstOrDefault();

        if (upcoming is null || upcoming.AppointmentSlot.StartTimeUtc >= DateTime.UtcNow.Date)
        {
            return;
        }

        var start = DateTime.UtcNow.Date.AddHours(UpcomingVisitHourUtc);
        upcoming.AppointmentSlot.StartTimeUtc = start;
        upcoming.AppointmentSlot.EndTimeUtc = start.AddMinutes(30);

        await context.SaveChangesAsync(cancellationToken);
    }
}
