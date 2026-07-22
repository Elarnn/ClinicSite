using ClinicSite.Application.DTOs.Admin;
using ClinicSite.Application.DTOs.Specialties;
using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CliniqueSite.Application.Services;

public class AdminSpecialtyService : IAdminSpecialtyService
{
    private readonly IApplicationDbContext _context;

    public AdminSpecialtyService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<SpecialtyDto>> GetAllAsync()
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

    public async Task<SpecialtyDto> CreateAsync(CreateSpecialtyDto dto)
    {
        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Specialty name is required.");
        }

        var exists = await _context.Specialties
            .AnyAsync(s => s.Name.ToLower() == name.ToLower());

        if (exists)
        {
            throw new InvalidOperationException(
                "Specialty with this name already exists.");
        }

        var specialty = new Specialty
        {
            Name = name
        };

        _context.Specialties.Add(specialty);
        await _context.SaveChangesAsync();

        return new SpecialtyDto
        {
            Id = specialty.Id,
            Name = specialty.Name
        };
    }

    public async Task<SpecialtyDto> UpdateAsync(
        Guid id,
        UpdateSpecialtyDto dto)
    {
        var specialty = await _context.Specialties.FindAsync(id);

        if (specialty == null)
        {
            throw new InvalidOperationException("Specialty not found.");
        }

        var name = dto.Name.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException("Specialty name is required.");
        }

        var duplicateExists = await _context.Specialties
            .AnyAsync(s =>
                s.Id != id &&
                s.Name.ToLower() == name.ToLower());

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                "Specialty with this name already exists.");
        }

        specialty.Name = name;

        await _context.SaveChangesAsync();

        return new SpecialtyDto
        {
            Id = specialty.Id,
            Name = specialty.Name
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        var specialty = await _context.Specialties.FindAsync(id);

        if (specialty == null)
        {
            throw new InvalidOperationException("Specialty not found.");
        }

        var hasDoctors = await _context.Doctors
            .AnyAsync(d => d.SpecialtyId == id);

        if (hasDoctors)
        {
            throw new InvalidOperationException(
                "Cannot delete a specialty that has doctors.");
        }

        _context.Specialties.Remove(specialty);

        await _context.SaveChangesAsync();
    }
}