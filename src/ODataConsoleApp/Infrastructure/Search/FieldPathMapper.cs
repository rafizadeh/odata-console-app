namespace ODataConsoleApp.Infrastructure.Search;

using ODataConsoleApp.Models;

public static class FieldPathMapper
{
    private static readonly FilterableField[] _filterableFields =
    {
        new FilterableField
        {
            FieldName = "FirstName",
            DisplayName = "First Name",
            ODataPath = "FirstName",
            IsNested = false,
            FieldType = FieldType.String
        },
        new FilterableField
        {
            FieldName = "LastName",
            DisplayName = "Last Name",
            ODataPath = "LastName",
            IsNested = false,
            FieldType = FieldType.String
        },
        new FilterableField
        {
            FieldName = "UserName",
            DisplayName = "User Name",
            ODataPath = "UserName",
            IsNested = false,
            FieldType = FieldType.String
        },
        new FilterableField
        {
            FieldName = "Age",
            DisplayName = "Age",
            ODataPath = "Age",
            IsNested = false,
            FieldType = FieldType.Number
        },
        new FilterableField
        {
            FieldName = "Gender",
            DisplayName = "Gender",
            ODataPath = "Gender",
            IsNested = false,
            FieldType = FieldType.String
        },
        new FilterableField
        {
            FieldName = "City",
            DisplayName = "City Name",
            ODataPath = "AddressInfo/any(a: {0}(a/City/Name, '{1}'))",
            IsNested = true,
            FieldType = FieldType.String
        },
        new FilterableField
        {
            FieldName = "Friend",
            DisplayName = "Friend Name",
            ODataPath = "Friends/any(f: {0}(f/FirstName, '{1}'))",
            IsNested = true,
            FieldType = FieldType.String
        },
        new FilterableField
        {
            FieldName = "Trip",
            DisplayName = "Trip Name",
            ODataPath = "Trips/any(t: {0}(t/Name, '{1}'))",
            IsNested = true,
            FieldType = FieldType.String
        }
    };

    public static FilterableField[] GetFilterableFields()
    {
        return _filterableFields;
    }

    public static string BuildNestedExpression(FilterableField field, string value, SearchType searchType)
    {
        if (field?.ODataPath == null)
        {
            throw new ArgumentNullException(nameof(field), "Field and its ODataPath cannot be null.");
        }

        var escapedValue = value.Replace("'", "''");

        string operatorOrFunction;
        string expression;

        switch (searchType)
        {
            case SearchType.ExactMatch:
                operatorOrFunction = "eq";
                expression = field.ODataPath
                    .Replace("{0}(", "")      
                    .Replace(", '{1}')", $" {operatorOrFunction} '{escapedValue}'"); 
                break;

            case SearchType.Contains:
                operatorOrFunction = "contains";
                expression = string.Format(field.ODataPath, operatorOrFunction, escapedValue);
                break;

            case SearchType.StartsWith:
                operatorOrFunction = "startswith";
                expression = string.Format(field.ODataPath, operatorOrFunction, escapedValue);
                break;

            default:
                throw new ArgumentException($"Unsupported search type: {searchType}", nameof(searchType));
        }

        return expression;
    }
}