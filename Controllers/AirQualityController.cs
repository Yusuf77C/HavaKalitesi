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
                city, aqi, dominant_pollutant, latitude, longitude, temperature, timestamp
            FROM air_quality_data
            ORDER BY city, timestamp DESC", 
            conn);
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            try
            {
                airQualityData.Add(new AirQualityData
                {
                    City = reader.GetString(0),
                    AQI = reader.GetInt32(1),
                    DominantPollutant = reader.GetString(2),
                    Latitude = reader.GetDouble(3),
                    Longitude = reader.GetDouble(4),
                    Temperature = !reader.IsDBNull(5) ? reader.GetFloat(5) : 0f,
                    Timestamp = reader.GetDateTime(6)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading data for a city: {ex.Message}");
            }
        }
        
        return Ok(airQualityData);
    }

    [HttpGet("weekly/{city}")]
    public async Task<IActionResult> GetWeeklyData(string city)
    {
        var weeklyData = new List<WeeklyAQIData>();
        
        using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();
        
        // Son 7 günün verilerini çek ve günlük ortalama hesapla
        using var cmd = new NpgsqlCommand(@"
            WITH daily_avg AS (
                SELECT 
                    DATE(timestamp) as date,
                    AVG(aqi) as avg_aqi
                FROM air_quality_data
                WHERE city = @city
                    AND timestamp >= NOW() - INTERVAL '7 days'
                GROUP BY DATE(timestamp)
                ORDER BY DATE(timestamp)
            )
            SELECT 
                date,
                ROUND(avg_aqi::numeric, 0) as aqi
            FROM daily_avg", 
            conn);
        
        cmd.Parameters.AddWithValue("city", city);
        
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            weeklyData.Add(new WeeklyAQIData
            {
                Date = reader.GetDateTime(0),
                AQI = reader.GetInt32(1)
            });
        }
        
        return Ok(weeklyData);
    }
}

public class AirQualityData
{
    public string? City { get; set; }
    public int AQI { get; set; }
    public string? DominantPollutant { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public float Temperature { get; set; }
    public DateTime Timestamp { get; set; }
}

public class WeeklyAQIData
{
    public DateTime Date { get; set; }
    public int AQI { get; set; }
} 