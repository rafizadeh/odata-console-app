namespace ODataConsoleApp.Core.Configuration;

public class AppSettings
{
    public ODataServiceSettings ODataService { get; set; } = new();
}
