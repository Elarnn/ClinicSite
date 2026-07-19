using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.DTOs.Bookings
{
    public class CreateBookingDto
    {
        public Guid AppointmentSlotId { get; set; }
        public string ReservationToken { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public string PatientEmail { get; set; } = string.Empty;

        public string? Comment { get; set; }
    }
}
