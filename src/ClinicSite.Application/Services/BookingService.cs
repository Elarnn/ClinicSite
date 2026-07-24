using ClinicSite.Application.Common;
using ClinicSite.Application.DTOs.Bookings;
using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Application.Notifications;
using ClinicSite.Application.Security;
using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicSite.Application.Services;

public class BookingService : IBookingService
{
    private const string InvalidLinkMessage = "This link is invalid or has expired.";
    private const string ExpiredMessage = "The confirmation window has expired. The slot has been released.";

    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly BookingOptions _options;
    private readonly ILogger<BookingService> _logger;

    public BookingService(
        IApplicationDbContext context,
        IEmailService emailService,
        IOptions<BookingOptions> options,
        ILogger<BookingService> logger)
    {
        _context = context;
        _emailService = emailService;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<BookingResultDto> CreateBookingAsync(CreateBookingDto dto, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        // Read-only lookup for validation and for the e-mail model. The slot is claimed with an
        // atomic conditional UPDATE below, so we don't need (and don't want) to track this instance.
        var slot = await _context.AppointmentSlots
            .AsNoTracking()
            .Include(s => s.Doctor)
                .ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(s => s.Id == dto.AppointmentSlotId, cancellationToken);

        if (slot is null)
        {
            throw new NotFoundException("Appointment slot not found.");
        }

        if (slot.Status != SlotStatus.Reserved)
        {
            throw new ConflictException("The appointment slot is not available for booking.");
        }
        if (!slot.ReservedUntilUtc.HasValue || slot.ReservedUntilUtc.Value <= now)
        {
            throw new ConflictException("The reservation for this appointment slot has expired.");
        }
        if (slot.ReservationToken != dto.ReservationToken)
        {
            throw new ConflictException("Invalid reservation token.");
        }

        // A slot can carry historical (expired / cancelled) bookings, but never two active ones.
        var hasActiveBooking = await _context.Bookings.AnyAsync(
            b => b.AppointmentSlotId == slot.Id
                && (b.Status == BookingStatus.PendingConfirmation || b.Status == BookingStatus.Confirmed),
            cancellationToken);

        if (hasActiveBooking)
        {
            throw new ConflictException("A booking already exists for this appointment slot.");
        }

        var (confirmationToken, confirmationHash) = BookingTokens.Create();
        var (cancellationToken2, cancellationHash) = BookingTokens.Create();

        var booking = new Booking
        {
            AppointmentSlotId = slot.Id,
            PatientName = dto.PatientName.Trim(),
            PatientEmail = dto.PatientEmail.Trim(),
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
            Status = BookingStatus.PendingConfirmation,
            ConfirmationTokenHash = confirmationHash,
            ConfirmationExpiresAtUtc = now.AddMinutes(_options.ConfirmationLifetimeMinutes),
            CancellationTokenHash = cancellationHash,
            ConfirmationEmailAttempts = 0
        };

        // Atomically claim the slot and insert the booking inside a short transaction. No network
        // call happens while the transaction is open.
        await using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
        {
            var rowsAffected = await _context.AppointmentSlots
                .Where(s => s.Id == slot.Id
                    && s.Status == SlotStatus.Reserved
                    && s.ReservationToken == dto.ReservationToken
                    && s.ReservedUntilUtc != null && s.ReservedUntilUtc > now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(s => s.Status, SlotStatus.Booked)
                    .SetProperty(s => s.ReservationToken, (string?)null)
                    .SetProperty(s => s.ReservedUntilUtc, (DateTime?)null), cancellationToken);

            if (rowsAffected == 0)
            {
                throw new ConflictException("The reservation for this appointment slot is no longer valid.");
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }

        var emailModel = ToEmailModel(booking, slot);

        // First e-mail is sent only AFTER the successful commit. If it fails we must not leave the
        // slot blocked for 30 minutes — compensate by expiring the booking and freeing the slot.
        try
        {
            await _emailService.SendConfirmationRequestAsync(emailModel, confirmationToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "First confirmation e-mail failed for booking {BookingId}; compensating (expire + free slot).",
                booking.Id);

            await CompensateFailedFirstEmailAsync(booking.Id, cancellationToken);

            throw new EmailDeliveryException(
                "Could not send the confirmation email. Please try again later.");
        }

        booking.ConfirmationEmailSentAtUtc = now;
        booking.LastConfirmationEmailSentAtUtc = now;
        booking.ConfirmationEmailAttempts += 1;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Booking {BookingId} created (PendingConfirmation); confirmation e-mail sent to {MaskedEmail}.",
            booking.Id, MaskEmail(booking.PatientEmail));

        return new BookingResultDto
        {
            BookingId = booking.Id,
            AppointmentSlotId = slot.Id,
            PatientName = booking.PatientName,
            PatientEmail = booking.PatientEmail,
            Comment = booking.Comment,
            DoctorName = slot.Doctor.FullName,
            StartTimeUtc = slot.StartTimeUtc,
            EndTimeUtc = slot.EndTimeUtc,
            CreatedAtUtc = booking.CreatedAtUtc,
            Status = booking.Status.ToString(),
            ConfirmationExpiresAtUtc = booking.ConfirmationExpiresAtUtc
        };
    }

    public async Task<ConfirmBookingResultDto> ConfirmBookingAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!BookingTokens.TryComputeHash(token, out var hash))
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        var booking = await LoadWithDetailsByConfirmationHashAsync(hash, cancellationToken);
        if (booking is null || !BookingTokens.HashesEqual(booking.ConfirmationTokenHash, hash))
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        var now = DateTime.UtcNow;

