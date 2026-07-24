using ClinicSite.Domain.Common;
using ClinicSite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Domain.Entities
{
    public class AppointmentSlot : BaseEntity
    {
        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; set; } = null!;

        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }

        public SlotStatus Status { get; set; }

        public DateTime? ReservedUntilUtc { get; set; }

        public string? ReservationToken { get; set; }

        // A slot may accumulate several bookings over its lifetime (e.g. one that expired or was
        // cancelled, then a new active one). At most one is active (Pending/Confirmed) at a time.
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
