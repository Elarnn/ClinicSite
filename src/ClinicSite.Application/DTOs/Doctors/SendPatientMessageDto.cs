namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>
/// A one-off message a doctor sends to the patient of a specific booking. The recipient is always
/// the booking's own e-mail — never an address supplied by the client.
/// </summary>
public class SendPatientMessageDto
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
