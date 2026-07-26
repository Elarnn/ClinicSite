using ClinicSite.Domain.Common;
using ClinicSite.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSite.Domain.Entities
{
    public class Doctor : BaseEntity
    {
        public string FullName { get; set; } = string.Empty;

        public Guid SpecialtyId { get; set; }
        public Specialty Specialty { get; set; } = null!;

        public bool IsActive { get; set; } = true;

        // --- Profile photo (uploaded by an admin, shown on the public site). Small headshots only,
        // stored inline; queries that only need doctor details use projections and never load these. ---
        public byte[]? Photo { get; set; }
        public string? PhotoContentType { get; set; }

        // --- Login account (bound by an admin, activated by the doctor via an invite link) ---

        /// <summary>Login email. Null until an admin invites the doctor. Unique across doctors.</summary>
        public string? Email { get; set; }

        /// <summary>PBKDF2 password hash (never a plaintext password). Null until the doctor sets one.</summary>
        public string? PasswordHash { get; set; }

        public DoctorAccountStatus AccountStatus { get; set; } = DoctorAccountStatus.None;

        /// <summary>SHA-256 hash of the current invite token; the raw token only lives in the email link.</summary>
        public string? InviteTokenHash { get; set; }

        public DateTime? InviteTokenExpiresAtUtc { get; set; }

        public ICollection<AppointmentSlot> Slots { get; set; } = new List<AppointmentSlot>();
    }
}
