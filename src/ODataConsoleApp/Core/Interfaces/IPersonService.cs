namespace ODataConsoleApp.Interfaces;

using ODataConsoleApp.Models;

public interface IPersonService
{
    Task<PaginatedResult<Person>> GetPeopleAsync(int page, int pageSize);
    Task<IEnumerable<Person>> SearchPeopleAsync(SearchCriteria criteria);
    Task<Person?> GetPersonDetailsAsync(string username);

    Task<IEnumerable<Person>> FilterPeopleAsync(FilterCriteria criteria);
}
