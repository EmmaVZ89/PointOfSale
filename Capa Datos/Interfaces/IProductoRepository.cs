using System.Collections.Generic;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la entidad Productos (Articulos).
    /// Extiende IRepository con metodos especificos para productos.
    /// </summary>
    public interface IProductoRepository : IRepository<CE_Productos>
    {
        /// <summary>
        /// Busca un producto por codigo de barras
        /// </summary>
        Task<CE_Productos> GetByCodigoAsync(string codigo);

        /// <summary>
        /// Obtiene todos los productos activos
        /// </summary>
        Task<IEnumerable<CE_Productos>> GetActivosAsync();

        /// <summary>
        /// Obtiene productos por grupo/categoria
        /// </summary>
        Task<IEnumerable<CE_Productos>> GetByGrupoAsync(int idGrupo);

        /// <summary>
        /// Busca productos por nombre (busqueda parcial)
        /// </summary>
        Task<IEnumerable<CE_Productos>> BuscarPorNombreAsync(string termino);

        /// <summary>
        /// Obtiene productos con stock bajo (cantidad menor al minimo)
        /// </summary>
        Task<IEnumerable<CE_Productos>> GetConStockBajoAsync(decimal cantidadMinima);

        /// <summary>
        /// Actualiza el stock de un producto
        /// </summary>
        Task ActualizarStockAsync(int idArticulo, decimal nuevaCantidad);

        /// <summary>
        /// Activa o desactiva un producto
        /// </summary>
        Task CambiarEstadoAsync(int idArticulo, bool activo);

        /// <summary>
        /// Obtiene productos paginados para listados grandes
        /// </summary>
        Task<IEnumerable<CE_Productos>> GetPaginadoAsync(int pagina, int tamanioPagina);

        /// <summary>
        /// Cuenta el total de productos (para paginacion)
        /// </summary>
        Task<int> ContarTotalAsync();
    }
}
