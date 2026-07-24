namespace ClinicSite.Application.DTOs.Bookings
{
    /// <summary>Request body for POST /api/bookings/confirm.</summary>
    public class ConfirmBookingDto
    {
        public string Token { get; set; } = string.Empty;
    }
}
