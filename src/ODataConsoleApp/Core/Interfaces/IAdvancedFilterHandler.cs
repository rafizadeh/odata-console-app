namespace ODataConsoleApp.Interfaces;

using ODataConsoleApp.Models;

public interface IAdvancedFilterHandler
{
    Task<FilterCriteria?> PromptAsync();
}
