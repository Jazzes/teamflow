namespace Lab2.Part1.Interfaces.Devices.After;

/// <summary>
/// Клиентский код офиса. Каждый метод требует ровно ту возможность, которая ему нужна,
/// поэтому передать в SendContract обычный принтер просто не получится: не скомпилируется.
/// </summary>
public static class OfficeService
{
    public static void PrintReport(IPrinter printer, string report)
    {
        printer.Print(report);
    }

    public static void DigitizeArchive(IScanner scanner, IEnumerable<string> documents)
    {
        foreach (var document in documents)
        {
            scanner.Scan(document);
        }
    }

    public static void SendContract(IFax fax, string contract, string phoneNumber)
    {
        fax.SendFax(contract, phoneNumber);
    }
}
