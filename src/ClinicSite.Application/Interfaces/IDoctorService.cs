using ClinicSite.Application.DTOs.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<List<DoctorDto>> GetDoctorsAsync(Guid? specialtyId = null);

        Task<List<DoctorDto>> GetAllDoctorsAsync();

        Task<DoctorDto> CreateDoctorAsync(CreateDoctorDto request);

        Task<DoctorDto?> UpdateDoctorAsync(Guid doctorId, UpdateDoctorDto request);

        Task<DoctorDto?> DeactivateDoctorAsync(Guid doctorId);

        Task<DoctorDto?> ActivateDoctorAsync(Guid doctorId);

        Task<bool> DeleteDoctorAsync(Guid doctorId);
    }
}
