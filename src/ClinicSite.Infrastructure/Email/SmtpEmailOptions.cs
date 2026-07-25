namespace ClinicSite.Infrastructure.Email;

/// <summary>
/// Configuration for SMTP e-mail delivery (Gmail by default). Bound from the "Email" section.
///
/// The <see cref="SmtpPassword"/> (a Gmail App Password) is a secret and must be supplied via
/// User Secrets or an environment variable — never committed to appsettings.json.
///
/// Why Gmail SMTP instead of a third-party API sender: providers such as Gmail/Yahoo/Microsoft now
/// require DKIM/DMARC alignment for the "From" domain. Sending through your own Gmail account lets
/// Google sign the message for gmail.com, so it passes those checks without owning a custom domain.
/// </summary>
public class SmtpEmailOptions
{
    public const string SectionName = "Email";

    /// <summary>SMTP host. Default is Gmail.</summary>
    public string SmtpHost { get; set; } = "smtp.gmail.com";

    /// <summary>SMTP submission port. 587 uses STARTTLS.</summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>SMTP login. Optional — falls back to <see cref="SenderEmail"/> when empty.</summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>SMTP password. For Gmail this is an App Password. Secret — provided out of band.</summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>The "From" address. For Gmail this must be your Gmail account (or a verified alias).</summary>
    public string SenderEmail { get; set; } = string.Empty;

    public string SenderName { get; set; } = "ClinicSite";

    /// <summary>Public base URL of the patient site, used to build confirmation / cancellation links.</summary>
    public string ClientBaseUrl { get; set; } = "http://localhost:5173";

    /// <summary>Public base URL of the doctor portal, used to build the account-invite (set-password) link.</summary>
    public string DoctorClientBaseUrl { get; set; } = "http://localhost:5175";

    /// <summary>Confirmation-link lifetime in minutes (shown in the first e-mail).</summary>
    public int ConfirmationLifetimeMinutes { get; set; } = 30;

    /// <summary>The login to authenticate with: an explicit username, or the sender address by default.</summary>
    public string EffectiveUsername =>
        string.IsNullOrWhiteSpace(SmtpUsername) ? SenderEmail : SmtpUsername;
}
