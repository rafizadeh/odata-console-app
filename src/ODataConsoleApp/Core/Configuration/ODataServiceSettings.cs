namespace ODataConsoleApp.Core.Configuration;

public class ODataServiceSettings
{
    public string BaseUrl { get; set; } = string.Empty;

    public int DefaultPageSize { get; set; }

    public int RequestTimeout { get; set; }
}
