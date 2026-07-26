using ClinicSite.Application.DTOs.Doctors;
using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers;

[ApiController]
[Route("api/admin/doctors")]
public sealed class AdminDoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly IDoctorAccountService _doctorAccountService;

    public AdminDoctorsController(IDoctorService doctorService, IDoctorAccountService doctorAccountService)
    {
        _doctorService = doctorService;
        _doctorAccountService = doctorAccountService;
    }

    /// <summary>
    /// Get all doctors (including deactivated ones).
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<DoctorDto>>> GetAll()
    {
        var doctors = await _doctorService.GetAllDoctorsAsync();

        return Ok(doctors);
    }

    /// <summary>
    /// Create a doctor.
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

    /// <summary>
    /// Update a doctor.
    /// </summary>
    [HttpPut("{doctorId:guid}")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> Update(
        Guid doctorId,
        [FromBody] UpdateDoctorDto request)
    {
        var doctor = await _doctorService.UpdateDoctorAsync(doctorId, request);

        return doctor is null
            ? NotFound()
            : Ok(doctor);
    }

    /// <summary>
    /// Bind an email to a doctor and send an account-setup invitation.
    /// </summary>
    [HttpPost("{doctorId:guid}/invite")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<DoctorDto>> Invite(
        Guid doctorId,
        [FromBody] InviteDoctorDto request,
        CancellationToken cancellationToken)
    {
        var doctor = await _doctorAccountService.InviteAsync(doctorId, request.Email, cancellationToken);
        return Ok(doctor);
    }

    /// <summary>
    /// Upload (or replace) a doctor's profile photo. Multipart form-data with a single "file" field.
    /// </summary>
    [HttpPost("{doctorId:guid}/photo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadPhoto(Guid doctorId, IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file was uploaded." });
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        await _doctorService.SetPhotoAsync(doctorId, stream.ToArray(), file.ContentType);
        return NoContent();
    }

    /// <summary>Remove a doctor's photo (the public site then shows a placeholder).</summary>
    [HttpDelete("{doctorId:guid}/photo")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePhoto(Guid doctorId)
    {
        await _doctorService.RemovePhotoAsync(doctorId);
        return NoContent();
    }

    /// <summary>
    /// Deactivate a doctor.
    /// </summary>
    [HttpPost("{doctorId:guid}/deactivate")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> Deactivate(Guid doctorId)
    {
        var doctor = await _doctorService.DeactivateDoctorAsync(doctorId);

        return doctor is null
            ? NotFound()
            : Ok(doctor);
    }

    /// <summary>
    /// Activate a doctor.
    /// </summary>
    [HttpPost("{doctorId:guid}/activate")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DoctorDto>> Activate(Guid doctorId)
    {
        var doctor = await _doctorService.ActivateDoctorAsync(doctorId);

        return doctor is null
            ? NotFound()
            : Ok(doctor);
    }

    /// <summary>
    /// Delete a doctor.
    /// </summary>
    [HttpDelete("{doctorId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid doctorId)
    {
        var deleted = await _doctorService.DeleteDoctorAsync(doctorId);

        return deleted
            ? NoContent()
            : NotFound();
    }
}
