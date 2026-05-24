using CliniqueSite.Domain.Common;


namespace CliniqueSite.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public Guid AppointmentSlotId { get; set; }

        public AppointmentSlot AppointmentSlot { get; set; } = null!;

        public string PatientName { get; set; } = string.Empty;

        public string PatientEmail { get; set; } = string.Empty;

        public string? Comment { get; set; }

        public bool IsCancelled { get; set; }

        public DateTime? CancelledAtUtc { get; set; }
    }
}
