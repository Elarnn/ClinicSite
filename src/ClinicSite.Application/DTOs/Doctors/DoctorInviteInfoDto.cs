namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>Non-sensitive info shown on the set-password page so the doctor knows the invite is theirs.</summary>
public class DoctorInviteInfoDto
{
    public string DoctorName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
