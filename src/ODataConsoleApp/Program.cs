namespace ODataConsoleApp;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var host = new ApplicationHost();
        return await host.RunAsync();
    }
}