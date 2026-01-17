/**
 * @Name Program.cs
 * @Purpose Application entry point
 * @Date 17 January 2026, 14:13:00
 * @Author Copilot
 * @Description Configures and runs the OAuthApi web application
 */

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
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
var certificatePath = builder.Configuration["Authentication:CertificatePath"] ?? "../certs/oauth-demo.pfx";
var certificatePassword = builder.Configuration["Authentication:CertificatePassword"] ?? "OAuthDemo2026!";

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
builder.Services.AddSingleton(certificate);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Authentication:Issuer"] ?? "OAuthDemo",
            ValidAudience = builder.Configuration["Authentication:Audience"] ?? "OAuthApiUsers",
            IssuerSigningKey = new X509SecurityKey(certificate)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
