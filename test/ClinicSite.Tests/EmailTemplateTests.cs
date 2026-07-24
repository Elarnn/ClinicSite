using ClinicSite.Application.Notifications;
using ClinicSite.Infrastructure.Email;

namespace ClinicSite.Tests;

public class EmailTemplateTests
{
    private static BookingEmailModel Model(string name, string? comment = null) => new()
    {
        PatientName = name,
        PatientEmail = "patient@example.com",
        DoctorName = "Dr. Smith",
        SpecialtyName = "Cardiology",
        StartTimeUtc = new DateTime(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc),
        EndTimeUtc = new DateTime(2026, 8, 1, 9, 30, 0, DateTimeKind.Utc),
        Comment = comment,
        ConfirmationLifetimeMinutes = 30
    };

    // ---- 15. Patient-supplied data is HTML-encoded --------------------------------------------
    [Fact]
    public void ConfirmationRequestHtml_encodes_patient_data()
    {
        var model = Model("<script>alert('x')</script>", comment: "<img src=x onerror=alert(1)>");

        var html = EmailTemplates.ConfirmationRequestHtml(
            model, "http://localhost:5173/booking/confirm?token=abc");

        Assert.DoesNotContain("<script>", html);
        Assert.DoesNotContain("<img src=x", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public void ConfirmedHtml_encodes_patient_data()
    {
        var model = Model("<b>Mallory</b>");

        var html = EmailTemplates.ConfirmedHtml(model, "http://localhost:5173/booking/cancel?token=abc");

        Assert.DoesNotContain("<b>Mallory</b>", html);
        Assert.Contains("&lt;b&gt;Mallory&lt;/b&gt;", html);
    }
}
