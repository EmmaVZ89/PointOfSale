using System.Collections.Generic;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la entidad VentaDetalle.
    /// Extiende IRepository con metodos especificos para detalles de venta.
    /// </summary>
    public interface IVentaDetalleRepository : IRepository<CE_VentaDetalle>
    {
        /// <summary>
        /// Obtiene los detalles de una venta
        /// </summary>
        Task<IEnumerable<CE_VentaDetalle>> GetByVentaAsync(int idVenta);

        /// <summary>
        /// Obtiene detalles con informacion de producto
        /// </summary>
        Task<IEnumerable<CE_VentaDetalle>> GetByVentaConProductoAsync(int idVenta);
    }
}
