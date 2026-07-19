using ClinicSite.Application.DTOs.Admin;
using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace ClinicSite.Application.Services;

public class AdminBookingService : IAdminBookingService
{
    private readonly IApplicationDbContext _context;

    public AdminBookingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminBookingDto>> GetAllAsync()
    {
        return await _context.Bookings
            .Include(b => b.AppointmentSlot)
                .ThenInclude(s => s.Doctor)
            .OrderByDescending(b => b.AppointmentSlot.StartTimeUtc)
            .Select(b => new AdminBookingDto
            {
                BookingId = b.Id,
                AppointmentSlotId = b.AppointmentSlotId,
                PatientName = b.PatientName,
                PatientEmail = b.PatientEmail,
                Comment = b.Comment,
                DoctorName = b.AppointmentSlot.Doctor.FullName,
                StartTimeUtc = b.AppointmentSlot.StartTimeUtc,
                EndTimeUtc = b.AppointmentSlot.EndTimeUtc,
                IsCancelled = b.IsCancelled,
                CreatedAtUtc = b.CreatedAtUtc
            })
            .ToListAsync();
    }

    public async Task CancelAsync(Guid bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
        {
            throw new InvalidOperationException("Booking not found.");
        }
        if (booking.IsCancelled)
        {
            throw new InvalidOperationException("Booking is already cancelled.");
        }


        booking.IsCancelled = true;
        booking.CancelledAtUtc = DateTime.UtcNow;

        var slot = await _context.AppointmentSlots.FindAsync(booking.AppointmentSlotId);

        if (slot == null)
        {
            throw new InvalidOperationException("Appointment slot not found.");
        }

        slot.Status = SlotStatus.Free;
        slot.ReservedUntilUtc = null;
        slot.ReservationToken = null;

        await _context.SaveChangesAsync();
        
    }
}