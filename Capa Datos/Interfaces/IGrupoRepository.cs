using System.Collections.Generic;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la entidad Grupos (Categorias de productos).
    /// Extiende IRepository con metodos especificos para grupos.
    /// </summary>
    public interface IGrupoRepository : IRepository<CE_Grupos>
    {
        /// <summary>
        /// Busca un grupo por nombre exacto
        /// </summary>
        Task<CE_Grupos> GetByNombreAsync(string nombre);

        /// <summary>
        /// Busca grupos por nombre (busqueda parcial)
        /// </summary>
        Task<IEnumerable<CE_Grupos>> BuscarPorNombreAsync(string termino);

        /// <summary>
        /// Verifica si un grupo tiene productos asociados
        /// </summary>
        Task<bool> TieneProductosAsync(int idGrupo);

        /// <summary>
        /// Obtiene el conteo de productos por grupo
        /// </summary>
        Task<int> ContarProductosAsync(int idGrupo);
    }
}
