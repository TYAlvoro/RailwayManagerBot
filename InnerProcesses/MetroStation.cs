using CsvHelper.Configuration.Attributes;

namespace InnerProcesses;

public class MetroStation
{
    [Name("ID")]
    public int Id { get; set; }

    [Name("NameOfStation")]
    public string NameOfStation { get; set; }
    
    [Name("Line")]
    public string Line { get; set; }
    
    [Name("Longitude_WGS84")]
    public double Longitude { get; set; }
    
    [Name("Latitude_WGS84")]
    public double Latitude { get; set; }
    
    [Name("AdmArea")]
    public string AdmArea { get; set; }
    
    [Name("District")]
    public string District { get; set; }
    
    [Name("Year")]
    public int Year { get; set; }
    
    [Name("Month")]
    public string Month { get; set; }
    
    [Name("global_id")]
    public long GlobalId { get; set; }
    
    [Name("geodata_center")]
    public string GeodataCenter { get; set; }
    
    [Name("geoarea")]
    public string Geoarea { get; set; }
}