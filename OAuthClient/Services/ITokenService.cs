/**
 * @Name ITokenService.cs
 * @Purpose Token service interface
 * @Date 17 January 2026, 14:14:00
 * @Author Copilot
 * @Description Interface for JWT token generation service
 */

namespace OAuthClient.Services;

/// <summary date="17-01-2026, 14:14:00" author="Copilot">
/// Service for generating JWT tokens using certificate-based authentication
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generates a JWT token signed with a certificate
    /// </summary>
    /// <returns>JWT token string</returns>
    string GenerateToken();
}
