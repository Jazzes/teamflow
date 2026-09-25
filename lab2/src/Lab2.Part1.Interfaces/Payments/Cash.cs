namespace Lab2.Part1.Interfaces.Payments;

/// <summary>
/// Оплата наличными из кошелька. Если купюр больше, чем нужно, выдаётся сдача.
/// </summary>
public class Cash : IPayable
{
    private decimal _inWallet;

    public Cash(decimal inWallet)
    {
        _inWallet = inWallet;
    }

    public string Title => "Наличные";

    public bool Pay(decimal amount)
    {
        if (amount > _inWallet)
        {
            Console.WriteLine($"  {Title}: не хватает денег, в кошельке {_inWallet:N2} руб.");
            return false;
        }

        _inWallet -= amount;
        Console.WriteLine($"  {Title}: оплачено {amount:N2} руб., в кошельке осталось {_inWallet:N2} руб.");
        return true;
    }
}
