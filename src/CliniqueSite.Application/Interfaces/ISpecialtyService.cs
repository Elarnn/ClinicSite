using CliniqueSite.Application.DTOs.Specialties;

namespace CliniqueSite.Application.Interfaces
{
    public interface ISpecialtyService
    {
        Task<List<SpecialtyDto>> GetSpecialtiesAsync();
    }
}
