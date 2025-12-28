namespace ODataConsoleApp.Infrastructure.Search;

using System;
using System.Linq;

public static class ODataStringHelper
{
    public static string EscapeAndValidate(string value)
    {
        ValidateValue(value);
        return Escape(value);
    }

    public static string Escape(string value)
    {
        return value.Replace("'", "''");
    }

    public static void ValidateValue(string value)
    {
        if (value.Any(c => char.IsControl(c) && c != '\t' && c != '\r' && c != '\n'))
        {
            throw new ArgumentException(
                "Search value contains invalid control characters.",
                nameof(value));
        }
    }
}