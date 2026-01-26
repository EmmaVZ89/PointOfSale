using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public interface IPresentacionService
    {
        Task<ApiResponse<List<PresentacionDTO>>> GetByProductoAsync(int idArticulo);
        Task<ApiResponse<List<PresentacionDTO>>> GetActivasByProductoAsync(int idArticulo);
        Task<ApiResponse<List<PresentacionDTO>>> GetAllActivasAsync();
        Task<ApiResponse<PresentacionDTO>> GetByIdAsync(int id);
        Task<ApiResponse<PresentacionDTO>> CreateAsync(PresentacionCreateDTO dto);
        Task<ApiResponse<PresentacionDTO>> CreateRapidaAsync(PresentacionRapidaDTO dto);
        Task<ApiResponse<PresentacionDTO>> UpdateAsync(int id, PresentacionUpdateDTO dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }

    public class PresentacionService : ApiServiceBase, IPresentacionService
    {
        public PresentacionService(HttpClient httpClient) : base(httpClient) { }

        /// <summary>
        /// Obtiene todas las presentaciones de un producto
        /// </summary>
        public async Task<ApiResponse<List<PresentacionDTO>>> GetByProductoAsync(int idArticulo)
            => await GetAsync<List<PresentacionDTO>>($"api/presentaciones/producto/{idArticulo}");

        /// <summary>
        /// Obtiene las presentaciones activas de un producto (para POS)
        /// </summary>
        public async Task<ApiResponse<List<PresentacionDTO>>> GetActivasByProductoAsync(int idArticulo)
            => await GetAsync<List<PresentacionDTO>>($"api/presentaciones/producto/{idArticulo}/activas");

        /// <summary>
        /// Obtiene todas las presentaciones activas (para lista de precios)
        /// </summary>
        public async Task<ApiResponse<List<PresentacionDTO>>> GetAllActivasAsync()
            => await GetAsync<List<PresentacionDTO>>("api/presentaciones/activas");

        /// <summary>
        /// Obtiene una presentacion por ID
        /// </summary>
        public async Task<ApiResponse<PresentacionDTO>> GetByIdAsync(int id)
            => await GetAsync<PresentacionDTO>($"api/presentaciones/{id}");

        /// <summary>
        /// Crea una nueva presentacion personalizada
        /// </summary>
        public async Task<ApiResponse<PresentacionDTO>> CreateAsync(PresentacionCreateDTO dto)
            => await PostAsync<PresentacionDTO>("api/presentaciones", dto);

        /// <summary>
        /// Crea un pack predefinido (x6, x12, x18, x24)
        /// </summary>
        public async Task<ApiResponse<PresentacionDTO>> CreateRapidaAsync(PresentacionRapidaDTO dto)
            => await PostAsync<PresentacionDTO>("api/presentaciones/rapida", dto);

        /// <summary>
        /// Actualiza una presentacion
        /// </summary>
        public async Task<ApiResponse<PresentacionDTO>> UpdateAsync(int id, PresentacionUpdateDTO dto)
            => await PutAsync<PresentacionDTO>($"api/presentaciones/{id}", dto);

        /// <summary>
        /// Desactiva una presentacion
        /// </summary>
        public async Task<ApiResponse<bool>> DeleteAsync(int id)
            => await DeleteAsync<bool>($"api/presentaciones/{id}");
    }
}
