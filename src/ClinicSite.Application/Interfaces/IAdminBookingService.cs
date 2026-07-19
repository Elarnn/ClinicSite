using ClinicSite.Application.DTOs.Admin;

namespace ClinicSite.Application.Interfaces;

public interface IAdminBookingService
{
    Task<List<AdminBookingDto>> GetAllAsync();

    Task CancelAsync(Guid bookingId);
}