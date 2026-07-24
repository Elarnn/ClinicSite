namespace ClinicSite.Api;

/// <summary>Named rate-limiting policy keys used by the booking endpoints.</summary>
public static class RateLimitPolicies
{
    public const string CreateBooking = "create-booking";
    public const string ConfirmCancel = "confirm-cancel";
}
