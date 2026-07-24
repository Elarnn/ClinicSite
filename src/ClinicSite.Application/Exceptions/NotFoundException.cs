namespace ClinicSite.Application.Exceptions;

/// <summary>Maps to HTTP 404. Also used for neutral "invalid or expired link" responses.</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}
