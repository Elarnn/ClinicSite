using System;

namespace ClinicSite.Application.DTOs.Doctors
{
    public class CreateDoctorDto
    {
        public string FullName { get; set; } = string.Empty;
        public Guid SpecialtyId { get; set; }
    }
}
