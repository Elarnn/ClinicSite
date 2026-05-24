using CliniqueSite.Application.DTOs.Doctors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliniqueSite.Application.Interfaces
{
    public interface IDoctorService
    {
        Task<List<DoctorDto>> GetDoctorsAsync();
    }
}
