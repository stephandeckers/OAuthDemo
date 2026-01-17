using Microsoft.AspNetCore.Mvc;
using OAuthClient.Models;
using OAuthClient.Services;
using System.Text.Json;

namespace OAuthClient.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientController : ControllerBase
{
    private readonly IOAuthService _oauthService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IOAuthService oauthService, IConfiguration configuration, ILogger<ClientController> logger)
    {
        _oauthService = oauthService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Test calling the public endpoint of OAuthApi
    /// </summary>
    [HttpGet("test-public")]
    public async Task<IActionResult> TestPublic()
    {
        var apiUrl = _configuration["OAuthApi:BaseUrl"];
        var client = new HttpClient(new HttpClientHandler 
        { 
            ServerCertificateCustomValidationCallback = (m, c, ch, e) => true 
        });

        _logger.LogInformation("Calling public endpoint: {Url}/WeatherForecast/public", apiUrl);
        
        try
        {
            var response = await client.GetAsync($"{apiUrl}/WeatherForecast/public");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var forecasts = JsonSerializer.Deserialize<List<WeatherForecast>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Ok(new { source = "OAuthApi Public", data = forecasts });
            }

            return StatusCode((int)response.StatusCode, new { error = "Failed to call public endpoint", details = content });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Test calling the secured endpoint of OAuthApi using certificate-based OAuth
    /// </summary>
    [HttpGet("test-secured")]
    public async Task<IActionResult> TestSecured()
    {
        var apiUrl = _configuration["OAuthApi:BaseUrl"];
        
        _logger.LogInformation("Attempting to call secured endpoint via OAuth flow");

        // 1. Get Access Token using certificate
        var token = await _oauthService.GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { error = "Failed to acquire access token using certificate" });
        }

        // 2. Call secured endpoint with Bearer token
        var client = _oauthService.GetAuthenticatedHttpClient(token);
        
        try
        {
            var response = await client.GetAsync($"{apiUrl}/WeatherForecast/secured");
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var forecasts = JsonSerializer.Deserialize<List<WeatherForecast>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Ok(new { source = "OAuthApi Secured", token_used = token[..10] + "...", data = forecasts });
            }

            return StatusCode((int)response.StatusCode, new { error = "Failed to call secured endpoint", details = content });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
