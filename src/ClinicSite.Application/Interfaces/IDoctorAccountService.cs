using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Domain.Entities;

namespace ClinicSite.Application.Interfaces;

/// <summary>
/// Doctor account lifecycle: an admin invites a doctor by email, the doctor activates by setting a
/// password via a one-time link, and then authenticates with email + password.
/// </summary>
public interface IDoctorAccountService
{
    /// <summary>Binds an email to the doctor, stores an invite token, and sends the invitation email.</summary>
    Task<DoctorDto> InviteAsync(Guid doctorId, string email, CancellationToken cancellationToken = default);

    /// <summary>Validates an invite token and returns non-sensitive info for the set-password page.</summary>
    Task<DoctorInviteInfoDto> GetInviteInfoAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>Consumes the invite token, stores the password hash, and activates the account.</summary>
    Task<DoctorInviteInfoDto> SetPasswordAsync(string token, string password, CancellationToken cancellationToken = default);

    /// <summary>Verifies credentials. Returns the doctor on success, or null on any failure (neutral).</summary>
    Task<Doctor?> AuthenticateAsync(string email, string password, CancellationToken cancellationToken = default);
}
