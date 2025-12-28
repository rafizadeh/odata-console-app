namespace ODataConsoleApp.UI;

using ODataConsoleApp.Interfaces;

public class ConsoleWrapper : IConsoleWrapper
{
    public string? ReadLine()
    {
        return Console.ReadLine();
    }

    public void Write(string value)
    {
        Console.Write(value);
    }

    public void WriteLine(string value)
    {
        Console.WriteLine(value);
    }

    public void WriteLine()
    {
        Console.WriteLine();
    }

    public void Clear()
    {
        Console.Clear();
    }

    public void ReadKey()
    {
        Console.ReadKey(intercept: true);
    }
}
