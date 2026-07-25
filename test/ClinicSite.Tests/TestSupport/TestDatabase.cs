using ClinicSite.Application.Common;
using ClinicSite.Application.Interfaces;
using ClinicSite.Application.Services;
using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using ClinicSite.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ClinicSite.Tests.TestSupport;

/// <summary>
/// A real relational database for the service tests, backed by SQLite in-memory. SQLite (unlike the
/// EF in-memory provider) supports transactions and <c>ExecuteUpdate</c>, which BookingService relies
/// on. The connection is kept open for the life of the fixture so the schema survives between contexts.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        using var context = CreateContext();
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        return new AppDbContext(options);
    }

    public BookingService CreateBookingService(AppDbContext context, IEmailService email, int lifetimeMinutes = 30)
    {
        var options = Options.Create(new BookingOptions { ConfirmationLifetimeMinutes = lifetimeMinutes });
        return new BookingService(context, email, options, NullLogger<BookingService>.Instance);
    }

    public DoctorAccountService CreateDoctorAccountService(AppDbContext context, IEmailService email) =>
        new DoctorAccountService(context, email, NullLogger<DoctorAccountService>.Instance);

    /// <summary>Seeds a doctor with no account and returns its id.</summary>
    public Guid SeedDoctor(string fullName = "Dr. Smith")
    {
        using var context = CreateContext();
        var specialty = new Specialty { Name = "Cardiology" };
        var doctor = new Doctor { FullName = fullName, Specialty = specialty };
        context.Doctors.Add(doctor);
        context.SaveChanges();
        return doctor.Id;
    }

    public Doctor GetDoctor(Guid doctorId)
    {
        using var context = CreateContext();
        return context.Doctors.AsNoTracking().Single(d => d.Id == doctorId);
    }

    /// <summary>Forces the single doctor's invite window into the past.</summary>
    public void ExpireDoctorInvite(Guid doctorId)
    {
        using var context = CreateContext();
        var doctor = context.Doctors.Single(d => d.Id == doctorId);
        doctor.InviteTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        context.SaveChanges();
    }

    /// <summary>Seeds a single reserved slot ready to be booked and returns its id + reservation token.</summary>
    public (Guid SlotId, string ReservationToken) SeedReservedSlot(DateTime? startTimeUtc = null)
    {
        using var context = CreateContext();

        var start = startTimeUtc ?? DateTime.UtcNow.AddDays(1);
        var specialty = new Specialty { Name = "Cardiology" };
        var doctor = new Doctor { FullName = "Dr. Smith", Specialty = specialty };
        var slot = new AppointmentSlot
        {
            Doctor = doctor,
            StartTimeUtc = start,
            EndTimeUtc = start.AddMinutes(30),
            Status = SlotStatus.Reserved,
            ReservedUntilUtc = DateTime.UtcNow.AddMinutes(10),
            ReservationToken = "reservation-token"
        };

        context.AppointmentSlots.Add(slot);
        context.SaveChanges();

        return (slot.Id, slot.ReservationToken!);
    }

    public Booking GetBooking()
    {
        using var context = CreateContext();
        return context.Bookings.AsNoTracking().Single();
    }

    public AppointmentSlot GetSlot(Guid slotId)
    {
        using var context = CreateContext();
        return context.AppointmentSlots.AsNoTracking().Single(s => s.Id == slotId);
    }

    /// <summary>Puts a (freed) slot back into the Reserved state, as a fresh reservation would.</summary>
    public string ReReserveSlot(Guid slotId)
    {
        using var context = CreateContext();
        var slot = context.AppointmentSlots.Single(s => s.Id == slotId);
        slot.Status = SlotStatus.Reserved;
        slot.ReservationToken = "reservation-token-2";
        slot.ReservedUntilUtc = DateTime.UtcNow.AddMinutes(10);
        context.SaveChanges();
        return slot.ReservationToken;
    }

    /// <summary>Forces the single booking's confirmation window into the past.</summary>
    public void ExpireConfirmationWindow()
    {
        using var context = CreateContext();
        var booking = context.Bookings.Single();
        booking.ConfirmationExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1);
        context.SaveChanges();
    }

    public void Dispose() => _connection.Dispose();
}
