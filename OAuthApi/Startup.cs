/**
 * @Name Startup.cs
 * @Purpose Application startup configuration
 * @Date 17 January 2026, 19:26:00
 * @Author Copilot
 * @Description Configures services and middleware pipeline for the OAuthApi application
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

namespace OAuthApi;

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

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "OAuthApi", Version = "v1" });
            
            // Add JWT authentication to Swagger
            c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Configure JWT Authentication with Certificate
        var certificatePath = Configuration["Authentication:CertificatePath"] ?? "../certs/oauth-demo.pfx";
        var certificatePassword = Configuration["Authentication:CertificatePassword"] ?? "OAuthDemo2026!";

        if (!File.Exists(certificatePath))
        {
            var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
            logger.LogError("Certificate file not found at {Path}", certificatePath);
            throw new FileNotFoundException($"Certificate file not found at {certificatePath}");
        }

        X509Certificate2 certificate;
        try
        {
            certificate = new X509Certificate2(certificatePath, certificatePassword);
            var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
            logger.LogInformation("Certificate loaded successfully from {Path}", certificatePath);
        }
        catch (Exception ex)
        {
            var logger = LoggerFactory.Create(config => config.AddConsole()).CreateLogger("Startup");
            logger.LogError(ex, "Failed to load certificate from {Path}", certificatePath);
            throw new InvalidOperationException($"Failed to load certificate from {certificatePath}", ex);
        }

        // Register certificate as singleton for reuse
        services.AddSingleton(certificate);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Authentication:Issuer"] ?? "OAuthDemo",
                    ValidAudience = Configuration["Authentication:Audience"] ?? "OAuthApiUsers",
                    IssuerSigningKey = new X509SecurityKey(certificate)
                };
            });

        services.AddAuthorization();
    }

    /// <summary date="17-01-2026, 19:26:00" author="Copilot">
    /// Configures the HTTP request pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="env">The web host environment</param>
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Configure the HTTP request pipeline.
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
