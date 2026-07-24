namespace ClinicSite.Infrastructure.Email;

/// <summary>
/// Configuration for the Brevo transactional e-mail integration. Bound from the "Email" section.
/// The <see cref="ApiKey"/> must be supplied via User Secrets or an environment variable — never
/// committed to appsettings.json.
/// </summary>
public class BrevoEmailOptions
{
    public const string SectionName = "Email";

    /// <summary>Brevo transactional API key. Secret — provided out of band.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>A sender address that has been verified in the Brevo dashboard.</summary>
    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = "ClinicSite";

    /// <summary>Public base URL of the patient site, used to build confirmation / cancellation links.</summary>
    public string ClientBaseUrl { get; set; } = "http://localhost:5173";

    /// <summary>Confirmation-link lifetime in minutes (shown in the first e-mail).</summary>
    public int ConfirmationLifetimeMinutes { get; set; } = 30;
}
