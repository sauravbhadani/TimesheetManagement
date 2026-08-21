namespace TimesheetManagement.Application.Exceptions;

/// <summary>Requested entity does not exist. Mapped to 404 by the global error middleware.</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Caller is authenticated but not allowed to perform this action. Mapped to 403.</summary>
public class ForbiddenException(string message) : Exception(message);

/// <summary>Action is not valid given the entity's current state (e.g. approving a Draft week). Mapped to 409.</summary>
public class ConflictException(string message) : Exception(message);

/// <summary>Server-side validation failure. Mapped to 400 with a field/message map.</summary>
public class ValidationAppException(IDictionary<string, string[]> errors)
    : Exception("One or more validation errors occurred.")
{
    public IDictionary<string, string[]> Errors { get; } = errors;

    public ValidationAppException(string field, string message)
        : this(new Dictionary<string, string[]> { [field] = [message] })
    {
    }
}
