using CliniqueSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CliniqueSite.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecialtiesController : ControllerBase
    {
        private readonly ISpecialtyService _specialtyService;

        public SpecialtiesController(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSpecialties()
        {
            var specialties = await _specialtyService.GetSpecialtiesAsync();
            return Ok(specialties);
        }
    }
}
