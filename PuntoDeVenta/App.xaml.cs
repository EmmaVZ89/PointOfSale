using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Capa_Datos.Context;
using Capa_Datos.Interfaces;
using Capa_Datos.Repositories;

namespace PuntoDeVenta
{
    /// <summary>
    /// Logica de interaccion para App.xaml
    ///
    /// TECNOLOGIA NUEVA: Dependency Injection (DI) / Inyeccion de Dependencias
    /// - DI es un patron de diseno que permite crear codigo desacoplado y testeable
    /// - En lugar de crear dependencias con "new", se "inyectan" desde afuera
    /// - ServiceProvider: Contenedor que crea y gestiona las instancias
    /// - Scoped: Una instancia por "scope" (en WPF, usualmente por ventana/operacion)
    /// - Transient: Nueva instancia cada vez que se solicita
    /// - Singleton: Una sola instancia para toda la aplicacion
    ///
    /// Beneficios:
    /// - Facilita testing (puedes inyectar mocks)
    /// - Codigo desacoplado y facil de mantener
    /// - Control centralizado de ciclo de vida de objetos
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Proveedor de servicios global.
        /// Permite acceder a los servicios registrados desde cualquier parte.
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// Se ejecuta al iniciar la aplicacion.
        /// Aqui configuramos el contenedor de DI.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Crear la coleccion de servicios
            var services = new ServiceCollection();

            // Configurar los servicios
            ConfigureServices(services);

            // Construir el proveedor de servicios
            ServiceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Configura todos los servicios de la aplicacion.
        /// Aqui se registran las dependencias y sus implementaciones.
        /// </summary>
        /// <param name="services">Coleccion de servicios a configurar</param>
        private void ConfigureServices(IServiceCollection services)
        {
            // ============================================
            // CAPA DATOS - Entity Framework Core
            // ============================================

            // DbContext - Scoped: Una instancia por operacion/scope
            // Esto es importante para EF Core ya que el contexto
            // trackea los cambios y debe liberarse apropiadamente
            services.AddScoped<ApplicationDbContext>();

            // ============================================
            // REPOSITORIOS - Patron Repository
            // ============================================

            // Repositorios especificos - Scoped
            // Scoped asegura que usen el mismo DbContext durante una operacion
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IProductoRepository, ProductoRepository>();
            services.AddScoped<IClienteRepository, ClienteRepository>();
            services.AddScoped<IGrupoRepository, GrupoRepository>();
            services.AddScoped<IMovimientoRepository, MovimientoRepository>();

            // ============================================
            // UNIT OF WORK - Patron Unit of Work
            // ============================================

            // UnitOfWork - Scoped: Coordina repositorios en transacciones
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ============================================
            // SERVICIOS DE NEGOCIO (Capa Negocio)
            // ============================================
            // TODO: Agregar servicios de negocio cuando se migren a interfaces
            // services.AddScoped<IUsuarioService, CN_Usuarios>();
            // services.AddScoped<IProductoService, CN_Productos>();
        }

        /// <summary>
        /// Se ejecuta al cerrar la aplicacion.
        /// Libera los recursos del contenedor de DI.
        /// </summary>
        protected override void OnExit(ExitEventArgs e)
        {
            if (ServiceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            base.OnExit(e);
        }

        /// <summary>
        /// Metodo helper para obtener servicios desde cualquier parte de la aplicacion.
        ///
        /// Uso:
        /// var uow = App.GetService{IUnitOfWork}();
        /// var usuarios = await uow.Usuarios.GetActivosAsync();
        /// </summary>
        /// <typeparam name="T">Tipo del servicio a obtener</typeparam>
        /// <returns>Instancia del servicio</returns>
        public static T GetService<T>() where T : class
        {
            return ServiceProvider.GetService<T>();
        }

        /// <summary>
        /// Crea un nuevo scope para operaciones que requieren su propio contexto.
        /// Util para operaciones en segundo plano o paralelas.
        ///
        /// Uso:
        /// using (var scope = App.CreateScope())
        /// {
        ///     var uow = scope.ServiceProvider.GetService{IUnitOfWork}();
        ///     // ... operaciones
        /// }
        /// </summary>
        public static IServiceScope CreateScope()
        {
            return ServiceProvider.CreateScope();
        }
    }
}
