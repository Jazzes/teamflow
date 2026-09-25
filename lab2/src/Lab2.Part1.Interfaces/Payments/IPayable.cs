namespace Lab2.Part1.Interfaces.Payments;

/// <summary>
/// Способ оплаты.
/// </summary>
public interface IPayable
{
    /// <summary>Название способа оплаты для вывода.</summary>
    string Title { get; }

    /// <summary>
    /// Пытается списать сумму. Возвращает true, если оплата прошла.
    /// </summary>
    bool Pay(decimal amount);
}
