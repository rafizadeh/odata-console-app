namespace ODataConsoleApp.UI;

using Microsoft.Extensions.Logging;
using ODataConsoleApp.Interfaces;

public class PersonDetailsHandler : IPersonDetailsHandler
{
    private readonly IConsoleWrapper _console;
    private readonly IPersonService _personService;
    private readonly IDisplayService _displayService;
    private readonly ILogger<PersonDetailsHandler> _logger;

    private const int BoxWidth = 59;

    public PersonDetailsHandler(
        IConsoleWrapper console,
        IPersonService personService,
        IDisplayService displayService,
        ILogger<PersonDetailsHandler> logger)
    {
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task DisplayPersonAsync(string username)
    {
        if (!ValidateUsername(username))
        {
            _displayService.DisplayError("Invalid username. Please enter a valid username.");
            return;
        }

        try
        {
            var person = await _personService.GetPersonDetailsAsync(username);

            if (person == null)
            {
                _displayService.DisplayError($"Person with username '{username}' was not found.");
                return;
            }

            _displayService.DisplayPersonDetails(person);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching details for username: {Username}", username);
            _displayService.DisplayError("An error occurred while fetching person details. Please try again later.");
        }
    }

    public bool ValidateUsername(string? username)
    {
        return !string.IsNullOrWhiteSpace(username);
    }

    private static string CenterText(string text, int width)
    {
        if (text.Length >= width)
            return text;

        int totalPadding = width - text.Length;
        int leftPadding = totalPadding / 2;
        int rightPadding = totalPadding - leftPadding;

        return new string(' ', leftPadding) + text + new string(' ', rightPadding);
    }
}
