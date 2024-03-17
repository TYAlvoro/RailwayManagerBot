using System.Text.Json.Serialization;
using CsvHelper.Configuration.Attributes;

namespace InnerProcesses;

public class MetroStation
{
    [JsonPropertyName("ID")]
    [Name("ID")]
    public int Id { get; set; }

    [JsonPropertyName("NameOfStation")]
    [Name("NameOfStation")]
    public string? NameOfStation { get; set; }
    
    [JsonPropertyName("Line")]
    [Name("Line")]
    public string? Line { get; set; }
    
    [JsonPropertyName("Longitude_WGS84")]
    [Name("Longitude_WGS84")]
    public double Longitude { get; set; }
    
    [JsonPropertyName("Latitude_WGS84")]
    [Name("Latitude_WGS84")]
    public double Latitude { get; set; }
    
    [JsonPropertyName("AdmArea")]
    [Name("AdmArea")]
    public string? AdmArea { get; set; }
    
    [JsonPropertyName("District")]
    [Name("District")]
    public string? District { get; set; }
    
    [JsonPropertyName("Year")]
    [Name("Year")]
    public int Year { get; set; }
    
    [JsonPropertyName("Month")]
    [Name("Month")]
    public string? Month { get; set; }
    
    [JsonPropertyName("global_id")]
    [Name("global_id")]
    public long GlobalId { get; set; }
    
    [JsonPropertyName("geodata_center")]
    [Name("geodata_center")]
    public string? GeodataCenter { get; set; }
    
    [JsonPropertyName("geoarea")]
    [Name("geoarea")]
    public string? Geoarea { get; set; }
}