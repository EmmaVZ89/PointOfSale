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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GruposController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public GruposController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GrupoDTO>>>> GetAll()
        {
            try
            {
                var grupos = await _unitOfWork.Grupos.GetAllAsync();

                var gruposDTO = new List<GrupoDTO>();
                foreach (var g in grupos)
                {
                    var cantProductos = await _unitOfWork.Grupos.ContarProductosAsync(g.IdGrupo);
                    gruposDTO.Add(new GrupoDTO
                    {
                        IdGrupo = g.IdGrupo,
                        Nombre = g.Nombre,
                        CantidadProductos = cantProductos
                    });
                }

                return Ok(ApiResponse<IEnumerable<GrupoDTO>>.Ok(gruposDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<GrupoDTO>>.Error(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GrupoDTO>>> GetById(int id)
        {
            try
            {
                var grupo = await _unitOfWork.Grupos.GetByIdAsync(id);

                if (grupo == null)
                {
                    return NotFound(ApiResponse<GrupoDTO>.Error("Grupo no encontrado"));
                }

                var cantProductos = await _unitOfWork.Grupos.ContarProductosAsync(id);

                var grupoDTO = new GrupoDTO
                {
                    IdGrupo = grupo.IdGrupo,
                    Nombre = grupo.Nombre,
                    CantidadProductos = cantProductos
                };

                return Ok(ApiResponse<GrupoDTO>.Ok(grupoDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GrupoDTO>.Error(ex.Message));
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<GrupoDTO>>> Create([FromBody] GrupoCreateDTO dto)
        {
            try
            {
                var existente = await _unitOfWork.Grupos.GetByNombreAsync(dto.Nombre);
                if (existente != null)
                {
                    return BadRequest(ApiResponse<GrupoDTO>.Error("Ya existe un grupo con ese nombre"));
                }

                var grupo = new CE_Grupos
                {
                    Nombre = dto.Nombre
                };

                await _unitOfWork.Grupos.AddAsync(grupo);
                await _unitOfWork.SaveChangesAsync();

                var grupoDTO = new GrupoDTO
                {
                    IdGrupo = grupo.IdGrupo,
                    Nombre = grupo.Nombre,
                    CantidadProductos = 0
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = grupo.IdGrupo },
                    ApiResponse<GrupoDTO>.Ok(grupoDTO, "Grupo creado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GrupoDTO>.Error(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<GrupoDTO>>> Update(int id, [FromBody] GrupoUpdateDTO dto)
        {
            try
            {
                var grupo = await _unitOfWork.Grupos.GetByIdAsync(id);

                if (grupo == null)
                {
                    return NotFound(ApiResponse<GrupoDTO>.Error("Grupo no encontrado"));
                }

                grupo.Nombre = dto.Nombre;

                await _unitOfWork.Grupos.UpdateAsync(grupo);
                await _unitOfWork.SaveChangesAsync();

                var cantProductos = await _unitOfWork.Grupos.ContarProductosAsync(id);

                var grupoDTO = new GrupoDTO
                {
                    IdGrupo = grupo.IdGrupo,
                    Nombre = grupo.Nombre,
                    CantidadProductos = cantProductos
                };

                return Ok(ApiResponse<GrupoDTO>.Ok(grupoDTO, "Grupo actualizado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<GrupoDTO>.Error(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var grupo = await _unitOfWork.Grupos.GetByIdAsync(id);

                if (grupo == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Grupo no encontrado"));
                }

                // Verificar si tiene productos
                var tieneProductos = await _unitOfWork.Grupos.TieneProductosAsync(id);
                if (tieneProductos)
                {
                    return BadRequest(ApiResponse<bool>.Error("No se puede eliminar el grupo porque tiene productos asociados"));
                }

                await _unitOfWork.Grupos.DeleteByIdAsync(id);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.Ok(true, "Grupo eliminado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error(ex.Message));
            }
        }
    }
}
