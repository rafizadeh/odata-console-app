namespace ODataConsoleApp.UI;

using ODataConsoleApp.Infrastructure.Search;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

public class AdvancedFilterHandler : IAdvancedFilterHandler
{
    private readonly IConsoleWrapper _consoleWrapper;
    private readonly FilterCriteria _criteria;

    public AdvancedFilterHandler(IConsoleWrapper consoleWrapper, IDisplayService displayService)
    {
        _consoleWrapper = consoleWrapper ?? throw new ArgumentNullException(nameof(consoleWrapper));
        _criteria = new FilterCriteria();
    }

    public Task<FilterCriteria?> PromptAsync()
    {
        while (true)
        {
            DisplayMenu();

            var choice = _consoleWrapper.ReadLine()?.Trim().ToUpperInvariant();

            switch (choice)
            {
                case "A":
                    AddFilter();
                    break;
                case "R":
                    RemoveFilter();
                    break;
                case "C":
                    ClearFilters();
                    break;
                case "S":
                    return Task.FromResult<FilterCriteria?>(_criteria);
                case "M":
                    return Task.FromResult<FilterCriteria?>(null);
                default:
                    _consoleWrapper.WriteLine("Invalid choice. Please try again.");
                    _consoleWrapper.WriteLine();
                    break;
            }
        }
    }

    private void DisplayMenu()
    {
        _consoleWrapper.WriteLine();
        _consoleWrapper.WriteLine("═══════════════════════════════════════════════════════════");
        _consoleWrapper.WriteLine("                  Advanced Filter Builder");
        _consoleWrapper.WriteLine("═══════════════════════════════════════════════════════════");

        // Display current filters
        if (_criteria.Filters.Count > 0)
        {
            _consoleWrapper.WriteLine();
            _consoleWrapper.WriteLine("Current Filters:");
            for (int i = 0; i < _criteria.Filters.Count; i++)
            {
                var filter = _criteria.Filters[i];
                _consoleWrapper.WriteLine($"  {i + 1}. {filter.FieldName} {filter.SearchType} '{filter.Value}'");
            }
            _consoleWrapper.WriteLine();
        }
        else
        {
            _consoleWrapper.WriteLine();
            _consoleWrapper.WriteLine("No filters added yet.");
            _consoleWrapper.WriteLine();
        }

        _consoleWrapper.WriteLine("───────────────────────────────────────────────────────────");
        _consoleWrapper.WriteLine("[A] Add Filter");
        _consoleWrapper.WriteLine("[R] Remove Filter");
        _consoleWrapper.WriteLine("[C] Clear All Filters");
        _consoleWrapper.WriteLine("[S] Search");
        _consoleWrapper.WriteLine("[M] Cancel");
        _consoleWrapper.WriteLine("───────────────────────────────────────────────────────────");
        _consoleWrapper.Write("Your choice: ");
    }

    private void AddFilter()
    {
        _consoleWrapper.WriteLine();

        var fields = FieldPathMapper.GetFilterableFields();
        _consoleWrapper.WriteLine("Select field to filter:");
        for (int i = 0; i < fields.Length; i++)
        {
            _consoleWrapper.WriteLine($"  {i + 1}. {fields[i].DisplayName}");
        }
        _consoleWrapper.Write("Field number: ");

        var fieldInput = _consoleWrapper.ReadLine();
        if (!int.TryParse(fieldInput, out int fieldIndex) || fieldIndex < 1 || fieldIndex > fields.Length)
        {
            _consoleWrapper.WriteLine("Invalid field selection.");
            return;
        }

        var selectedField = fields[fieldIndex - 1];

        _consoleWrapper.WriteLine();
        _consoleWrapper.WriteLine("Select search type:");
        _consoleWrapper.WriteLine("  1. ExactMatch");
        _consoleWrapper.WriteLine("  2. Contains");
        _consoleWrapper.WriteLine("  3. StartsWith");
        _consoleWrapper.Write("Search type number: ");

        var typeInput = _consoleWrapper.ReadLine();
        if (!int.TryParse(typeInput, out int typeIndex) || typeIndex < 1 || typeIndex > 3)
        {
            _consoleWrapper.WriteLine("Invalid search type selection.");
            return;
        }

        var searchType = typeIndex switch
        {
            1 => SearchType.ExactMatch,
            2 => SearchType.Contains,
            3 => SearchType.StartsWith,
            _ => SearchType.Contains
        };

        _consoleWrapper.WriteLine();
        _consoleWrapper.Write($"Enter value for {selectedField.DisplayName}: ");
        var value = _consoleWrapper.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            _consoleWrapper.WriteLine("Value cannot be empty.");
            return;
        }

        _criteria.Filters.Add(new FieldFilter
        {
            FieldName = selectedField.FieldName,
            Value = value,
            SearchType = searchType
        });

        _consoleWrapper.WriteLine();
        _consoleWrapper.WriteLine($"Filter added: {selectedField.DisplayName} {searchType} '{value}'");
    }

    private void RemoveFilter()
    {
        if (_criteria.Filters.Count == 0)
        {
            _consoleWrapper.WriteLine();
            _consoleWrapper.WriteLine("No filters to remove.");
            return;
        }

        _consoleWrapper.WriteLine();
        _consoleWrapper.Write("Enter filter number to remove: ");
        var input = _consoleWrapper.ReadLine();

        if (!int.TryParse(input, out int index) || index < 1 || index > _criteria.Filters.Count)
        {
            _consoleWrapper.WriteLine("Invalid filter number.");
            return;
        }

        var removedFilter = _criteria.Filters[index - 1];
        _criteria.Filters.RemoveAt(index - 1);

        _consoleWrapper.WriteLine();
        _consoleWrapper.WriteLine($"Filter removed: {removedFilter.FieldName} {removedFilter.SearchType} '{removedFilter.Value}'");
    }

    private void ClearFilters()
    {
        var count = _criteria.Filters.Count;
        _criteria.Filters.Clear();

        _consoleWrapper.WriteLine();
        _consoleWrapper.WriteLine($"All filters cleared ({count} filter(s) removed).");
    }
}