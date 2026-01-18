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
    /// Controller para gestion de productos.
    ///
    /// TECNOLOGIA NUEVA: RESTful API
    /// - GET: Obtener recursos (lectura)
    /// - POST: Crear recursos
    /// - PUT: Actualizar recursos (completo)
    /// - PATCH: Actualizar parcialmente
    /// - DELETE: Eliminar recursos
    ///
    /// Convenciones:
    /// - api/productos -> GET (listar), POST (crear)
    /// - api/productos/{id} -> GET (uno), PUT (actualizar), DELETE (eliminar)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticacion para todos los endpoints
    public class ProductosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ProductosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todos los productos activos
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductoDTO>>>> GetAll()
        {
            try
            {
                var productos = await _unitOfWork.Productos.GetActivosAsync();
                var grupos = await _unitOfWork.Grupos.GetAllAsync();
                var gruposDict = grupos.ToDictionary(g => g.IdGrupo, g => g.Nombre);

                var productosDTO = productos.Select(p => new ProductoDTO
                {
                    IdArticulo = p.IdArticulo,
                    Nombre = p.Nombre,
                    Grupo = p.Grupo,
                    GrupoNombre = gruposDict.ContainsKey(p.Grupo) ? gruposDict[p.Grupo] : "Sin grupo",
                    Codigo = p.Codigo,
                    Precio = p.Precio,
                    Activo = p.Activo,
                    Cantidad = p.Cantidad,
                    UnidadMedida = p.UnidadMedida,
                    Descripcion = p.Descripcion,
                    TieneImagen = p.Img != null && p.Img.Length > 0
                });

                return Ok(ApiResponse<IEnumerable<ProductoDTO>>.Ok(productosDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductoDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene un producto por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductoDTO>>> GetById(int id)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);

                if (producto == null)
                {
                    return NotFound(ApiResponse<ProductoDTO>.Error("Producto no encontrado"));
                }

                var grupo = await _unitOfWork.Grupos.GetByIdAsync(producto.Grupo);

                var productoDTO = new ProductoDTO
                {
                    IdArticulo = producto.IdArticulo,
                    Nombre = producto.Nombre,
                    Grupo = producto.Grupo,
                    GrupoNombre = grupo?.Nombre ?? "Sin grupo",
                    Codigo = producto.Codigo,
                    Precio = producto.Precio,
                    Activo = producto.Activo,
                    Cantidad = producto.Cantidad,
                    UnidadMedida = producto.UnidadMedida,
                    Descripcion = producto.Descripcion,
                    TieneImagen = producto.Img != null && producto.Img.Length > 0
                };

                return Ok(ApiResponse<ProductoDTO>.Ok(productoDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductoDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Busca un producto por codigo de barras
        /// </summary>
        [HttpGet("codigo/{codigo}")]
        public async Task<ActionResult<ApiResponse<ProductoDTO>>> GetByCodigo(string codigo)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByCodigoAsync(codigo);

                if (producto == null)
                {
                    return NotFound(ApiResponse<ProductoDTO>.Error("Producto no encontrado"));
                }

                var grupo = await _unitOfWork.Grupos.GetByIdAsync(producto.Grupo);

                var productoDTO = new ProductoDTO
                {
                    IdArticulo = producto.IdArticulo,
                    Nombre = producto.Nombre,
                    Grupo = producto.Grupo,
                    GrupoNombre = grupo?.Nombre ?? "Sin grupo",
                    Codigo = producto.Codigo,
                    Precio = producto.Precio,
                    Activo = producto.Activo,
                    Cantidad = producto.Cantidad,
                    UnidadMedida = producto.UnidadMedida,
                    Descripcion = producto.Descripcion,
                    TieneImagen = producto.Img != null && producto.Img.Length > 0
                };

                return Ok(ApiResponse<ProductoDTO>.Ok(productoDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductoDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Busca productos por nombre
        /// </summary>
        [HttpGet("buscar")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductoDTO>>>> Buscar([FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    return BadRequest(ApiResponse<IEnumerable<ProductoDTO>>.Error("El termino de busqueda es requerido"));
                }

                var productos = await _unitOfWork.Productos.BuscarPorNombreAsync(termino);
                var grupos = await _unitOfWork.Grupos.GetAllAsync();
                var gruposDict = grupos.ToDictionary(g => g.IdGrupo, g => g.Nombre);

                var productosDTO = productos.Select(p => new ProductoDTO
                {
                    IdArticulo = p.IdArticulo,
                    Nombre = p.Nombre,
                    Grupo = p.Grupo,
                    GrupoNombre = gruposDict.ContainsKey(p.Grupo) ? gruposDict[p.Grupo] : "Sin grupo",
                    Codigo = p.Codigo,
                    Precio = p.Precio,
                    Activo = p.Activo,
                    Cantidad = p.Cantidad,
                    UnidadMedida = p.UnidadMedida,
                    Descripcion = p.Descripcion,
                    TieneImagen = p.Img != null && p.Img.Length > 0
                });

                return Ok(ApiResponse<IEnumerable<ProductoDTO>>.Ok(productosDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductoDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene productos por grupo/categoria
        /// </summary>
        [HttpGet("grupo/{idGrupo}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductoDTO>>>> GetByGrupo(int idGrupo)
        {
            try
            {
                var productos = await _unitOfWork.Productos.GetByGrupoAsync(idGrupo);
                var grupo = await _unitOfWork.Grupos.GetByIdAsync(idGrupo);

                var productosDTO = productos.Select(p => new ProductoDTO
                {
                    IdArticulo = p.IdArticulo,
                    Nombre = p.Nombre,
                    Grupo = p.Grupo,
                    GrupoNombre = grupo?.Nombre ?? "Sin grupo",
                    Codigo = p.Codigo,
                    Precio = p.Precio,
                    Activo = p.Activo,
                    Cantidad = p.Cantidad,
                    UnidadMedida = p.UnidadMedida,
                    Descripcion = p.Descripcion,
                    TieneImagen = p.Img != null && p.Img.Length > 0
                });

                return Ok(ApiResponse<IEnumerable<ProductoDTO>>.Ok(productosDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductoDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProductoDTO>>> Create([FromBody] ProductoCreateDTO dto)
        {
            try
            {
                // Verificar si el codigo ya existe
                var existente = await _unitOfWork.Productos.GetByCodigoAsync(dto.Codigo);
                if (existente != null)
                {
                    return BadRequest(ApiResponse<ProductoDTO>.Error("Ya existe un producto con ese codigo"));
                }

                var producto = new CE_Productos
                {
                    Nombre = dto.Nombre,
                    Grupo = dto.Grupo,
                    Codigo = dto.Codigo,
                    Precio = dto.Precio,
                    Cantidad = dto.Cantidad,
                    UnidadMedida = dto.UnidadMedida,
                    Descripcion = dto.Descripcion,
                    Activo = true
                };

                await _unitOfWork.Productos.AddAsync(producto);
                await _unitOfWork.SaveChangesAsync();

                var grupo = await _unitOfWork.Grupos.GetByIdAsync(producto.Grupo);

                var productoDTO = new ProductoDTO
                {
                    IdArticulo = producto.IdArticulo,
                    Nombre = producto.Nombre,
                    Grupo = producto.Grupo,
                    GrupoNombre = grupo?.Nombre ?? "Sin grupo",
                    Codigo = producto.Codigo,
                    Precio = producto.Precio,
                    Activo = producto.Activo,
                    Cantidad = producto.Cantidad,
                    UnidadMedida = producto.UnidadMedida,
                    Descripcion = producto.Descripcion,
                    TieneImagen = false
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = producto.IdArticulo },
                    ApiResponse<ProductoDTO>.Ok(productoDTO, "Producto creado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductoDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Actualiza un producto
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ProductoDTO>>> Update(int id, [FromBody] ProductoUpdateDTO dto)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);

                if (producto == null)
                {
                    return NotFound(ApiResponse<ProductoDTO>.Error("Producto no encontrado"));
                }

                // Actualizar solo los campos proporcionados
                if (!string.IsNullOrEmpty(dto.Nombre)) producto.Nombre = dto.Nombre;
                if (dto.Grupo.HasValue) producto.Grupo = dto.Grupo.Value;
                if (!string.IsNullOrEmpty(dto.Codigo)) producto.Codigo = dto.Codigo;
                if (dto.Precio.HasValue) producto.Precio = dto.Precio.Value;
                if (dto.Activo.HasValue) producto.Activo = dto.Activo.Value;
                if (dto.Cantidad.HasValue) producto.Cantidad = dto.Cantidad.Value;
                if (!string.IsNullOrEmpty(dto.UnidadMedida)) producto.UnidadMedida = dto.UnidadMedida;
                if (dto.Descripcion != null) producto.Descripcion = dto.Descripcion;

                await _unitOfWork.Productos.UpdateAsync(producto);
                await _unitOfWork.SaveChangesAsync();

                var grupo = await _unitOfWork.Grupos.GetByIdAsync(producto.Grupo);

                var productoDTO = new ProductoDTO
                {
                    IdArticulo = producto.IdArticulo,
                    Nombre = producto.Nombre,
                    Grupo = producto.Grupo,
                    GrupoNombre = grupo?.Nombre ?? "Sin grupo",
                    Codigo = producto.Codigo,
                    Precio = producto.Precio,
                    Activo = producto.Activo,
                    Cantidad = producto.Cantidad,
                    UnidadMedida = producto.UnidadMedida,
                    Descripcion = producto.Descripcion,
                    TieneImagen = producto.Img != null && producto.Img.Length > 0
                };

                return Ok(ApiResponse<ProductoDTO>.Ok(productoDTO, "Producto actualizado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ProductoDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Elimina (desactiva) un producto
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);

                if (producto == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Producto no encontrado"));
                }

                // Soft delete - solo desactivar
                await _unitOfWork.Productos.CambiarEstadoAsync(id, false);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.Ok(true, "Producto eliminado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene productos con stock bajo
        /// </summary>
        [HttpGet("stock-bajo")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductoDTO>>>> GetStockBajo([FromQuery] decimal minimo = 10)
        {
            try
            {
                var productos = await _unitOfWork.Productos.GetConStockBajoAsync(minimo);
                var grupos = await _unitOfWork.Grupos.GetAllAsync();
                var gruposDict = grupos.ToDictionary(g => g.IdGrupo, g => g.Nombre);

                var productosDTO = productos.Select(p => new ProductoDTO
                {
                    IdArticulo = p.IdArticulo,
                    Nombre = p.Nombre,
                    Grupo = p.Grupo,
                    GrupoNombre = gruposDict.ContainsKey(p.Grupo) ? gruposDict[p.Grupo] : "Sin grupo",
                    Codigo = p.Codigo,
                    Precio = p.Precio,
                    Activo = p.Activo,
                    Cantidad = p.Cantidad,
                    UnidadMedida = p.UnidadMedida,
                    Descripcion = p.Descripcion,
                    TieneImagen = p.Img != null && p.Img.Length > 0
                });

                return Ok(ApiResponse<IEnumerable<ProductoDTO>>.Ok(productosDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductoDTO>>.Error(ex.Message));
            }
        }
    }
}
