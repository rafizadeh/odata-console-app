namespace ODataConsoleApp.UI;

using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

public class GlobalSearchHandler : IGlobalSearchHandler
{
    private readonly IConsoleWrapper _consoleWrapper;

    public GlobalSearchHandler(IConsoleWrapper consoleWrapper, IDisplayService displayService)
    {
        _consoleWrapper = consoleWrapper ?? throw new ArgumentNullException(nameof(consoleWrapper));
    }

    public Task<SearchCriteria?> PromptAsync()
    {
        _consoleWrapper.WriteLine();
        _consoleWrapper.WriteLine("═══════════════════════════════════════════════════════════");
        _consoleWrapper.WriteLine("                    Global Search");
        _consoleWrapper.WriteLine("═══════════════════════════════════════════════════════════");
        _consoleWrapper.WriteLine("Search across: First Name, Last Name, Username");
        _consoleWrapper.WriteLine("Uses: Contains (partial match)");
        _consoleWrapper.WriteLine();
        _consoleWrapper.Write("Enter search term (or M to cancel): ");

        var input = _consoleWrapper.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input) || input.Equals("M", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<SearchCriteria?>(null);
        }

        var criteria = new SearchCriteria
        {
            FirstName = input,
            LastName = input,
            UserName = input,
            SearchType = SearchType.Contains
        };

        return Task.FromResult<SearchCriteria?>(criteria);
    }
}