using ClinicSite.Application.DTOs.Admin;
using ClinicSite.Application.DTOs.Specialties;
using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Application.Services;

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
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .Select(s => new SpecialtyDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .ToListAsync();
    }

    public async Task<SpecialtyDto?> GetSpecialtyByIdAsync(Guid specialtyId)
    {
        return await _context.Specialties
            .AsNoTracking()
            .Where(s => s.Id == specialtyId)
            .Select(s => new SpecialtyDto
            {
                Id = s.Id,
                Name = s.Name
            })
            .FirstOrDefaultAsync();
    }

    public async Task<SpecialtyDto> CreateSpecialtyAsync(
        CreateSpecialtyDto request)
    {
        var normalizedName = request.Name.Trim();

        var alreadyExists = await _context.Specialties
            .AnyAsync(s => s.Name.ToLower() == normalizedName.ToLower());

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "Специальность с таким названием уже существует.");
        }

        var specialty = new Specialty
        {
            Id = Guid.NewGuid(),
            Name = normalizedName
        };

        _context.Specialties.Add(specialty);

        await _context.SaveChangesAsync();

        return new SpecialtyDto
        {
            Id = specialty.Id,
            Name = specialty.Name
        };
    }

    public async Task<SpecialtyDto?> UpdateSpecialtyAsync(
        Guid specialtyId,
        UpdateSpecialtyDto request)
    {
        var specialty = await _context.Specialties
            .FirstOrDefaultAsync(s => s.Id == specialtyId);

        if (specialty is null)
        {
            return null;
        }

        var normalizedName = request.Name.Trim();

        var alreadyExists = await _context.Specialties
            .AnyAsync(s =>
                s.Id != specialtyId &&
                s.Name.ToLower() == normalizedName.ToLower());

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "Специальность с таким названием уже существует.");
        }

        specialty.Name = normalizedName;

        await _context.SaveChangesAsync();

        return new SpecialtyDto
        {
            Id = specialty.Id,
            Name = specialty.Name
        };
    }

    public async Task<bool> DeleteSpecialtyAsync(Guid specialtyId)
    {
        var specialty = await _context.Specialties
            .FirstOrDefaultAsync(s => s.Id == specialtyId);

        if (specialty is null)
        {
            return false;
        }

        var hasDoctors = await _context.Doctors
            .AnyAsync(d => d.SpecialtyId == specialtyId);

        if (hasDoctors)
        {
            throw new InvalidOperationException(
                "Нельзя удалить специальность, к которой привязаны врачи.");
        }

        _context.Specialties.Remove(specialty);

        await _context.SaveChangesAsync();

        return true;
    }
}