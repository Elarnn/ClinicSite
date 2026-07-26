using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Application.DTOs.Doctors
{
    public class DoctorDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public Guid SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        /// <summary>Whether a profile photo has been uploaded (fetch it from /api/doctors/{id}/photo).</summary>
        public bool HasPhoto { get; set; }

        /// <summary>Bound login email, or null if the doctor has no account yet.</summary>
        public string? Email { get; set; }

        /// <summary>Account lifecycle as a string: "None", "Invited", or "Active".</summary>
        public string AccountStatus { get; set; } = "None";
    }
}
