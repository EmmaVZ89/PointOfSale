using System.Collections.Generic;
using System.Threading.Tasks;
using Capa_Entidad;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz de repositorio para la entidad Clientes.
    /// Extiende IRepository con metodos especificos para clientes.
    /// </summary>
    public interface IClienteRepository : IRepository<CE_Clientes>
    {
        /// <summary>
        /// Busca un cliente por documento (DNI/CUIT)
        /// </summary>
        Task<CE_Clientes> GetByDocumentoAsync(string documento);

        /// <summary>
        /// Obtiene todos los clientes activos
        /// </summary>
        Task<IEnumerable<CE_Clientes>> GetActivosAsync();

        /// <summary>
        /// Busca clientes por razon social (busqueda parcial)
        /// </summary>
        Task<IEnumerable<CE_Clientes>> BuscarPorRazonSocialAsync(string termino);

        /// <summary>
        /// Busca clientes por email
        /// </summary>
        Task<CE_Clientes> GetByEmailAsync(string email);

        /// <summary>
        /// Activa o desactiva un cliente
        /// </summary>
        Task CambiarEstadoAsync(int idCliente, bool activo);

        /// <summary>
        /// Obtiene clientes dados de alta en un rango de fechas
        /// </summary>
        Task<IEnumerable<CE_Clientes>> GetByFechaAltaAsync(System.DateTime desde, System.DateTime hasta);
    }
}
