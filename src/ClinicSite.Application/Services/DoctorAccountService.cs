using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Application.Security;
using ClinicSite.Domain.Entities;
using ClinicSite.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClinicSite.Application.Services;

public class DoctorAccountService : IDoctorAccountService
{
    private const string InvalidLinkMessage = "This link is invalid or has expired.";
    private const string ExpiredMessage = "This invitation has expired. Ask the clinic to send a new one.";
    private const int InviteLifetimeHours = 48;
    private const int MinPasswordLength = 8;

    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<DoctorAccountService> _logger;

    public DoctorAccountService(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<DoctorAccountService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<DoctorDto> InviteAsync(Guid doctorId, string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = (email ?? string.Empty).Trim();
        if (normalizedEmail.Length == 0)
        {
            throw new ValidationException("An email address is required.");
        }

        var doctor = await _context.Doctors
            .Include(d => d.Specialty)
            .FirstOrDefaultAsync(d => d.Id == doctorId, cancellationToken);

        if (doctor is null)
        {
            throw new NotFoundException("Doctor not found.");
        }

        if (doctor.AccountStatus == DoctorAccountStatus.Active)
        {
            throw new ConflictException("This doctor already has an active account.");
        }

        var emailTaken = await _context.Doctors.AnyAsync(
            d => d.Id != doctorId && d.Email != null && d.Email.ToLower() == normalizedEmail.ToLower(),
            cancellationToken);

        if (emailTaken)
        {
            throw new ConflictException("This email is already used by another doctor.");
        }

        // Remember the previous account state so we can roll back if the invite email fails to send.
        var previousEmail = doctor.Email;
        var previousStatus = doctor.AccountStatus;
        var previousHash = doctor.InviteTokenHash;
        var previousExpiry = doctor.InviteTokenExpiresAtUtc;

        var (inviteToken, inviteHash) = BookingTokens.Create();
        doctor.Email = normalizedEmail;
        doctor.InviteTokenHash = inviteHash;
        doctor.InviteTokenExpiresAtUtc = DateTime.UtcNow.AddHours(InviteLifetimeHours);
        doctor.AccountStatus = DoctorAccountStatus.Invited;
        await _context.SaveChangesAsync(cancellationToken);

        try
        {
            await _emailService.SendDoctorInviteAsync(doctor.FullName, normalizedEmail, inviteToken, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invite e-mail failed for doctor {DoctorId}; rolling back the invitation.", doctorId);

            doctor.Email = previousEmail;
            doctor.AccountStatus = previousStatus;
            doctor.InviteTokenHash = previousHash;
            doctor.InviteTokenExpiresAtUtc = previousExpiry;
            await _context.SaveChangesAsync(cancellationToken);

            throw new EmailDeliveryException("Could not send the invitation email. Please try again later.");
        }

        _logger.LogInformation("Doctor {DoctorId} invited ({MaskedEmail}).", doctorId, MaskEmail(normalizedEmail));
        return ToDto(doctor);
    }

    public async Task<DoctorInviteInfoDto> GetInviteInfoAsync(string token, CancellationToken cancellationToken = default)
    {
        var doctor = await LoadByInviteTokenAsync(token, cancellationToken);
        return new DoctorInviteInfoDto { DoctorName = doctor.FullName, Email = doctor.Email ?? string.Empty };
    }

    public async Task<DoctorInviteInfoDto> SetPasswordAsync(string token, string password, CancellationToken cancellationToken = default)
    {
        var doctor = await LoadByInviteTokenAsync(token, cancellationToken);

        if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
        {
            throw new ValidationException($"Password must be at least {MinPasswordLength} characters.");
        }

        doctor.PasswordHash = PasswordHasher.Hash(password);
        doctor.AccountStatus = DoctorAccountStatus.Active;
        doctor.InviteTokenHash = null;          // single-use: the link can't be replayed
        doctor.InviteTokenExpiresAtUtc = null;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Doctor {DoctorId} activated their account.", doctor.Id);
        return new DoctorInviteInfoDto { DoctorName = doctor.FullName, Email = doctor.Email ?? string.Empty };
    }

    public async Task<Doctor?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = (email ?? string.Empty).Trim();
        if (normalizedEmail.Length == 0 || string.IsNullOrEmpty(password))
        {
            return null;
        }

        var doctor = await _context.Doctors.FirstOrDefaultAsync(
            d => d.IsActive
                && d.AccountStatus == DoctorAccountStatus.Active
                && d.Email != null && d.Email.ToLower() == normalizedEmail.ToLower(),
            cancellationToken);

        if (doctor is null || !PasswordHasher.Verify(password, doctor.PasswordHash))
        {
            return null;
        }

        return doctor;
    }

    // --- helpers -----------------------------------------------------------------------------

    private async Task<Doctor> LoadByInviteTokenAsync(string token, CancellationToken cancellationToken)
    {
        if (!BookingTokens.TryComputeHash(token, out var hash))
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        var doctor = await _context.Doctors
            .FirstOrDefaultAsync(d => d.InviteTokenHash == hash, cancellationToken);

        if (doctor is null
            || doctor.AccountStatus != DoctorAccountStatus.Invited
            || !BookingTokens.HashesEqual(doctor.InviteTokenHash, hash))
        {
            throw new NotFoundException(InvalidLinkMessage);
        }

        if (doctor.InviteTokenExpiresAtUtc is null || doctor.InviteTokenExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            throw new GoneException(ExpiredMessage);
        }

        return doctor;
    }

    private static DoctorDto ToDto(Doctor doctor) => new()
    {
        Id = doctor.Id,
        FullName = doctor.FullName,
        SpecialtyId = doctor.SpecialtyId,
        SpecialtyName = doctor.Specialty?.Name ?? string.Empty,
        IsActive = doctor.IsActive,
        Email = doctor.Email,
        AccountStatus = doctor.AccountStatus.ToString()
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
