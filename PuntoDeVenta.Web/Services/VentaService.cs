using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public interface IVentaService
    {
        Task<ApiResponse<PaginatedResponse<VentaDTO>>> GetAllAsync(VentaFiltroDTO? filtro = null);
        Task<ApiResponse<VentaDTO>> GetByIdAsync(int id);
        Task<ApiResponse<VentaDTO>> CreateAsync(VentaCreateDTO dto);
        Task<ApiResponse<VentaDTO>> CancelarAsync(int id, string? motivo = null);
        Task<ApiResponse<List<TopProductoVentaDTO>>> GetTopProductosAsync(DateTime desde, DateTime hasta, int top = 10);
        Task<byte[]?> GetTicketPdfAsync(int idVenta);
        Task<byte[]?> GetReporteCajaPdfAsync(DateTime fecha);
    }

    public class VentaService : ApiServiceBase, IVentaService
    {
        public VentaService(HttpClient httpClient) : base(httpClient) { }

        public async Task<ApiResponse<PaginatedResponse<VentaDTO>>> GetAllAsync(VentaFiltroDTO? filtro = null)
        {
            var queryParams = new List<string>();

            if (filtro != null)
            {
                if (filtro.IdCliente.HasValue)
                    queryParams.Add($"idCliente={filtro.IdCliente}");
                if (filtro.IdUsuario.HasValue)
                    queryParams.Add($"idUsuario={filtro.IdUsuario}");
                if (!string.IsNullOrEmpty(filtro.Estado))
                    queryParams.Add($"estado={Uri.EscapeDataString(filtro.Estado)}");
                if (filtro.FechaDesde.HasValue)
                    queryParams.Add($"fechaDesde={filtro.FechaDesde:yyyy-MM-dd}");
                if (filtro.FechaHasta.HasValue)
                    queryParams.Add($"fechaHasta={filtro.FechaHasta:yyyy-MM-dd}");
                queryParams.Add($"page={filtro.Page}");
                queryParams.Add($"pageSize={filtro.PageSize}");
            }

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await GetAsync<PaginatedResponse<VentaDTO>>($"api/ventas{query}");
        }

        public async Task<ApiResponse<VentaDTO>> GetByIdAsync(int id)
            => await GetAsync<VentaDTO>($"api/ventas/{id}");

        public async Task<ApiResponse<VentaDTO>> CreateAsync(VentaCreateDTO dto)
            => await PostAsync<VentaDTO>("api/ventas", dto);

        public async Task<ApiResponse<VentaDTO>> CancelarAsync(int id, string? motivo = null)
            => await PostAsync<VentaDTO>($"api/ventas/{id}/cancelar", new { Motivo = motivo });

        public async Task<ApiResponse<List<TopProductoVentaDTO>>> GetTopProductosAsync(DateTime desde, DateTime hasta, int top = 10)
            => await GetAsync<List<TopProductoVentaDTO>>($"api/ventas/top?desde={desde:yyyy-MM-dd}&hasta={hasta:yyyy-MM-dd}&top={top}");

        public async Task<byte[]?> GetTicketPdfAsync(int idVenta)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/recibo/venta/{idVenta}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<byte[]?> GetReporteCajaPdfAsync(DateTime fecha)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/recibo/caja/{fecha:yyyy-MM-dd}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}
