using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public interface IProductoService
    {
        Task<ApiResponse<List<ProductoDTO>>> GetAllAsync();
        Task<ApiResponse<List<ProductoDTO>>> GetActivosAsync();
        Task<ApiResponse<ProductoDTO>> GetByIdAsync(int id);
        Task<ApiResponse<ProductoDTO>> GetByCodigoAsync(string codigo);
        Task<ApiResponse<List<ProductoDTO>>> SearchAsync(string term);
        Task<ApiResponse<ProductoDTO>> CreateAsync(ProductoCreateDTO dto);
        Task<ApiResponse<ProductoDTO>> UpdateAsync(int id, ProductoUpdateDTO dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<bool>> CambiarEstadoAsync(int id, bool activo);
    }

    public class ProductoService : ApiServiceBase, IProductoService
    {
        public ProductoService(HttpClient httpClient) : base(httpClient) { }

        /// <summary>
        /// Obtiene todos los productos (activos e inactivos)
        /// </summary>
        public async Task<ApiResponse<List<ProductoDTO>>> GetAllAsync()
            => await GetAsync<List<ProductoDTO>>("api/productos");

        /// <summary>
        /// Obtiene solo productos activos (para POS)
        /// </summary>
        public async Task<ApiResponse<List<ProductoDTO>>> GetActivosAsync()
            => await GetAsync<List<ProductoDTO>>("api/productos?soloActivos=true");

        public async Task<ApiResponse<ProductoDTO>> GetByIdAsync(int id)
            => await GetAsync<ProductoDTO>($"api/productos/{id}");

        public async Task<ApiResponse<ProductoDTO>> GetByCodigoAsync(string codigo)
            => await GetAsync<ProductoDTO>($"api/productos/codigo/{codigo}");

        public async Task<ApiResponse<List<ProductoDTO>>> SearchAsync(string term)
            => await GetAsync<List<ProductoDTO>>($"api/productos/buscar?termino={Uri.EscapeDataString(term)}");

        public async Task<ApiResponse<ProductoDTO>> CreateAsync(ProductoCreateDTO dto)
            => await PostAsync<ProductoDTO>("api/productos", dto);

        public async Task<ApiResponse<ProductoDTO>> UpdateAsync(int id, ProductoUpdateDTO dto)
            => await PutAsync<ProductoDTO>($"api/productos/{id}", dto);

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
            => await DeleteAsync<bool>($"api/productos/{id}");

        /// <summary>
        /// Activa o desactiva un producto
        /// </summary>
        public async Task<ApiResponse<bool>> CambiarEstadoAsync(int id, bool activo)
            => await PatchAsync<bool>($"api/productos/{id}/estado", new { Activo = activo });
    }
}
