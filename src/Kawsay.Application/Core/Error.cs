namespace Application.Core;

public record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public static readonly Error NullValue = new("Error.NullValue", "A null value was provided unexpectedly.",
        ErrorType.Validation);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);
    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure); // General failure
}

public enum ErrorType
{
    None = 0,
    Failure = 1, // General processing failure
    Validation = 2, // Input data failed validation
    NotFound = 3, // Resource not found
    Conflict = 4 // Conflict with existing state
}