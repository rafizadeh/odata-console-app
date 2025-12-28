namespace ODataConsoleApp.Models;

public class FilterCriteria
{
    public List<FieldFilter> Filters { get; set; } = new List<FieldFilter>();

    public bool IsEmpty()
    {
        if (Filters == null || Filters.Count == 0)
        {
            return true;
        }

        return Filters.All(f => string.IsNullOrWhiteSpace(f.Value));
    }
}