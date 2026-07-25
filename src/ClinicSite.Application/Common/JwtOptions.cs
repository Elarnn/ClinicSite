namespace ClinicSite.Application.Common;

/// <summary>
/// JWT settings, bound from the "Jwt" section. <see cref="Key"/> is a secret and must come from
/// User Secrets or an environment variable — never appsettings.json.
/// </summary>
public class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Symmetric signing key. Secret — provided out of band. Must be long (>= 32 chars).</summary>
    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = "ClinicSite";

    public string Audience { get; set; } = "ClinicSiteDoctor";

    /// <summary>Access-token lifetime in minutes.</summary>
    public int ExpiryMinutes { get; set; } = 480;
}
