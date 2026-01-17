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

    /// <summary>
    /// Request an OAuth token using certificate authentication
    /// </summary>
    [HttpPost("token")]
    [AllowAnonymous]
    public IActionResult GetToken()
    {
        try
        {
            // Get the client certificate from the request
            var clientCertificate = HttpContext.Connection.ClientCertificate;

            if (clientCertificate == null)
            {
                _logger.LogWarning("Token request without client certificate");
                return Unauthorized(new { error = "Client certificate required" });
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

    private bool ValidateCertificate(X509Certificate2 certificate)
    {
        // Basic validation - check if certificate is not expired
        if (certificate.NotAfter < DateTime.Now)
        {
            _logger.LogWarning("Certificate expired: {NotAfter}", certificate.NotAfter);
            return false;
        }

        if (certificate.NotBefore > DateTime.Now)
        {
            _logger.LogWarning("Certificate not yet valid: {NotBefore}", certificate.NotBefore);
            return false;
        }

        // In production, you would validate against a trusted certificate store
        // For this demo, we accept any valid certificate
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
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"] ?? "60");

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
            expires: DateTime.Now.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
