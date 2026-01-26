using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Capa_Datos.Interfaces;
using Capa_Entidad;
using PuntoDeVenta.API.DTOs;

namespace PuntoDeVenta.API.Controllers
{
    /// <summary>
    /// Controller para gestionar presentaciones de productos (unidad, pack, caja, etc.)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PresentacionesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public PresentacionesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todas las presentaciones de un producto
        /// </summary>
        [HttpGet("producto/{idArticulo}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PresentacionDTO>>>> GetByProducto(int idArticulo)
        {
            try
            {
                var presentaciones = await _unitOfWork.Presentaciones.GetByProductoAsync(idArticulo);
                var producto = await _unitOfWork.Productos.GetByIdAsync(idArticulo);

                var dtos = presentaciones.Select(p => new PresentacionDTO
                {
                    IdPresentacion = p.IdPresentacion,
                    IdArticulo = p.IdArticulo,
                    Nombre = p.Nombre,
                    CantidadUnidades = p.CantidadUnidades,
                    Precio = p.Precio,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    ProductoNombre = producto?.Nombre,
                    ProductoCodigo = producto?.Codigo
                });

                return Ok(ApiResponse<IEnumerable<PresentacionDTO>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<PresentacionDTO>>.Error($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene todas las presentaciones activas de un producto (para POS)
        /// </summary>
        [HttpGet("producto/{idArticulo}/activas")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PresentacionDTO>>>> GetActivasByProducto(int idArticulo)
        {
            try
            {
                var presentaciones = await _unitOfWork.Presentaciones.GetActivasByProductoAsync(idArticulo);
                var producto = await _unitOfWork.Productos.GetByIdAsync(idArticulo);

                var dtos = presentaciones.Select(p => new PresentacionDTO
                {
                    IdPresentacion = p.IdPresentacion,
                    IdArticulo = p.IdArticulo,
                    Nombre = p.Nombre,
                    CantidadUnidades = p.CantidadUnidades,
                    Precio = p.Precio,
                    Activo = p.Activo,
                    FechaCreacion = p.FechaCreacion,
                    ProductoNombre = producto?.Nombre,
                    ProductoCodigo = producto?.Codigo
                });

                return Ok(ApiResponse<IEnumerable<PresentacionDTO>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<PresentacionDTO>>.Error($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene todas las presentaciones activas (para lista de precios)
        /// </summary>
        [HttpGet("activas")]
        public async Task<ActionResult<ApiResponse<IEnumerable<PresentacionDTO>>>> GetAllActivas()
        {
            try
            {
                var presentaciones = await _unitOfWork.Presentaciones.GetAllActivasAsync();
                var productos = await _unitOfWork.Productos.GetAllAsync();
                var productosDict = productos.ToDictionary(p => p.IdArticulo);

                var dtos = presentaciones
                    .Where(p => productosDict.ContainsKey(p.IdArticulo) && productosDict[p.IdArticulo].Activo)
                    .Select(p => new PresentacionDTO
                    {
                        IdPresentacion = p.IdPresentacion,
                        IdArticulo = p.IdArticulo,
                        Nombre = p.Nombre,
                        CantidadUnidades = p.CantidadUnidades,
                        Precio = p.Precio,
                        Activo = p.Activo,
                        FechaCreacion = p.FechaCreacion,
                        ProductoNombre = productosDict[p.IdArticulo].Nombre,
                        ProductoCodigo = productosDict[p.IdArticulo].Codigo
                    })
                    .OrderBy(p => p.ProductoNombre)
                    .ThenBy(p => p.CantidadUnidades);

                return Ok(ApiResponse<IEnumerable<PresentacionDTO>>.Ok(dtos));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<PresentacionDTO>>.Error($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Obtiene una presentacion por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<PresentacionDTO>>> GetById(int id)
        {
            try
            {
                var presentacion = await _unitOfWork.Presentaciones.GetByIdAsync(id);

                if (presentacion == null)
                {
                    return NotFound(ApiResponse<PresentacionDTO>.Error("Presentación no encontrada"));
                }

                var producto = await _unitOfWork.Productos.GetByIdAsync(presentacion.IdArticulo);

                var dto = new PresentacionDTO
                {
                    IdPresentacion = presentacion.IdPresentacion,
                    IdArticulo = presentacion.IdArticulo,
                    Nombre = presentacion.Nombre,
                    CantidadUnidades = presentacion.CantidadUnidades,
                    Precio = presentacion.Precio,
                    Activo = presentacion.Activo,
                    FechaCreacion = presentacion.FechaCreacion,
                    ProductoNombre = producto?.Nombre,
                    ProductoCodigo = producto?.Codigo
                };

                return Ok(ApiResponse<PresentacionDTO>.Ok(dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PresentacionDTO>.Error($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Crea una nueva presentacion
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PresentacionDTO>>> Create([FromBody] PresentacionCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<PresentacionDTO>.Error("Datos inválidos", errors.ToList()));
                }

                // Verificar que el producto existe
                var producto = await _unitOfWork.Productos.GetByIdAsync(dto.IdArticulo);
                if (producto == null)
                {
                    return NotFound(ApiResponse<PresentacionDTO>.Error("Producto no encontrado"));
                }

                // Verificar que no existe una presentacion con la misma cantidad
                if (await _unitOfWork.Presentaciones.ExistePresentacionAsync(dto.IdArticulo, dto.CantidadUnidades))
                {
                    return BadRequest(ApiResponse<PresentacionDTO>.Error($"Ya existe una presentación con {dto.CantidadUnidades} unidades para este producto"));
                }

                // Si no existe presentacion unitaria y la que se crea no es unitaria, crear "Unidad"
                var presentacionesExistentes = await _unitOfWork.Presentaciones.GetByProductoAsync(dto.IdArticulo);
                var existeUnidad = presentacionesExistentes.Any(p => p.CantidadUnidades == 1);
                if (!existeUnidad && dto.CantidadUnidades != 1)
                {
                    var presentacionUnidad = new CE_ProductoPresentacion
                    {
                        IdArticulo = dto.IdArticulo,
                        Nombre = "Unidad",
                        CantidadUnidades = 1,
                        Precio = producto.Precio,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow
                    };
                    await _unitOfWork.Presentaciones.AddAsync(presentacionUnidad);
                }

                var presentacion = new CE_ProductoPresentacion
                {
                    IdArticulo = dto.IdArticulo,
                    Nombre = dto.Nombre,
                    CantidadUnidades = dto.CantidadUnidades,
                    Precio = dto.Precio,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await _unitOfWork.Presentaciones.AddAsync(presentacion);
                await _unitOfWork.SaveChangesAsync();

                var result = new PresentacionDTO
                {
                    IdPresentacion = presentacion.IdPresentacion,
                    IdArticulo = presentacion.IdArticulo,
                    Nombre = presentacion.Nombre,
                    CantidadUnidades = presentacion.CantidadUnidades,
                    Precio = presentacion.Precio,
                    Activo = presentacion.Activo,
                    FechaCreacion = presentacion.FechaCreacion,
                    ProductoNombre = producto.Nombre,
                    ProductoCodigo = producto.Codigo
                };

                return CreatedAtAction(nameof(GetById), new { id = presentacion.IdPresentacion },
                    ApiResponse<PresentacionDTO>.Ok(result, "Presentación creada exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PresentacionDTO>.Error($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Alta rapida de pack predefinido (x6, x12, x18, x24)
        /// </summary>
        [HttpPost("rapida")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PresentacionDTO>>> CreateRapida([FromBody] PresentacionRapidaDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ApiResponse<PresentacionDTO>.Error("Datos inválidos", errors.ToList()));
                }

                var producto = await _unitOfWork.Productos.GetByIdAsync(dto.IdArticulo);
                if (producto == null)
                {
                    return NotFound(ApiResponse<PresentacionDTO>.Error("Producto no encontrado"));
                }

                if (await _unitOfWork.Presentaciones.ExistePresentacionAsync(dto.IdArticulo, dto.TipoPack))
                {
                    return BadRequest(ApiResponse<PresentacionDTO>.Error($"Ya existe un Pack x{dto.TipoPack} para este producto"));
                }

                // Si no existe presentacion unitaria, crear "Unidad"
                var presentacionesExistentes = await _unitOfWork.Presentaciones.GetByProductoAsync(dto.IdArticulo);
                var existeUnidad = presentacionesExistentes.Any(p => p.CantidadUnidades == 1);
                if (!existeUnidad)
                {
                    var presentacionUnidad = new CE_ProductoPresentacion
                    {
                        IdArticulo = dto.IdArticulo,
                        Nombre = "Unidad",
                        CantidadUnidades = 1,
                        Precio = producto.Precio,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow
                    };
                    await _unitOfWork.Presentaciones.AddAsync(presentacionUnidad);
                }

                var presentacion = new CE_ProductoPresentacion
                {
                    IdArticulo = dto.IdArticulo,
                    Nombre = $"Pack x{dto.TipoPack}",
                    CantidadUnidades = dto.TipoPack,
                    Precio = dto.Precio,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                await _unitOfWork.Presentaciones.AddAsync(presentacion);
                await _unitOfWork.SaveChangesAsync();

                var result = new PresentacionDTO
                {
                    IdPresentacion = presentacion.IdPresentacion,
                    IdArticulo = presentacion.IdArticulo,
                    Nombre = presentacion.Nombre,
                    CantidadUnidades = presentacion.CantidadUnidades,
                    Precio = presentacion.Precio,
                    Activo = presentacion.Activo,
                    FechaCreacion = presentacion.FechaCreacion,
                    ProductoNombre = producto.Nombre,
                    ProductoCodigo = producto.Codigo
                };

                return CreatedAtAction(nameof(GetById), new { id = presentacion.IdPresentacion },
                    ApiResponse<PresentacionDTO>.Ok(result, $"Pack x{dto.TipoPack} creado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PresentacionDTO>.Error($"Error interno: {ex.Message}"));
            }
        }

        /// <summary>
        /// Actualiza una presentacion
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<PresentacionDTO>>> Update(int id, [FromBody] PresentacionUpdateDTO dto)
        {
            try
            {
                var presentacion = await _unitOfWork.Presentaciones.GetByIdAsync(id);

                if (presentacion == null)
                {
                    return NotFound(ApiResponse<PresentacionDTO>.Error("Presentación no encontrada"));
                }

                // No permitir modificar la presentación unitaria
                if (presentacion.CantidadUnidades == 1 && dto.CantidadUnidades.HasValue && dto.CantidadUnidades != 1)
                {
                    return BadRequest(ApiResponse<PresentacionDTO>.Error("No se puede modificar la cantidad de la presentación unitaria"));
                }

                // Actualizar solo los campos que vienen en el DTO
                if (!string.IsNullOrEmpty(dto.Nombre)) presentacion.Nombre = dto.Nombre;
                if (dto.CantidadUnidades.HasValue) presentacion.CantidadUnidades = dto.CantidadUnidades.Value;
                if (dto.Precio.HasValue) presentacion.Precio = dto.Precio.Value;
                if (dto.Activo.HasValue) presentacion.Activo = dto.Activo.Value;

                // No usar UpdateAsync - la entidad ya esta siendo tracked por FindAsync
                // Solo guardar los cambios
                await _unitOfWork.SaveChangesAsync();

                // Si se actualiza el precio de la presentacion unitaria, actualizar tambien el precio del producto
                // para mantener compatibilidad con WPF
                if (presentacion.CantidadUnidades == 1 && dto.Precio.HasValue)
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(presentacion.IdArticulo);
                    if (producto != null)
                    {
                        producto.Precio = dto.Precio.Value;
                        // La entidad ya esta tracked por FindAsync, solo guardar
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                var productoInfo = await _unitOfWork.Productos.GetByIdAsync(presentacion.IdArticulo);

                var result = new PresentacionDTO
                {
                    IdPresentacion = presentacion.IdPresentacion,
                    IdArticulo = presentacion.IdArticulo,
                    Nombre = presentacion.Nombre,
                    CantidadUnidades = presentacion.CantidadUnidades,
                    Precio = presentacion.Precio,
                    Activo = presentacion.Activo,
                    FechaCreacion = presentacion.FechaCreacion,
                    ProductoNombre = productoInfo?.Nombre,
                    ProductoCodigo = productoInfo?.Codigo
                };

                return Ok(ApiResponse<PresentacionDTO>.Ok(result, "Presentación actualizada exitosamente"));
            }
            catch (Exception ex)
            {
                var innerMessage = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, ApiResponse<PresentacionDTO>.Error($"Error interno: {innerMessage}"));
            }
        }

        /// <summary>
        /// Elimina (desactiva) una presentacion
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var presentacion = await _unitOfWork.Presentaciones.GetByIdAsync(id);

                if (presentacion == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Presentación no encontrada"));
                }

                // No permitir eliminar la presentación unitaria
                if (presentacion.CantidadUnidades == 1)
                {
                    return BadRequest(ApiResponse<bool>.Error("No se puede eliminar la presentación unitaria"));
                }

                // Desactivar en lugar de eliminar
                presentacion.Activo = false;
                // No usar UpdateAsync - la entidad ya esta siendo tracked por FindAsync
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.Ok(true, "Presentación desactivada exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error($"Error interno: {ex.Message}"));
            }
        }
    }
}
