namespace ODataConsoleApp.Interfaces;

public interface IPersonDetailsHandler
{
    Task DisplayPersonAsync(string username);

    bool ValidateUsername(string? username);
}
