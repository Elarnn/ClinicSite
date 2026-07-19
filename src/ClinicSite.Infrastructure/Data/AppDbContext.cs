using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Entities;
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

        modelBuilder.Entity<AppointmentSlot>()
            .HasOne(slot => slot.Booking)
            .WithOne(booking => booking.AppointmentSlot)
            .HasForeignKey<Booking>(booking => booking.AppointmentSlotId);
    }
}