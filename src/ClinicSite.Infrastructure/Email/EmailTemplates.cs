using System.Globalization;
using System.Net;
using ClinicSite.Application.Notifications;

namespace ClinicSite.Infrastructure.Email;

/// <summary>
/// Builds the HTML and plain-text bodies for the booking e-mails.
///
/// Security: every value that originates from patient input (name, comment) or from doctor data is
/// HTML-encoded before it goes into the HTML body. Subjects and links are produced entirely by the
/// server; nothing about the message is accepted from the client.
/// </summary>
internal static class EmailTemplates
{
    public const string ConfirmationRequestSubject = "Confirm your ClinicSite appointment";
    public const string ConfirmedSubject = "Your ClinicSite appointment is confirmed";
    public const string CancelledSubject = "Your ClinicSite appointment has been cancelled";
    public const string DoctorInviteSubject = "Set up your ClinicSite doctor account";

    // --- Doctor account invitation -----------------------------------------------------------

    public static string DoctorInviteHtml(string doctorName, string setPasswordUrl)
    {
        var name = Enc(doctorName);
        return $$"""
        <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#1f2937">
          <h2 style="color:#0f766e">Welcome, {{name}}!</h2>
          <p>An account has been created for you in the ClinicSite doctor portal. Set your password to activate it.</p>
          <p style="margin:24px 0">
            <a href="{{Attr(setPasswordUrl)}}"
               style="background:#0f766e;color:#ffffff;text-decoration:none;padding:14px 28px;border-radius:8px;font-weight:bold;display:inline-block">
              Set your password
            </a>
          </p>
          <p style="color:#6b7280;font-size:14px">
            This link is valid for 48 hours. If the button doesn't work, open this link manually:<br>
            <a href="{{Attr(setPasswordUrl)}}" style="color:#0f766e;word-break:break-all">{{Enc(setPasswordUrl)}}</a>
          </p>
          <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0">
          <p style="color:#9ca3af;font-size:12px">
            If you weren't expecting this invitation, you can safely ignore this e-mail.
          </p>
        </div>
        """;
    }

    public static string DoctorInviteText(string doctorName, string setPasswordUrl)
    {
        return
$@"Welcome, {doctorName}!

An account has been created for you in the ClinicSite doctor portal. Set your password to activate it (link valid for 48 hours):

{setPasswordUrl}

If you weren't expecting this invitation, you can safely ignore this e-mail.";
    }

    // --- First e-mail: please confirm --------------------------------------------------------

    public static string ConfirmationRequestHtml(BookingEmailModel m, string confirmUrl)
    {
        var name = Enc(m.PatientName);
        return $$"""
        <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#1f2937">
          <h2 style="color:#0f766e">Hello, {{name}}!</h2>
          <p>You've requested an appointment at ClinicSite. Please confirm it to make it active.</p>
          {{DetailsTable(m)}}
          <p style="margin:24px 0">
            <a href="{{Attr(confirmUrl)}}"
               style="background:#0f766e;color:#ffffff;text-decoration:none;padding:14px 28px;border-radius:8px;font-weight:bold;display:inline-block">
              Confirm appointment
            </a>
          </p>
          <p style="color:#6b7280;font-size:14px">
            This link is valid for {{m.ConfirmationLifetimeMinutes}} minutes. If the button doesn't work, open this link manually:<br>
            <a href="{{Attr(confirmUrl)}}" style="color:#0f766e;word-break:break-all">{{Enc(confirmUrl)}}</a>
          </p>
          <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0">
          <p style="color:#9ca3af;font-size:12px">
            If you didn't request this appointment, simply ignore this e-mail — nothing will happen.
          </p>
        </div>
        """;
    }

    public static string ConfirmationRequestText(BookingEmailModel m, string confirmUrl)
    {
        return
$@"Hello, {m.PatientName}!

You've requested an appointment at ClinicSite. Please confirm it within {m.ConfirmationLifetimeMinutes} minutes.

{DetailsText(m)}

Confirm your appointment:
{confirmUrl}

If you didn't request this appointment, please ignore this e-mail.";
    }

    // --- Second e-mail: confirmed ------------------------------------------------------------

    public static string ConfirmedHtml(BookingEmailModel m, string cancelUrl)
    {
        var name = Enc(m.PatientName);
        return $$"""
        <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#1f2937">
          <h2 style="color:#0f766e">Appointment confirmed ✅</h2>
          <p>Hello, {{name}}! Your appointment is confirmed. We look forward to seeing you.</p>
          {{DetailsTable(m)}}
          <p style="margin:24px 0">
            <a href="{{Attr(cancelUrl)}}"
               style="background:#ffffff;color:#b91c1c;text-decoration:none;padding:12px 24px;border-radius:8px;font-weight:bold;display:inline-block;border:1px solid #fecaca">
              Cancel appointment
            </a>
          </p>
          <p style="color:#6b7280;font-size:14px">
            Need to cancel? Open this link:<br>
            <a href="{{Attr(cancelUrl)}}" style="color:#0f766e;word-break:break-all">{{Enc(cancelUrl)}}</a>
          </p>
        </div>
        """;
    }

