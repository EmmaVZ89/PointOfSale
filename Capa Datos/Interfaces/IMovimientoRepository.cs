using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la entidad Movimientos de Inventario.
    /// Extiende IRepository con metodos especificos para movimientos.
    /// </summary>
    public interface IMovimientoRepository : IRepository<CE_Movimiento>
    {
        /// <summary>
        /// Obtiene movimientos de un producto especifico
        /// </summary>
        Task<IEnumerable<CE_Movimiento>> GetByArticuloAsync(int idArticulo);

        /// <summary>
        /// Obtiene movimientos realizados por un usuario
        /// </summary>
        Task<IEnumerable<CE_Movimiento>> GetByUsuarioAsync(int idUsuario);

        /// <summary>
        /// Obtiene movimientos por tipo (entrada, salida, ajuste)
        /// </summary>
        Task<IEnumerable<CE_Movimiento>> GetByTipoAsync(string tipoMovimiento);

        /// <summary>
        /// Obtiene movimientos en un rango de fechas
        /// </summary>
        Task<IEnumerable<CE_Movimiento>> GetByFechaAsync(DateTime desde, DateTime hasta);

        /// <summary>
        /// Obtiene movimientos con datos de navegacion (producto y usuario)
        /// para mostrar en grillas sin necesidad de joins adicionales
        /// </summary>
        Task<IEnumerable<CE_Movimiento>> GetConDetallesAsync();

        /// <summary>
        /// Obtiene movimientos de un producto en un rango de fechas
        /// </summary>
        Task<IEnumerable<CE_Movimiento>> GetByArticuloYFechaAsync(int idArticulo, DateTime desde, DateTime hasta);

        /// <summary>
        /// Obtiene el ultimo movimiento de un producto
        /// </summary>
        Task<CE_Movimiento> GetUltimoMovimientoAsync(int idArticulo);

        /// <summary>
        /// Calcula el total de entradas de un producto
        /// </summary>
        Task<decimal> GetTotalEntradasAsync(int idArticulo);

        /// <summary>
        /// Calcula el total de salidas de un producto
        /// </summary>
        Task<decimal> GetTotalSalidasAsync(int idArticulo);
    }
}
