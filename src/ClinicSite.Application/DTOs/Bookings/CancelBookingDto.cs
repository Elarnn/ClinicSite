namespace ClinicSite.Application.DTOs.Bookings
{
    /// <summary>Request body for POST /api/bookings/cancel and /api/bookings/cancel-info.</summary>
    public class CancelBookingDto
    {
        public string Token { get; set; } = string.Empty;
    }
}
