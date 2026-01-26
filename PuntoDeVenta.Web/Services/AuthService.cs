using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task LogoutAsync();
        Task<UsuarioDTO?> GetCurrentUserAsync();
        Task<int?> GetCurrentUserIdAsync();
        Task<bool> IsAuthenticatedAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthStateProvider _authStateProvider;

        private const string TokenKey = "authToken";
        private const string RefreshTokenKey = "refreshToken";
        private const string UserKey = "currentUser";

        public AuthService(
            HttpClient httpClient,
            ILocalStorageService localStorage,
            AuthStateProvider authStateProvider)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();

                if (result?.Success == true && result.Data != null)
                {
                    await _localStorage.SetItemAsStringAsync(TokenKey, result.Data.Token);
                    await _localStorage.SetItemAsStringAsync(RefreshTokenKey, result.Data.RefreshToken);

                    if (result.Data.Usuario != null)
                    {
                        await _localStorage.SetItemAsync(UserKey, result.Data.Usuario);
                    }

                    _authStateProvider.NotifyUserAuthentication(result.Data.Token);
                }

                return result ?? ApiResponse<LoginResponse>.Error("Error en la respuesta del servidor");
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponse>
                {
                    Success = false,
                    Message = $"Error de conexion: {ex.Message}"
                };
            }
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync(TokenKey);
            await _localStorage.RemoveItemAsync(RefreshTokenKey);
            await _localStorage.RemoveItemAsync(UserKey);
            _authStateProvider.NotifyUserLogout();
        }

        public async Task<UsuarioDTO?> GetCurrentUserAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<UsuarioDTO>(UserKey);
            }
            catch
            {
                return null;
            }
        }

        public async Task<int?> GetCurrentUserIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user?.IdUsuario;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _localStorage.GetItemAsStringAsync(TokenKey);
            return !string.IsNullOrEmpty(token);
        }
    }
}
