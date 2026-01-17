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
        if (_cachedToken != null && DateTime.Now < _tokenExpiry.AddSeconds(-30))
        {
            return _cachedToken;
        }

        _logger.LogInformation("Acquiring new access token from OAuthApi...");

        var client = GetCertificateHttpClient();
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
                _cachedToken = tokenResponse.Access_token;
                _tokenExpiry = DateTime.Now.AddSeconds(tokenResponse.Expires_in);
                _logger.LogInformation("Successfully acquired access token. Expires in {ExpiresIn}s", tokenResponse.Expires_in);
                return _cachedToken;
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
        var certPath = _configuration["Certificate:Path"];
        var certPassword = _configuration["Certificate:Password"];

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
