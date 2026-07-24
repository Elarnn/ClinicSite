using ClinicSite.Application.DTOs.Admin;
using ClinicSite.Application.DTOs.Specialties;
using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers;

[ApiController]
[Route("api/admin/specialties")]
public sealed class AdminSpecialtiesController : ControllerBase
{
    private readonly ISpecialtyService _specialtyService;

    public AdminSpecialtiesController(ISpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
    }

    /// <summary>
    /// Get all specialties.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<SpecialtyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<SpecialtyDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var specialties = await _specialtyService.GetSpecialtiesAsync();

        return Ok(specialties);
    }

    /// <summary>
    /// Create a specialty.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SpecialtyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SpecialtyDto>> Create(
        [FromBody] CreateSpecialtyDto request,
        CancellationToken cancellationToken)
    {
        var specialty = await _specialtyService.CreateSpecialtyAsync(
            request);

        return CreatedAtAction(
            nameof(GetById),
            new { specialtyId = specialty.Id },
            specialty);
    }

    /// <summary>
    /// Get a specialty by id.
    /// </summary>
    [HttpGet("{specialtyId:guid}")]
    [ProducesResponseType(typeof(SpecialtyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpecialtyDto>> GetById(
        Guid specialtyId,
        CancellationToken cancellationToken)
    {
        var specialty = await _specialtyService.GetSpecialtyByIdAsync(
            specialtyId);

        return specialty is null
            ? NotFound()
            : Ok(specialty);
    }

    /// <summary>
    /// Update a specialty.
    /// </summary>
    [HttpPut("{specialtyId:guid}")]
    [ProducesResponseType(typeof(SpecialtyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SpecialtyDto>> Update(
        Guid specialtyId,
        [FromBody] UpdateSpecialtyDto request,
        CancellationToken cancellationToken)
    {
        var specialty = await _specialtyService.UpdateSpecialtyAsync(
            specialtyId,
            request);

        return specialty is null
            ? NotFound()
            : Ok(specialty);
    }

    /// <summary>
    /// Delete a specialty.
    /// </summary>
    [HttpDelete("{specialtyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(
        Guid specialtyId,
        CancellationToken cancellationToken)
    {
        var deleted = await _specialtyService.DeleteSpecialtyAsync(
            specialtyId);

        return deleted
            ? NoContent()
            : NotFound();
    }
}