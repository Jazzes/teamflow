namespace Lab2.Part1.Interfaces.Shapes;

/// <summary>
/// Геометрическая фигура, у которой можно вычислить площадь и периметр.
/// </summary>
public interface IShape
{
    /// <summary>Название фигуры для вывода пользователю.</summary>
    string Name { get; }

    double GetArea();

    double GetPerimeter();
}
