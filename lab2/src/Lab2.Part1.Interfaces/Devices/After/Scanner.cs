namespace Lab2.Part1.Interfaces.Devices.After;

/// <summary>Сканер реализует только сканирование.</summary>
public class Scanner : IScanner
{
    public void Scan(string document)
    {
        Console.WriteLine($"  [Сканер] Сканирую «{document}»");
    }
}
