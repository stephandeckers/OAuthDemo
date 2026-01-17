/**
 * @Name WeatherForecast.cs
 * @Purpose Weather forecast data model
 * @Date 17 January 2026, 14:13:00
 * @Author Copilot
 * @Description Data transfer object for weather forecast information
 */

namespace OAuthApi;

/// <summary date="17-01-2026, 14:13:00" author="Copilot">
/// Weather forecast data model
/// </summary>
public class WeatherForecast
{
    /// <summary>
    /// Date of the forecast
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Temperature in Celsius
    /// </summary>
    public int TemperatureC { get; set; }

    /// <summary>
    /// Temperature in Fahrenheit (calculated from Celsius)
    /// </summary>
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    /// <summary>
    /// Weather summary description
    /// </summary>
    public string? Summary { get; set; }
}
