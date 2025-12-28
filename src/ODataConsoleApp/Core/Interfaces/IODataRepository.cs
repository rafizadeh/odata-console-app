namespace ODataConsoleApp.Interfaces;

using ODataConsoleApp.Models;

public interface IODataRepository
{
    Task<PaginatedResult<Person>> GetPeopleAsync(int skip, int top);
    Task<IEnumerable<Person>> SearchPeopleAsync(string filterQuery);
    Task<Person?> GetPersonByUsernameAsync(string username, bool includeRelated = false);
}
