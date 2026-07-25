using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Application.Services;

public class DoctorBookingService : IDoctorBookingService
{
    private readonly IApplicationDbContext _context;

    public DoctorBookingService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<DoctorBookingDto>> GetByDoctorAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.AppointmentSlot.DoctorId == doctorId)
            .OrderByDescending(b => b.AppointmentSlot.StartTimeUtc)
            .Select(b => new DoctorBookingDto
            {
                PatientName = b.PatientName,
                PatientEmail = b.PatientEmail,
                Comment = b.Comment,
                StartTimeUtc = b.AppointmentSlot.StartTimeUtc,
                EndTimeUtc = b.AppointmentSlot.EndTimeUtc,
                Status = b.Status.ToString()
            })
            .ToListAsync(cancellationToken);
    }
}
