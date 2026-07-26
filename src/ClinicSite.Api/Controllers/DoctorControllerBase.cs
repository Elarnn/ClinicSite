using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers;

/// <summary>
/// Base for all authenticated doctor endpoints. Requires the "Doctor" role and exposes the doctor id
/// taken from the JWT's <c>doctorId</c> claim — never from the request body/query, so a doctor can
/// only ever act on their own data.
/// </summary>
[ApiController]
[Authorize(Roles = "Doctor")]
public abstract class DoctorControllerBase : ControllerBase
{
    // Matches JwtTokenService.DoctorIdClaim.
    protected const string DoctorIdClaim = "doctorId";

    protected bool TryGetDoctorId(out Guid doctorId)
    {
        var value = User.FindFirst(DoctorIdClaim)?.Value;
        return Guid.TryParse(value, out doctorId);
    }
}
