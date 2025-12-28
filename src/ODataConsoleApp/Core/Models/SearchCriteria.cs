namespace ODataConsoleApp.Models;

public class SearchCriteria
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? UserName { get; set; }
    public SearchType SearchType { get; set; } = SearchType.ExactMatch;

    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(FirstName) &&
               string.IsNullOrWhiteSpace(LastName) &&
               string.IsNullOrWhiteSpace(UserName);
    }
}
