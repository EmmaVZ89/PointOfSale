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
        /// Obtiene todos los movimientos con detalles
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<MovimientoDTO>>>> GetAll()
        {
            try
            {
                var movimientos = await _unitOfWork.Movimientos.GetConDetallesAsync();

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
                var userIdClaim = User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
                int userId = 0;
                int.TryParse(userIdClaim, out userId);

                var movimiento = new CE_Movimiento
                {
                    IdArticulo = dto.IdArticulo,
                    IdUsuario = userId,
                    TipoMovimiento = dto.TipoMovimiento.ToUpper(),
                    Cantidad = dto.Cantidad,
                    FechaMovimiento = DateTime.Now,
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
                return StatusCode(500, ApiResponse<MovimientoDTO>.Error(ex.Message));
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
    }
}
