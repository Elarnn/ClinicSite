using CliniqueSite.Application.DTOs.Appointments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliniqueSite.Application.Interfaces
{
    public interface IAppointmentSlotService
    {
        Task<List<AppointmentSlotDto>> GetFreeByDoctorAsync(Guid doctorId);
        Task<ReserveSlotResultDto> ReserveSlotAsync(Guid slotId);
    }
}
