using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OAuthApi.Models;

namespace OAuthApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;

    public WeatherForecastController(ILogger<WeatherForecastController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get weather forecast - No authentication required
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public IEnumerable<WeatherForecast> GetPublic()
    {
        _logger.LogInformation("Public endpoint accessed - no authentication required");
        
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    /// <summary>
    /// Get weather forecast - OAuth authentication required
    /// </summary>
    [HttpGet("secured")]
    [Authorize]
    public IEnumerable<WeatherForecast> GetSecured()
    {
        var username = User.Identity?.Name ?? "Unknown";
        var claims = string.Join(", ", User.Claims.Select(c => $"{c.Type}={c.Value}"));
        
        _logger.LogInformation("Secured endpoint accessed by user: {Username}, Claims: {Claims}", username, claims);
        
        return Enumerable.Range(1, 10).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
