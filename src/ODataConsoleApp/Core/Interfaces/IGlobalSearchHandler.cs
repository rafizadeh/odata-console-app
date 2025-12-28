namespace ODataConsoleApp.Interfaces;

using ODataConsoleApp.Models;

public interface IGlobalSearchHandler
{
    Task<SearchCriteria?> PromptAsync();
}
