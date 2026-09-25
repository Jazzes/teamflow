namespace Lab2.Part1.Interfaces.Shapes;

/// <summary>
/// Круг. Реализует сразу три интерфейса: его можно нарисовать,
/// посчитать площадь и периметр и переместить (через центр).
/// </summary>
public class Circle : IDrawable, IShape, IMovable
{
    public Point Center { get; }

    public double Radius { get; }

    public string Name => "Круг";

    public Circle(Point center, double radius)
    {
        if (radius <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radius), "Радиус должен быть положительным.");
        }

        Center = center;
        Radius = radius;
    }

    public void Draw()
    {
        Console.WriteLine($"  Рисую круг: центр {Center}, радиус {Radius}");
    }

    public double GetArea() => Math.PI * Radius * Radius;

    public double GetPerimeter() => 2 * Math.PI * Radius;

    public void Move(int x, int y) => Center.Move(x, y);
}
