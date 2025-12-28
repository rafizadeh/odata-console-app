namespace ODataConsoleApp.Models;

public class Person
{
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Gender { get; set; }
    public int? Age { get; set; }
    public List<string> Emails { get; set; } = new();
    public string? FavoriteFeature { get; set; }
    public List<string> Features { get; set; } = new();
    public List<AddressInfo> AddressInfo { get; set; } = new();
    public AddressInfo? HomeAddress { get; set; }
    public List<Friend> Friends { get; set; } = new();
    public List<Trip> Trips { get; set; } = new();
}
