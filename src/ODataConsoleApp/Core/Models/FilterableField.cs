namespace ODataConsoleApp.Models;

public class FilterableField
{
    public string? FieldName { get; set; }

    public string? DisplayName { get; set; }

    public string? ODataPath { get; set; }

    public bool IsNested { get; set; }

    public FieldType FieldType { get; set; } = FieldType.String;
}
