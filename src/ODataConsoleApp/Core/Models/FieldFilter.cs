namespace ODataConsoleApp.Models;

public class FieldFilter
{
    public string? FieldName { get; set; }
    
    public string? Value { get; set; }

    public SearchType SearchType { get; set; } = SearchType.ExactMatch;
}
