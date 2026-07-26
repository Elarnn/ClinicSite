using ClinicSite.Domain.Enums;

namespace ClinicSite.Application.DTOs.Doctors;

public class UpdateBookingStatusDto
{
    public AppointmentStatus Status { get; set; }
}
