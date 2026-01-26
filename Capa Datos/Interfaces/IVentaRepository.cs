using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la entidad Ventas.
    /// Extiende IRepository con metodos especificos para ventas.
    /// </summary>
    public interface IVentaRepository : IRepository<CE_Ventas>
    {
        /// <summary>
        /// Obtiene una venta por numero de factura
        /// </summary>
        Task<CE_Ventas?> GetByFacturaAsync(string noFactura);

        /// <summary>
        /// Obtiene ventas por cliente
        /// </summary>
        Task<IEnumerable<CE_Ventas>> GetByClienteAsync(int idCliente);

        /// <summary>
        /// Obtiene ventas por usuario
        /// </summary>
        Task<IEnumerable<CE_Ventas>> GetByUsuarioAsync(int idUsuario);

        /// <summary>
        /// Obtiene ventas en un rango de fechas
        /// </summary>
        Task<IEnumerable<CE_Ventas>> GetByFechaAsync(DateTime fechaDesde, DateTime fechaHasta);

        /// <summary>
        /// Obtiene ventas paginadas con filtros
        /// </summary>
        Task<(IEnumerable<CE_Ventas> Items, int TotalCount)> GetPaginadoAsync(
            int? idCliente,
            int? idUsuario,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            bool? cancelada,
            int pagina,
            int tamanioPagina);

        /// <summary>
        /// Cancela una venta y registra motivo
        /// </summary>
        Task CancelarVentaAsync(int idVenta, int idUsuarioCancelo, string motivo);

        /// <summary>
        /// Genera el siguiente numero de factura
        /// </summary>
        string GenerarNumeroFactura();
    }
}
