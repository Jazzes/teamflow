namespace Lab2.Part1.Interfaces.Shapes;

/// <summary>
/// Методы, которые работают с фигурами только через интерфейсы
/// и ничего не знают о конкретных классах.
/// </summary>
public static class ShapeService
{
    /// <summary>
    /// Вызывает Draw у каждого элемента списка.
    /// </summary>
    public static void DrawAll(List<IDrawable> shapes)
    {
        foreach (var shape in shapes)
        {
            shape.Draw();
        }
    }

    /// <summary>
    /// Выводит площадь и периметр любой фигуры. Если фигура объёмная,
    /// дополнительно выводится объём (проверка через сопоставление с образцом).
    /// </summary>
    public static void PrintShapeInfo(IShape shape)
    {
        Console.WriteLine($"  {shape.Name}: площадь = {shape.GetArea():F2}, периметр = {shape.GetPerimeter():F2}");

        if (shape is I3DShape solid)
        {
            Console.WriteLine($"  {shape.Name}: объём = {solid.GetVolume():F2}");
        }
    }
}
