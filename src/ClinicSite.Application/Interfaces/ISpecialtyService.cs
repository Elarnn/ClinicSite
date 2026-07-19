using ClinicSite.Application.DTOs.Admin;
using ClinicSite.Application.DTOs.Specialties;

namespace ClinicSite.Application.Interfaces;

public interface ISpecialtyService
{
    Task<List<SpecialtyDto>> GetSpecialtiesAsync();

    Task<SpecialtyDto?> GetSpecialtyByIdAsync(Guid specialtyId);

    Task<SpecialtyDto> CreateSpecialtyAsync(
        CreateSpecialtyDto request);

    Task<SpecialtyDto?> UpdateSpecialtyAsync(
        Guid specialtyId,
        UpdateSpecialtyDto request);

    Task<bool> DeleteSpecialtyAsync(Guid specialtyId);
}