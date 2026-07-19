using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.DTOs.Bookings
{
    public class BookingResultDto
    {
        public Guid BookingId { get; set; }
        public Guid AppointmentSlotId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        public string? Comment { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    
    }
}
