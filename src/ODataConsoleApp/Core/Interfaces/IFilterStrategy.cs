namespace ODataConsoleApp.Interfaces;

using ODataConsoleApp.Models;

public interface IFilterStrategy
{
    string BuildFilterQuery(FilterCriteria criteria);
}
