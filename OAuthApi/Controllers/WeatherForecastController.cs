/**
 * @Name WeatherForecastController.cs
 * @Purpose Weather forecast API controller
 * @Date 17 January 2026, 14:13:00
 * @Author Copilot
 * @Description Controller for weather forecast data with authenticated and non-authenticated endpoints
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OAuthApi.Controllers;

/// <summary date="17-01-2026, 14:13:00" author="Copilot">
/// Weather forecast controller with OAuth authentication support
/// </summary>
[ApiController]
[Route("[controller]")]
public class WeatherForecastController(ILogger<WeatherForecastController> logger) : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    /// <summary date="17-01-2026, 14:13:00" author="Copilot">
    /// Get weather forecast data (no authentication required)
    /// </summary>
    /// <returns>Collection of weather forecasts</returns>
    [HttpGet("Get1", Name = "GetWeatherForecast1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<WeatherForecast> Get1()
    {
        logger.LogInformation("Get1 method called");
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    /// <summary date="17-01-2026, 14:13:00" author="Copilot">
    /// Get weather forecast data for the next 7 days (no authentication required)
    /// </summary>
    /// <returns>Collection of weather forecasts</returns>
    [HttpGet("Get2", Name = "GetWeatherForecast2")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<WeatherForecast> Get2()
    {
        logger.LogInformation("Get2 method called");
        return Enumerable.Range(1, 7).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    /// <summary date="17-01-2026, 14:13:00" author="Copilot">
    /// Get secured weather forecast data (OAuth authentication required)
    /// </summary>
    /// <returns>Collection of weather forecasts</returns>
    [Authorize]
    [HttpGet("GetSecured", Name = "GetWeatherForecastSecured")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IEnumerable<WeatherForecast> GetSecured()
    {
        logger.LogInformation("GetSecured method called by authenticated user");
        return Enumerable.Range(1, 10).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
