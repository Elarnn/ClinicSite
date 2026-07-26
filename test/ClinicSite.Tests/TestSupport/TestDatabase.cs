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

    public DoctorBookingService CreateDoctorBookingService(AppDbContext context, IEmailService email) =>
        new DoctorBookingService(context, email);

    public DoctorSlotService CreateDoctorSlotService(AppDbContext context) =>
        new DoctorSlotService(context);

    /// <summary>Seeds a doctor with a booked slot + a booking, and returns the ids.</summary>
    public (Guid DoctorId, Guid SlotId, Guid BookingId) SeedBooking(
        string patientEmail = "patient@example.com",
        string patientName = "Pat Patient",
        DateTime? startTimeUtc = null,
        BookingStatus bookingStatus = BookingStatus.Confirmed,
        AppointmentStatus appointmentStatus = AppointmentStatus.Scheduled,
        string doctorName = "Dr. Smith",
        string? patientComment = null)
    {
        using var context = CreateContext();

        var start = startTimeUtc ?? DateTime.UtcNow.AddDays(1);
        var doctor = new Doctor { FullName = doctorName, Specialty = GetOrCreateSpecialty(context) };
        var slot = new AppointmentSlot
        {
            Doctor = doctor,
            StartTimeUtc = start,
            EndTimeUtc = start.AddMinutes(30),
            Status = SlotStatus.Booked
        };
        var booking = new Booking
        {
            AppointmentSlot = slot,
            PatientName = patientName,
            PatientEmail = patientEmail,
            Comment = patientComment,
            Status = bookingStatus,
            AppointmentStatus = appointmentStatus
        };

        context.AppointmentSlots.Add(slot);
        context.Bookings.Add(booking);
        context.SaveChanges();

        return (doctor.Id, slot.Id, booking.Id);
    }

    /// <summary>Seeds a doctor with a single free slot and returns the ids.</summary>
    public (Guid DoctorId, Guid SlotId) SeedFreeSlot(DateTime? startTimeUtc = null)
    {
        using var context = CreateContext();

        var start = startTimeUtc ?? DateTime.UtcNow.AddDays(1);
        var doctor = new Doctor { FullName = "Dr. Smith", Specialty = GetOrCreateSpecialty(context) };
        var slot = new AppointmentSlot
        {
            Doctor = doctor,
            StartTimeUtc = start,
            EndTimeUtc = start.AddMinutes(30),
            Status = SlotStatus.Free
        };

        context.AppointmentSlots.Add(slot);
        context.SaveChanges();

        return (doctor.Id, slot.Id);
    }

    /// <summary>Creates a doctor (with their own specialty) and returns the id — for multi-doctor scenarios.</summary>
    public Guid SeedDoctorWithSpecialty(string doctorName, string specialtyName)
    {
        using var context = CreateContext();
        var doctor = new Doctor { FullName = doctorName, Specialty = GetOrCreateSpecialty(context, specialtyName) };
        context.Doctors.Add(doctor);
        context.SaveChanges();
        return doctor.Id;
    }

    /// <summary>Adds a booked slot + booking to an existing doctor and returns the booking id.</summary>
    public Guid AddBooking(
        Guid doctorId,
        DateTime startTimeUtc,
        string patientEmail,
        string patientName = "Pat Patient",
        BookingStatus bookingStatus = BookingStatus.Confirmed,
        AppointmentStatus appointmentStatus = AppointmentStatus.Scheduled)
    {
        using var context = CreateContext();

        var slot = new AppointmentSlot
        {
            DoctorId = doctorId,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = startTimeUtc.AddMinutes(30),
            Status = SlotStatus.Booked
        };
        var booking = new Booking
        {
            AppointmentSlot = slot,
            PatientName = patientName,
            PatientEmail = patientEmail,
            Status = bookingStatus,
            AppointmentStatus = appointmentStatus
        };

        context.AppointmentSlots.Add(slot);
        context.Bookings.Add(booking);
        context.SaveChanges();

        return booking.Id;
    }

    /// <summary>Adds another free slot to an existing doctor at the given time and returns its id.</summary>
    public Guid AddFreeSlot(Guid doctorId, DateTime startTimeUtc)
    {
        using var context = CreateContext();
        var slot = new AppointmentSlot
        {
            DoctorId = doctorId,
            StartTimeUtc = startTimeUtc,
            EndTimeUtc = startTimeUtc.AddMinutes(30),
            Status = SlotStatus.Free
        };
        context.AppointmentSlots.Add(slot);
        context.SaveChanges();
        return slot.Id;
    }

    // Reuses the seeded specialty when present so multiple seeds don't collide on the unique name index.
    private static Specialty GetOrCreateSpecialty(AppDbContext context, string name = "Cardiology") =>
        context.Specialties.FirstOrDefault(s => s.Name == name) ?? new Specialty { Name = name };

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
