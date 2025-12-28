namespace ODataConsoleApp.Core.Configuration;

public class ValidationResult
{
    public bool IsValid { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = Array.Empty<string>();

    private ValidationResult() { }

    public static ValidationResult Success() => new() { IsValid = true, Errors = Array.Empty<string>() };

    public static ValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors ?? Array.Empty<string>()
    };
}
