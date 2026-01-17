/**
 * @Name OAuthTestResult.cs
 * @Purpose Model for OAuth validation test results
 * @Date 17 January 2026, 20:30:00
 * @Author Copilot
 * @Description Models for representing OAuth validation test results
 */

namespace OAuthClient.Models;

/// <summary date="17-01-2026, 20:30:00" author="Copilot">
/// Represents a single test step in the OAuth validation test
/// </summary>
public class TestStep
{
    public int Step { get; set; }
    public string Action { get; set; } = string.Empty;
    public string ExpectedResult { get; set; } = string.Empty;
    public string ActualResult { get; set; } = string.Empty;
    public bool Success { get; set; }
}

/// <summary date="17-01-2026, 20:30:00" author="Copilot">
/// Represents the overall OAuth validation test result
/// </summary>
public class OAuthTestResult
{
    public string Test { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<TestStep> Steps { get; set; } = new();
    public string Conclusion { get; set; } = string.Empty;
}
