using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de costos históricos de productos.
    /// Solo utilizado por administradores.
    /// </summary>
    public interface IProductoCostoRepository : IRepository<CE_ProductoCostoHistorico>
    {
        /// <summary>
        /// Obtiene el último costo registrado para un producto
        /// </summary>
        Task<CE_ProductoCostoHistorico> GetUltimoCostoAsync(int idArticulo);

        /// <summary>
        /// Obtiene el histórico de costos de un producto ordenado por fecha descendente
        /// </summary>
        Task<IEnumerable<CE_ProductoCostoHistorico>> GetHistoricoAsync(int idArticulo);

        /// <summary>
        /// Obtiene los últimos costos de múltiples productos (para reportes)
        /// </summary>
        Task<Dictionary<int, decimal>> GetUltimosCostosAsync(IEnumerable<int> idArticulos);

        /// <summary>
        /// Obtiene el costo vigente de un producto a una fecha específica.
        /// Busca el registro de costo más reciente antes o igual a la fecha indicada.
        /// </summary>
        Task<decimal?> GetCostoVigenteAsync(int idArticulo, DateTime fecha);

        /// <summary>
        /// Obtiene los costos vigentes de múltiples productos a una fecha específica.
        /// Para cada producto, busca el costo más reciente antes o igual a la fecha.
        /// </summary>
        Task<Dictionary<int, decimal>> GetCostosVigentesAsync(IEnumerable<int> idArticulos, DateTime fecha);
    }
}
