using ClinicSite.Application.DTOs.Doctors;

namespace ClinicSite.Application.Interfaces;

/// <summary>Read access to the bookings that belong to a single doctor's slots.</summary>
public interface IDoctorBookingService
{
    Task<List<DoctorBookingDto>> GetByDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default);
}