    public static string ConfirmedText(BookingEmailModel m, string cancelUrl)
    {
        return
$@"Appointment confirmed!

Hello, {m.PatientName}! Your appointment is confirmed.

{DetailsText(m)}

Cancel your appointment:
{cancelUrl}";
    }

    // --- Optional: cancelled -----------------------------------------------------------------

    public static string CancelledHtml(BookingEmailModel m)
    {
        var name = Enc(m.PatientName);
        return $$"""
        <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#1f2937">
          <h2 style="color:#b91c1c">Appointment cancelled</h2>
          <p>Hello, {{name}}! Your appointment has been cancelled.</p>
          {{DetailsTable(m)}}
          <p style="color:#6b7280;font-size:14px">You can book a new appointment on ClinicSite at any time.</p>
        </div>
        """;
    }

    public static string CancelledText(BookingEmailModel m)
    {
        return
$@"Appointment cancelled.

Hello, {m.PatientName}! Your appointment has been cancelled.

{DetailsText(m)}

You can book a new appointment on ClinicSite at any time.";
    }

    // --- Doctor -> patient free-form message -------------------------------------------------

    public static string PatientMessageHtml(string patientName, string message)
    {
        var name = Enc(patientName);
        // Encode first, then turn newlines into <br> so the doctor's line breaks survive.
        var body = Enc(message).Replace("\r\n", "\n").Replace("\n", "<br>");
        return $$"""
        <div style="font-family:Arial,Helvetica,sans-serif;max-width:520px;margin:0 auto;color:#1f2937">
          <h2 style="color:#0f766e">Hello, {{name}}!</h2>
          <p>You have a message from ClinicSite regarding your appointment:</p>
          <div style="background:#f9fafb;border-radius:12px;padding:16px;margin:16px 0;white-space:normal">{{body}}</div>
          <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0">
          <p style="color:#9ca3af;font-size:12px">This message was sent by your clinic through ClinicSite.</p>
        </div>
        """;
    }

    public static string PatientMessageText(string patientName, string message)
    {
        return
$@"Hello, {patientName}!

You have a message from ClinicSite regarding your appointment:

{message}

— Sent by your clinic through ClinicSite.";
    }

    // --- shared rendering --------------------------------------------------------------------

    private static string DetailsTable(BookingEmailModel m)
    {
        var rows =
            Row("Doctor", m.DoctorName) +
            Row("Specialty", m.SpecialtyName) +
            Row("Date", FormatDate(m.StartTimeUtc)) +
            Row("Time", $"{FormatTime(m.StartTimeUtc)} – {FormatTime(m.EndTimeUtc)} (UTC)");

        if (!string.IsNullOrWhiteSpace(m.Comment))
        {
            rows += Row("Comment", m.Comment!);
        }

        return $"""
        <table style="width:100%;border-collapse:collapse;margin:16px 0">
          {rows}
        </table>
        """;
    }

    private static string Row(string label, string value) => $"""
        <tr>
          <td style="padding:8px 0;color:#6b7280;width:40%">{Enc(label)}</td>
          <td style="padding:8px 0;font-weight:bold">{Enc(value)}</td>
        </tr>
        """;

    private static string DetailsText(BookingEmailModel m)
    {
        var lines =
$@"Doctor: {m.DoctorName}
Specialty: {m.SpecialtyName}
Date: {FormatDate(m.StartTimeUtc)}
Time: {FormatTime(m.StartTimeUtc)} – {FormatTime(m.EndTimeUtc)} (UTC)";

        if (!string.IsNullOrWhiteSpace(m.Comment))
        {
            lines += $"\nComment: {m.Comment}";
        }

        return lines;
    }

    private static string FormatDate(DateTime utc) =>
        utc.ToString("dddd, MMMM d, yyyy", CultureInfo.GetCultureInfo("en-US"));

    private static string FormatTime(DateTime utc) =>
        utc.ToString("HH:mm", CultureInfo.InvariantCulture);

    // HTML text-node encoding for untrusted values.
    private static string Enc(string value) => WebUtility.HtmlEncode(value);

    // HTML attribute encoding for URLs placed inside href="...".
    private static string Attr(string value) => WebUtility.HtmlEncode(value);
}
