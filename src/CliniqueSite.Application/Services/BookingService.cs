using CliniqueSite.Application.DTOs.Bookings;
using CliniqueSite.Application.Interfaces;
using CliniqueSite.Domain.Entities;
using CliniqueSite.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliniqueSite.Application.Services;

public class BookingService : IBookingService
{
    private readonly IApplicationDbContext _context;
    public BookingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResultDto> CreateBookingAsync(CreateBookingDto dto)
    {
        var slot = await _context.AppointmentSlots
           .Include(s => s.Doctor)
           .Include(s => s.Booking)
           .FirstOrDefaultAsync(s => s.Id == dto.AppointmentSlotId);

        if (slot == null)
        {
            throw new InvalidOperationException("Appointment slot not found.");
        }

        if (slot.Status != SlotStatus.Reserved)
        {
            throw new InvalidOperationException("The appointment slot is not available for booking.");
        }
        if (!slot.ReservedUntilUtc.HasValue || slot.ReservedUntilUtc.Value <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("The reservation for this appointment slot has expired.");
        }
        if (slot.Booking != null)
        {
            throw new InvalidOperationException("The appointment slot already has a booking.");
        }
        if (slot.ReservationToken != dto.ReservationToken)
        {
            throw new InvalidOperationException("Invalid reservation token.");
        }

        var booking = new Booking
        {
            AppointmentSlotId = slot.Id,
            PatientName = dto.PatientName,
            PatientEmail = dto.PatientEmail,
            Comment = dto.Comment,
            IsCancelled = false
        };

        _context.Bookings.Add(booking);

        slot.Booking = booking;
        slot.Status = SlotStatus.Booked;
        slot.ReservationToken = null;
        slot.ReservedUntilUtc = null;

        await _context.SaveChangesAsync();

        return new BookingResultDto
        {
            BookingId = booking.Id,
            AppointmentSlotId = slot.Id,
            PatientName = booking.PatientName,
            PatientEmail = booking.PatientEmail,
            Comment = booking.Comment,
            DoctorName = slot.Doctor.FullName,
            StartTimeUtc = slot.StartTimeUtc,
            EndTimeUtc = slot.EndTimeUtc,
            CreatedAtUtc = booking.CreatedAtUtc
        };
    }
}