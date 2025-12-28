namespace ODataConsoleApp.Models;

public class Trip
{
    public int TripId { get; set; }
    public Guid ShareId { get; set; }
    public string? Name { get; set; }
    public float Budget { get; set; }
    public string? Description { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
}
