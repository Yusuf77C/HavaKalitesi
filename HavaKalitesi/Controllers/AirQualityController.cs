using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
public class AirQualityController : ControllerBase
{
    private readonly string _connString = "Host=localhost;Database=air_quality_data;Username=postgres;Password=123";

    [HttpGet]
    public async Task<IActionResult> GetAllData()
    {
        var airQualityData = new List<AirQualityData>();
        
        using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();
        
        using var cmd = new NpgsqlCommand(@"
            SELECT DISTINCT ON (city) 
                city, aqi, dominant_pollutant, latitude, longitude, timestamp
            FROM air_quality_data
            ORDER BY city, timestamp DESC", 
            conn);
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            airQualityData.Add(new AirQualityData
            {
                City = reader.GetString(0),
                AQI = reader.GetInt32(1),
                DominantPollutant = reader.GetString(2),
                Latitude = reader.GetDouble(3),
                Longitude = reader.GetDouble(4),
                Timestamp = reader.GetDateTime(5)
            });
        }
        
        return Ok(airQualityData);
    }
}

public class AirQualityData
{
    public string? City { get; set; }
    public int AQI { get; set; }
    public string? DominantPollutant { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public DateTime Timestamp { get; set; }
} 