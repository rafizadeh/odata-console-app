namespace ODataConsoleApp.Infrastructure.Search;

using ODataConsoleApp.Interfaces;

public class ExactMatchStrategy : BaseSearchStrategy
{
    protected override string BuildFieldFilter(string fieldName, string value)
    {
        var escapedValue = EscapeODataValue(value);
        return $"{fieldName} eq '{escapedValue}'";
    }
}