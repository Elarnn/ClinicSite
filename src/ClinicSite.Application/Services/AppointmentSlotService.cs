using ClinicSite.Application.DTOs.Appointments;
using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.Services
{
    public class AppointmentSlotService : IAppointmentSlotService
    {
        private readonly IApplicationDbContext _context;

        public AppointmentSlotService(IApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<AppointmentSlotDto>> GetFreeByDoctorAsync(Guid doctorId)
        {
            var now = DateTime.UtcNow;

            // Only slots that are still in the future — a past slot can't be booked.
            return await _context.AppointmentSlots
                .Where(s => s.DoctorId == doctorId && s.Status == SlotStatus.Free && s.StartTimeUtc > now)
                .Select(s => new AppointmentSlotDto
                {
                    SlotId = s.Id,
                    StartTimeUtc = s.StartTimeUtc,
                    EndTimeUtc = s.EndTimeUtc,
                    Status = s.Status
                })
                .OrderBy(s => s.StartTimeUtc)
                .ToListAsync();
        }

        public async Task<List<AppointmentSlotDto>> GetAllByDoctorAsync(Guid doctorId)
        {
            var now = DateTime.UtcNow;

            // The public time-picker calls this; hide slots whose start time has already passed.
            return await _context.AppointmentSlots
                .Where(s => s.DoctorId == doctorId && s.StartTimeUtc > now)
                .Select(s => new AppointmentSlotDto
                {
                    SlotId = s.Id,
                    StartTimeUtc = s.StartTimeUtc,
                    EndTimeUtc = s.EndTimeUtc,
                    Status = s.Status
                })
                .OrderBy(s => s.StartTimeUtc)
                .ToListAsync();
        }

        public async Task<ReserveSlotResultDto> ReserveSlotAsync(Guid slotId)
        {
            var now = DateTime.UtcNow;
            var reservationToken = Guid.NewGuid().ToString("N");
            var reservedUntil = now.AddMinutes(10);

            // Atomic conditional UPDATE: the WHERE clause and the write happen as a single
            // statement on the database, so two concurrent requests for the same slot can't
            // both read "Free" and both win — only one UPDATE can match the row at a time.
            var rowsAffected = await _context.AppointmentSlots
                .Where(s => s.Id == slotId && s.StartTimeUtc > now && (
                    s.Status == SlotStatus.Free ||
                    (s.Status == SlotStatus.Reserved && s.ReservedUntilUtc != null && s.ReservedUntilUtc <= now)))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.Status, SlotStatus.Reserved)
                    .SetProperty(s => s.ReservedUntilUtc, reservedUntil)
                    .SetProperty(s => s.ReservationToken, reservationToken));

            if (rowsAffected == 0)
            {
                var slotExists = await _context.AppointmentSlots.AnyAsync(s => s.Id == slotId);
                if (!slotExists)
                {
                    throw new InvalidOperationException("Slot is not available for reservation.");
                }

                throw new ConflictException("Slot is not available for reservation.");
            }

            return new ReserveSlotResultDto
            {
                SlotId = slotId,
                ReservedUntilUtc = reservedUntil,
                ReservationToken = reservationToken
            };
        }
    }
}
