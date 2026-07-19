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
    }
}
