using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public interface IClienteService
    {
        Task<ApiResponse<List<ClienteDTO>>> GetAllAsync();
        Task<ApiResponse<List<ClienteDTO>>> GetActivosAsync();
        Task<ApiResponse<ClienteDTO>> GetByIdAsync(int id);
        Task<ApiResponse<List<ClienteDTO>>> SearchAsync(string term);
        Task<ApiResponse<ClienteDTO>> CreateAsync(ClienteCreateDTO dto);
        Task<ApiResponse<ClienteDTO>> UpdateAsync(int id, ClienteUpdateDTO dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<bool>> CambiarEstadoAsync(int id, bool activo);
        Task<ApiResponse<ClienteEstadisticasDTO>> GetEstadisticasAsync(int id);
        Task<ApiResponse<List<ClienteCompraDTO>>> GetComprasAsync(int id, int limite = 10);
    }

    public class ClienteService : ApiServiceBase, IClienteService
    {
        public ClienteService(HttpClient httpClient) : base(httpClient) { }

        /// <summary>
        /// Obtiene todos los clientes (activos e inactivos)
        /// </summary>
        public async Task<ApiResponse<List<ClienteDTO>>> GetAllAsync()
            => await GetAsync<List<ClienteDTO>>("api/clientes");

        /// <summary>
        /// Obtiene solo clientes activos (para POS)
        /// </summary>
        public async Task<ApiResponse<List<ClienteDTO>>> GetActivosAsync()
            => await GetAsync<List<ClienteDTO>>("api/clientes?soloActivos=true");

        public async Task<ApiResponse<ClienteDTO>> GetByIdAsync(int id)
            => await GetAsync<ClienteDTO>($"api/clientes/{id}");

        public async Task<ApiResponse<List<ClienteDTO>>> SearchAsync(string term)
            => await GetAsync<List<ClienteDTO>>($"api/clientes/buscar?termino={Uri.EscapeDataString(term)}");

        public async Task<ApiResponse<ClienteDTO>> CreateAsync(ClienteCreateDTO dto)
            => await PostAsync<ClienteDTO>("api/clientes", dto);

        public async Task<ApiResponse<ClienteDTO>> UpdateAsync(int id, ClienteUpdateDTO dto)
            => await PutAsync<ClienteDTO>($"api/clientes/{id}", dto);

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
            => await DeleteAsync<bool>($"api/clientes/{id}");

        /// <summary>
        /// Activa o desactiva un cliente
        /// </summary>
        public async Task<ApiResponse<bool>> CambiarEstadoAsync(int id, bool activo)
            => await PatchAsync<bool>($"api/clientes/{id}/estado", new { Activo = activo });

        /// <summary>
        /// Obtiene estadisticas del cliente (compras, monto acumulado, etc.)
        /// </summary>
        public async Task<ApiResponse<ClienteEstadisticasDTO>> GetEstadisticasAsync(int id)
            => await GetAsync<ClienteEstadisticasDTO>($"api/clientes/{id}/estadisticas");

        /// <summary>
        /// Obtiene historial de compras del cliente
        /// </summary>
        public async Task<ApiResponse<List<ClienteCompraDTO>>> GetComprasAsync(int id, int limite = 10)
            => await GetAsync<List<ClienteCompraDTO>>($"api/clientes/{id}/compras?limite={limite}");
    }
}
