namespace InnerProcesses;

public class DataTool
{
    public MetroStation[] Filter(MetroStation[] stations, string filterValue, string filterField)
    {
        return filterField switch
        {
            "name" => stations.Where(station => station.NameOfStation == filterValue).ToArray(),
            "line" => stations.Where(station => station.Line == filterValue).ToArray(),
            _ => throw new ArgumentException("Недопустимое значение поля!")
        };
    }
    
    public MetroStation[] FilterByTwoFields(MetroStation[] stations, string nameValue, string monthValue)
    {
        return stations.Where(station => station.NameOfStation == nameValue || station.Month == monthValue).ToArray();
    }

    public MetroStation[] SortByYear(MetroStation[] stations)
    {
       return stations.OrderBy(station => station.Year).ToArray();
    }
    
    public MetroStation[] SortByName(MetroStation[] stations)
    {
        return stations.OrderBy(station => station.NameOfStation).ToArray();
    }
}