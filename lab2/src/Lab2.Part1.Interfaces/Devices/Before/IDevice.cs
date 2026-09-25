namespace Lab2.Part1.Interfaces.Devices.Before;

/// <summary>
/// «Толстый» интерфейс офисного устройства. Пример нарушения принципа ISP:
/// любой класс обязан реализовать все три операции, даже если физически их не умеет.
/// </summary>
public interface IDevice
{
    void Print(string document);

    void Scan(string document);

    void Fax(string document, string phoneNumber);
}
