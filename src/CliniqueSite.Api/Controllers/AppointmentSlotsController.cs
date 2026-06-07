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

        [HttpGet("free")]
        public async Task<IActionResult> GetAppointmentSlots(Guid doctorId)
        {
            var slots = await _appointmentSlotsService.GetFreeByDoctorAsync(doctorId);
            return Ok(slots);
        }

        [HttpPost("{slotId:guid}/reserve")]
        public async Task<IActionResult> ReserveSlot(Guid slotId)
        {
            try
            {
                var result = await _appointmentSlotsService.ReserveSlotAsync(slotId);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }
    }
}
