using Npgsql;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Kestrel sunucu ayarlarını yapılandır
builder.WebHost.UseUrls("http://localhost:5000"); // Burada URL'yi doğrudan belirtiyoruz

var app = builder.Build();

// Veri çekme işlemini başlat
var apiToken = "e368bba3de018b8a400c48c2b727c1471c160d3e";
var baseUrl = "https://api.waqi.info/feed/";

var cities = new[] { 
    "adana", "adiyaman", "afyonkarahisar", "agri", "aksaray", "amasya",
    "ankara", "antalya", "ardahan", "artvin", "aydin", "balikesir",
    "bartin", "batman", "bayburt", "bilecik", "bingol", "bitlis",
    "bolu", "burdur", "bursa", "canakkale", "cankiri", "corum", "denizli",
    "diyarbakir", "duzce", "edirne", "elazig", "erzincan", "erzurum",
    "eskisehir", "gaziantep", "giresun", "gumushane", "hakkari", "hatay",
    "igdir", "isparta", "istanbul", "izmir", "kahramanmaras", "karabuk",
    "karaman", "kars", "kastamonu", "kayseri", "kilis", "kirikkale",
    "kirklareli", "kirsehir", "izmit", "konya", "kutahya", "malatya",
    "manisa", "mardin", "mersin", "mugla", "mus", "nevsehir", "nigde",
    "ordu", "osmaniye", "rize", "sakarya", "samsun", "sanliurfa",
    "siirt", "sinop", "sirnak", "sivas", "tekirdag", "tokat", "trabzon",
    "tunceli", "usak", "van", "yalova", "yozgat", "zonguldak"
};

// Veri çekme işlemini arka planda başlat
_ = Task.Run(async () => 
{
    await FetchAndSaveAirQualityData(apiToken, baseUrl, cities);
});

// Middleware'leri ekle
app.UseDefaultFiles(); // index.html'i varsayılan olarak serve et
app.UseStaticFiles();  // wwwroot klasöründeki statik dosyaları serve et
app.UseRouting();
app.MapControllers();

Console.WriteLine("Web uygulaması başlatıldı: http://localhost:5000");

app.Run();

static async Task FetchAndSaveAirQualityData(string apiToken, string baseUrl, string[] cities)
{
    using var client = new HttpClient();
    var connString = "Host=localhost;Database=air_quality_data;Username=postgres;Password=123";

    foreach (var city in cities)
    {
        try 
        {
            Console.WriteLine($"Fetching data for {city}...");
            var url = $"{baseUrl}{city}/?token={apiToken}";
            var response = await client.GetStringAsync(url);
            Console.WriteLine($"API Response for {city}: {response}");
            
            var data = JsonSerializer.Deserialize<JsonElement>(response);
            await SaveToDatabase(connString, city, data);
            Console.WriteLine($"Data saved for {city}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing {city}: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}

static async Task SaveToDatabase(string connString, string city, JsonElement data)
{
    try 
    {
        using var conn = new NpgsqlConnection(connString);
        await conn.OpenAsync();
        Console.WriteLine($"Database connection opened for {city}");

        var dataObj = data.GetProperty("data");
        
        var aqi = dataObj.TryGetProperty("aqi", out var aqiElement) ? aqiElement.GetInt32() : 0;
        var dominantPoll = dataObj.TryGetProperty("dominentpol", out var pollElement) ? 
            pollElement.GetString() ?? "Unknown" : "Unknown";
        
        Console.WriteLine($"AQI: {aqi}, Dominant Pollutant: {dominantPoll}");

        double latitude = 0, longitude = 0;
        if (dataObj.TryGetProperty("city", out var cityElement) && 
            cityElement.TryGetProperty("geo", out var geoElement))
        {
            var geoArray = geoElement.EnumerateArray().ToArray();
            if (geoArray.Length >= 2)
            {
                latitude = geoArray[0].GetDouble();
                longitude = geoArray[1].GetDouble();
            }
        }
        
        var timestamp = DateTime.UtcNow;
        if (dataObj.TryGetProperty("time", out var timeElement) && 
            timeElement.TryGetProperty("v", out var vElement))
        {
            timestamp = DateTimeOffset.FromUnixTimeSeconds(vElement.GetInt64()).DateTime;
        }

        using var cmd = new NpgsqlCommand();
        cmd.Connection = conn;
        cmd.CommandText = @"
            INSERT INTO air_quality_data (
                city, 
                aqi, 
                dominant_pollutant, 
                latitude, 
                longitude, 
                timestamp
            )
            VALUES (
                @city, 
                @aqi, 
                @dominant_pollutant, 
                @latitude, 
                @longitude, 
                @timestamp
            )";
        
        cmd.Parameters.AddWithValue("city", (object)city ?? DBNull.Value);
        cmd.Parameters.AddWithValue("aqi", aqi);
        cmd.Parameters.AddWithValue("dominant_pollutant", (object)dominantPoll ?? DBNull.Value);
        cmd.Parameters.AddWithValue("latitude", latitude);
        cmd.Parameters.AddWithValue("longitude", longitude);
        cmd.Parameters.AddWithValue("timestamp", timestamp);

        await cmd.ExecuteNonQueryAsync();
        Console.WriteLine($"Data inserted successfully for {city}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error saving data for {city}: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
    }
} 