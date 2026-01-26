using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public interface IGrupoService
    {
        Task<ApiResponse<List<GrupoDTO>>> GetAllAsync();
        Task<ApiResponse<GrupoDTO>> GetByIdAsync(int id);
        Task<ApiResponse<GrupoDTO>> CreateAsync(GrupoCreateDTO dto);
        Task<ApiResponse<GrupoDTO>> UpdateAsync(int id, GrupoUpdateDTO dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }

    public class GrupoService : ApiServiceBase, IGrupoService
    {
        public GrupoService(HttpClient httpClient) : base(httpClient) { }

        public async Task<ApiResponse<List<GrupoDTO>>> GetAllAsync()
            => await GetAsync<List<GrupoDTO>>("api/grupos");

        public async Task<ApiResponse<GrupoDTO>> GetByIdAsync(int id)
            => await GetAsync<GrupoDTO>($"api/grupos/{id}");

        public async Task<ApiResponse<GrupoDTO>> CreateAsync(GrupoCreateDTO dto)
            => await PostAsync<GrupoDTO>("api/grupos", dto);

        public async Task<ApiResponse<GrupoDTO>> UpdateAsync(int id, GrupoUpdateDTO dto)
            => await PutAsync<GrupoDTO>($"api/grupos/{id}", dto);

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
            => await DeleteAsync<bool>($"api/grupos/{id}");
    }
}
