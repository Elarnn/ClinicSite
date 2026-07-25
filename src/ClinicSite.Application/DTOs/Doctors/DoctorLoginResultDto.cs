namespace ClinicSite.Application.DTOs.Doctors;

public class DoctorLoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
}
