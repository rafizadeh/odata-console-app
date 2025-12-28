namespace ODataConsoleApp.Interfaces;

public interface IConsoleWrapper
{
    string? ReadLine();

    void Write(string value);

    void WriteLine(string value);

    void WriteLine();

    void Clear();

    void ReadKey();
}
