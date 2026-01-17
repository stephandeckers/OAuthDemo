using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace OAuthClient.Services;

public interface IOAuthService
{
    Task<string?> GetAccessTokenAsync();
    Task<string?> GetAccessTokenAsync(string? overrideCertPath, string? overrideCertPassword);
    HttpClient GetAuthenticatedHttpClient(string? accessToken = null);
    HttpClient GetCertificateHttpClient();
    HttpClient GetCertificateHttpClient(string? overrideCertPath, string? overrideCertPassword);
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

    /// <summary date="17-01-2026, 21:16:00" author="Copilot">
    /// Acquires an access token from the OAuthApi using certificate authentication.
    /// Due to TLS client certificate issues with Kestrel, the certificate is sent in the request body.
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

        var certPath = overrideCertPath ?? _configuration["Certificate:Path"];
        var certPassword = overrideCertPassword ?? _configuration["Certificate:Password"];

        if (string.IsNullOrEmpty(certPath) || !File.Exists(certPath))
        {
            _logger.LogError("Certificate not found at {CertPath}", certPath);
            return null;
        }

        // Load certificate
        X509Certificate2 certificate;
        try
        {
            certificate = new X509Certificate2(certPath, certPassword);
            _logger.LogInformation("Loaded certificate: {Subject}, Thumbprint: {Thumbprint}", 
                certificate.Subject, certificate.Thumbprint);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load certificate from {CertPath}", certPath);
            return null;
        }

        var apiUrl = _configuration["OAuthApi:BaseUrl"];
        
        try
        {
            // Create HTTP client without TLS client certificate (avoiding Kestrel issues)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };
            var client = new HttpClient(handler);

            // Send certificate in request body as base64-encoded DER format
            var certBytes = certificate.Export(X509ContentType.Cert);
            var certBase64 = Convert.ToBase64String(certBytes);

            var requestBody = new
            {
                CertificateBase64 = certBase64
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                System.Text.Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Sending token request to {ApiUrl}/Auth/token", apiUrl);
            var response = await client.PostAsync($"{apiUrl}/Auth/token", jsonContent);
            
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
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        var client = new HttpClient(handler);
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
