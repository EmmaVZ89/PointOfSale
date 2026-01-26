using System.Net.Http.Json;
using System.Text.Json;
using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public abstract class ApiServiceBase
    {
        protected readonly HttpClient _httpClient;
        protected static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        protected ApiServiceBase(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<ApiResponse<T>> GetAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.GetAsync(endpoint);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>(ex);
            }
        }

        protected async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(endpoint, data);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>(ex);
            }
        }

        protected async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync(endpoint, data);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>(ex);
            }
        }

        protected async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(endpoint);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>(ex);
            }
        }

        protected async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object? data = null)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Patch, endpoint);
                if (data != null)
                {
                    request.Content = JsonContent.Create(data);
                }
                var response = await _httpClient.SendAsync(request);
                return await HandleResponse<T>(response);
            }
            catch (Exception ex)
            {
                return CreateErrorResponse<T>(ex);
            }
        }

        private async Task<ApiResponse<T>> HandleResponse<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                try
                {
                    var result = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
                    return result ?? new ApiResponse<T> { Success = true };
                }
                catch
                {
                    // Si no puede deserializar como ApiResponse, intentar deserializar directamente
                    try
                    {
                        var data = JsonSerializer.Deserialize<T>(content, JsonOptions);
                        return new ApiResponse<T> { Success = true, Data = data };
                    }
                    catch
                    {
                        return new ApiResponse<T> { Success = true };
                    }
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = "Sesion expirada. Por favor, inicie sesion nuevamente."
                };
            }
            else
            {
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<ApiResponse<T>>(content, JsonOptions);
                    return errorResponse ?? new ApiResponse<T>
                    {
                        Success = false,
                        Message = $"Error: {response.StatusCode}"
                    };
                }
                catch
                {
                    return new ApiResponse<T>
                    {
                        Success = false,
                        Message = $"Error: {response.StatusCode} - {content}"
                    };
                }
            }
        }

        private ApiResponse<T> CreateErrorResponse<T>(Exception ex)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = ex.Message.Contains("Failed to fetch") || ex.Message.Contains("NetworkError")
                    ? "Error de conexion. Verifique que el servidor este disponible."
                    : $"Error: {ex.Message}"
            };
        }
    }
}
