using ClinicSite.Application.DTOs.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.Interfaces
{
    public interface IAppointmentSlotService
    {
        Task<List<AppointmentSlotDto>> GetFreeByDoctorAsync(Guid doctorId);
        Task<List<AppointmentSlotDto>> GetAllByDoctorAsync(Guid doctorId);
        Task<ReserveSlotResultDto> ReserveSlotAsync(Guid slotId);
    }
}
