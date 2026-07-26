using ClinicSite.Application.Common;
using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicSite.Application.Services;

public class DoctorBookingService : IDoctorBookingService
{
    private const int MaxNoteLength = 2000;
    private const int MaxSubjectLength = 200;
    private const int MaxMessageLength = 4000;

    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public DoctorBookingService(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    // A booking counts as a real appointment on the doctor's schedule while its confirmation-lifecycle
    // status is one of these (Expired / patient-Cancelled bookings freed their slot and are not shown).
    private static bool IsLive(BookingStatus s) =>
        s == BookingStatus.Confirmed || s == BookingStatus.PendingConfirmation;

    private static int MinutesOfDay(DateTime utc) => utc.Hour * 60 + utc.Minute;

    public async Task<PagedResult<DoctorBookingDto>> GetBookingsAsync(Guid doctorId, DoctorBookingFilterDto filter, CancellationToken cancellationToken = default)
    {
        var query = _context.Bookings.AsNoTracking()
            .Where(b => b.AppointmentSlot.DoctorId == doctorId
                && (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.PendingConfirmation));

        if (filter.From.HasValue)
        {
            query = query.Where(b => b.AppointmentSlot.StartTimeUtc >= filter.From.Value);
        }
        if (filter.To.HasValue)
        {
            query = query.Where(b => b.AppointmentSlot.StartTimeUtc <= filter.To.Value);
        }
        if (filter.Status.HasValue)
        {
            query = query.Where(b => b.AppointmentStatus == filter.Status.Value);
        }
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(b => b.PatientName.ToLower().Contains(term) || b.PatientEmail.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = Math.Clamp(filter.PageSize, 1, 100);

        var items = await query
            .OrderByDescending(b => b.AppointmentSlot.StartTimeUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new DoctorBookingDto
            {
                BookingId = b.Id,
                AppointmentSlotId = b.AppointmentSlotId,
                PatientName = b.PatientName,
                PatientEmail = b.PatientEmail,
                PatientComment = b.Comment,
                DoctorNote = b.DoctorNote,
                StartTimeUtc = b.AppointmentSlot.StartTimeUtc,
                EndTimeUtc = b.AppointmentSlot.EndTimeUtc,
                Status = b.AppointmentStatus
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<DoctorBookingDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<List<DoctorScheduleItemDto>> GetScheduleAsync(Guid doctorId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        // Times-of-day that are blocked on a future day → a daily/recurring block is active for them.
        var now = DateTime.UtcNow;
        var futureBlockedTimes = await _context.AppointmentSlots.AsNoTracking()
            .Where(s => s.DoctorId == doctorId && s.Status == SlotStatus.Blocked && s.StartTimeUtc > now)
            .Select(s => s.StartTimeUtc)
            .ToListAsync(cancellationToken);
        var recurringBlockedMinutes = futureBlockedTimes.Select(MinutesOfDay).ToHashSet();

        var rows = await _context.AppointmentSlots.AsNoTracking()
            .Where(s => s.DoctorId == doctorId && s.StartTimeUtc >= fromUtc && s.StartTimeUtc < toUtc)
            .OrderBy(s => s.StartTimeUtc)
            .Select(s => new
            {
                s.Id,
                s.StartTimeUtc,
                s.EndTimeUtc,
                s.Status,
                Booking = s.Bookings
                    .Where(b => b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.PendingConfirmation)
                    .OrderByDescending(b => b.CreatedAtUtc)
                    .Select(b => new
                    {
                        b.Id,
                        b.PatientName,
                        b.PatientEmail,
                        b.Comment,
                        b.DoctorNote,
                        b.AppointmentStatus
                    })
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        return rows.Select(r => new DoctorScheduleItemDto
        {
            SlotId = r.Id,
            StartTimeUtc = r.StartTimeUtc,
            EndTimeUtc = r.EndTimeUtc,
            SlotStatus = r.Status.ToString(),
            RecurringBlocked = recurringBlockedMinutes.Contains(MinutesOfDay(r.StartTimeUtc)),
            BookingId = r.Booking?.Id,
            PatientName = r.Booking?.PatientName,
            PatientEmail = r.Booking?.PatientEmail,
            PatientComment = r.Booking?.Comment,
            DoctorNote = r.Booking?.DoctorNote,
            Status = r.Booking?.AppointmentStatus
        }).ToList();
    }

    public async Task<DoctorDashboardDto> GetDashboardAsync(Guid doctorId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var dayStart = now.Date;               // today, in UTC
        var dayEnd = dayStart.AddDays(1);

        var today = await GetScheduleAsync(doctorId, dayStart, dayEnd, cancellationToken);

        var booked = today.Where(i => i.BookingId is not null).ToList();

        var remaining = booked
            .Where(i => i.StartTimeUtc >= now
                && i.Status != AppointmentStatus.Completed
                && i.Status != AppointmentStatus.NoShow
                && i.Status != AppointmentStatus.Cancelled)
            .OrderBy(i => i.StartTimeUtc)
            .ToList();

        return new DoctorDashboardDto
        {
            TodayCount = booked.Count,
            RemainingCount = remaining.Count,
            NextPatientStartUtc = remaining.FirstOrDefault()?.StartTimeUtc,
            FreeWindowsCount = today.Count(i => i.SlotStatus == nameof(SlotStatus.Free) && i.StartTimeUtc >= now),
            Today = today
        };
    }

    public async Task UpdateStatusAsync(Guid doctorId, Guid bookingId, AppointmentStatus status, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ValidationException("Invalid appointment status.");
        }

        var booking = await LoadOwnedBookingAsync(doctorId, bookingId, cancellationToken);
        booking.AppointmentStatus = status;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> UpdateNoteAsync(Guid doctorId, Guid bookingId, string? note, CancellationToken cancellationToken = default)
    {
        if (note is not null && note.Length > MaxNoteLength)
        {
            throw new ValidationException($"Note must be at most {MaxNoteLength} characters.");
        }

        var booking = await LoadOwnedBookingAsync(doctorId, bookingId, cancellationToken);
        var normalized = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        booking.DoctorNote = normalized;
        await _context.SaveChangesAsync(cancellationToken);
        return normalized;
    }

    public async Task<List<PatientHistoryItemDto>> GetPatientHistoryAsync(Guid doctorId, Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await LoadOwnedBookingAsync(doctorId, bookingId, cancellationToken);
        var email = booking.PatientEmail.Trim().ToLower();

        return await _context.Bookings.AsNoTracking()
            .Where(b => b.Id != booking.Id
                && b.Status != BookingStatus.Expired
                && b.PatientEmail.ToLower() == email)
            .OrderByDescending(b => b.AppointmentSlot.StartTimeUtc)
            .Select(b => new PatientHistoryItemDto
            {
                StartTimeUtc = b.AppointmentSlot.StartTimeUtc,
                EndTimeUtc = b.AppointmentSlot.EndTimeUtc,
                DoctorName = b.AppointmentSlot.Doctor.FullName,
                SpecialtyName = b.AppointmentSlot.Doctor.Specialty.Name,
                Status = b.AppointmentStatus
            })
            .ToListAsync(cancellationToken);
    }

    public async Task SendMessageAsync(Guid doctorId, Guid bookingId, string subject, string message, CancellationToken cancellationToken = default)
    {
        var trimmedSubject = (subject ?? string.Empty).Trim();
        var trimmedMessage = (message ?? string.Empty).Trim();

        if (trimmedSubject.Length == 0 || trimmedSubject.Length > MaxSubjectLength)
        {
            throw new ValidationException($"Subject must be 1–{MaxSubjectLength} characters.");
        }
        if (trimmedMessage.Length == 0 || trimmedMessage.Length > MaxMessageLength)
        {
            throw new ValidationException($"Message must be 1–{MaxMessageLength} characters.");
        }

        var booking = await LoadOwnedBookingAsync(doctorId, bookingId, cancellationToken);

        // Recipient is always the booking's own e-mail — never anything from the client.
        await _emailService.SendPatientMessageAsync(booking.PatientEmail, booking.PatientName, trimmedSubject, trimmedMessage, cancellationToken);
    }

    // --- helpers -----------------------------------------------------------------------------

    /// <summary>Loads a booking and enforces that its slot belongs to the given doctor (else 404).</summary>
    private async Task<Booking> LoadOwnedBookingAsync(Guid doctorId, Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await _context.Bookings
            .Include(b => b.AppointmentSlot)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        if (booking is null || booking.AppointmentSlot.DoctorId != doctorId)
        {
            // Same response whether it doesn't exist or isn't theirs — no ownership leak.
            throw new NotFoundException("Booking not found.");
        }

        return booking;
    }
}
