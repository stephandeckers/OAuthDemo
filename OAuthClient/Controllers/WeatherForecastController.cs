/**
 * @Name OAuthDemoController.cs
 * @Purpose OAuth demonstration controller
 * @Date 17 January 2026, 14:14:00
 * @Author Copilot
 * @Description Controller that demonstrates calling OAuthApi with and without authentication
 */

using Microsoft.AspNetCore.Mvc;
using OAuthClient.Services;
using System.Net.Http.Headers;
using System.Text.Json;

namespace OAuthClient.Controllers;

/// <summary date="17-01-2026, 14:14:00" author="Copilot">
/// OAuth demonstration controller
/// </summary>
[ApiController]
[Route("[controller]")]
public class OAuthDemoController(
    ITokenService tokenService, 
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<OAuthDemoController> logger) : ControllerBase
{
    /// <summary date="17-01-2026, 14:14:00" author="Copilot">
    /// Call OAuthApi Get1 endpoint without authentication
    /// </summary>
    /// <returns>Weather forecast data from OAuthApi</returns>
    [HttpGet("CallGet1")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CallGet1()
    {
        logger.LogInformation("Calling OAuthApi Get1 endpoint (no auth)");
        
        var client = httpClientFactory.CreateClient();
        var apiBaseUrl = configuration["OAuthApi:BaseUrl"] ?? "https://localhost:7001";
        
        try
        {
            var response = await client.GetAsync($"{apiBaseUrl}/WeatherForecast/Get1");
            var content = await response.Content.ReadAsStringAsync();
            
            var data = JsonSerializer.Deserialize<JsonElement>(content);
            
            return Ok(new
            {
                StatusCode = (int)response.StatusCode,
                Message = "Called OAuthApi Get1 (no authentication)",
                Data = data
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling OAuthApi Get1");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary date="17-01-2026, 14:14:00" author="Copilot">
    /// Call OAuthApi Get2 endpoint without authentication
    /// </summary>
    /// <returns>Weather forecast data from OAuthApi</returns>
    [HttpGet("CallGet2")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CallGet2()
    {
        logger.LogInformation("Calling OAuthApi Get2 endpoint (no auth)");
        
        var client = httpClientFactory.CreateClient();
        var apiBaseUrl = configuration["OAuthApi:BaseUrl"] ?? "https://localhost:7001";
        
        try
        {
            var response = await client.GetAsync($"{apiBaseUrl}/WeatherForecast/Get2");
            var content = await response.Content.ReadAsStringAsync();
            
            var data = JsonSerializer.Deserialize<JsonElement>(content);
            
            return Ok(new
            {
                StatusCode = (int)response.StatusCode,
                Message = "Called OAuthApi Get2 (no authentication)",
                Data = data
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling OAuthApi Get2");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary date="17-01-2026, 14:14:00" author="Copilot">
    /// Call OAuthApi GetSecured endpoint with OAuth authentication
    /// </summary>
    /// <returns>Weather forecast data from OAuthApi</returns>
    [HttpGet("CallGetSecured")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CallGetSecured()
    {
        logger.LogInformation("Calling OAuthApi GetSecured endpoint (with OAuth)");
        
        var client = httpClientFactory.CreateClient();
        var apiBaseUrl = configuration["OAuthApi:BaseUrl"] ?? "https://localhost:7001";
        
        try
        {
            // Generate JWT token
            var token = tokenService.GenerateToken();
            
            // Add token to request header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.GetAsync($"{apiBaseUrl}/WeatherForecast/GetSecured");
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<JsonElement>(content);
                
                return Ok(new
                {
                    StatusCode = (int)response.StatusCode,
                    Message = "Called OAuthApi GetSecured (with OAuth authentication)",
                    Token = token,
                    Data = data
                });
            }
            else
            {
                return StatusCode((int)response.StatusCode, new
                {
                    StatusCode = (int)response.StatusCode,
                    Message = "Authentication failed",
                    Error = content
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calling OAuthApi GetSecured");
            return StatusCode(500, new { Error = ex.Message });
        }
    }

    /// <summary date="17-01-2026, 14:14:00" author="Copilot">
    /// Generate a JWT token for testing
    /// </summary>
    /// <returns>JWT token</returns>
    [HttpGet("GenerateToken")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GenerateToken()
    {
        logger.LogInformation("Generating JWT token");
        
        try
        {
            var token = tokenService.GenerateToken();
            return Ok(new
            {
                Token = token,
                Message = "JWT token generated successfully. Use this in the Authorization header as 'Bearer {token}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating token");
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}

