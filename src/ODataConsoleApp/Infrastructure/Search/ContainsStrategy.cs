namespace ODataConsoleApp.Infrastructure.Search;

using ODataConsoleApp.Interfaces;

public class ContainsStrategy : BaseSearchStrategy
{
    protected override string BuildFieldFilter(string fieldName, string value)
    {
        var escapedValue = EscapeODataValue(value);
        return $"contains({fieldName}, '{escapedValue}')";
    }
}
