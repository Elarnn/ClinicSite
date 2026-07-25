namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>Admin request to bind an email to a doctor and send an account invitation.</summary>
public class InviteDoctorDto
{
    public string Email { get; set; } = string.Empty;
}
