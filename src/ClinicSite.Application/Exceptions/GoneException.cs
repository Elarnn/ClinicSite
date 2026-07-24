namespace ClinicSite.Application.Exceptions;

/// <summary>Maps to HTTP 410 Gone — e.g. a confirmation link whose 30-minute window has elapsed.</summary>
public class GoneException : Exception
{
    public GoneException(string message)
        : base(message)
    {
    }
}
