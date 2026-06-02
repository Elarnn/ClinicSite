using CliniqueSite.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CliniqueSite.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentSlotsController : ControllerBase
    {
        private readonly IAppointmentSlotService _appointmentSlotsService;

        public AppointmentSlotsController(IAppointmentSlotService appointmentSlotsService)
        {
            _appointmentSlotsService = appointmentSlotsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAppointmentSlots(Guid doctorId)
        {
            var slots = await _appointmentSlotsService.GetFreeByDoctorAsync(doctorId);
            return Ok(slots);
        }
    }
}
