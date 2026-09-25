namespace Lab2.Part1.Interfaces.Devices.After;

/// <summary>Устройство, умеющее отправлять факс.</summary>
public interface IFax
{
    void SendFax(string document, string phoneNumber);
}
