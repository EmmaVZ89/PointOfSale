using System;
using System.Threading.Tasks;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz para el patron Unit of Work.
    ///
    /// TECNOLOGIA NUEVA: Patron Unit of Work
    /// - Coordina la escritura de cambios de multiples repositorios
    /// - Garantiza que todas las operaciones se guarden en una sola transaccion
    /// - Si algo falla, todos los cambios se revierten automaticamente
    /// - Evita guardar cambios parciales que dejarian la BD en estado inconsistente
    ///
    /// Ejemplo de uso:
    /// using (var uow = new UnitOfWork(context))
    /// {
    ///     await uow.Productos.AddAsync(producto);
    ///     await uow.Movimientos.AddAsync(movimiento);
    ///     await uow.SaveChangesAsync(); // Guarda ambos en una transaccion
    /// }
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        #region Repositorios

        /// <summary>
        /// Repositorio de Usuarios
        /// </summary>
        IUsuarioRepository Usuarios { get; }

        /// <summary>
        /// Repositorio de Productos
        /// </summary>
        IProductoRepository Productos { get; }

        /// <summary>
        /// Repositorio de Clientes
        /// </summary>
        IClienteRepository Clientes { get; }

        /// <summary>
        /// Repositorio de Grupos
        /// </summary>
        IGrupoRepository Grupos { get; }

        /// <summary>
        /// Repositorio de Movimientos
        /// </summary>
        IMovimientoRepository Movimientos { get; }

        /// <summary>
        /// Repositorio de Ventas
        /// </summary>
        IVentaRepository Ventas { get; }

        /// <summary>
        /// Repositorio de Detalles de Venta
        /// </summary>
        IVentaDetalleRepository VentaDetalles { get; }

        /// <summary>
        /// Repositorio de Presentaciones de Producto
        /// </summary>
        IPresentacionRepository Presentaciones { get; }

        /// <summary>
        /// Repositorio de Costos Históricos de Producto
        /// </summary>
        IProductoCostoRepository ProductoCostos { get; }

        #endregion

        #region Transacciones

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos
        /// </summary>
        /// <returns>Numero de entidades afectadas</returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Inicia una transaccion explicita.
        /// Usar cuando necesitas control manual de transacciones.
        /// </summary>
        Task BeginTransactionAsync();

        /// <summary>
        /// Confirma la transaccion actual
        /// </summary>
        Task CommitTransactionAsync();

        /// <summary>
        /// Revierte la transaccion actual
        /// </summary>
        Task RollbackTransactionAsync();

        #endregion
    }
}
