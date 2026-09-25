namespace Lab2.Part1.Interfaces.Devices.Before;

/// <summary>
/// Сканер, вынужденный реализовать весь IDevice.
/// Методы Print и Fax пустые.
/// </summary>
public class Scanner : IDevice
{
    public void Print(string document)
    {
    }

    public void Scan(string document)
    {
        Console.WriteLine($"  [Сканер] Сканирую «{document}»");
    }

    public void Fax(string document, string phoneNumber)
    {
    }
}
