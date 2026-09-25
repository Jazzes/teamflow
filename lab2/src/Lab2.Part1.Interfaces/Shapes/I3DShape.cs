namespace Lab2.Part1.Interfaces.Shapes;

/// <summary>
/// Объёмная фигура. Наследует контракт плоской фигуры и добавляет объём.
/// Для тела GetArea возвращает площадь полной поверхности,
/// а GetPerimeter сумму длин всех рёбер.
/// </summary>
public interface I3DShape : IShape
{
    double GetVolume();
}
