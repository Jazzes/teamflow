namespace Lab2.Part1.Interfaces.Logging;

/// <summary>
/// Имитация полезной работы, которая сообщает о своих шагах через ILogger.
/// </summary>
public static class Worker
{
    public static void DoWork(ILogger logger)
    {
        logger.Log("Начало обработки заказов");

        var orders = new[] { 1001, 1002, 1003 };
        foreach (var order in orders)
        {
            logger.Log($"Заказ №{order} обработан");
        }

        logger.Log($"Готово, обработано заказов: {orders.Length}");
    }
}
