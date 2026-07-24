using ClinicSite.Application.DTOs.Admin;
using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Enums;
using Microsoft.EntityFrameworkCore;

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
                    .ThenInclude(d => d.Specialty)
            .OrderBy(b => b.AppointmentSlot.Doctor.Specialty.Name)
                .ThenBy(b => b.AppointmentSlot.Doctor.FullName)
                .ThenByDescending(b => b.AppointmentSlot.StartTimeUtc)
            .Select(b => new AdminBookingDto
            {
                BookingId = b.Id,
                AppointmentSlotId = b.AppointmentSlotId,
                PatientName = b.PatientName,
                PatientEmail = b.PatientEmail,
                Comment = b.Comment,
                DoctorId = b.AppointmentSlot.DoctorId,
                DoctorName = b.AppointmentSlot.Doctor.FullName,
                SpecialtyId = b.AppointmentSlot.Doctor.SpecialtyId,
                SpecialtyName = b.AppointmentSlot.Doctor.Specialty.Name,
                StartTimeUtc = b.AppointmentSlot.StartTimeUtc,
                EndTimeUtc = b.AppointmentSlot.EndTimeUtc,
                Status = b.Status.ToString(),
                IsCancelled = b.Status == BookingStatus.Cancelled,
                CreatedAtUtc = b.CreatedAtUtc
            })
            .ToListAsync();
    }

    public async Task CancelAsync(Guid bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null)
        {
            throw new NotFoundException("Booking not found.");
        }
        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new ConflictException("Booking is already cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = DateTime.UtcNow;

        var slot = await _context.AppointmentSlots.FindAsync(booking.AppointmentSlotId);
        if (slot == null)
        {
            throw new NotFoundException("Appointment slot not found.");
        }

        slot.Status = SlotStatus.Free;
        slot.ReservedUntilUtc = null;
        slot.ReservationToken = null;

        await _context.SaveChangesAsync();
    }
}
