namespace ODataConsoleApp.Infrastructure.Search;

using System;
using System.Collections.Generic;
using System.Linq;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

public class CompositeFilterStrategy : IFilterStrategy
{
    public string BuildFilterQuery(FilterCriteria criteria)
    {
        if (criteria == null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        if (criteria.IsEmpty())
        {
            return string.Empty;
        }

        var filterExpressions = new List<string>();
        var filterableFields = FieldPathMapper.GetFilterableFields();

        foreach (var fieldFilter in criteria.Filters)
        {
            if (string.IsNullOrWhiteSpace(fieldFilter.Value))
            {
                continue;
            }

            ValidateValue(fieldFilter.Value);

            var trimmedValue = fieldFilter.Value.Trim();

            var fieldMetadata = filterableFields.FirstOrDefault(f => f.FieldName == fieldFilter.FieldName);

            string expression;

            if (fieldMetadata != null && fieldMetadata.IsNested)
            {
                expression = FieldPathMapper.BuildNestedExpression(fieldMetadata, trimmedValue, fieldFilter.SearchType);
            }
            else
            {
                expression = BuildSimpleFieldExpression(
                    fieldFilter.FieldName ?? "Unknown",
                    trimmedValue,
                    fieldFilter.SearchType);
            }

            filterExpressions.Add(expression);
        }

        return string.Join(" and ", filterExpressions);
    }

    private string BuildSimpleFieldExpression(string fieldName, string value, SearchType searchType)
    {
        var escapedValue = EscapeODataValue(value);

        return searchType switch
        {
            SearchType.ExactMatch => $"{fieldName} eq '{escapedValue}'",
            SearchType.Contains => $"contains({fieldName}, '{escapedValue}')",
            SearchType.StartsWith => $"startswith({fieldName}, '{escapedValue}')",
            _ => throw new ArgumentException($"Unsupported search type: {searchType}", nameof(searchType))
        };
    }

    private static string EscapeODataValue(string value)
    {
        return ODataStringHelper.Escape(value);
    }

    private static void ValidateValue(string value)
    {
        ODataStringHelper.ValidateValue(value);
    }
}