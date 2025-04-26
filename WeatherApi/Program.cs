using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 1. Register HttpClient for outgoing calls
builder.Services.AddHttpClient();

var app = builder.Build();

// 2. One GET endpoint that proxies to OpenWeatherMap
app.MapGet("/weather/{zip}", async (string zip, IHttpClientFactory httpFactory, IConfiguration config) =>
{
    // Read your API key from configuration (appsettings.json, env var, or User Secrets)
    var apiKey = config["OpenWeatherMap:Key"];
    if (string.IsNullOrWhiteSpace(apiKey))
        return Results.Problem("Missing OpenWeatherMap API key in configuration", statusCode: 500);

    // Build the request URL
    var url = $"https://api.openweathermap.org/data/2.5/weather?zip={zip},us&appid={apiKey}&units=imperial";

    // Call the public weather service
    var client = httpFactory.CreateClient();
    var response = await client.GetAsync(url);

    if (!response.IsSuccessStatusCode)
        return Results.Problem($"Weather API returned {(int)response.StatusCode}", statusCode: (int)response.StatusCode);

    var json = await response.Content.ReadAsStringAsync();
    return Results.Content(json, "application/json");
})
.WithName("GetWeatherByZip");

app.Run();