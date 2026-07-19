using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.DTOs.Appointments;

    public class AppointmentSlotDto
    {
        public Guid SlotId { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        public SlotStatus Status { get; set; }
    }

