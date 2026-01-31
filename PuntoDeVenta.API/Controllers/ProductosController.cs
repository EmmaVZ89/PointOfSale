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
        /// Obtiene todos los productos (activos e inactivos).
        /// Usa query param ?soloActivos=true para filtrar solo activos.
        /// Incluye CostoUnitario solo para usuarios Admin.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductoDTO>>>> GetAll([FromQuery] bool soloActivos = false)
        {
            try
            {
                // Verificar si el usuario es Admin para mostrar costos
                var isAdmin = User.IsInRole("Admin");

                IEnumerable<CE_Productos> productos;

                if (soloActivos)
                {
                    productos = await _unitOfWork.Productos.GetActivosAsync();
                }
                else
                {
                    // Obtener todos (activos e inactivos) para administracion
                    productos = await _unitOfWork.Productos.GetAllAsync();
                }

                var grupos = await _unitOfWork.Grupos.GetAllAsync();
                var gruposDict = grupos.ToDictionary(g => g.IdGrupo, g => g.Nombre);

                // Obtener cuenta de presentaciones tipo pack (CantidadUnidades > 1) activas por producto
                // Solo cuenta packs, no la presentacion "Unidad", para determinar si mostrar modal en POS
                var presentaciones = await _unitOfWork.Presentaciones.GetAllAsync();
                var presentacionesCount = presentaciones
                    .Where(p => p.Activo && p.CantidadUnidades > 1)
                    .GroupBy(p => p.IdArticulo)
                    .ToDictionary(g => g.Key, g => g.Count());

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
                    TieneImagen = p.Img != null && p.Img.Length > 0,
                    CantidadPresentaciones = presentacionesCount.ContainsKey(p.IdArticulo) ? presentacionesCount[p.IdArticulo] : 0,
                    // Solo incluir costo para Admin
                    CostoUnitario = isAdmin ? p.CostoUnitario : null
                }).OrderBy(p => p.Nombre);

                return Ok(ApiResponse<IEnumerable<ProductoDTO>>.Ok(productosDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ProductoDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Activa o desactiva un producto
        /// </summary>
        [HttpPatch("{id}/estado")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> CambiarEstado(int id, [FromBody] CambiarEstadoDTO dto)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);

                if (producto == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Producto no encontrado"));
                }

                await _unitOfWork.Productos.CambiarEstadoAsync(id, dto.Activo);
                await _unitOfWork.SaveChangesAsync();

                var mensaje = dto.Activo ? "Producto activado" : "Producto desactivado";
                return Ok(ApiResponse<bool>.Ok(true, mensaje));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene un producto por ID.
        /// Incluye CostoUnitario solo para usuarios Admin.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductoDTO>>> GetById(int id)
        {
            try
            {
                var isAdmin = User.IsInRole("Admin");
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
                    TieneImagen = producto.Img != null && producto.Img.Length > 0,
                    CostoUnitario = isAdmin ? producto.CostoUnitario : null
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
                    return BadRequest(ApiResponse<IEnumerable<ProductoDTO>>.Error("El término de búsqueda es requerido"));
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
                // Verificar si el código ya existe
                var existente = await _unitOfWork.Productos.GetByCodigoAsync(dto.Codigo);
                if (existente != null)
                {
                    return BadRequest(ApiResponse<ProductoDTO>.Error("Ya existe un producto con ese código"));
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

                // Crear automaticamente la presentacion "Unidad" para el nuevo producto
                var presentacionUnidad = new CE_ProductoPresentacion
                {
                    IdArticulo = producto.IdArticulo,
                    Nombre = "Unidad",
                    CantidadUnidades = 1,
                    Precio = producto.Precio,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };
                await _unitOfWork.Presentaciones.AddAsync(presentacionUnidad);
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
        /// Actualiza un producto.
        /// Si se proporciona CostoUnitario y es diferente al actual, se registra en histórico.
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

                // Manejar costo unitario - solo si es diferente al actual
                if (dto.CostoUnitario.HasValue && dto.CostoUnitario != producto.CostoUnitario)
                {
                    // Obtener usuario actual
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    var userId = int.TryParse(userIdClaim?.Value, out var uid) ? uid : (int?)null;

                    // Registrar en histórico
                    var costoHistorico = new CE_ProductoCostoHistorico
                    {
                        IdArticulo = id,
                        CostoUnitario = dto.CostoUnitario.Value,
                        FechaRegistro = DateTime.UtcNow,
                        IdUsuarioRegistro = userId
                    };

                    await _unitOfWork.ProductoCostos.AddAsync(costoHistorico);

                    // Actualizar cache en producto
                    producto.CostoUnitario = dto.CostoUnitario.Value;
                }

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
                    TieneImagen = producto.Img != null && producto.Img.Length > 0,
                    CostoUnitario = producto.CostoUnitario
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

        #region Endpoints de Costo (Solo Admin)

        /// <summary>
        /// Obtiene el último costo de compra de un producto (Admin)
        /// </summary>
        [HttpGet("{id}/costo")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<decimal?>>> GetCosto(int id)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);

                if (producto == null)
                {
                    return NotFound(ApiResponse<decimal?>.Error("Producto no encontrado"));
                }

                return Ok(ApiResponse<decimal?>.Ok(producto.CostoUnitario));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<decimal?>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Actualiza el costo de compra de un producto (Admin).
        /// Registra en histórico si el costo es diferente al actual.
        /// </summary>
        [HttpPut("{id}/costo")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> UpdateCosto(int id, [FromBody] ActualizarCostoDTO dto)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);

                if (producto == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Producto no encontrado"));
                }

                // Solo registrar si el costo es diferente al actual
                if (producto.CostoUnitario != dto.CostoUnitario)
                {
                    // Obtener usuario actual
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                    var userId = int.TryParse(userIdClaim?.Value, out var uid) ? uid : (int?)null;

                    // Registrar en histórico
                    var costoHistorico = new CE_ProductoCostoHistorico
                    {
                        IdArticulo = id,
                        CostoUnitario = dto.CostoUnitario,
                        FechaRegistro = DateTime.UtcNow,
                        IdUsuarioRegistro = userId
                    };

                    await _unitOfWork.ProductoCostos.AddAsync(costoHistorico);

                    // Actualizar cache en producto
                    producto.CostoUnitario = dto.CostoUnitario;
                    await _unitOfWork.Productos.UpdateAsync(producto);

                    await _unitOfWork.SaveChangesAsync();

                    return Ok(ApiResponse<bool>.Ok(true, "Costo actualizado exitosamente"));
                }

                return Ok(ApiResponse<bool>.Ok(true, "El costo no ha cambiado"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene el histórico de costos de un producto (Admin)
        /// </summary>
        [HttpGet("{id}/costos-historico")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<CostoHistoricoDTO>>>> GetCostosHistorico(int id)
        {
            try
            {
                var producto = await _unitOfWork.Productos.GetByIdAsync(id);

                if (producto == null)
                {
                    return NotFound(ApiResponse<IEnumerable<CostoHistoricoDTO>>.Error("Producto no encontrado"));
                }

                var historico = await _unitOfWork.ProductoCostos.GetHistoricoAsync(id);

                // Obtener nombres de usuarios que registraron costos
                var usuarioIds = historico
                    .Where(h => h.IdUsuarioRegistro.HasValue)
                    .Select(h => h.IdUsuarioRegistro.Value)
                    .Distinct()
                    .ToList();

                var usuarios = new Dictionary<int, string>();
                foreach (var uid in usuarioIds)
                {
                    var user = await _unitOfWork.Usuarios.GetByIdAsync(uid);
                    if (user != null)
                    {
                        usuarios[uid] = user.Nombre;
                    }
                }

                var historicoDTO = historico.Select(h => new CostoHistoricoDTO
                {
                    IdCostoHistorico = h.IdCostoHistorico,
                    IdArticulo = h.IdArticulo,
                    CostoUnitario = h.CostoUnitario,
                    FechaRegistro = h.FechaRegistro,
                    IdUsuarioRegistro = h.IdUsuarioRegistro,
                    NombreUsuario = h.IdUsuarioRegistro.HasValue && usuarios.ContainsKey(h.IdUsuarioRegistro.Value)
                        ? usuarios[h.IdUsuarioRegistro.Value]
                        : null
                });

                return Ok(ApiResponse<IEnumerable<CostoHistoricoDTO>>.Ok(historicoDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<CostoHistoricoDTO>>.Error(ex.Message));
            }
        }

        #endregion
    }
}
