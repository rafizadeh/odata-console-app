namespace ODataConsoleApp.Interfaces;

using ODataConsoleApp.Models;

public interface IDisplayService
{
    void DisplayPeopleList(PaginatedResult<Person> people);

    void DisplayPersonDetails(Person person);

    void DisplayEmptyResult(string message);

    void DisplayError(string message);

    void DisplayFilterSummary(SearchCriteria? searchCriteria, FilterCriteria? filterCriteria, int resultCount);
}
