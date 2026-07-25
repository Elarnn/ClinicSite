namespace ClinicSite.Application.Exceptions;

/// <summary>
/// Raised when a transactional e-mail could not be delivered (SMTP failure or missing configuration).
/// Maps to HTTP 503. The message is safe to show to the client and never contains secrets or tokens.
/// </summary>
public class EmailDeliveryException : Exception
{
    public EmailDeliveryException(string message)
        : base(message)
    {
    }

    public EmailDeliveryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
