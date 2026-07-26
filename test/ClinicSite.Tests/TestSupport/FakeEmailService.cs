using ClinicSite.Application.Exceptions;
using ClinicSite.Application.Interfaces;
using ClinicSite.Application.Notifications;

namespace ClinicSite.Tests.TestSupport;

/// <summary>
/// In-memory <see cref="IEmailService"/> for tests: it never sends real e-mail, records the raw tokens
/// it is handed, and can be told to fail a specific send to exercise the error paths.
/// </summary>
public sealed class FakeEmailService : IEmailService
{
    public int ConfirmationRequestCount { get; private set; }
    public int ConfirmedCount { get; private set; }
    public int CancelledCount { get; private set; }
    public int DoctorInviteCount { get; private set; }
    public int PatientMessageCount { get; private set; }

    public string? LastConfirmationToken { get; private set; }
    public string? LastCancellationToken { get; private set; }
    public string? LastInviteToken { get; private set; }
    public string? LastInviteEmail { get; private set; }
    public string? LastMessageEmail { get; private set; }
    public string? LastMessageSubject { get; private set; }
    public string? LastMessageBody { get; private set; }
    public BookingEmailModel? LastConfirmationModel { get; private set; }

    public bool ThrowOnConfirmationRequest { get; set; }
    public bool ThrowOnConfirmed { get; set; }
    public bool ThrowOnDoctorInvite { get; set; }
    public bool ThrowOnPatientMessage { get; set; }

    public Task SendConfirmationRequestAsync(BookingEmailModel model, string confirmationToken, CancellationToken cancellationToken = default)
    {
        ConfirmationRequestCount++;
        LastConfirmationToken = confirmationToken;
        LastConfirmationModel = model;

        if (ThrowOnConfirmationRequest)
        {
            throw new EmailDeliveryException("Simulated first-email failure.");
        }

        return Task.CompletedTask;
    }

    public Task SendBookingConfirmedAsync(BookingEmailModel model, string cancellationToken2, CancellationToken ct = default)
    {
        ConfirmedCount++;
        LastCancellationToken = cancellationToken2;

        if (ThrowOnConfirmed)
        {
            throw new EmailDeliveryException("Simulated confirmed-email failure.");
        }

        return Task.CompletedTask;
    }

    public Task SendBookingCancelledAsync(BookingEmailModel model, CancellationToken cancellationToken = default)
    {
        CancelledCount++;
        return Task.CompletedTask;
    }

    public Task SendDoctorInviteAsync(string doctorName, string toEmail, string inviteToken, CancellationToken cancellationToken = default)
    {
        DoctorInviteCount++;
        LastInviteToken = inviteToken;
        LastInviteEmail = toEmail;

        if (ThrowOnDoctorInvite)
        {
            throw new EmailDeliveryException("Simulated invite-email failure.");
        }

        return Task.CompletedTask;
    }

    public Task SendPatientMessageAsync(string toEmail, string patientName, string subject, string message, CancellationToken cancellationToken = default)
    {
        PatientMessageCount++;
        LastMessageEmail = toEmail;
        LastMessageSubject = subject;
        LastMessageBody = message;

        if (ThrowOnPatientMessage)
        {
            throw new EmailDeliveryException("Simulated patient-message failure.");
        }

        return Task.CompletedTask;
    }
}
