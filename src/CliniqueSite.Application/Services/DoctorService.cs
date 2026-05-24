using CliniqueSite.Application.DTOs.Doctors;
using CliniqueSite.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliniqueSite.Application.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IApplicationDbContext _context;

        public DoctorService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<DoctorDto>> GetDoctorsAsync()
        {
            var doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    FullName = d.FullName,
                    SpecialtyName = d.Specialty.Name
                })
                .ToListAsync();
            return doctors;
        }
    }
}
