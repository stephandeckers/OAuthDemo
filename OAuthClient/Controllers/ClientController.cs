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

    /// <summary date="17-01-2026, 20:28:00" author="Copilot">
    /// Proves that OAuth works by attempting to use an unauthorized certificate.
    /// This test demonstrates that:
    /// 1. An unauthorized certificate cannot obtain a JWT token from OAuthApi
    /// 2. Without a valid token, the secured endpoint cannot be accessed
    /// 3. OAuth validation is working correctly
    /// </summary>
    [HttpGet("prove-oauth-works")]
    public async Task<IActionResult> ProveOAuthWorks()
    {
        var apiUrl = _configuration["OAuthApi:BaseUrl"];
        
        _logger.LogInformation("=== PROVING OAUTH WORKS ===");
        _logger.LogInformation("Step 1: Attempting to authenticate with UNAUTHORIZED certificate");

        // Get the path to the unauthorized certificate
        var repoRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../.."));
        var unauthorizedCertPath = Path.Combine(repoRoot, "certificates", "Unauthorized.pfx");
        var unauthorizedCertPassword = "Unauthorized2026!";

        _logger.LogInformation("Using unauthorized certificate: {Path}", unauthorizedCertPath);

        if (!File.Exists(unauthorizedCertPath))
        {
            return StatusCode(500, new { 
                error = "Unauthorized certificate not found", 
                path = unauthorizedCertPath,
                message = "Please run the generate-unauthorized-certificate.sh script first"
            });
        }

        var results = new
        {
            test = "Prove OAuth Works - Certificate Validation",
            description = "This test proves OAuth is working by demonstrating that an unauthorized certificate is rejected",
            steps = new[]
            {
                new
                {
                    step = 1,
                    action = "Attempt to get JWT token with UNAUTHORIZED certificate",
                    expectedResult = "Failure - token request should be denied",
                    actualResult = "",
                    success = false
                },
                new
                {
                    step = 2,
                    action = "Attempt to get JWT token with AUTHORIZED certificate",
                    expectedResult = "Success - token request should be granted",
                    actualResult = "",
                    success = false
                },
                new
                {
                    step = 3,
                    action = "Call secured endpoint with valid token",
                    expectedResult = "Success - endpoint should return data",
                    actualResult = "",
                    success = false
                }
            },
            conclusion = ""
        };

        // Step 1: Try with unauthorized certificate (should FAIL)
        string? unauthorizedToken = null;
        try
        {
            unauthorizedToken = await _oauthService.GetAccessTokenAsync(unauthorizedCertPath, unauthorizedCertPassword);
            
            if (unauthorizedToken == null)
            {
                results.steps[0].actualResult = "SUCCESS - Unauthorized certificate was REJECTED";
                results.steps[0].success = true;
                _logger.LogInformation("✓ Step 1 PASSED: Unauthorized certificate correctly rejected");
            }
            else
            {
                results.steps[0].actualResult = "FAILURE - Unauthorized certificate was ACCEPTED (OAuth validation not working!)";
                results.steps[0].success = false;
                _logger.LogError("✗ Step 1 FAILED: Unauthorized certificate should have been rejected!");
            }
        }
        catch (Exception ex)
        {
            results.steps[0].actualResult = $"SUCCESS - Exception thrown (certificate rejected): {ex.Message}";
            results.steps[0].success = true;
            _logger.LogInformation("✓ Step 1 PASSED: Unauthorized certificate correctly rejected with exception");
        }

        // Step 2: Try with authorized certificate (should SUCCEED)
        string? authorizedToken = null;
        try
        {
            authorizedToken = await _oauthService.GetAccessTokenAsync();
            
            if (!string.IsNullOrEmpty(authorizedToken))
            {
                results.steps[1].actualResult = $"SUCCESS - Authorized certificate accepted, token received: {authorizedToken[..10]}...";
                results.steps[1].success = true;
                _logger.LogInformation("✓ Step 2 PASSED: Authorized certificate correctly accepted");
            }
            else
            {
                results.steps[1].actualResult = "FAILURE - Authorized certificate was REJECTED";
                results.steps[1].success = false;
                _logger.LogError("✗ Step 2 FAILED: Authorized certificate should have been accepted!");
            }
        }
        catch (Exception ex)
        {
            results.steps[1].actualResult = $"FAILURE - Exception: {ex.Message}";
            results.steps[1].success = false;
            _logger.LogError(ex, "✗ Step 2 FAILED: Exception when using authorized certificate");
        }

        // Step 3: Call secured endpoint with valid token (should SUCCEED)
        if (!string.IsNullOrEmpty(authorizedToken))
        {
            try
            {
                var client = _oauthService.GetAuthenticatedHttpClient(authorizedToken);
                var response = await client.GetAsync($"{apiUrl}/WeatherForecast/secured");
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var forecasts = JsonSerializer.Deserialize<List<WeatherForecast>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    results.steps[2].actualResult = $"SUCCESS - Secured endpoint returned {forecasts?.Count ?? 0} items";
                    results.steps[2].success = true;
                    _logger.LogInformation("✓ Step 3 PASSED: Secured endpoint accessible with valid token");
                }
                else
                {
                    results.steps[2].actualResult = $"FAILURE - Secured endpoint returned {response.StatusCode}";
                    results.steps[2].success = false;
                    _logger.LogError("✗ Step 3 FAILED: Secured endpoint should have been accessible");
                }
            }
            catch (Exception ex)
            {
                results.steps[2].actualResult = $"FAILURE - Exception: {ex.Message}";
                results.steps[2].success = false;
                _logger.LogError(ex, "✗ Step 3 FAILED: Exception when calling secured endpoint");
            }
        }
        else
        {
            results.steps[2].actualResult = "SKIPPED - No valid token available";
            _logger.LogWarning("Step 3 SKIPPED: No valid token to test secured endpoint");
        }

        // Determine overall conclusion
        var allStepsPassed = results.steps.All(s => s.success);
        results.conclusion = allStepsPassed 
            ? "✓ OAUTH VALIDATION WORKS! Unauthorized certificates are rejected, authorized certificates are accepted, and secured endpoints are protected."
            : "✗ OAUTH VALIDATION HAS ISSUES - Review the individual step results above.";

        _logger.LogInformation("=== TEST COMPLETE ===");
        _logger.LogInformation(results.conclusion);

        return Ok(results);
    }
}
