using ClinicSite.Application.DTOs.Appointments;
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
            return await _context.AppointmentSlots
                .Where(s => s.DoctorId == doctorId && s.Status == SlotStatus.Free)
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
            return await _context.AppointmentSlots
                .Where(s => s.DoctorId == doctorId)
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

            var slot = await _context.AppointmentSlots.FindAsync(slotId);

            if (slot == null) throw new InvalidOperationException("Slot is not available for reservation.");

            if (slot.Status == SlotStatus.Reserved && slot.ReservedUntilUtc.HasValue && slot.ReservedUntilUtc.Value <= now)
            {
                slot.Status = SlotStatus.Free;
                slot.ReservedUntilUtc = null;
                slot.ReservationToken = null;
            }
            if (slot.Status != SlotStatus.Free) throw new InvalidOperationException("Slot is not available for reservation.");

            var reservationToken = Guid.NewGuid().ToString("N");

            var reservedUntil = now.AddMinutes(10);

            slot.Status = SlotStatus.Reserved;
            slot.ReservedUntilUtc = reservedUntil;
            slot.ReservationToken = reservationToken;

            await _context.SaveChangesAsync();

            return new ReserveSlotResultDto
            {
                SlotId = slot.Id,
                ReservedUntilUtc = reservedUntil,
                ReservationToken = reservationToken
            };
        }
    }
}
