namespace Lab2.Part1.Interfaces.Devices.After;

/// <summary>Принтер реализует только то, что умеет.</summary>
public class Printer : IPrinter
{
    public void Print(string document)
    {
        Console.WriteLine($"  [Принтер] Печатаю «{document}»");
    }
}
