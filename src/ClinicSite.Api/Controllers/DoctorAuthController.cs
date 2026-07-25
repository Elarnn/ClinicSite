using ClinicSite.Api;
using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicSite.Api.Controllers;

/// <summary>
/// Public doctor-account endpoints: validate an invite, set the initial password, and log in.
/// None of these require authentication (the doctor has no session yet).
/// </summary>
[ApiController]
[Route("api/doctor/auth")]
public sealed class DoctorAuthController : ControllerBase
{
    private readonly IDoctorAccountService _accounts;
    private readonly IJwtTokenService _jwt;

    public DoctorAuthController(IDoctorAccountService accounts, IJwtTokenService jwt)
    {
        _accounts = accounts;
        _jwt = jwt;
    }

    /// <summary>Validate an invite token and return the doctor's name/email for the set-password page.</summary>
    [HttpPost("invite-info")]
    public async Task<ActionResult<DoctorInviteInfoDto>> InviteInfo(
        [FromBody] InviteTokenDto request,
        CancellationToken cancellationToken)
    {
        var info = await _accounts.GetInviteInfoAsync(request.Token, cancellationToken);
        return Ok(info);
    }

    /// <summary>Consume the invite token and set the account password (activates the account).</summary>
    [HttpPost("set-password")]
    public async Task<ActionResult<DoctorInviteInfoDto>> SetPassword(
        [FromBody] SetDoctorPasswordDto request,
        CancellationToken cancellationToken)
    {
        var info = await _accounts.SetPasswordAsync(request.Token, request.Password, cancellationToken);
        return Ok(info);
    }

    /// <summary>Exchange email + password for a JWT access token.</summary>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.DoctorLogin)]
    public async Task<ActionResult<DoctorLoginResultDto>> Login(
        [FromBody] DoctorLoginDto request,
        CancellationToken cancellationToken)
    {
        var doctor = await _accounts.AuthenticateAsync(request.Email, request.Password, cancellationToken);
        if (doctor is null)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = _jwt.CreateDoctorToken(doctor.Id, doctor.FullName, doctor.Email!);
        return Ok(new DoctorLoginResultDto
        {
            Token = token.Token,
            DoctorName = doctor.FullName,
            ExpiresAtUtc = token.ExpiresAtUtc
        });
    }
}
