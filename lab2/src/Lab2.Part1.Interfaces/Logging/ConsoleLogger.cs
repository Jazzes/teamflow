namespace Lab2.Part1.Interfaces.Logging;

/// <summary>Пишет сообщения в консоль с отметкой времени.</summary>
public class ConsoleLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"  [console {DateTime.Now:HH:mm:ss}] {message}");
    }
}
