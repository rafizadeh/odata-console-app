namespace ODataConsoleApp.Models;

public class Friend
{
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public List<string> Emails { get; set; } = new();
}
