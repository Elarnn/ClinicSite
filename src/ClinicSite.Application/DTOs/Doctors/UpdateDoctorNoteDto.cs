namespace ClinicSite.Application.DTOs.Doctors;

public class UpdateDoctorNoteDto
{
    /// <summary>The note text. Empty/whitespace clears the note. Max length enforced by the service.</summary>
    public string? Note { get; set; }
}
