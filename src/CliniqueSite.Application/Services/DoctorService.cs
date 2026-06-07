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
    }
}
