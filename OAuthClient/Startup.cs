/**
 * @Name Startup.cs
 * @Purpose Application startup configuration
 * @Date 17 January 2026, 19:26:00
 * @Author Copilot
 * @Description Configures services and middleware pipeline for the OAuthClient application
 */

using OAuthClient.Services;

namespace OAuthClient;

/// <summary date="17-01-2026, 19:26:00" author="Copilot">
/// Startup class for configuring application services and middleware
/// </summary>
public class Startup
{
    /// <summary date="17-01-2026, 19:26:00" author="Copilot">
    /// Gets the application configuration
    /// </summary>
    public IConfiguration Configuration { get; }

    /// <summary date="17-01-2026, 19:26:00" author="Copilot">
    /// Initializes a new instance of the Startup class
    /// </summary>
    /// <param name="configuration">The application configuration</param>
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary date="17-01-2026, 19:26:00" author="Copilot">
    /// Configures application services
    /// </summary>
    /// <param name="services">The service collection</param>
    public void ConfigureServices(IServiceCollection services)
    {
		g.WriteLine( GetType().Name);

        // Add services to the container.
        services.AddControllers();

        // Add HttpClient for calling external APIs
        services.AddHttpClient();

        // Register TokenService
        services.AddScoped<ITokenService, TokenService>();

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "OAuthClient", Version = "v1" });
        });
    }

    /// <summary date="17-01-2026, 19:26:00" author="Copilot">
    /// Configures the HTTP request pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="env">The web host environment</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
		g.WriteLine( GetType().Name);

        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
