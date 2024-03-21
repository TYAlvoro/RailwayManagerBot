namespace InnerProcesses;

/// <summary>
/// Класс для сортировки и фильтрации данных.
/// </summary>
public class DataTool
{
    /// <summary>
    /// Метод, реализующий фильтрацию по заданному полю и значению фильтра.
    /// </summary>
    /// <param name="stations">Массив объектов для фильтрации.</param>
    /// <param name="filterValue">Значение фильтра.</param>
    /// <param name="filterField">Поле для фильтрации.</param>
    /// <returns>Массив объектов станций.</returns>
    /// <exception cref="ArgumentException">Если значение поля фильтрации будет недекларированное в тз.</exception>
    public MetroStation[] Filter(MetroStation[] stations, string filterValue, string filterField)
    {
        // Возвращается отфильтрованный массив объектов в зависимости от выбранного поля для фильтрации.
        return filterField switch
        {
            "name" => stations.Where(station => station.NameOfStation == filterValue).ToArray(),
            "line" => stations.Where(station => station.Line == filterValue).ToArray(),
            _ => throw new ArgumentException("Недопустимое значение поля!")
        };
    }
    
    /// <summary>
    /// Метод, реализующий фильтрацию по двум полям.
    /// </summary>
    /// <param name="stations">Массив объектов для фильтрации.</param>
    /// <param name="nameValue">Значение названия станции для фильтрации.</param>
    /// <param name="monthValue">Значение названия месяца для фильтрации.</param>
    /// <returns>Массив объектов станций.</returns>
    public MetroStation[] FilterByTwoFields(MetroStation[] stations, string nameValue, string monthValue)
    {
        return stations.Where(station => station.NameOfStation == nameValue || station.Month == monthValue).ToArray();
    }

    /// <summary>
    /// Метод, реализующий сортировку по году.
    /// </summary>
    /// <param name="stations">Массив объектов для сортировки.</param>
    /// <returns>Массив отсортированных объектов.</returns>
    public MetroStation[] SortByYear(MetroStation[] stations)
    {
       return stations.OrderBy(station => station.Year).ToArray();
    }
    
    /// <summary>
    /// Метод, реализующий сортировку по названию станции.
    /// </summary>
    /// <param name="stations">Массив объектов для сортировки.</param>
    /// <returns>Массив отсортированных объектов.</returns>
    public MetroStation[] SortByName(MetroStation[] stations)
    {
        return stations.OrderBy(station => station.NameOfStation).ToArray();
    }
}