namespace ODataConsoleApp.Infrastructure.Search;

using System;
using System.Collections.Generic;
using System.Linq;
using ODataConsoleApp.Interfaces;
using ODataConsoleApp.Models;

public abstract class BaseSearchStrategy : ISearchStrategy
{
    public string BuildFilterQuery(SearchCriteria criteria)
    {
        if (criteria == null)
        {
            throw new ArgumentNullException(nameof(criteria));
        }

        if (criteria.IsEmpty())
        {
            return string.Empty;
        }

        var filters = new List<string>();

        if (!string.IsNullOrWhiteSpace(criteria.FirstName))
        {
            filters.Add(BuildFieldFilter("FirstName", criteria.FirstName.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(criteria.LastName))
        {
            filters.Add(BuildFieldFilter("LastName", criteria.LastName.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(criteria.UserName))
        {
            filters.Add(BuildFieldFilter("UserName", criteria.UserName.Trim()));
        }

        return string.Join(" or ", filters);
    }

    protected abstract string BuildFieldFilter(string fieldName, string value);

    protected static string EscapeODataValue(string value)
    {
        return ODataStringHelper.EscapeAndValidate(value);
    }
}