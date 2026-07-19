using ClinicSite.Application.DTOs.Admin;
using ClinicSite.Application.DTOs.Specialties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.Interfaces
{
    public interface IAdminSpecialtyService
    {
        Task<List<SpecialtyDto>> GetAllAsync();
        Task<SpecialtyDto> CreateAsync(CreateSpecialtyDto dto);
        Task<SpecialtyDto> UpdateAsync(Guid id, UpdateSpecialtyDto dto);
        Task DeleteAsync(Guid id);
    }
}
