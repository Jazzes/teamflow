namespace Lab2.Part1.Interfaces.Shapes;

/// <summary>
/// Объект, положение которого на плоскости можно изменить.
/// </summary>
public interface IMovable
{
    /// <summary>
    /// Переносит объект в точку с координатами (x; y).
    /// </summary>
    void Move(int x, int y);
}
