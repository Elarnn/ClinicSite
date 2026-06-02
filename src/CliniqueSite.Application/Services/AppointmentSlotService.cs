using CliniqueSite.Application.DTOs.Appointments;
using CliniqueSite.Application.Interfaces;
using CliniqueSite.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliniqueSite.Application.Services
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
                    Id = s.Id,
                    StartTimeUtc = s.StartTimeUtc,
                    EndTimeUtc = s.EndTimeUtc,
                    Status = s.Status
                })
                .OrderBy(s => s.StartTimeUtc)
                .ToListAsync();
        }
    }
}
