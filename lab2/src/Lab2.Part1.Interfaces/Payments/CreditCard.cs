namespace Lab2.Part1.Interfaces.Payments;

/// <summary>
/// Кредитная карта с кредитным лимитом. Номер хранится только в маскированном виде.
/// </summary>
public class CreditCard : IPayable
{
    private readonly string _maskedNumber;
    private decimal _availableLimit;

    public CreditCard(string cardNumber, decimal creditLimit)
    {
        var digits = new string(cardNumber.Where(char.IsDigit).ToArray());
        if (digits.Length < 12)
        {
            throw new ArgumentException("Номер карты должен содержать не меньше 12 цифр.", nameof(cardNumber));
        }

        _maskedNumber = "**** " + digits[^4..];
        _availableLimit = creditLimit;
    }

    public string Title => $"Карта {_maskedNumber}";

    public bool Pay(decimal amount)
    {
        if (amount > _availableLimit)
        {
            Console.WriteLine($"  {Title}: отказ, недостаточно лимита (доступно {_availableLimit:N2} руб.)");
            return false;
        }

        _availableLimit -= amount;
        Console.WriteLine($"  {Title}: списано {amount:N2} руб., остаток лимита {_availableLimit:N2} руб.");
        return true;
    }
}
