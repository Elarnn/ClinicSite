using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers;

[ApiController]
[Route("api/admin/doctors")]
public sealed class AdminDoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;

    public AdminDoctorsController(IDoctorService doctorService)
    {
        _doctorService = doctorService;
    }

    /// <summary>
    /// Создать врача.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DoctorDto>> Create(
        [FromBody] CreateDoctorDto request)
    {
        var doctor = await _doctorService.CreateDoctorAsync(request);

        return StatusCode(StatusCodes.Status201Created, doctor);
    }
}
