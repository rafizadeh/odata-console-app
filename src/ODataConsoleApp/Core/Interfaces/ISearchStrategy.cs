namespace ODataConsoleApp.Interfaces;

using ODataConsoleApp   .Models;

public interface ISearchStrategy
{
    string BuildFilterQuery(SearchCriteria criteria);
}
