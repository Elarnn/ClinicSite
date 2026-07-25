namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>Doctor's request to activate their account by setting a password via the invite link.</summary>
public class SetDoctorPasswordDto
{
    public string Token { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
