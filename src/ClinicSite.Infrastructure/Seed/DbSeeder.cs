using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using ClinicSite.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Infrastructure.Seed;

public class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await SeedSpecialtiesAsync(context);
        await SeedDoctorsAsync(context);
        await SeedAppointmentsAsync(context);
    }
    public static async Task SeedSpecialtiesAsync(AppDbContext context)
    {
        if (!await context.Specialties.AnyAsync())
        {
            var specialties = new List<Specialty>
        {
            new Specialty { Name = "Cardiology" },
            new Specialty { Name = "Dermatology" },
            new Specialty { Name = "Pediatrics" }
        };

            context.Specialties.AddRange(specialties);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedDoctorsAsync(AppDbContext context)
    {
        if (!await context.Doctors.AnyAsync())
        {
            var cardiology = await context.Specialties
                .FirstOrDefaultAsync(s => s.Name == "Cardiology");

            var dermatology = await context.Specialties
                .FirstOrDefaultAsync(s => s.Name == "Dermatology");

            var pediatrics = await context.Specialties
                .FirstOrDefaultAsync(s => s.Name == "Pediatrics");

            if (cardiology == null || dermatology == null || pediatrics == null)
            {
                throw new Exception("Seed error: specialties were not found.");
            }

            var doctors = new List<Doctor>
        {
            new Doctor
            {
                FullName = "Dr. Smith",
                SpecialtyId = cardiology.Id,
                IsActive = true
            },
            new Doctor
            {
                FullName = "Dr. Johnson",
                SpecialtyId = dermatology.Id,
                IsActive = true
            },
            new Doctor
            {
                FullName = "Dr. Williams",
                SpecialtyId = pediatrics.Id,
                IsActive = true
            }
        };

            context.Doctors.AddRange(doctors);
            await context.SaveChangesAsync();
        }
    }

    public static async Task SeedAppointmentsAsync(AppDbContext context)
    {
        if (await context.AppointmentSlots.AnyAsync())
        {
            return;
        }

        var doctors = await context.Doctors.ToListAsync();
        if (doctors.Count == 0)
        {
            throw new Exception("Seed error: no doctors found to create appointment slots.");
        }

        var appointmentSlots = new List<AppointmentSlot>();

        foreach (var doctor in doctors)
        {
            for (int dayOffset = 0; dayOffset < 14; dayOffset++) // Create slots for the next 14 days
            {
                var startTime = DateTime.UtcNow.AddDays(dayOffset + 1).Date.AddHours(9); // Start at 9 AM tomorrow
                for (int i = 0; i < 16; i++) // Create 16 slots (9 AM to 5 PM)
                {
                    appointmentSlots.Add(new AppointmentSlot
                    {
                        DoctorId = doctor.Id,
                        StartTimeUtc = startTime.AddMinutes(i * 30),
                        EndTimeUtc = startTime.AddMinutes((i + 1) * 30),
                        Status = SlotStatus.Free
                    });
                }
            }
        }
        context.AppointmentSlots.AddRange(appointmentSlots);
        await context.SaveChangesAsync();
    }
}

