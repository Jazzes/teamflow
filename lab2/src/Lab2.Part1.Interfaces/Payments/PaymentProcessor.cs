namespace Lab2.Part1.Interfaces.Payments;

/// <summary>
/// Обработчик платежей. Работает с любым способом оплаты через IPayable.
/// </summary>
public static class PaymentProcessor
{
    public static bool ProcessPayment(IPayable method, decimal amount)
    {
        if (amount <= 0)
        {
            Console.WriteLine($"  Сумма {amount:N2} некорректна, платёж не отправлен");
            return false;
        }

        Console.WriteLine($"  Платёж на {amount:N2} руб., способ: {method.Title}");
        return method.Pay(amount);
    }
}
