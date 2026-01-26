using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public interface IUsuarioService
    {
        Task<ApiResponse<List<UsuarioDTO>>> GetAllAsync();
        Task<ApiResponse<UsuarioDTO>> GetByIdAsync(int id);
        Task<ApiResponse<UsuarioDTO>> CreateAsync(UsuarioCreateDTO dto);
        Task<ApiResponse<UsuarioDTO>> UpdateAsync(int id, UsuarioUpdateDTO dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
        Task<ApiResponse<bool>> ReactivarAsync(int id);
        Task<ApiResponse<bool>> CambiarContrasenaAsync(int id, CambiarContrasenaDTO dto);
    }

    public class UsuarioService : ApiServiceBase, IUsuarioService
    {
        public UsuarioService(HttpClient httpClient) : base(httpClient) { }

        public async Task<ApiResponse<List<UsuarioDTO>>> GetAllAsync()
            => await GetAsync<List<UsuarioDTO>>("api/usuarios");

        public async Task<ApiResponse<UsuarioDTO>> GetByIdAsync(int id)
            => await GetAsync<UsuarioDTO>($"api/usuarios/{id}");

        public async Task<ApiResponse<UsuarioDTO>> CreateAsync(UsuarioCreateDTO dto)
            => await PostAsync<UsuarioDTO>("api/usuarios", dto);

        public async Task<ApiResponse<UsuarioDTO>> UpdateAsync(int id, UsuarioUpdateDTO dto)
            => await PutAsync<UsuarioDTO>($"api/usuarios/{id}", dto);

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
            => await DeleteAsync<bool>($"api/usuarios/{id}");

        public async Task<ApiResponse<bool>> ReactivarAsync(int id)
            => await PostAsync<bool>($"api/usuarios/{id}/reactivar", new { });

        public async Task<ApiResponse<bool>> CambiarContrasenaAsync(int id, CambiarContrasenaDTO dto)
            => await PostAsync<bool>($"api/usuarios/{id}/cambiar-contrasena", dto);
    }
}
