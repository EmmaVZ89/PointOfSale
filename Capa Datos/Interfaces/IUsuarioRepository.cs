using System.Collections.Generic;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la entidad Usuarios.
    /// Extiende IRepository con metodos especificos para usuarios.
    /// </summary>
    public interface IUsuarioRepository : IRepository<CE_Usuarios>
    {
        /// <summary>
        /// Busca un usuario por nombre de usuario y contrasena (login)
        /// </summary>
        Task<CE_Usuarios> ValidarCredencialesAsync(string usuario, string contrasena);

        /// <summary>
        /// Busca un usuario por patron de desbloqueo
        /// </summary>
        Task<CE_Usuarios> ValidarPatronAsync(string patron);

        /// <summary>
        /// Busca un usuario por DNI
        /// </summary>
        Task<CE_Usuarios> GetByDniAsync(int dni);

        /// <summary>
        /// Busca un usuario por nombre de usuario
        /// </summary>
        Task<CE_Usuarios> GetByUsuarioAsync(string usuario);

        /// <summary>
        /// Obtiene todos los usuarios activos
        /// </summary>
        Task<IEnumerable<CE_Usuarios>> GetActivosAsync();

        /// <summary>
        /// Busca usuarios por nombre o apellido (busqueda parcial)
        /// </summary>
        Task<IEnumerable<CE_Usuarios>> BuscarPorNombreAsync(string termino);

        /// <summary>
        /// Activa o desactiva un usuario
        /// </summary>
        Task CambiarEstadoAsync(int idUsuario, bool activo);
    }
}
