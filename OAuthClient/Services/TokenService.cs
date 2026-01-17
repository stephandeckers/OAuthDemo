/**
 * @Name TokenService.cs
 * @Purpose Token generation service implementation
 * @Date 17 January 2026, 14:14:00
 * @Author Copilot
 * @Description Service that generates JWT tokens using certificate-based signing
 */

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace OAuthClient.Services;

/// <summary date="17-01-2026, 14:14:00" author="Copilot">
/// Implementation of token generation service
/// </summary>
public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenService> _logger;

    public TokenService(IConfiguration configuration, ILogger<TokenService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary date="17-01-2026, 14:14:00" author="Copilot">
    /// Generates a JWT token signed with the configured certificate
    /// </summary>
    /// <returns>JWT token string</returns>
    public string GenerateToken()
    {
        _logger.LogInformation("Generating JWT token");

        var certificatePath = _configuration["Authentication:CertificatePath"] ?? "../certs/oauth-demo.pfx";
        var certificatePassword = _configuration["Authentication:CertificatePassword"] ?? "OAuthDemo2026!";
        var issuer = _configuration["Authentication:Issuer"] ?? "OAuthDemo";
        var audience = _configuration["Authentication:Audience"] ?? "OAuthApiUsers";

        if (!File.Exists(certificatePath))
        {
            _logger.LogError("Certificate not found at path: {Path}", certificatePath);
            throw new FileNotFoundException("Certificate file not found", certificatePath);
        }

        X509Certificate2 certificate;
        try
        {
            certificate = new X509Certificate2(certificatePath, certificatePassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load certificate from {Path}", certificatePath);
            throw new InvalidOperationException($"Failed to load certificate from {certificatePath}. Ensure the password is correct.", ex);
        }

        var securityKey = new X509SecurityKey(certificate);
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, "demo-user"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim("client", "OAuthClient")
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        _logger.LogInformation("JWT token generated successfully");

        return tokenString;
    }
}
