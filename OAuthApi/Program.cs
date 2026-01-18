/**
 * @Name Program.cs
 * @Purpose Application entry point
 * @Date 18 January 2026, 06:14:39
 * @Author Copilot
 * @Description Entry point for the OAuthApi web application using traditional Main method
 */

namespace OAuthApi;

/// <summary date="18-01-2026, 06:14:39" author="Copilot">
/// Main program class for OAuthApi application
/// </summary>
public class Program
{
    /// <summary date="18-01-2026, 06:14:39" author="Copilot">
    /// Main entry point for the application
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    /// <summary date="18-01-2026, 06:14:39" author="Copilot">
    /// Creates and configures the host builder
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>Configured host builder</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
