using CliniqueSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CliniqueSite.Api.Controllers
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
    }
}
