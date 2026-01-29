using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Capa_Datos;
using Capa_Datos.Context;
using Capa_Datos.Interfaces;
using Capa_Datos.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using PuntoDeVenta.API.Security;
using PuntoDeVenta.API.Validators;
using Serilog;
using Serilog.Events;

/// <summary>
/// Punto de entrada de la API ASP.NET Core.
/// Con seguridad OWASP Top 10 implementada.
/// </summary>

// ============================================
// SERILOG - Logging estructurado (OWASP A09)
// ============================================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithMachineName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/puntodeventa-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Iniciando PuntoDeVenta API...");

    var builder = WebApplication.CreateBuilder(args);

    // Usar Serilog
    builder.Host.UseSerilog();

    #region Configuracion de Servicios

    // ============================================
    // CONTROLLERS + FLUENT VALIDATION
    // ============================================
    builder.Services.AddControllers();
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

    // ============================================
    // RATE LIMITING (OWASP A04, A07)
    // ============================================
    builder.Services.AddRateLimitingServices();

    // ============================================
    // HTTP CONTEXT ACCESSOR (para AuditLogger)
    // ============================================
    builder.Services.AddHttpContextAccessor();

    // ============================================
    // SWAGGER / OPENAPI
    // ============================================
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "PuntoDeVenta API",
            Version = "v1",
            Description = "API REST para Gestión POS con seguridad OWASP",
            Contact = new OpenApiContact
            {
                Name = "Emmanuel Valdez",
                Url = new Uri("https://github.com/EmmaVZ89")
            }
        });

        // Configurar JWT en Swagger
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingrese el token JWT en el formato: Bearer {token}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ============================================
    // JWT AUTHENTICATION (OWASP A02, A07)
    // Sin fallback - Error si no hay JWT_KEY
    // ============================================
    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
        ?? ConfigurationHelper.GetAppSetting("JwtKey");
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
        ?? ConfigurationHelper.GetAppSetting("JwtIssuer")
        ?? "PuntoDeVenta.API";
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
        ?? ConfigurationHelper.GetAppSetting("JwtAudience")
        ?? "PuntoDeVenta.Clients";

    // Validar JWT_KEY en produccion
    if (builder.Environment.IsProduction() && string.IsNullOrEmpty(jwtKey))
    {
        throw new InvalidOperationException(
            "JWT_KEY no configurado. Configure la variable de entorno JWT_KEY o JwtKey en appsettings.json");
    }

    // Fallback solo para desarrollo
    if (string.IsNullOrEmpty(jwtKey))
    {
        jwtKey = "PuntoDeVenta_DevKey_2024_MinLength32Characters!";
        Log.Warning("Usando JWT_KEY de desarrollo. NO usar en produccion!");
    }

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew = TimeSpan.Zero // Sin tolerancia de tiempo
        };

        // Log de eventos de autenticacion
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Log.Warning("Autenticacion JWT fallida: {Error}", context.Exception.Message);
                return System.Threading.Tasks.Task.CompletedTask;
            }
        };
    });

    builder.Services.AddAuthorization();

    // ============================================
    // CORS - Hardened (OWASP A05)
    // ============================================
    var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")
        ?? ConfigurationHelper.GetAppSetting("AllowedOrigins")
        ?? "http://localhost:8081";

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Production", policy =>
        {
            var origins = allowedOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
            policy.WithOrigins(origins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
        });

        // Solo para desarrollo
        options.AddPolicy("Development", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // ============================================
    // HEALTH CHECKS (Railway)
    // ============================================
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? ConfigurationHelper.GetConnectionString();

    builder.Services.AddHealthChecks()
        .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "db", "ready" });

    // ============================================
    // EXCEPTION HANDLER (OWASP A09)
    // ============================================
    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
    builder.Services.AddProblemDetails();

    // ============================================
    // SECURITY SERVICES
    // ============================================
    builder.Services.AddSingleton<ILoginAttemptTracker, LoginAttemptTracker>();
    builder.Services.AddSingleton<IAuditLogger, AuditLogger>();

    // ============================================
    // DEPENDENCY INJECTION - Capa Datos
    // ============================================
    builder.Services.AddScoped<ApplicationDbContext>();
    builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
    builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
    builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
    builder.Services.AddScoped<IGrupoRepository, GrupoRepository>();
    builder.Services.AddScoped<IMovimientoRepository, MovimientoRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    #endregion

    var app = builder.Build();

    #region Configuracion del Pipeline HTTP

    // ============================================
    // EXCEPTION HANDLER (primero en el pipeline)
    // ============================================
    app.UseExceptionHandler();

    // ============================================
    // SECURITY HEADERS (OWASP A05)
    // ============================================
    app.UseSecurityHeaders();

    // ============================================
    // HTTPS/HSTS (solo produccion)
    // ============================================
    if (!app.Environment.IsDevelopment())
    {
        app.UseHsts();
    }

    // ============================================
    // RATE LIMITING (OWASP A04, A07)
    // ============================================
    app.UseRateLimiter();

    // ============================================
    // CORS
    // ============================================
    var corsPolicy = app.Environment.IsDevelopment() ? "Development" : "Production";
    app.UseCors(corsPolicy);

    // ============================================
    // SERILOG REQUEST LOGGING
    // ============================================
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("ClientIP", httpContext.Connection.RemoteIpAddress?.ToString());
        };
    });

    // ============================================
    // SWAGGER (solo en desarrollo)
    // ============================================
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "PuntoDeVenta API v1");
            options.RoutePrefix = "swagger";
        });
    }

    // ============================================
    // AUTENTICACION Y AUTORIZACION
    // ============================================
    app.UseAuthentication();
    app.UseAuthorization();

    // ============================================
    // HEALTH CHECK ENDPOINT
    // ============================================
    app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow.ToString("O"),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration.TotalMilliseconds
                })
            };
            await context.Response.WriteAsJsonAsync(response);
        }
    });

    // ============================================
    // MAPEAR CONTROLADORES
    // ============================================
    app.MapControllers();

    #endregion

    Log.Information("PuntoDeVenta API iniciada correctamente");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicacion fallo al iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
