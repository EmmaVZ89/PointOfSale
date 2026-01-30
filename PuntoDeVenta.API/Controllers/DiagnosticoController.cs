using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Capa_Datos.Context;

namespace PuntoDeVenta.API.Controllers
{
    /// <summary>
    /// Controller de diagnóstico TEMPORAL - ELIMINAR DESPUÉS DE RESOLVER EL PROBLEMA
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class DiagnosticoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DiagnosticoController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Diagnóstico de la tabla Ventas_Detalle
        /// </summary>
        [HttpGet("ventas-detalle/{idVenta}")]
        public async Task<IActionResult> DiagnosticoVentasDetalle(int idVenta)
        {
            try
            {
                // 1. Contar TODOS los registros en Ventas_Detalle
                var totalRegistros = await _context.VentaDetalles.CountAsync();

                // 2. Buscar detalles para el idVenta específico
                var detallesParaVenta = await _context.VentaDetalles
                    .AsNoTracking()
                    .Where(d => d.Id_Venta == idVenta)
                    .ToListAsync();

                // 3. Obtener muestra de los primeros 10 registros (cualquier venta)
                var muestra = await _context.VentaDetalles
                    .AsNoTracking()
                    .Take(10)
                    .Select(d => new
                    {
                        d.Id_Detalle,
                        d.Id_Venta,
                        d.Id_Articulo,
                        d.Cantidad,
                        d.Precio_Venta,
                        d.Monto_Total
                    })
                    .ToListAsync();

                // 4. Verificar si la venta existe
                var ventaExiste = await _context.Ventas
                    .AsNoTracking()
                    .AnyAsync(v => v.Id_Venta == idVenta);

                // 5. Obtener IDs de ventas que SÍ tienen detalles
                var ventasConDetalles = await _context.VentaDetalles
                    .AsNoTracking()
                    .Select(d => d.Id_Venta)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();

                return Ok(new
                {
                    mensaje = "Diagnóstico de Ventas_Detalle",
                    timestamp = DateTime.UtcNow,

                    tabla_ventas_detalle = new
                    {
                        total_registros = totalRegistros,
                        muestra_primeros_10 = muestra
                    },

                    busqueda_especifica = new
                    {
                        id_venta_buscado = idVenta,
                        venta_existe_en_tabla_ventas = ventaExiste,
                        detalles_encontrados = detallesParaVenta.Count,
                        detalles = detallesParaVenta.Select(d => new
                        {
                            d.Id_Detalle,
                            d.Id_Venta,
                            d.Id_Articulo,
                            d.Cantidad,
                            d.Precio_Venta,
                            d.Monto_Total
                        })
                    },

                    info_adicional = new
                    {
                        ids_ventas_con_detalles_ejemplo = ventasConDetalles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = true,
                    mensaje = ex.Message,
                    tipo_error = ex.GetType().Name,
                    inner_exception = ex.InnerException?.Message,
                    stack_trace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Test básico de conexión a BD
        /// </summary>
        [HttpGet("test-db")]
        public async Task<IActionResult> TestDb()
        {
            try
            {
                var ventasCount = await _context.Ventas.CountAsync();
                var productosCount = await _context.Productos.CountAsync();
                var detallesCount = await _context.VentaDetalles.CountAsync();

                return Ok(new
                {
                    conexion = "OK",
                    conteos = new
                    {
                        ventas = ventasCount,
                        productos = productosCount,
                        ventas_detalle = detallesCount
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    conexion = "ERROR",
                    mensaje = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
    }
}
