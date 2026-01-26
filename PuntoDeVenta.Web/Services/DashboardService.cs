using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.Services
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardResumen>> GetResumenAsync();
        Task<ApiResponse<List<ProductoStockBajo>>> GetProductosStockBajoAsync();
        Task<ApiResponse<List<ProductoTop>>> GetProductosTopAsync();
    }

    public class DashboardService : ApiServiceBase, IDashboardService
    {
        public DashboardService(HttpClient httpClient) : base(httpClient) { }

        public async Task<ApiResponse<DashboardResumen>> GetResumenAsync()
            => await GetAsync<DashboardResumen>("api/dashboard/resumen");

        public async Task<ApiResponse<List<ProductoStockBajo>>> GetProductosStockBajoAsync()
            => await GetAsync<List<ProductoStockBajo>>("api/dashboard/stock-bajo");

        public async Task<ApiResponse<List<ProductoTop>>> GetProductosTopAsync()
            => await GetAsync<List<ProductoTop>>("api/dashboard/productos-top");
    }
}
