using System;
using Microsoft.EntityFrameworkCore;
using Capa_Entidad;

namespace Capa_Datos.Context
{
    /// <summary>
    /// DbContext de Entity Framework Core para la aplicacion PuntoDeVenta.
    ///
    /// TECNOLOGIA NUEVA: Entity Framework Core
    /// - Es el ORM (Object-Relational Mapper) oficial de Microsoft para .NET
    /// - Permite trabajar con la base de datos usando objetos C# en lugar de SQL
    /// - Soporta LINQ para consultas tipadas y seguras
    /// - Maneja automaticamente las conexiones, transacciones y tracking de cambios
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        #region DbSets - Tablas de la base de datos

        /// <summary>
        /// Tabla de usuarios del sistema
        /// </summary>
        public DbSet<CE_Usuarios> Usuarios { get; set; }

        /// <summary>
        /// Tabla de productos/articulos
        /// </summary>
        public DbSet<CE_Productos> Productos { get; set; }

        /// <summary>
        /// Tabla de clientes
        /// </summary>
        public DbSet<CE_Clientes> Clientes { get; set; }

        /// <summary>
        /// Tabla de grupos/categorias de productos
        /// </summary>
        public DbSet<CE_Grupos> Grupos { get; set; }

        /// <summary>
        /// Tabla de movimientos de inventario
        /// </summary>
        public DbSet<CE_Movimiento> Movimientos { get; set; }

        #endregion

        #region Constructores

        /// <summary>
        /// Constructor sin parametros - usa la cadena de conexion del ConfigurationHelper
        /// </summary>
        public ApplicationDbContext() : base()
        {
        }

        /// <summary>
        /// Constructor con opciones - usado para Dependency Injection
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #endregion

        #region Configuracion

        /// <summary>
        /// Configura la conexion a la base de datos.
        /// Solo se usa cuando no se pasan opciones en el constructor.
        /// </summary>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Obtener la cadena de conexion desde appsettings.json
                string connectionString = ConfigurationHelper.GetConnectionString();
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        /// <summary>
        /// Configura el mapeo de las entidades a las tablas de la base de datos.
        /// Aqui se definen los nombres de tablas, columnas, claves primarias, etc.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuracion de CE_Usuarios
            modelBuilder.Entity<CE_Usuarios>(entity =>
            {
                entity.ToTable("usuarios");
                entity.HasKey(e => e.IdUsuario);
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.Apellido).HasColumnName("apellido");
                entity.Property(e => e.Dni).HasColumnName("dni");
                entity.Property(e => e.Cuit).HasColumnName("cuit");
                entity.Property(e => e.Correo).HasColumnName("correo");
                entity.Property(e => e.Telefono).HasColumnName("telefono");
                entity.Property(e => e.Fecha_Nac).HasColumnName("fecha_nac");
                entity.Property(e => e.Privilegio).HasColumnName("privilegio");
                entity.Property(e => e.Img).HasColumnName("img");
                entity.Property(e => e.Usuario).HasColumnName("usuario");
                entity.Property(e => e.Contrasenia).HasColumnName("contrasenia");
                entity.Property(e => e.Patron).HasColumnName("patron");
                entity.Property(e => e.Activo).HasColumnName("activo");
            });

            // Configuracion de CE_Productos
            modelBuilder.Entity<CE_Productos>(entity =>
            {
                entity.ToTable("articulos");
                entity.HasKey(e => e.IdArticulo);
                entity.Property(e => e.IdArticulo).HasColumnName("id_articulo");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.Grupo).HasColumnName("grupo");
                entity.Property(e => e.Codigo).HasColumnName("codigo");
                entity.Property(e => e.Precio).HasColumnName("precio");
                entity.Property(e => e.Activo).HasColumnName("activo");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.UnidadMedida).HasColumnName("unidad_medida");
                entity.Property(e => e.Img).HasColumnName("img");
                entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            });

            // Configuracion de CE_Clientes
            modelBuilder.Entity<CE_Clientes>(entity =>
            {
                entity.ToTable("clientes");
                entity.HasKey(e => e.IdCliente);
                entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
                entity.Property(e => e.RazonSocial).HasColumnName("razon_social");
                entity.Property(e => e.Documento).HasColumnName("documento");
                entity.Property(e => e.Telefono).HasColumnName("telefono");
                entity.Property(e => e.Email).HasColumnName("email");
                entity.Property(e => e.Activo).HasColumnName("activo");
                entity.Property(e => e.FechaAlta).HasColumnName("fecha_alta");
            });

            // Configuracion de CE_Grupos
            modelBuilder.Entity<CE_Grupos>(entity =>
            {
                entity.ToTable("grupos");
                entity.HasKey(e => e.IdGrupo);
                entity.Property(e => e.IdGrupo).HasColumnName("id_grupo");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
            });

            // Configuracion de CE_Movimiento
            modelBuilder.Entity<CE_Movimiento>(entity =>
            {
                entity.ToTable("movimientos_inventario");
                entity.HasKey(e => e.IdMovimiento);
                entity.Property(e => e.IdMovimiento).HasColumnName("id_movimiento");
                entity.Property(e => e.IdArticulo).HasColumnName("id_articulo");
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.TipoMovimiento).HasColumnName("tipo_movimiento");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.FechaMovimiento).HasColumnName("fecha_movimiento");
                entity.Property(e => e.Observacion).HasColumnName("observacion");

                // Ignorar propiedades de navegacion que no estan en la BD
                entity.Ignore(e => e.NombreProducto);
                entity.Ignore(e => e.CodigoProducto);
                entity.Ignore(e => e.UsuarioResponsable);
            });
        }

        #endregion
    }
}
