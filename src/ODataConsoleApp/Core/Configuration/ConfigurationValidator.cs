namespace ODataConsoleApp.Core.Configuration;

public class ConfigurationValidator
{
    private const int MaxPageSize = 1000;

    private const int MaxTimeoutSeconds = 300;

    public ValidationResult Validate(AppSettings settings)
    {
        if (settings == null)
        {
            return ValidationResult.Failure("AppSettings cannot be null");
        }

        var errors = new List<string>();

        if (settings.ODataService == null)
        {
            errors.Add("ODataService configuration is required");
        }
        else
        {
            ValidateODataServiceSettings(settings.ODataService, errors);
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors.ToArray());
    }

    private void ValidateODataServiceSettings(ODataServiceSettings settings, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            errors.Add("ODataService.BaseUrl is required and cannot be empty");
        }
        else
        {
            if (!Uri.TryCreate(settings.BaseUrl, UriKind.Absolute, out var uri))
            {
                errors.Add("ODataService.BaseUrl must be a valid URL");
            }
            else if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                errors.Add("ODataService.BaseUrl must use HTTP or HTTPS protocol");
            }
        }

        if (settings.DefaultPageSize <= 0)
        {
            errors.Add("ODataService.DefaultPageSize must be greater than 0");
        }
        else if (settings.DefaultPageSize > MaxPageSize)
        {
            errors.Add($"ODataService.DefaultPageSize must not exceed {MaxPageSize}");
        }

        if (settings.RequestTimeout <= 0)
        {
            errors.Add("ODataService.RequestTimeout must be greater than 0");
        }
        else if (settings.RequestTimeout > MaxTimeoutSeconds)
        {
            errors.Add($"ODataService.RequestTimeout must not exceed {MaxTimeoutSeconds} seconds");
        }
    }
}
