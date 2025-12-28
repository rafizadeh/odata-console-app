namespace ODataConsoleApp.UI;

using ODataConsoleApp.Core.Configuration;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

public class PeopleExplorerHandler : IPeopleExplorerHandler
{
    private readonly IPersonService _personService;
    private readonly IDisplayService _displayService;
    private readonly IConsoleWrapper _console;
    private readonly IGlobalSearchHandler _globalSearchHandler;
    private readonly IAdvancedFilterHandler _advancedFilterHandler;
    private readonly IPersonDetailsHandler _personDetailsHandler;
    private readonly int _pageSize;

    private int _currentPage = 1;
    private SearchCriteria? _activeSearchCriteria;
    private FilterCriteria? _activeFilterCriteria;
    private List<Person>? _filteredResults;

    public PeopleExplorerHandler(
        IPersonService personService,
        IDisplayService displayService,
        IConsoleWrapper console,
        IGlobalSearchHandler globalSearchHandler,
        IAdvancedFilterHandler advancedFilterHandler,
        IPersonDetailsHandler personDetailsHandler,
        AppSettings appSettings)
    {
        _personService = personService ?? throw new ArgumentNullException(nameof(personService));
        _displayService = displayService ?? throw new ArgumentNullException(nameof(displayService));
        _console = console ?? throw new ArgumentNullException(nameof(console));
        _globalSearchHandler = globalSearchHandler ?? throw new ArgumentNullException(nameof(globalSearchHandler));
        _advancedFilterHandler = advancedFilterHandler ?? throw new ArgumentNullException(nameof(advancedFilterHandler));
        _personDetailsHandler = personDetailsHandler ?? throw new ArgumentNullException(nameof(personDetailsHandler));

        if (appSettings == null)
            throw new ArgumentNullException(nameof(appSettings));

        _pageSize = appSettings.ODataService?.DefaultPageSize ?? 10;
    }

    public async Task RunAsync()
    {
        var shouldContinue = true;

        while (shouldContinue)
        {
            try
            {
                await LoadAndDisplayCurrentViewAsync();

                _console.Write("Your choice: ");
                var choice = _console.ReadLine()?.Trim().ToUpperInvariant();

                shouldContinue = await ProcessCommandAsync(choice);
            }
            catch (Exception ex)
            {
                _displayService.DisplayError($"Error: {ex.Message}");
            }
        }
    }

    private async Task LoadAndDisplayCurrentViewAsync()
    {
        _console.Clear();

        DisplayHeader();

        DisplayFilterSummary();

        if (HasActiveFilter())
        {
            await LoadAndDisplayFilteredResultsAsync();
        }
        else
        {
            await LoadAndDisplayPaginatedResultsAsync();
        }

        DisplayNavigationControls();
    }

    private void DisplayHeader()
    {
        _console.WriteLine("═══════════════════════════════════════════════════════════");
        _console.WriteLine("                    People Explorer");
        _console.WriteLine("═══════════════════════════════════════════════════════════");
        _console.WriteLine("[G] Global Search  [F] Advanced Filter  [C] Clear Filter");
        _console.WriteLine("[X] Exit");
        _console.WriteLine("───────────────────────────────────────────────────────────");
    }

    private void DisplayFilterSummary()
    {
        if (_activeSearchCriteria != null && !_activeSearchCriteria.IsEmpty())
        {
            var searchType = _activeSearchCriteria.SearchType.ToString().ToLower();
            var fields = new List<string>();
            if (!string.IsNullOrWhiteSpace(_activeSearchCriteria.FirstName)) fields.Add("FirstName");
            if (!string.IsNullOrWhiteSpace(_activeSearchCriteria.LastName)) fields.Add("LastName");
            if (!string.IsNullOrWhiteSpace(_activeSearchCriteria.UserName)) fields.Add("UserName");

            var fieldList = string.Join("/", fields);
            var value = _activeSearchCriteria.FirstName ?? _activeSearchCriteria.LastName ??
                       _activeSearchCriteria.UserName ?? "";

            var count = _filteredResults?.Count ?? 0;
            _console.WriteLine($"Current Filter: {fieldList} {searchType.ToString().ToLower()} '{value}' ({count} results)");
        }
        else if (_activeFilterCriteria != null && !_activeFilterCriteria.IsEmpty())
        {
            var filterDescriptions = _activeFilterCriteria.Filters
                .Select(f => $"{f.FieldName} {f.SearchType.ToString().ToLower()} '{f.Value}'")
                .ToList();

            var count = _filteredResults?.Count ?? 0;
            _console.WriteLine($"Current Filter: {string.Join(" AND ", filterDescriptions)} ({count} results)");
        }
        else
        {
            _console.WriteLine("Current Filter: None");
        }
        _console.WriteLine("───────────────────────────────────────────────────────────");
        _console.WriteLine();
    }

    private Task LoadAndDisplayFilteredResultsAsync()
    {
        if (_filteredResults == null || _filteredResults.Count == 0)
        {
            _displayService.DisplayEmptyResult("No people found matching the filter.");
            return Task.CompletedTask;
        }

        var totalPages = (int)Math.Ceiling((double)_filteredResults.Count / _pageSize);
        var skip = (_currentPage - 1) * _pageSize;
        var currentPageItems = _filteredResults.Skip(skip).Take(_pageSize).ToList();

        _console.WriteLine($"People (Page {_currentPage} of {totalPages})");
        _console.WriteLine("═══════════════════════════════════════════════════════════");
        _console.WriteLine();

        for (int i = 0; i < currentPageItems.Count; i++)
        {
            var person = currentPageItems[i];
            _console.WriteLine($" {i + 1}. {person.FirstName} {person.LastName} - {person.UserName}");
        }

        _console.WriteLine();
        return Task.CompletedTask;
    }

