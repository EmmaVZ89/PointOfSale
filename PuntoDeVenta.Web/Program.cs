using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Blazored.LocalStorage;
using Blazored.Toast;
using PuntoDeVenta.Web;
using PuntoDeVenta.Web.Services;
using PuntoDeVenta.Web.State;
using System.Globalization;

// Configurar cultura argentina (es-AR) para toda la aplicacion
var cultureInfo = new CultureInfo("es-AR");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configurar la URL base de la API
var apiBaseUrl = builder.Configuration.GetValue<string>("ApiBaseUrl") ?? "http://localhost:5207/";

// Registrar HttpClient con la URL de la API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Registrar Blazored.LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Registrar Blazored.Toast
builder.Services.AddBlazoredToast();

// Registrar AuthStateProvider personalizado
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthStateProvider>());

// Registrar servicio de autenticacion
builder.Services.AddScoped<IAuthService, AuthService>();

// Registrar servicios de la API
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IGrupoService, GrupoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IMovimientoService, MovimientoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IVentaService, VentaService>();
builder.Services.AddScoped<IPresentacionService, PresentacionService>();

// Registrar estado del carrito (singleton para mantener estado entre paginas)
builder.Services.AddSingleton<CartState>();

// Registrar estado del sidebar (singleton para mantener estado entre paginas)
builder.Services.AddSingleton<SidebarState>();

// Registrar servicio de temas (singleton para mantener estado entre paginas)
builder.Services.AddScoped<IThemeService, ThemeService>();

// Configurar autorizacion
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
