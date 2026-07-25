namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>Carries an invite token in the request body (never in the query string / logs).</summary>
public class InviteTokenDto
{
    public string Token { get; set; } = string.Empty;
}