    private async Task<PaginatedResult<Person>?> LoadAndDisplayPaginatedResultsAsync()
    {
        var people = await _personService.GetPeopleAsync(_currentPage, _pageSize);

        if (people.Items == null || !people.Items.Any())
        {
            _displayService.DisplayEmptyResult("No people found.");
            return null;
        }

        _currentPage = people.CurrentPage;

        _console.WriteLine($"People (Page {people.CurrentPage} of {people.TotalPages})");
        _console.WriteLine("═══════════════════════════════════════════════════════════");
        _console.WriteLine();

        var itemsList = people.Items.ToList();
        for (int i = 0; i < itemsList.Count; i++)
        {
            var person = itemsList[i];
            _console.WriteLine($" {i + 1}. {person.FirstName} {person.LastName} - {person.UserName}");
        }

        _console.WriteLine();
        return people;
    }

    private void DisplayNavigationControls()
    {
        _console.WriteLine("───────────────────────────────────────────────────────────");
        _console.WriteLine("[N] Next  [P] Prev  [S] Select Person");
        _console.WriteLine();
    }

    private async Task<bool> ProcessCommandAsync(string? choice)
    {
        if (string.IsNullOrWhiteSpace(choice))
        {
            return HandleInvalidCommand();
        }

        return choice switch
        {
            "N" => await HandleNextPageAsync(),
            "P" => await HandlePreviousPageAsync(),
            "S" => await HandleSelectPersonAsync(),
            "G" => await HandleGlobalSearchAsync(),
            "F" => await HandleAdvancedFilterAsync(),
            "C" => await HandleClearFilterAsync(),
            "X" => false,
            _ => HandleInvalidCommand()
        };
    }

    private async Task<bool> HandleNextPageAsync()
    {
        var totalPages = await GetTotalPagesAsync();
        if (_currentPage >= totalPages)
        {
            _displayService.DisplayError("You are already on the last page.");
            return true;
        }

        _currentPage++;
        return true;
    }

    private Task<bool> HandlePreviousPageAsync()
    {
        if (_currentPage <= 1)
        {
            _displayService.DisplayError("You are already on the first page.");
            return Task.FromResult(true);
        }

        _currentPage--;
        return Task.FromResult(true);
    }

    private async Task<bool> HandleSelectPersonAsync()
    {
        _console.Write("Enter person number: ");
        var input = _console.ReadLine()?.Trim();

        if (!int.TryParse(input, out int personNumber) || personNumber < 1)
        {
            _displayService.DisplayError("Invalid person number. Please enter a valid number.");
            return true;
        }

        var currentPageItems = await GetCurrentPageItemsAsync();
        if (personNumber > currentPageItems.Count)
        {
            _displayService.DisplayError($"Invalid person number. Please select between 1 and {currentPageItems.Count}.");
            return true;
        }

        var selectedPerson = currentPageItems[personNumber - 1];

        _console.Clear();

        await _personDetailsHandler.DisplayPersonAsync(selectedPerson.UserName!);

        _console.WriteLine();
        _console.WriteLine("Press any key to return to the list...");
        _console.ReadKey();

        return true;
    }

    private async Task<bool> HandleGlobalSearchAsync()
    {
        var searchCriteria = await _globalSearchHandler.PromptAsync();
        if (searchCriteria == null)
        {
            return true;
        }

        var results = await _personService.SearchPeopleAsync(searchCriteria);
        _filteredResults = results.ToList();
        _activeSearchCriteria = searchCriteria;
        _activeFilterCriteria = null; 
        _currentPage = 1;

        return true;
    }

    private async Task<bool> HandleAdvancedFilterAsync()
    {
        var filterCriteria = await _advancedFilterHandler.PromptAsync();
        if (filterCriteria == null)
        {
            return true;
        }

        var results = await _personService.FilterPeopleAsync(filterCriteria);
        _filteredResults = results.ToList();
        _activeFilterCriteria = filterCriteria;
        _activeSearchCriteria = null; 
        _currentPage = 1; 

        return true;
    }

    private Task<bool> HandleClearFilterAsync()
    {
        _activeSearchCriteria = null;
        _activeFilterCriteria = null;
        _filteredResults = null;
        _currentPage = 1;

        return Task.FromResult(true);
    }

    private bool HandleInvalidCommand()
    {
        _displayService.DisplayError("Invalid command. Please try again.");
        return true;
    }

    private bool HasActiveFilter()
    {
        return _filteredResults != null;
    }

    private async Task<int> GetTotalPagesAsync()
    {
        if (HasActiveFilter())
        {
            return (int)Math.Ceiling((double)_filteredResults!.Count / _pageSize);
        }
        else
        {
            var people = await _personService.GetPeopleAsync(_currentPage, _pageSize);
            return people.TotalPages;
        }
    }

    private async Task<List<Person>> GetCurrentPageItemsAsync()
    {
        if (HasActiveFilter())
        {
            var skip = (_currentPage - 1) * _pageSize;
            return _filteredResults!.Skip(skip).Take(_pageSize).ToList();
        }
        else
        {
            var people = await _personService.GetPeopleAsync(_currentPage, _pageSize);
            return people.Items?.ToList() ?? new List<Person>();
        }
    }
}