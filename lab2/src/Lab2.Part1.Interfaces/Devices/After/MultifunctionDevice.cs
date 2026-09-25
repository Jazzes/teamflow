namespace Lab2.Part1.Interfaces.Devices.After;

/// <summary>
/// МФУ собирает нужный набор возможностей из трёх узких интерфейсов.
/// </summary>
public class MultifunctionDevice : IPrinter, IScanner, IFax
{
    public void Print(string document)
    {
        Console.WriteLine($"  [МФУ] Печатаю «{document}»");
    }

    public void Scan(string document)
    {
        Console.WriteLine($"  [МФУ] Сканирую «{document}»");
    }

    public void SendFax(string document, string phoneNumber)
    {
        Console.WriteLine($"  [МФУ] Отправляю факс «{document}» на номер {phoneNumber}");
    }
}
