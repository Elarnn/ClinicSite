using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClinicSite.Application.Common;
using ClinicSite.Application.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ClinicSite.Infrastructure.Auth;

/// <summary>
/// Issues HS256-signed JWTs for doctors. The signing key comes from <see cref="JwtOptions.Key"/>
/// (a User Secret). Tokens carry a custom <c>doctorId</c> claim plus the <c>Doctor</c> role.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    public const string DoctorIdClaim = "doctorId";
    public const string DoctorRole = "Doctor";

    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public JwtTokenResult CreateDoctorToken(Guid doctorId, string doctorName, string email)
    {
        if (string.IsNullOrWhiteSpace(_options.Key) || _options.Key.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured. Set Jwt:Key (at least 32 characters) via User Secrets.");
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(DoctorIdClaim, doctorId.ToString()),
            new Claim(ClaimTypes.Name, doctorName),
            new Claim(ClaimTypes.Role, DoctorRole),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);
        return new JwtTokenResult(jwt, expiresAt);
    }
}
