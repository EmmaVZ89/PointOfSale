using System;
using Microsoft.EntityFrameworkCore;
using Capa_Entidad;

namespace Capa_Datos.Context
{
    /// <summary>
    /// DbContext de Entity Framework Core para la aplicacion PuntoDeVenta.
    /// Configurado para coincidir con el schema PostgreSQL (PascalCase con comillas).
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        #region DbSets - Tablas de la base de datos

        public DbSet<CE_Usuarios> Usuarios { get; set; }
        public DbSet<CE_Productos> Productos { get; set; }
        public DbSet<CE_Clientes> Clientes { get; set; }
        public DbSet<CE_Grupos> Grupos { get; set; }
        public DbSet<CE_Movimiento> Movimientos { get; set; }

        #endregion

        #region Constructores

        public ApplicationDbContext() : base()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        #endregion

        #region Configuracion

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = ConfigurationHelper.GetConnectionString();
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        /// <summary>
        /// Configura el mapeo para coincidir con el script PostgreSQL.
        /// Los nombres usan PascalCase exacto del SQL.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================================
            // CE_Usuarios -> "Usuarios"
            // ============================================
            modelBuilder.Entity<CE_Usuarios>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.IdUsuario);
                entity.Property(e => e.IdUsuario).HasColumnName("IdUsuario");
                entity.Property(e => e.Nombre).HasColumnName("Nombre");
                entity.Property(e => e.Apellido).HasColumnName("Apellido");
                entity.Property(e => e.Dni).HasColumnName("DNI");
                entity.Property(e => e.Cuit).HasColumnName("CUIT");
                entity.Property(e => e.Correo).HasColumnName("Correo");
                entity.Property(e => e.Telefono).HasColumnName("Telefono");
                entity.Property(e => e.Fecha_Nac).HasColumnName("Fecha_Nac");
                entity.Property(e => e.Privilegio).HasColumnName("Privilegio");
                entity.Property(e => e.Img).HasColumnName("img");
                entity.Property(e => e.Usuario).HasColumnName("usuario");
                // Contrasenia es BYTEA encriptada - ignoramos para evitar error de tipo
                // La validacion se hace con raw SQL usando pgp_sym_decrypt
                entity.Ignore(e => e.Contrasenia);
                entity.Property(e => e.Patron).HasColumnName("Patron");
                entity.Property(e => e.Activo).HasColumnName("Activo");
            });

            // ============================================
            // CE_Productos -> "Articulos"
            // ============================================
            modelBuilder.Entity<CE_Productos>(entity =>
            {
                entity.ToTable("Articulos");
                entity.HasKey(e => e.IdArticulo);
                entity.Property(e => e.IdArticulo).HasColumnName("IdArticulo");
                entity.Property(e => e.Nombre).HasColumnName("Nombre");
                entity.Property(e => e.Grupo).HasColumnName("Grupo");
                entity.Property(e => e.Codigo).HasColumnName("Codigo");
                entity.Property(e => e.Precio).HasColumnName("Precio");
                entity.Property(e => e.Activo).HasColumnName("Activo");
                entity.Property(e => e.Cantidad).HasColumnName("Cantidad");
                entity.Property(e => e.UnidadMedida).HasColumnName("UnidadMedida");
                entity.Property(e => e.Img).HasColumnName("Img");
                entity.Property(e => e.Descripcion).HasColumnName("Descripcion");
            });

            // ============================================
            // CE_Clientes -> "Clientes"
            // ============================================
            modelBuilder.Entity<CE_Clientes>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(e => e.IdCliente);
                entity.Property(e => e.IdCliente).HasColumnName("IdCliente");
                entity.Property(e => e.RazonSocial).HasColumnName("RazonSocial");
                entity.Property(e => e.Documento).HasColumnName("Documento");
                entity.Property(e => e.Telefono).HasColumnName("Telefono");
                entity.Property(e => e.Email).HasColumnName("Email");
                entity.Property(e => e.Activo).HasColumnName("Activo");
                entity.Property(e => e.FechaAlta).HasColumnName("FechaAlta");
            });

            // ============================================
            // CE_Grupos -> "Grupos"
            // ============================================
            modelBuilder.Entity<CE_Grupos>(entity =>
            {
                entity.ToTable("Grupos");
                entity.HasKey(e => e.IdGrupo);
                entity.Property(e => e.IdGrupo).HasColumnName("IdGrupo");
                entity.Property(e => e.Nombre).HasColumnName("Nombre");
            });

            // ============================================
            // CE_Movimiento -> "MovimientosInventario"
            // ============================================
            modelBuilder.Entity<CE_Movimiento>(entity =>
            {
                entity.ToTable("MovimientosInventario");
                entity.HasKey(e => e.IdMovimiento);
                entity.Property(e => e.IdMovimiento).HasColumnName("IdMovimiento");
                entity.Property(e => e.IdArticulo).HasColumnName("IdArticulo");
                entity.Property(e => e.IdUsuario).HasColumnName("IdUsuario");
                entity.Property(e => e.TipoMovimiento).HasColumnName("TipoMovimiento");
                entity.Property(e => e.Cantidad).HasColumnName("Cantidad");
                entity.Property(e => e.FechaMovimiento).HasColumnName("FechaMovimiento");
                entity.Property(e => e.Observacion).HasColumnName("Observacion");

                // Propiedades de navegacion (no en BD)
                entity.Ignore(e => e.NombreProducto);
                entity.Ignore(e => e.CodigoProducto);
                entity.Ignore(e => e.UsuarioResponsable);
            });
        }

        #endregion
    }
}
