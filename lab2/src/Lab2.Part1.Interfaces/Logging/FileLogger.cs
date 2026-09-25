namespace Lab2.Part1.Interfaces.Logging;

/// <summary>Дописывает сообщения в текстовый файл.</summary>
public class FileLogger : ILogger
{
    public string FilePath { get; }

    public FileLogger(string filePath)
    {
        FilePath = filePath;

        var directory = Path.GetDirectoryName(Path.GetFullPath(filePath));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public void Log(string message)
    {
        File.AppendAllText(FilePath, $"[file {DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
    }
}
