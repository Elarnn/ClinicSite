using ClinicSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ClinicSite.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }
        [HttpGet]
        public async Task<IActionResult> GetDoctors([FromQuery] Guid? specialtyId = null)
        {
            var doctors = await _doctorService.GetDoctorsAsync(specialtyId);
            return Ok(doctors);
        }

        /// <summary>Public: the doctor's photo, or 404 when none has been uploaded.</summary>
        [HttpGet("{doctorId:guid}/photo")]
        public async Task<IActionResult> GetPhoto(Guid doctorId)
        {
            var photo = await _doctorService.GetPhotoAsync(doctorId);
            if (photo is null)
            {
                return NotFound();
            }

            return File(photo.Data, photo.ContentType);
        }
    }
}
