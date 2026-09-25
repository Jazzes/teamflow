namespace Lab2.Part1.Interfaces.Devices.Before;

/// <summary>
/// Принтер, вынужденный реализовать весь IDevice.
/// Методы Scan и Fax пустые: вызов проходит молча и ничего не делает.
/// </summary>
public class Printer : IDevice
{
    public void Print(string document)
    {
        Console.WriteLine($"  [Принтер] Печатаю «{document}»");
    }

    public void Scan(string document)
    {
    }

    public void Fax(string document, string phoneNumber)
    {
    }
}