        switch (booking.Status)
        {
            case BookingStatus.Confirmed:
                // Idempotent: already confirmed (e.g. duplicate / concurrent request). No re-send.
                return ToConfirmResult(booking);

            case BookingStatus.Expired:
                throw new GoneException(ExpiredMessage);

            case BookingStatus.PendingConfirmation:
                break;

            default:
                // Cancelled / Completed / NoShow — the link is no longer usable.
                throw new NotFoundException(InvalidLinkMessage);
        }

        // An expired confirmation token must never confirm a booking — enforce the window here in
        // addition to the background sweep.
        if (booking.ConfirmationExpiresAtUtc is null || booking.ConfirmationExpiresAtUtc.Value <= now)
        {
            ExpireInMemory(booking);
            await SaveIgnoringConcurrencyAsync(cancellationToken);
            throw new GoneException(ExpiredMessage);
        }

        booking.Status = BookingStatus.Confirmed;
        booking.ConfirmedAtUtc = now;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // A concurrent confirm or the background sweep won the race. Re-resolve from the DB.
            return await ResolveConfirmConflictAsync(hash, cancellationToken);
        }

        _logger.LogInformation("Booking {BookingId} confirmed.", booking.Id);

        // Second e-mail is best-effort: a delivery failure must NOT roll back the confirmation.
        try
        {
            await _emailService.SendBookingConfirmedAsync(
                ToEmailModel(booking, booking.AppointmentSlot),
                await GetCancellationTokenForConfirmationAsync(booking),
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Confirmation succeeded for booking {BookingId} but the confirmed e-mail failed to send.",
                booking.Id);
        }

        return ToConfirmResult(booking);
    }

    public async Task<BookingSummaryDto> GetCancellableBookingAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!BookingTokens.TryComputeHash(token, out var hash))
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        var booking = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.AppointmentSlot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(b => b.CancellationTokenHash == hash, cancellationToken);

        if (booking is null || !BookingTokens.HashesEqual(booking.CancellationTokenHash, hash))
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        var slot = booking.AppointmentSlot;
        return new BookingSummaryDto
        {
            DoctorName = slot.Doctor.FullName,
            SpecialtyName = slot.Doctor.Specialty.Name,
            StartTimeUtc = slot.StartTimeUtc,
            EndTimeUtc = slot.EndTimeUtc,
            Status = booking.Status.ToString(),
            Cancellable = booking.Status == BookingStatus.Confirmed && slot.StartTimeUtc > DateTime.UtcNow
        };
    }

    public async Task<CancelBookingResultDto> CancelBookingAsync(string token, CancellationToken cancellationToken = default)
    {
        if (!BookingTokens.TryComputeHash(token, out var hash))
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        var booking = await _context.Bookings
            .Include(b => b.AppointmentSlot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(b => b.CancellationTokenHash == hash, cancellationToken);

        if (booking is null || !BookingTokens.HashesEqual(booking.CancellationTokenHash, hash))
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        var now = DateTime.UtcNow;

        if (booking.Status == BookingStatus.Cancelled)
        {
            // Idempotent: a second cancellation must not change state.
            return ToCancelResult(booking);
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new ConflictException("This booking cannot be cancelled.");
        }

        if (booking.AppointmentSlot.StartTimeUtc <= now)
        {
            throw new ConflictException("A booking cannot be cancelled after the appointment has started.");
        }

        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = now;
        FreeSlot(booking.AppointmentSlot);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await ResolveCancelConflictAsync(hash, cancellationToken);
        }

        _logger.LogInformation("Booking {BookingId} cancelled; slot freed.", booking.Id);

        try
        {
            await _emailService.SendBookingCancelledAsync(ToEmailModel(booking, booking.AppointmentSlot), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancellation succeeded for booking {BookingId} but the cancelled e-mail failed to send.", booking.Id);
        }

        return ToCancelResult(booking);
    }

    public async Task<int> ExpirePendingBookingsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var expired = await _context.Bookings
            .Include(b => b.AppointmentSlot)
            .Where(b => b.Status == BookingStatus.PendingConfirmation
                && b.ConfirmationExpiresAtUtc != null
                && b.ConfirmationExpiresAtUtc <= now)
            .ToListAsync(cancellationToken);

        var count = 0;
        foreach (var booking in expired)
        {
            try
            {
                ExpireInMemory(booking);
                await _context.SaveChangesAsync(cancellationToken);
                count++;
            }
            catch (DbUpdateConcurrencyException)
            {
                // The patient confirmed (or another sweep handled it) in the meantime. Skip this one.
                _context.Entry(booking).State = EntityState.Detached;
                _logger.LogInformation("Skipped concurrently-modified booking {BookingId} during expiration sweep.", booking.Id);
            }
            catch (Exception ex)
            {
                // One bad row must not abort the whole sweep.
                _context.Entry(booking).State = EntityState.Detached;
                _logger.LogError(ex, "Failed to expire booking {BookingId}.", booking.Id);
            }
        }

        return count;
    }

    // --- helpers -----------------------------------------------------------------------------

    private Task<Booking?> LoadWithDetailsByConfirmationHashAsync(string hash, CancellationToken ct) =>
        _context.Bookings
            .Include(b => b.AppointmentSlot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(b => b.ConfirmationTokenHash == hash, ct);

    private async Task CompensateFailedFirstEmailAsync(Guid bookingId, CancellationToken ct)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId, ct);
        if (booking is null)
        {
            return;
        }

        booking.Status = BookingStatus.Expired;

        // Free the slot with a direct conditional UPDATE so we never depend on stale tracked state.
        await _context.AppointmentSlots
            .Where(s => s.Id == booking.AppointmentSlotId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.Status, SlotStatus.Free)
                .SetProperty(s => s.ReservationToken, (string?)null)
                .SetProperty(s => s.ReservedUntilUtc, (DateTime?)null), ct);

        await _context.SaveChangesAsync(ct);
    }

    private async Task<ConfirmBookingResultDto> ResolveConfirmConflictAsync(string hash, CancellationToken ct)
    {
        var fresh = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.AppointmentSlot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(b => b.ConfirmationTokenHash == hash, ct);

        if (fresh is null)
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        return fresh.Status switch
        {
            BookingStatus.Confirmed => ToConfirmResult(fresh),
            BookingStatus.Expired => throw new GoneException(ExpiredMessage),
            _ => throw new NotFoundException(InvalidLinkMessage)
        };
    }

    private async Task<CancelBookingResultDto> ResolveCancelConflictAsync(string hash, CancellationToken ct)
    {
        var fresh = await _context.Bookings
            .AsNoTracking()
            .Include(b => b.AppointmentSlot).ThenInclude(s => s.Doctor).ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(b => b.CancellationTokenHash == hash, ct);

        if (fresh is null)
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        if (fresh.Status == BookingStatus.Cancelled)
        {
            return ToCancelResult(fresh);
        }

        throw new ConflictException("This booking cannot be cancelled.");
    }

    private async Task SaveIgnoringConcurrencyAsync(CancellationToken ct)
    {
        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Someone else already transitioned this booking — nothing more to do.
        }
    }

    // The cancellation token is only generated at creation time and never stored in plaintext, so we
    // cannot recover it here. When a booking is confirmed via the API we therefore mint a fresh
    // cancellation token, replacing the stored hash, and hand the raw value to the confirmed e-mail.
    private async Task<string> GetCancellationTokenForConfirmationAsync(Booking booking)
    {
        var (cancellationToken, cancellationHash) = BookingTokens.Create();
        booking.CancellationTokenHash = cancellationHash;
        await _context.SaveChangesAsync();
        return cancellationToken;
    }

    private static void ExpireInMemory(Booking booking)
    {
        booking.Status = BookingStatus.Expired;
        if (booking.AppointmentSlot is not null)
        {
            FreeSlot(booking.AppointmentSlot);
        }
    }

    private static void FreeSlot(AppointmentSlot slot)
    {
        slot.Status = SlotStatus.Free;
        slot.ReservedUntilUtc = null;
        slot.ReservationToken = null;
    }

    private BookingEmailModel ToEmailModel(Booking booking, AppointmentSlot slot) => new()
    {
        PatientName = booking.PatientName,
        PatientEmail = booking.PatientEmail,
        DoctorName = slot.Doctor.FullName,
        SpecialtyName = slot.Doctor.Specialty.Name,
        StartTimeUtc = slot.StartTimeUtc,
        EndTimeUtc = slot.EndTimeUtc,
        Comment = booking.Comment,
        ConfirmationLifetimeMinutes = _options.ConfirmationLifetimeMinutes
    };

    private static ConfirmBookingResultDto ToConfirmResult(Booking booking) => new()
    {
        PatientName = booking.PatientName,
        DoctorName = booking.AppointmentSlot.Doctor.FullName,
        SpecialtyName = booking.AppointmentSlot.Doctor.Specialty.Name,
        StartTimeUtc = booking.AppointmentSlot.StartTimeUtc,
        EndTimeUtc = booking.AppointmentSlot.EndTimeUtc,
        Comment = booking.Comment,
        Status = booking.Status.ToString()
    };

    private static CancelBookingResultDto ToCancelResult(Booking booking) => new()
    {
        DoctorName = booking.AppointmentSlot.Doctor.FullName,
        SpecialtyName = booking.AppointmentSlot.Doctor.Specialty.Name,
        StartTimeUtc = booking.AppointmentSlot.StartTimeUtc,
        EndTimeUtc = booking.AppointmentSlot.EndTimeUtc,
        Status = booking.Status.ToString()
    };

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1)
        {
            return "***";
        }

        return $"{email[0]}***{email[(at - 1)..]}";
    }
}
