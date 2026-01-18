using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Capa_Datos;
using Capa_Datos.Context;
using Capa_Datos.Interfaces;
using Capa_Datos.Repositories;

/// <summary>
/// Punto de entrada de la API ASP.NET Core.
///
/// TECNOLOGIA NUEVA: ASP.NET Core Web API
/// - Framework para crear APIs RESTful
/// - Usa el patron "Minimal APIs" o Controllers
/// - Pipeline de middleware configurable
/// - Inyeccion de dependencias integrada
/// </summary>

var builder = WebApplication.CreateBuilder(args);

#region Configuracion de Servicios

// ============================================
// CONTROLLERS
// ============================================
// Agrega soporte para controladores MVC
builder.Services.AddControllers();

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
        Description = "API REST para el sistema de Punto de Venta",
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
// JWT AUTHENTICATION
// ============================================
var jwtKey = ConfigurationHelper.GetAppSetting("JwtKey") ?? "PuntoDeVenta_SuperSecretKey_2024_MinLength32Chars!";
var jwtIssuer = ConfigurationHelper.GetAppSetting("JwtIssuer") ?? "PuntoDeVenta.API";
var jwtAudience = ConfigurationHelper.GetAppSetting("JwtAudience") ?? "PuntoDeVenta.Clients";

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
});

builder.Services.AddAuthorization();

// ============================================
// CORS - Cross-Origin Resource Sharing
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    // Politica mas restrictiva para produccion
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://tudominio.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

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
// SWAGGER (solo en desarrollo)
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "PuntoDeVenta API v1");
        options.RoutePrefix = string.Empty; // Swagger en la raiz
    });
}

// ============================================
// MIDDLEWARE PIPELINE
// ============================================
app.UseCors("AllowAll");

// Autenticacion y autorizacion
app.UseAuthentication();
app.UseAuthorization();

// Mapear controladores
app.MapControllers();

#endregion

// Ejecutar la aplicacion
app.Run();
