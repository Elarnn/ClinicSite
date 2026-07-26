using System.Text.Json.Serialization;

namespace ClinicSite.Domain.Enums;

/// <summary>
/// The visit outcome of a booking, managed by the doctor. Distinct from <see cref="BookingStatus"/>
/// (the e-mail confirmation lifecycle) and from <see cref="SlotStatus"/> (the slot's own state).
///
/// Serialized to/from JSON as its string name (e.g. "Completed") so the doctor client works with
/// readable values; this converter is scoped to this enum only, leaving numeric SlotStatus untouched.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AppointmentStatus
{
    Scheduled = 0,
    CheckedIn = 1,
    InProgress = 2,
    Completed = 3,
    NoShow = 4,
    Cancelled = 5
}
