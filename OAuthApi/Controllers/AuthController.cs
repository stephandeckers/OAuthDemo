using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OAuthApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary date="17-01-2026, 21:15:00" author="Copilot">
    /// Request an OAuth token using certificate authentication.
    /// Due to TLS client certificate issues with Kestrel, certificates are sent in the request body.
    /// The client sends the certificate as base64-encoded DER format to prove identity.
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult GetToken([FromBody] TokenRequest? request = null)
    {
        try
        {
            // If no request body, try to get from TLS connection (legacy support)
            X509Certificate2? clientCertificate = null;
            
            if (request != null && !string.IsNullOrEmpty(request.CertificateBase64))
            {
                // Certificate sent in request body (preferred method)
                try
                {
                    var certBytes = Convert.FromBase64String(request.CertificateBase64);
                    clientCertificate = new X509Certificate2(certBytes);
                    _logger.LogInformation("Certificate received in request body: {Subject}", clientCertificate.Subject);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to parse certificate from request body");
                    return Unauthorized(new { error = "Invalid certificate format" });
                }
            }
            else
            {
                // Try TLS connection certificate (fallback)
                clientCertificate = HttpContext.Connection.ClientCertificate;
            }

            if (clientCertificate == null)
            {
                _logger.LogWarning("Token request without client certificate");
                return Unauthorized(new { error = "Client certificate required. Send certificate in request body as base64-encoded DER format." });
            }

            // Validate the certificate
            if (!ValidateCertificate(clientCertificate))
            {
                _logger.LogWarning("Invalid client certificate: {Subject}", clientCertificate.Subject);
                return Unauthorized(new { error = "Invalid client certificate" });
            }

            _logger.LogInformation("Valid certificate presented: {Subject}, Thumbprint: {Thumbprint}", 
                clientCertificate.Subject, clientCertificate.Thumbprint);

            // Generate JWT token
            var token = GenerateJwtToken(clientCertificate);

            return Ok(new
            {
                access_token = token,
                token_type = "Bearer",
                expires_in = 3600,
                client_id = clientCertificate.Subject
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    public class TokenRequest
    {
        public string? CertificateBase64 { get; set; }
    }

    private bool ValidateCertificate(X509Certificate2 certificate)
    {
        // Basic validation - check if certificate is not expired
        if (certificate.NotAfter < DateTime.UtcNow)
        {
            _logger.LogWarning("Certificate expired: {NotAfter}", certificate.NotAfter);
            return false;
        }

        if (certificate.NotBefore > DateTime.UtcNow)
        {
            _logger.LogWarning("Certificate not yet valid: {NotBefore}", certificate.NotBefore);
            return false;
        }

        // Validate against the configured trusted certificate thumbprint
        // Only certificates with matching thumbprints are accepted
        var expectedThumbprint = _configuration["Authentication:Certificate:Thumbprint"];
        if (!string.IsNullOrEmpty(expectedThumbprint))
        {
            if (!certificate.Thumbprint.Equals(expectedThumbprint, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Certificate thumbprint mismatch. Expected: {Expected}, Got: {Actual}",
                    expectedThumbprint, certificate.Thumbprint);
                return false;
            }
        }

        return true;
    }

    private string GenerateJwtToken(X509Certificate2 certificate)
    {
        var jwtSettings = _configuration.GetSection("Authentication:Jwt");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        var issuer = jwtSettings["Issuer"] ?? "OAuthApi";
        var audience = jwtSettings["Audience"] ?? "OAuthClient";
        
        // Parse expiration with error handling
        if (!int.TryParse(jwtSettings["ExpirationMinutes"], out int expirationMinutes))
        {
            expirationMinutes = 60; // Default to 60 minutes
            _logger.LogWarning("Invalid ExpirationMinutes configuration, using default: {Default}", expirationMinutes);
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, certificate.Subject),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("thumbprint", certificate.Thumbprint),
            new Claim(ClaimTypes.Name, certificate.Subject)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
