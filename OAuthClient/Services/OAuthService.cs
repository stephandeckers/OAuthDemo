using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace OAuthClient.Services;

public interface IOAuthService
{
    Task<string?> GetAccessTokenAsync();
    HttpClient GetAuthenticatedHttpClient(string? accessToken = null);
    HttpClient GetCertificateHttpClient();
}

public class OAuthService : IOAuthService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<OAuthService> _logger;
    private string? _cachedToken;
    private DateTime _tokenExpiry;

    public OAuthService(IConfiguration configuration, ILogger<OAuthService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        return await GetAccessTokenAsync(null, null);
    }

    /// <summary date="17-01-2026, 20:26:00" author="Copilot">
    /// Acquires an access token from the OAuthApi using certificate authentication.
    /// Allows overriding the default certificate for testing unauthorized access.
    /// </summary>
    /// <param name="overrideCertPath">Optional path to override the default certificate</param>
    /// <param name="overrideCertPassword">Optional password to override the default certificate password</param>
    /// <returns>JWT access token if successful, null otherwise</returns>
    public async Task<string?> GetAccessTokenAsync(string? overrideCertPath, string? overrideCertPassword)
    {
        // Don't use cached token when using override certificates (for testing)
        if (overrideCertPath == null && _cachedToken != null && DateTime.Now < _tokenExpiry.AddSeconds(-30))
        {
            return _cachedToken;
        }

        _logger.LogInformation("Acquiring new access token from OAuthApi...");

        var client = GetCertificateHttpClient(overrideCertPath, overrideCertPassword);
        var apiUrl = _configuration["OAuthApi:BaseUrl"];
        
        try
        {
            // The Auth controller expects a POST to /Auth/token with the certificate
            var response = await client.PostAsync($"{apiUrl}/Auth/token", null);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to acquire token. Status: {Status}, Error: {Error}", response.StatusCode, error);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (tokenResponse?.Access_token != null)
            {
                // Only cache token if using default certificate
                if (overrideCertPath == null)
                {
                    _cachedToken = tokenResponse.Access_token;
                    _tokenExpiry = DateTime.Now.AddSeconds(tokenResponse.Expires_in);
                }
                _logger.LogInformation("Successfully acquired access token. Expires in {ExpiresIn}s", tokenResponse.Expires_in);
                return tokenResponse.Access_token;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception while acquiring access token");
        }

        return null;
    }

    public HttpClient GetAuthenticatedHttpClient(string? accessToken = null)
    {
        var client = new HttpClient();
        if (accessToken != null)
        {
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        }
        return client;
    }

    public HttpClient GetCertificateHttpClient()
    {
        return GetCertificateHttpClient(null, null);
    }

    /// <summary date="17-01-2026, 20:25:00" author="Copilot">
    /// Creates an HTTP client configured with a specific certificate for authentication.
    /// Allows overriding the default certificate path and password for testing purposes.
    /// </summary>
    /// <param name="overrideCertPath">Optional path to override the default certificate</param>
    /// <param name="overrideCertPassword">Optional password to override the default certificate password</param>
    /// <returns>HttpClient configured with the specified certificate</returns>
    public HttpClient GetCertificateHttpClient(string? overrideCertPath, string? overrideCertPassword)
    {
        var certPath = overrideCertPath ?? _configuration["Certificate:Path"];
        var certPassword = overrideCertPassword ?? _configuration["Certificate:Password"];

        if (string.IsNullOrEmpty(certPath) || !File.Exists(certPath))
        {
            throw new FileNotFoundException($"Certificate not found at {certPath}");
        }

        // Load certificate from file
        var certificate = new X509Certificate2(certPath, certPassword);

        var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(certificate);
        
        // For development: allow self-signed certificates
        handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

        return new HttpClient(handler);
    }

    private class TokenResponse
    {
        public string? Access_token { get; set; }
        public string? Token_type { get; set; }
        public int Expires_in { get; set; }
    }
}
