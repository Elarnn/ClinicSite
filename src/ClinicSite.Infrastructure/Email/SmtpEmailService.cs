using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Application.Notifications;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace ClinicSite.Infrastructure.Email;

/// <summary>
/// Sends booking e-mails over SMTP (Gmail by default) using MailKit.
///
/// The SMTP password (Gmail App Password) is read from <see cref="SmtpEmailOptions"/> and is never
/// logged. Delivery failures are surfaced as <see cref="EmailDeliveryException"/> with a generic,
/// secret-free message. The HTML/text bodies come from <see cref="EmailTemplates"/>; links are built
/// here from the trusted <see cref="SmtpEmailOptions.ClientBaseUrl"/> and the raw token.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly SmtpEmailOptions _options;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpEmailOptions> options, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task SendConfirmationRequestAsync(BookingEmailModel model, string confirmationToken, CancellationToken cancellationToken = default)
    {
        var confirmUrl = BuildUrl("booking/confirm", confirmationToken);
        return SendAsync(
            model.PatientEmail,
            model.PatientName,
            EmailTemplates.ConfirmationRequestSubject,
            EmailTemplates.ConfirmationRequestHtml(model, confirmUrl),
            EmailTemplates.ConfirmationRequestText(model, confirmUrl),
            "confirmation-request",
            cancellationToken);
    }

    public Task SendBookingConfirmedAsync(BookingEmailModel model, string cancellationTokenValue, CancellationToken ct = default)
    {
        var cancelUrl = BuildUrl("booking/cancel", cancellationTokenValue);
        return SendAsync(
            model.PatientEmail,
            model.PatientName,
            EmailTemplates.ConfirmedSubject,
            EmailTemplates.ConfirmedHtml(model, cancelUrl),
            EmailTemplates.ConfirmedText(model, cancelUrl),
            "confirmed",
            ct);
    }

    public Task SendBookingCancelledAsync(BookingEmailModel model, CancellationToken cancellationToken = default)
    {
        return SendAsync(
            model.PatientEmail,
            model.PatientName,
            EmailTemplates.CancelledSubject,
            EmailTemplates.CancelledHtml(model),
            EmailTemplates.CancelledText(model),
            "cancelled",
            cancellationToken);
    }

    public Task SendDoctorInviteAsync(string doctorName, string toEmail, string inviteToken, CancellationToken cancellationToken = default)
    {
        var setPasswordUrl = BuildDoctorUrl("set-password", inviteToken);
        return SendAsync(
            toEmail,
            doctorName,
            EmailTemplates.DoctorInviteSubject,
            EmailTemplates.DoctorInviteHtml(doctorName, setPasswordUrl),
            EmailTemplates.DoctorInviteText(doctorName, setPasswordUrl),
            "doctor-invite",
            cancellationToken);
    }

    public Task SendPatientMessageAsync(string toEmail, string patientName, string subject, string message, CancellationToken cancellationToken = default)
    {
        return SendAsync(
            toEmail,
            patientName,
            subject,
            EmailTemplates.PatientMessageHtml(patientName, message),
            EmailTemplates.PatientMessageText(patientName, message),
            "patient-message",
            cancellationToken);
    }

    private async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        string textBody,
        string emailType,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody, TextBody = textBody }.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            await client.ConnectAsync(_options.SmtpHost, _options.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_options.EffectiveUsername, _options.SmtpPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Never log the app password. MailKit exceptions carry the SMTP status but not our credentials.
            _logger.LogError(ex, "SMTP send failed ({EmailType}) to {MaskedEmail}.", emailType, MaskEmail(toEmail));
            throw new EmailDeliveryException("E-mail service is temporarily unavailable.", ex);
        }

        _logger.LogInformation("Sent {EmailType} e-mail to {MaskedEmail} via SMTP.", emailType, MaskEmail(toEmail));
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.SenderEmail) || string.IsNullOrWhiteSpace(_options.SmtpPassword))
        {
            _logger.LogError("SMTP e-mail is not configured (missing Email:SenderEmail or Email:SmtpPassword).");
            throw new EmailDeliveryException(
                "E-mail sending is not configured. Set Email:SenderEmail and Email:SmtpPassword (see README).");
        }
    }

    private string BuildUrl(string path, string token)
    {
        var baseUrl = _options.ClientBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{path}?token={Uri.EscapeDataString(token)}";
    }

    private string BuildDoctorUrl(string path, string token)
    {
        var baseUrl = _options.DoctorClientBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{path}?token={Uri.EscapeDataString(token)}";
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1)
        {
            return "***";
        }

        return $"{email[0]}***{email[(at - 1)..]}";
    }
}
