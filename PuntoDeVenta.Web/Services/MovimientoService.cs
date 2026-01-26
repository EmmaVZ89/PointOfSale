using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public interface IMovimientoService
    {
        Task<ApiResponse<PaginatedResponse<MovimientoDTO>>> GetAllAsync(MovimientoFiltroDTO? filtro = null);
        Task<ApiResponse<List<MovimientoDTO>>> GetByArticuloAsync(int idArticulo);
        Task<ApiResponse<MovimientoResumenDTO>> GetResumenAsync(int idArticulo);
        Task<ApiResponse<MovimientoDTO>> CreateAsync(MovimientoCreateDTO dto);
        Task<ApiResponse<InventarioDashboardDTO>> GetDashboardAsync();
        Task<ApiResponse<StockResumenDTO>> GetStockAsync(string? buscar = null, bool? soloStockBajo = null, int? idGrupo = null);
    }

    public class MovimientoService : ApiServiceBase, IMovimientoService
    {
        public MovimientoService(HttpClient httpClient) : base(httpClient) { }

        public async Task<ApiResponse<PaginatedResponse<MovimientoDTO>>> GetAllAsync(MovimientoFiltroDTO? filtro = null)
        {
            var queryParams = new List<string>();

            if (filtro != null)
            {
                if (!string.IsNullOrEmpty(filtro.Buscar))
                    queryParams.Add($"buscar={Uri.EscapeDataString(filtro.Buscar)}");
                if (filtro.IdArticulo.HasValue)
                    queryParams.Add($"idArticulo={filtro.IdArticulo}");
                if (filtro.IdUsuario.HasValue)
                    queryParams.Add($"idUsuario={filtro.IdUsuario}");
                if (!string.IsNullOrEmpty(filtro.TipoMovimiento))
                    queryParams.Add($"tipoMovimiento={Uri.EscapeDataString(filtro.TipoMovimiento)}");
                if (filtro.FechaDesde.HasValue)
                    queryParams.Add($"fechaDesde={filtro.FechaDesde:yyyy-MM-dd}");
                if (filtro.FechaHasta.HasValue)
                    queryParams.Add($"fechaHasta={filtro.FechaHasta:yyyy-MM-dd}");
                queryParams.Add($"page={filtro.Page}");
                queryParams.Add($"pageSize={filtro.PageSize}");
            }

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await GetAsync<PaginatedResponse<MovimientoDTO>>($"api/movimientos{query}");
        }

        public async Task<ApiResponse<List<MovimientoDTO>>> GetByArticuloAsync(int idArticulo)
            => await GetAsync<List<MovimientoDTO>>($"api/movimientos/articulo/{idArticulo}");

        public async Task<ApiResponse<MovimientoResumenDTO>> GetResumenAsync(int idArticulo)
            => await GetAsync<MovimientoResumenDTO>($"api/movimientos/resumen/{idArticulo}");

        public async Task<ApiResponse<MovimientoDTO>> CreateAsync(MovimientoCreateDTO dto)
            => await PostAsync<MovimientoDTO>("api/movimientos", dto);

        public async Task<ApiResponse<InventarioDashboardDTO>> GetDashboardAsync()
            => await GetAsync<InventarioDashboardDTO>("api/movimientos/dashboard");

        public async Task<ApiResponse<StockResumenDTO>> GetStockAsync(string? buscar = null, bool? soloStockBajo = null, int? idGrupo = null)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrEmpty(buscar))
                queryParams.Add($"buscar={Uri.EscapeDataString(buscar)}");
            if (soloStockBajo.HasValue)
                queryParams.Add($"soloStockBajo={soloStockBajo.Value.ToString().ToLower()}");
            if (idGrupo.HasValue)
                queryParams.Add($"idGrupo={idGrupo}");

            var query = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
            return await GetAsync<StockResumenDTO>($"api/movimientos/stock{query}");
        }
    }
}
