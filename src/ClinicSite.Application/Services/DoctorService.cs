using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IApplicationDbContext _context;

        public DoctorService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DoctorDto>> GetDoctorsAsync(Guid? specialtyId = null)
        {
            var query = _context.Doctors.Where(d => d.IsActive);

            if (specialtyId.HasValue)
                query = query.Where(d => d.SpecialtyId == specialtyId.Value);

            return await query
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    SpecialtyName = d.Specialty.Name
                })
                .ToListAsync();
        }

        public async Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto request)
        {
            var fullName = request.FullName.Trim();

            if (string.IsNullOrEmpty(fullName))
            {
                throw new ValidationException("Имя врача не может быть пустым.");
            }

            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.Id == request.SpecialtyId);

            if (specialty is null)
            {
                throw new ValidationException("Указанная специальность не найдена.");
            }

            var doctor = new Doctor
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                SpecialtyId = specialty.Id,
                IsActive = true
            };

            _context.Doctors.Add(doctor);

            await _context.SaveChangesAsync();

            return new DoctorDto
            {
                Id = doctor.Id,
                FullName = doctor.FullName,
                SpecialtyName = specialty.Name
            };
        }
    }
}
