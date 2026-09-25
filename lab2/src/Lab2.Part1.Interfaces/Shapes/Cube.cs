namespace Lab2.Part1.Interfaces.Shapes;

/// <summary>
/// Куб с ребром заданной длины.
/// </summary>
public class Cube : I3DShape
{
    public double Edge { get; }

    public string Name => "Куб";

    public Cube(double edge)
    {
        if (edge <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(edge), "Ребро куба должно быть положительным.");
        }

        Edge = edge;
    }

    /// <summary>Площадь полной поверхности: 6 граней по a^2.</summary>
    public double GetArea() => 6 * Edge * Edge;

    /// <summary>Сумма длин 12 рёбер.</summary>
    public double GetPerimeter() => 12 * Edge;

    public double GetVolume() => Edge * Edge * Edge;
}
