using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Capa_Datos.Interfaces;
using Capa_Entidad;
using PuntoDeVenta.API.DTOs;

namespace PuntoDeVenta.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MovimientosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public MovimientosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todos los movimientos con filtros y paginacion
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<MovimientoDTO>>>> GetAll(
            [FromQuery] string buscar = null,
            [FromQuery] int? idArticulo = null,
            [FromQuery] int? idUsuario = null,
            [FromQuery] string tipoMovimiento = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var movimientos = await _unitOfWork.Movimientos.GetConDetallesAsync();

                // Aplicar filtros
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    var buscarLower = buscar.ToLower();
                    movimientos = movimientos.Where(m =>
                        m.NombreProducto.ToLower().Contains(buscarLower) ||
                        m.CodigoProducto.ToLower().Contains(buscarLower) ||
                        m.UsuarioResponsable.ToLower().Contains(buscarLower));
                }

                if (idArticulo.HasValue)
                    movimientos = movimientos.Where(m => m.IdArticulo == idArticulo.Value);

                if (idUsuario.HasValue)
                    movimientos = movimientos.Where(m => m.IdUsuario == idUsuario.Value);

                if (!string.IsNullOrWhiteSpace(tipoMovimiento))
                    movimientos = movimientos.Where(m => m.TipoMovimiento.ToUpper() == tipoMovimiento.ToUpper());

                if (fechaDesde.HasValue)
                    movimientos = movimientos.Where(m => m.FechaMovimiento >= fechaDesde.Value);

                if (fechaHasta.HasValue)
                {
                    var fechaHastaFin = fechaHasta.Value.Date.AddDays(1).AddSeconds(-1);
                    movimientos = movimientos.Where(m => m.FechaMovimiento <= fechaHastaFin);
                }

                // Ordenar por fecha descendente
                var movimientosOrdenados = movimientos.OrderByDescending(m => m.FechaMovimiento).ToList();
                var totalItems = movimientosOrdenados.Count;
                var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                var movimientosPaginados = movimientosOrdenados
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new MovimientoDTO
                    {
                        IdMovimiento = m.IdMovimiento,
                        IdArticulo = m.IdArticulo,
                        NombreProducto = m.NombreProducto,
                        CodigoProducto = m.CodigoProducto,
                        IdUsuario = m.IdUsuario,
                        UsuarioResponsable = m.UsuarioResponsable,
                        TipoMovimiento = m.TipoMovimiento,
                        Cantidad = m.Cantidad,
                        FechaMovimiento = m.FechaMovimiento,
                        Observacion = m.Observacion
                    }).ToList();

                var response = new PaginatedResponse<MovimientoDTO>
                {
                    Items = movimientosPaginados,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                };

                return Ok(ApiResponse<PaginatedResponse<MovimientoDTO>>.Ok(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<MovimientoDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene movimientos de un producto especifico
        /// </summary>
        [HttpGet("articulo/{idArticulo}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MovimientoDTO>>>> GetByArticulo(int idArticulo)
        {
            try
            {
                var movimientos = await _unitOfWork.Movimientos.GetByArticuloAsync(idArticulo);
                var producto = await _unitOfWork.Productos.GetByIdAsync(idArticulo);

                var movimientosDTO = movimientos.Select(m => new MovimientoDTO
                {
                    IdMovimiento = m.IdMovimiento,
                    IdArticulo = m.IdArticulo,
                    NombreProducto = producto?.Nombre ?? "N/A",
                    CodigoProducto = producto?.Codigo ?? "N/A",
                    IdUsuario = m.IdUsuario,
                    UsuarioResponsable = m.UsuarioResponsable,
                    TipoMovimiento = m.TipoMovimiento,
                    Cantidad = m.Cantidad,
                    FechaMovimiento = m.FechaMovimiento,
                    Observacion = m.Observacion
                });

                return Ok(ApiResponse<IEnumerable<MovimientoDTO>>.Ok(movimientosDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<MovimientoDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene movimientos por rango de fechas
        /// </summary>
        [HttpGet("fechas")]
        public async Task<ActionResult<ApiResponse<IEnumerable<MovimientoDTO>>>> GetByFechas(
            [FromQuery] DateTime desde,
            [FromQuery] DateTime hasta)
        {
            try
            {
                var movimientos = await _unitOfWork.Movimientos.GetByFechaAsync(desde, hasta);

                var movimientosDTO = movimientos.Select(m => new MovimientoDTO
                {
                    IdMovimiento = m.IdMovimiento,
                    IdArticulo = m.IdArticulo,
                    NombreProducto = m.NombreProducto,
                    CodigoProducto = m.CodigoProducto,
                    IdUsuario = m.IdUsuario,
                    UsuarioResponsable = m.UsuarioResponsable,
                    TipoMovimiento = m.TipoMovimiento,
                    Cantidad = m.Cantidad,
                    FechaMovimiento = m.FechaMovimiento,
                    Observacion = m.Observacion
                });

                return Ok(ApiResponse<IEnumerable<MovimientoDTO>>.Ok(movimientosDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<MovimientoDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Registra un nuevo movimiento de inventario
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<MovimientoDTO>>> Create([FromBody] MovimientoCreateDTO dto)
        {
            try
            {
                // Verificar que el producto existe
                var producto = await _unitOfWork.Productos.GetByIdAsync(dto.IdArticulo);
                if (producto == null)
                {
                    return BadRequest(ApiResponse<MovimientoDTO>.Error("Producto no encontrado"));
                }

                // Obtener ID del usuario del token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                var userId = int.TryParse(userIdClaim?.Value, out var id) ? id : 1;

                var movimiento = new CE_Movimiento
                {
                    IdArticulo = dto.IdArticulo,
                    IdUsuario = userId,
                    TipoMovimiento = dto.TipoMovimiento.ToUpper(),
                    Cantidad = dto.Cantidad,
                    FechaMovimiento = DateTime.UtcNow,
                    Observacion = dto.Observacion
                };

                // Actualizar stock segun tipo de movimiento
                decimal nuevoStock = producto.Cantidad;
                switch (dto.TipoMovimiento.ToUpper())
                {
                    case "ENTRADA":
                        nuevoStock += dto.Cantidad;
                        break;
                    case "SALIDA":
                        if (producto.Cantidad < dto.Cantidad)
                        {
                            return BadRequest(ApiResponse<MovimientoDTO>.Error(
                                $"Stock insuficiente. Stock actual: {producto.Cantidad}"));
                        }
                        nuevoStock -= dto.Cantidad;
                        break;
                    case "AJUSTE":
                        nuevoStock = dto.Cantidad;
                        break;
                }

                await _unitOfWork.Movimientos.AddAsync(movimiento);
                await _unitOfWork.Productos.ActualizarStockAsync(dto.IdArticulo, nuevoStock);
                await _unitOfWork.SaveChangesAsync();

                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(userId);

                var movimientoDTO = new MovimientoDTO
                {
                    IdMovimiento = movimiento.IdMovimiento,
                    IdArticulo = movimiento.IdArticulo,
                    NombreProducto = producto.Nombre,
                    CodigoProducto = producto.Codigo,
                    IdUsuario = movimiento.IdUsuario,
                    UsuarioResponsable = usuario != null ? $"{usuario.Nombre} {usuario.Apellido}" : "N/A",
                    TipoMovimiento = movimiento.TipoMovimiento,
                    Cantidad = movimiento.Cantidad,
                    FechaMovimiento = movimiento.FechaMovimiento,
                    Observacion = movimiento.Observacion
                };

                return CreatedAtAction(
                    nameof(GetByArticulo),
                    new { idArticulo = movimiento.IdArticulo },
                    ApiResponse<MovimientoDTO>.Ok(movimientoDTO, "Movimiento registrado exitosamente"));
            }
            catch (Exception ex)
            {
                // Mostrar inner exception para debug
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, ApiResponse<MovimientoDTO>.Error(errorMsg));
            }
        }

        /// <summary>
        /// Obtiene el resumen de movimientos de un producto
        /// </summary>
        [HttpGet("resumen/{idArticulo}")]
        public async Task<ActionResult<ApiResponse<object>>> GetResumen(int idArticulo)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(idArticulo);
                if (producto == null)
                {
                    return NotFound(ApiResponse<object>.Error("Producto no encontrado"));
                }

                var totalEntradas = await _unitOfWork.Movimientos.GetTotalEntradasAsync(idArticulo);
                var totalSalidas = await _unitOfWork.Movimientos.GetTotalSalidasAsync(idArticulo);
                var ultimoMovimiento = await _unitOfWork.Movimientos.GetUltimoMovimientoAsync(idArticulo);

                var resumen = new
                {
                    IdArticulo = idArticulo,
                    NombreProducto = producto.Nombre,
                    StockActual = producto.Cantidad,
                    TotalEntradas = totalEntradas,
                    TotalSalidas = totalSalidas,
                    UltimoMovimiento = ultimoMovimiento != null ? new
                    {
                        Tipo = ultimoMovimiento.TipoMovimiento,
                        Cantidad = ultimoMovimiento.Cantidad,
                        Fecha = ultimoMovimiento.FechaMovimiento
                    } : null
                };

                return Ok(ApiResponse<object>.Ok(resumen));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene datos para el dashboard de inventario (KPIs y graficos)
        /// </summary>
        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<InventarioDashboardDTO>>> GetDashboard()
        {
            try
            {
                var hoyInicio = DateTime.UtcNow.Date;
                var hoyFin = DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1);

                var movimientos = await _unitOfWork.Movimientos.GetConDetallesAsync();
                var movimientosHoy = movimientos.Where(m =>
                    m.FechaMovimiento >= hoyInicio && m.FechaMovimiento <= hoyFin).ToList();

                var dashboard = new InventarioDashboardDTO
                {
                    MovimientosHoy = movimientosHoy.Count,
                    EntradasHoy = movimientosHoy.Where(m => m.TipoMovimiento == "ENTRADA").Sum(m => m.Cantidad),
                    SalidasHoy = movimientosHoy.Where(m => m.TipoMovimiento == "SALIDA").Sum(m => m.Cantidad),
                    AjustesHoy = movimientosHoy.Where(m => m.TipoMovimiento == "AJUSTE").Sum(m => m.Cantidad),
                    TopProductosMovidos = movimientosHoy
                        .GroupBy(m => new { m.IdArticulo, m.NombreProducto, m.CodigoProducto })
                        .Select(g => new TopProductoMovidoDTO
                        {
                            IdArticulo = g.Key.IdArticulo,
                            NombreProducto = g.Key.NombreProducto,
                            CodigoProducto = g.Key.CodigoProducto,
                            TotalMovido = g.Sum(m => m.Cantidad)
                        })
                        .OrderByDescending(x => x.TotalMovido)
                        .Take(5)
                        .ToList()
                };

                return Ok(ApiResponse<InventarioDashboardDTO>.Ok(dashboard));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<InventarioDashboardDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene reporte de stock actual de todos los productos activos
        /// </summary>
        [HttpGet("stock")]
        public async Task<ActionResult<ApiResponse<StockResumenDTO>>> GetStockActual(
            [FromQuery] string buscar = null,
            [FromQuery] bool? soloStockBajo = null,
            [FromQuery] int? idGrupo = null)
        {
            try
            {
                var productos = await _unitOfWork.Productos.GetActivosAsync();
                var grupos = await _unitOfWork.Grupos.GetAllAsync();
                var gruposDict = grupos.ToDictionary(g => g.IdGrupo, g => g.Nombre);

                // Nota: StockMinimo y PrecioCompra no existen en la BD actual,
                // usamos valores estimados (StockMinimo=5, PrecioCompra=70% del precio venta)
                // ValorInventario se calcula a precio de venta para consistencia con Reportes
                var productosDTO = productos.Select(p => new StockActualDTO
                {
                    IdArticulo = p.IdArticulo,
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    Grupo = gruposDict.ContainsKey(p.Grupo) ? gruposDict[p.Grupo] : "Sin grupo",
                    StockActual = p.Cantidad,
                    StockMinimo = 5, // Valor por defecto
                    PrecioCompra = p.Precio * 0.7m, // Estimado como 70% del precio venta
                    PrecioVenta = p.Precio,
                    ValorInventario = p.Cantidad * p.Precio, // Valor a precio de venta
                    StockBajo = p.Cantidad <= 5 // Usar el mismo valor por defecto
                }).ToList();

                // Aplicar filtros
                if (!string.IsNullOrWhiteSpace(buscar))
                {
                    var buscarLower = buscar.ToLower();
                    productosDTO = productosDTO.Where(p =>
                        p.Nombre.ToLower().Contains(buscarLower) ||
                        p.Codigo.ToLower().Contains(buscarLower)).ToList();
                }

                if (soloStockBajo == true)
                {
                    productosDTO = productosDTO.Where(p => p.StockBajo).ToList();
                }

                if (idGrupo.HasValue)
                {
                    var grupoNombre = gruposDict.ContainsKey(idGrupo.Value) ? gruposDict[idGrupo.Value] : "";
                    productosDTO = productosDTO.Where(p => p.Grupo == grupoNombre).ToList();
                }

                var resumen = new StockResumenDTO
                {
                    TotalProductos = productosDTO.Count,
                    ProductosConStockBajo = productosDTO.Count(p => p.StockBajo && p.StockActual > 0),
                    ProductosSinStock = productosDTO.Count(p => p.StockActual <= 0),
                    ValorTotalInventario = productosDTO.Sum(p => p.ValorInventario),
                    Productos = productosDTO.OrderBy(p => p.Nombre).ToList()
                };

                return Ok(ApiResponse<StockResumenDTO>.Ok(resumen));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<StockResumenDTO>.Error(ex.Message));
            }
        }
    }
}
