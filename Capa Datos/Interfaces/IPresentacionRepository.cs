using System.Collections.Generic;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Repositorio para gestionar presentaciones de productos
    /// </summary>
    public interface IPresentacionRepository : IRepository<CE_ProductoPresentacion>
    {
        /// <summary>
        /// Obtiene todas las presentaciones de un producto
        /// </summary>
        Task<IEnumerable<CE_ProductoPresentacion>> GetByProductoAsync(int idArticulo);

        /// <summary>
        /// Obtiene todas las presentaciones activas de un producto
        /// </summary>
        Task<IEnumerable<CE_ProductoPresentacion>> GetActivasByProductoAsync(int idArticulo);

        /// <summary>
        /// Obtiene la presentacion unitaria de un producto (CantidadUnidades = 1)
        /// </summary>
        Task<CE_ProductoPresentacion> GetPresentacionUnitariaAsync(int idArticulo);

        /// <summary>
        /// Verifica si existe una presentacion con la misma cantidad de unidades para un producto
        /// </summary>
        Task<bool> ExistePresentacionAsync(int idArticulo, int cantidadUnidades);

        /// <summary>
        /// Obtiene todas las presentaciones activas (para listados de precios)
        /// </summary>
        Task<IEnumerable<CE_ProductoPresentacion>> GetAllActivasAsync();
    }
}
