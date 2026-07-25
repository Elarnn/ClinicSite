namespace ClinicSite.Application.Interfaces;

/// <summary>Issues signed JWT access tokens for authenticated doctors.</summary>
public interface IJwtTokenService
{
    JwtTokenResult CreateDoctorToken(Guid doctorId, string doctorName, string email);
}

public sealed record JwtTokenResult(string Token, DateTime ExpiresAtUtc);
