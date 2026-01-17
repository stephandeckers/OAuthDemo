/**
 * @Name Program.cs
 * @Purpose Application entry point
 * @Date 17 January 2026, 19:26:00
 * @Author Copilot
 * @Description Main entry point for the OAuthClient web application
 */

namespace OAuthClient;

/// <summary date="17-01-2026, 19:26:00" author="Copilot">
/// Main program class
/// </summary>
public class Program
{
    /// <summary date="17-01-2026, 19:26:00" author="Copilot">
    /// Main entry point of the application
    /// </summary>
    /// <param name="args">Command line arguments</param>
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    /// <summary date="17-01-2026, 19:26:00" author="Copilot">
    /// Creates the host builder with configured startup
    /// </summary>
    /// <param name="args">Command line arguments</param>
    /// <returns>The configured host builder</returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
