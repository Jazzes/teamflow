namespace Lab2.Part1.Interfaces.Shapes;

/// <summary>
/// Точка на плоскости с целочисленными координатами.
/// Координаты меняются только через метод Move интерфейса IMovable.
/// </summary>
public class Point : IMovable
{
    public int X { get; private set; }

    public int Y { get; private set; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void Move(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString() => $"({X}; {Y})";
}
