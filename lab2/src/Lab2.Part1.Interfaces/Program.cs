using System.Globalization;
using System.Text;
using Lab2.Part1.Interfaces.Devices.After;
using Lab2.Part1.Interfaces.Logging;
using Lab2.Part1.Interfaces.Payments;
using Lab2.Part1.Interfaces.Shapes;
using OldDevices = Lab2.Part1.Interfaces.Devices.Before;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ru-RU");

Header("Задание 1.1. IMovable и Point");
var point = new Point(0, 0);
Console.WriteLine($"  Исходная точка: {point}");
IMovable movable = point;
movable.Move(5, -3);
Console.WriteLine($"  После Move(5, -3): {point}");

Header("Задание 1.2. IDrawable, Circle, Rectangle, DrawAll");
var circle = new Circle(new Point(0, 0), 3);
var rectangle = new Rectangle(new Point(2, 8), 4, 2.5);
ShapeService.DrawAll(new List<IDrawable> { circle, rectangle });
Console.WriteLine("  Перемещаем обе фигуры в (10; 10) через IMovable и рисуем снова:");
foreach (IMovable item in new IMovable[] { circle, rectangle })
{
    item.Move(10, 10);
}
ShapeService.DrawAll(new List<IDrawable> { circle, rectangle });

Header("Задание 2. IShape, I3DShape, PrintShapeInfo");
var shapes = new List<IShape> { circle, rectangle, new Cube(2) };
foreach (var shape in shapes)
{
    ShapeService.PrintShapeInfo(shape);
}

Header("Задание 3.1. «Толстый» интерфейс IDevice");
var oldDevices = new List<OldDevices.IDevice> { new OldDevices.Printer(), new OldDevices.Scanner() };
foreach (var device in oldDevices)
{
    Console.WriteLine($"  {device.GetType().Name}: вызываем Print, Scan и Fax");
    device.Print("Отчёт.docx");
    device.Scan("Паспорт.pdf");
    device.Fax("Договор.pdf", "+7 495 000-00-00");
}
Console.WriteLine("  Половина вызовов молча ничего не сделала: интерфейс обещает больше, чем умеет класс.");

Header("Задания 3.2 и 3.3. Разделённые интерфейсы IPrinter, IScanner, IFax");
var printer = new Printer();
var scanner = new Scanner();
var mfu = new MultifunctionDevice();
OfficeService.PrintReport(printer, "Отчёт.docx");
OfficeService.DigitizeArchive(scanner, new[] { "Приказ №1.pdf", "Приказ №2.pdf" });
OfficeService.PrintReport(mfu, "Счёт.pdf");
OfficeService.SendContract(mfu, "Договор.pdf", "+7 495 000-00-00");
Console.WriteLine($"  Printer реализует IFax? {printer is IFax}");
Console.WriteLine($"  МФУ реализует IPrinter, IScanner, IFax? {mfu is IPrinter && mfu is IScanner && mfu is IFax}");

Header("Задание 4.1. IPayable и ProcessPayment");
IPayable card = new CreditCard("2200 1234 5678 9010", 5000m);
IPayable cash = new Cash(1500m);
PaymentProcessor.ProcessPayment(card, 3200m);
PaymentProcessor.ProcessPayment(cash, 990.50m);
PaymentProcessor.ProcessPayment(card, 2500m);
PaymentProcessor.ProcessPayment(cash, -10m);

Header("Задание 4.2. ILogger, ConsoleLogger, FileLogger, DoWork");
Worker.DoWork(new ConsoleLogger());
var logPath = Path.Combine("logs", "work.log");
if (File.Exists(logPath))
{
    File.Delete(logPath);
}
var fileLogger = new FileLogger(logPath);
Worker.DoWork(fileLogger);
Console.WriteLine($"  Содержимое файла {fileLogger.FilePath}:");
foreach (var line in File.ReadAllLines(fileLogger.FilePath))
{
    Console.WriteLine($"  {line}");
}

static void Header(string title)
{
    Console.WriteLine();
    Console.WriteLine($"=== {title} ===");
}
