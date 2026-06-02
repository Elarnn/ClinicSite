using CliniqueSite.Domain.Entities;
using CliniqueSite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliniqueSite.Application.DTOs.Appointments;

    public class AppointmentSlotDto
    {
        public Guid Id { get; set; }
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        public SlotStatus Status { get; set; }
    }

