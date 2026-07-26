namespace ClinicSite.Application.DTOs.Doctors;

/// <summary>A doctor's stored photo bytes plus its content type, for serving as an image response.</summary>
public sealed record DoctorPhoto(byte[] Data, string ContentType);
