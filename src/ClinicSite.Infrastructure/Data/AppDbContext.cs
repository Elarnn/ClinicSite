using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Entities;
using ClinicSite.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Infrastructure.Data;

public class AppDbContext : DbContext, IApplicationDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Specialty> Specialties { get; set; }
    public DbSet<AppointmentSlot> AppointmentSlots { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // One slot -> many bookings over time (only one active at once). This lets a slot be booked
        // again after a previous booking expires or is cancelled, instead of a one-to-one that would
        // permanently block the slot once any booking row exists.
        modelBuilder.Entity<Booking>()
            .HasOne(booking => booking.AppointmentSlot)
            .WithMany(slot => slot.Bookings)
            .HasForeignKey(booking => booking.AppointmentSlotId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Booking>(booking =>
        {
            booking.Property(b => b.Status)
                .HasConversion<int>();

            booking.Property(b => b.AppointmentStatus)
                .HasConversion<int>();

            booking.Property(b => b.DoctorNote).HasMaxLength(2000);

            booking.Property(b => b.ConfirmationTokenHash).HasMaxLength(128);
            booking.Property(b => b.CancellationTokenHash).HasMaxLength(128);

            // Token lookups go through these indexes.
            booking.HasIndex(b => b.ConfirmationTokenHash);
            booking.HasIndex(b => b.CancellationTokenHash);

            // Optimistic concurrency: a real SQL Server rowversion in production. Providers that don't
            // support it (SQLite in tests) simply ignore the column.
            if (Database.IsSqlServer())
            {
                booking.Property(b => b.RowVersion).IsRowVersion();
            }
            else
            {
                booking.Ignore(b => b.RowVersion);
            }
        });

        modelBuilder.Entity<Doctor>(doctor =>
        {
            doctor.Property(d => d.AccountStatus).HasConversion<int>();
            doctor.Property(d => d.PhotoContentType).HasMaxLength(100);
            doctor.Property(d => d.Email).HasMaxLength(256);
            doctor.Property(d => d.PasswordHash).HasMaxLength(256);
            doctor.Property(d => d.InviteTokenHash).HasMaxLength(128);

            // At most one account per email. NULL emails (doctors without an account) are exempt: on
            // SQL Server a filtered unique index allows many NULLs; other providers (SQLite in tests)
            // treat NULLs as distinct already, so the filter is only needed on SQL Server.
            var emailIndex = doctor.HasIndex(d => d.Email).IsUnique();
            if (Database.IsSqlServer())
            {
                emailIndex.HasFilter("[Email] IS NOT NULL");
            }

            doctor.HasIndex(d => d.InviteTokenHash);
        });

        modelBuilder.ApplyConfiguration(new SpecialtyConfiguration());
    }
}
