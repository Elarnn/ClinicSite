using CliniqueSite.Application.DTOs.Specialties;
using CliniqueSite.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CliniqueSite.Application.Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly IApplicationDbContext _context;

        public SpecialtyService(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SpecialtyDto>> GetSpecialtiesAsync()
        {
            return await _context.Specialties
                .OrderBy(s => s.Name)
                .Select(s => new SpecialtyDto
                {
                    Id = s.Id,
                    Name = s.Name
                })
                .ToListAsync();
        }
    }
}
