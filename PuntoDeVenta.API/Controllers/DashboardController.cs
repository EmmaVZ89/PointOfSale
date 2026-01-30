using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using Capa_Datos;
using Capa_Datos.Interfaces;
using PuntoDeVenta.API.DTOs;

namespace PuntoDeVenta.API.Controllers
{
    /// <summary>
    /// Controller para el dashboard con metricas y KPIs.
    /// Replica la funcionalidad del legacy CN_Dashboard/CD_Dashboard.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly string _connectionString;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _connectionString = ConfigurationHelper.GetConnectionString();
        }

        /// <summary>
        /// Obtiene el resumen general para el dashboard.
        /// Replica exactamente la funcionalidad del legacy.
        /// </summary>
        [HttpGet("resumen")]
        public async Task<ActionResult<ApiResponse<DashboardResumenDTO>>> GetResumen()
        {
            try
            {
                var resumen = new DashboardResumenDTO();

                // 1. Obtener KPIs de ventas del dia (desde tabla Ventas)
                var ventasDia = await ObtenerResumenDiaAsync();
                resumen.VentasHoy = ventasDia.TotalVendido;
                resumen.TransaccionesHoy = ventasDia.CantidadVentas;

                // 2. Obtener resumen de inventario
                var inventario = await ObtenerResumenInventarioAsync();
                resumen.ProductosActivos = inventario.TotalProductos;
                resumen.ProductosBajoStock = inventario.StockBajo;

                // 3. Obtener ventas semanales para el grafico
                resumen.VentasUltimos7Dias = await ObtenerVentasSemanalesAsync();

                return Ok(ApiResponse<DashboardResumenDTO>.Ok(resumen));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<DashboardResumenDTO>.Error($"Error al cargar dashboard: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el resumen de ventas del dia actual.
        /// Usa zona horaria Argentina (UTC-3).
        /// </summary>
        private async Task<(decimal TotalVendido, int CantidadVentas)> ObtenerResumenDiaAsync()
        {
            const string sql = @"
                SELECT
                    COALESCE(SUM(""Monto_Total""), 0) AS ""TotalVendido"",
                    COUNT(*) AS ""CantidadVentas""
                FROM ""Ventas""
                WHERE (""Fecha_Venta"" AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE =
                      (CURRENT_TIMESTAMP AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE
                AND (""Cancelada"" = FALSE OR ""Cancelada"" IS NULL)";

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return (
                        reader.GetDecimal(0),
                        reader.GetInt32(1)
                    );
                }
            }
            catch
            {
                // Si la tabla Ventas no existe o hay error, retornar 0
            }

            return (0, 0);
        }

        /// <summary>
        /// Obtiene la lista de productos con stock bajo.
        /// </summary>
        [HttpGet("stock-bajo")]
        public async Task<ActionResult<ApiResponse<List<ProductoStockBajoDTO>>>> GetProductosStockBajo()
        {
            try
            {
                var productos = new List<ProductoStockBajoDTO>();

                const string sql = @"
                    SELECT ""IdArticulo"", ""Codigo"", ""Nombre"", ""Cantidad""
                    FROM ""Articulos""
                    WHERE ""Activo"" = TRUE AND ""Cantidad"" <= 10
                    ORDER BY ""Cantidad"" ASC, ""Nombre"" ASC
                    LIMIT 10";

                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    productos.Add(new ProductoStockBajoDTO
                    {
                        IdArticulo = reader.GetInt32(0),
                        Codigo = reader.IsDBNull(1) ? "" : reader.GetString(1),
                        Nombre = reader.GetString(2),
                        StockActual = reader.GetInt32(3)
                    });
                }

                return Ok(ApiResponse<List<ProductoStockBajoDTO>>.Ok(productos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductoStockBajoDTO>>.Error($"Error al obtener productos con stock bajo: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene los productos mas vendidos (por cantidad, no por monto).
        /// </summary>
        [HttpGet("productos-top")]
        public async Task<ActionResult<ApiResponse<List<ProductoTopDTO>>>> GetProductosTop()
        {
            try
            {
                var productos = new List<ProductoTopDTO>();

                const string sql = @"
                    SELECT
                        a.""IdArticulo"",
                        a.""Nombre"",
                        COALESCE(SUM(dv.""Cantidad""), 0) AS ""CantidadVendida""
                    FROM ""Articulos"" a
                    LEFT JOIN ""Ventas_Detalle"" dv ON a.""IdArticulo"" = dv.""Id_Articulo""
                    LEFT JOIN ""Ventas"" v ON dv.""Id_Venta"" = v.""Id_Venta""
                    WHERE a.""Activo"" = TRUE
                    AND (v.""Cancelada"" = FALSE OR v.""Cancelada"" IS NULL OR v.""Id_Venta"" IS NULL)
                    GROUP BY a.""IdArticulo"", a.""Nombre""
                    HAVING COALESCE(SUM(dv.""Cantidad""), 0) > 0
                    ORDER BY ""CantidadVendida"" DESC
                    LIMIT 10";

                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    productos.Add(new ProductoTopDTO
                    {
                        IdArticulo = reader.GetInt32(0),
                        Nombre = reader.GetString(1),
                        CantidadVendida = reader.GetInt32(2),
                        TotalVentas = 0 // No mostramos montos para vendedores
                    });
                }

                return Ok(ApiResponse<List<ProductoTopDTO>>.Ok(productos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ProductoTopDTO>>.Error($"Error al obtener productos top: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene el resumen de inventario (productos activos y con stock bajo).
        /// </summary>
        private async Task<(int TotalProductos, int StockBajo)> ObtenerResumenInventarioAsync()
        {
            const string sql = @"
                SELECT
                    (SELECT COUNT(*) FROM ""Articulos"" WHERE ""Activo"" = TRUE) AS ""TotalProductos"",
                    (SELECT COUNT(*) FROM ""Articulos"" WHERE ""Activo"" = TRUE AND ""Cantidad"" <= 10) AS ""StockBajo""";

            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return (
                        reader.GetInt32(0),
                        reader.GetInt32(1)
                    );
                }
            }
            catch
            {
                // Si hay error, usar el fallback con EF Core
                var productos = await _unitOfWork.Productos.GetAllAsync();
                var activos = 0;
                var stockBajo = 0;
                foreach (var p in productos)
                {
                    if (p.Activo)
                    {
                        activos++;
                        if (p.Cantidad <= 10) stockBajo++;
                    }
                }
                return (activos, stockBajo);
            }

            return (0, 0);
        }

        /// <summary>
        /// Obtiene las ventas de los ultimos 7 dias.
        /// Intenta usar el stored procedure sp_d_ventassemanales() si existe.
        /// </summary>
        private async Task<List<VentaDiariaDTO>> ObtenerVentasSemanalesAsync()
        {
            var ventas = new List<VentaDiariaDTO>();

            // Primero intentar con el stored procedure
            try
            {
                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand("SELECT * FROM sp_d_ventassemanales()", conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ventas.Add(new VentaDiariaDTO
                    {
                        Fecha = reader.GetDateTime(reader.GetOrdinal("Fecha")),
                        Total = reader.GetDecimal(reader.GetOrdinal("Total")),
                        CantidadVentas = reader.IsDBNull(reader.GetOrdinal("CantidadVentas"))
                            ? 0
                            : reader.GetInt32(reader.GetOrdinal("CantidadVentas"))
                    });
                }

                if (ventas.Count > 0) return ventas;
            }
            catch
            {
                // El stored procedure no existe, usar query directo
            }

            // Fallback: Query directo a la tabla Ventas
            try
            {
                const string sql = @"
                    SELECT
                        (""Fecha_Venta"" AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE as ""Fecha"",
                        COALESCE(SUM(""Monto_Total""), 0) as ""Total"",
                        COUNT(*) as ""CantidadVentas""
                    FROM ""Ventas""
                    WHERE (""Fecha_Venta"" AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE >=
                          (CURRENT_TIMESTAMP AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE - INTERVAL '6 days'
                    AND (""Cancelada"" = FALSE OR ""Cancelada"" IS NULL)
                    GROUP BY (""Fecha_Venta"" AT TIME ZONE 'America/Argentina/Buenos_Aires')::DATE
                    ORDER BY ""Fecha""";

                await using var conn = new NpgsqlConnection(_connectionString);
                await conn.OpenAsync();
                await using var cmd = new NpgsqlCommand(sql, conn);
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    ventas.Add(new VentaDiariaDTO
                    {
                        Fecha = reader.GetDateTime(0),
                        Total = reader.GetDecimal(1),
                        CantidadVentas = reader.GetInt32(2)
                    });
                }

                if (ventas.Count > 0) return ventas;
            }
            catch
            {
                // La tabla Ventas no existe o hay error
            }

            // Si no hay datos reales, generar estructura vacia para los ultimos 7 dias
            var hoyArgentina = DateTimeHelper.GetArgentinaToday();
            for (int i = 6; i >= 0; i--)
            {
                ventas.Add(new VentaDiariaDTO
                {
                    Fecha = hoyArgentina.AddDays(-i),
                    Total = 0,
                    CantidadVentas = 0
                });
            }

            return ventas;
        }
    }
}
