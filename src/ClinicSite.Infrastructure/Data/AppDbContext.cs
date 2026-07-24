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

        modelBuilder.ApplyConfiguration(new SpecialtyConfiguration());
    }
}
