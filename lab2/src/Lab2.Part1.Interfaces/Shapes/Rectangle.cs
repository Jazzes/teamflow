namespace Lab2.Part1.Interfaces.Shapes;

/// <summary>
/// Прямоугольник, заданный левым верхним углом, шириной и высотой.
/// </summary>
public class Rectangle : IDrawable, IShape, IMovable
{
    public Point TopLeft { get; }

    public double Width { get; }

    public double Height { get; }

    public string Name => "Прямоугольник";

    public Rectangle(Point topLeft, double width, double height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "Стороны прямоугольника должны быть положительными.");
        }

        TopLeft = topLeft;
        Width = width;
        Height = height;
    }

    public void Draw()
    {
        Console.WriteLine($"  Рисую прямоугольник: левый верхний угол {TopLeft}, размер {Width} x {Height}");
    }

    public double GetArea() => Width * Height;

    public double GetPerimeter() => 2 * (Width + Height);

    public void Move(int x, int y) => TopLeft.Move(x, y);
}
