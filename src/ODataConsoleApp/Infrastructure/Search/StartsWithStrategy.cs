namespace ODataConsoleApp.Infrastructure.Search;

using ODataConsoleApp.Interfaces;

public class StartsWithStrategy : BaseSearchStrategy
{
    protected override string BuildFieldFilter(string fieldName, string value)
    {
        var escapedValue = EscapeODataValue(value);
        return $"startswith({fieldName}, '{escapedValue}')";
    }
}