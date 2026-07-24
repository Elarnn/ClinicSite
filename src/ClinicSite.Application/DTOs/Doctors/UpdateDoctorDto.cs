using System;

namespace ClinicSite.Application.DTOs.Doctors
{
    public class UpdateDoctorDto
    {
        public string FullName { get; set; } = string.Empty;
        public Guid SpecialtyId { get; set; }
    }
}
