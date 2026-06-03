using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliniqueSite.Application.DTOs.Appointments
{
    public class ReserveSlotResultDto
    {
        public Guid SlotId { get; set; }
        public DateTime? ReservedUntilUtc { get; set; }

        public string? ReservationToken { get; set; }
    }
}
