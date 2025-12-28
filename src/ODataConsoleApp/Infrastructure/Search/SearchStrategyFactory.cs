namespace ODataConsoleApp.Infrastructure.Search;

using System;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

public class SearchStrategyFactory
{
    public ISearchStrategy CreateStrategy(SearchType searchType)
    {
        return searchType switch
        {
            SearchType.ExactMatch => new ExactMatchStrategy(),
            SearchType.Contains => new ContainsStrategy(),
            SearchType.StartsWith => new StartsWithStrategy(),
            _ => throw new ArgumentException(
                $"Unsupported search type: {searchType}. Valid values are: {string.Join(", ", Enum.GetNames(typeof(SearchType)))}",
                nameof(searchType))
        };
    }
}
