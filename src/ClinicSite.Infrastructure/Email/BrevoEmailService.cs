using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Application.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ClinicSite.Infrastructure.Email;

/// <summary>
/// Sends booking e-mails through the Brevo transactional REST API
/// (<c>POST https://api.brevo.com/v3/smtp/email</c>) over HTTPS using a typed <see cref="HttpClient"/>.
///
/// The API key is attached per-request as the <c>api-key</c> header and is never logged. Failures are
/// surfaced as <see cref="EmailDeliveryException"/> with a generic, secret-free message.
/// </summary>
public class BrevoEmailService : IEmailService
{
    private const string SendEndpoint = "smtp/email"; // relative to the client's BaseAddress (…/v3/)

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly BrevoEmailOptions _options;
    private readonly ILogger<BrevoEmailService> _logger;

    public BrevoEmailService(
        HttpClient httpClient,
        IOptions<BrevoEmailOptions> options,
        ILogger<BrevoEmailService> logger)
    {
        _httpClient = httpClient;
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

    public Task SendBookingConfirmedAsync(BookingEmailModel model, string cancellationToken2, CancellationToken ct = default)
    {
        var cancelUrl = BuildUrl("booking/cancel", cancellationToken2);
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

    private async Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlContent,
        string textContent,
        string emailType,
        CancellationToken cancellationToken)
    {
        EnsureConfigured();

        var payload = new
        {
            sender = new { name = _options.SenderName, email = _options.SenderEmail },
            to = new[] { new { email = toEmail, name = toName } },
            subject,
            htmlContent,
            textContent
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, SendEndpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        // Attach the secret per request so it never lives on the shared client and is easy to keep out of logs.
        request.Headers.TryAddWithoutValidation("api-key", _options.ApiKey);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            // Network / timeout failure. Log without any secret; surface a generic error.
            _logger.LogError(ex, "Brevo request failed ({EmailType}) to {MaskedEmail}.", emailType, MaskEmail(toEmail));
            throw new EmailDeliveryException("E-mail service is temporarily unavailable.", ex);
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                // Brevo error bodies contain a code/message but never our api-key; safe to log, not to return.
                var body = await SafeReadAsync(response, cancellationToken);
                _logger.LogError(
                    "Brevo returned {StatusCode} for {EmailType} e-mail to {MaskedEmail}. Response: {Response}",
                    (int)response.StatusCode, emailType, MaskEmail(toEmail), body);

                throw new EmailDeliveryException("E-mail service returned an error.");
            }

            _logger.LogInformation(
                "Sent {EmailType} e-mail to {MaskedEmail} (Brevo {StatusCode}).",
                emailType, MaskEmail(toEmail), (int)response.StatusCode);
        }
    }

    private void EnsureConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.SenderEmail))
        {
            _logger.LogError("Brevo e-mail is not configured (missing Email:ApiKey or Email:SenderEmail).");
            throw new EmailDeliveryException(
                "E-mail sending is not configured. Set Email:ApiKey and Email:SenderEmail (see README).");
        }
    }

    private string BuildUrl(string path, string token)
    {
        var baseUrl = _options.ClientBaseUrl.TrimEnd('/');
        return $"{baseUrl}/{path}?token={Uri.EscapeDataString(token)}";
    }

    private static async Task<string> SafeReadAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            return text.Length > 500 ? text[..500] : text;
        }
        catch
        {
            return "<unreadable>";
        }
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
