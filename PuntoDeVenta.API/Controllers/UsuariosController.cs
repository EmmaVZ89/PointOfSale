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
    public class UsuariosController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UsuariosController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todos los usuarios (activos e inactivos) - solo Admin
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioDTO>>>> GetAll()
        {
            try
            {
                var usuarios = await _unitOfWork.Usuarios.GetAllAsync();

                var usuariosDTO = usuarios.Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Dni = u.Dni,
                    Correo = u.Correo,
                    Telefono = u.Telefono,
                    FechaNac = u.Fecha_Nac,
                    Privilegio = u.Privilegio,
                    Activo = u.Activo,
                    Usuario = u.Usuario
                });

                return Ok(ApiResponse<IEnumerable<UsuarioDTO>>.Ok(usuariosDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<UsuarioDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<UsuarioDTO>>> GetById(int id)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);

                if (usuario == null)
                {
                    return NotFound(ApiResponse<UsuarioDTO>.Error("Usuario no encontrado"));
                }

                var usuarioDTO = new UsuarioDTO
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Dni = usuario.Dni,
                    Correo = usuario.Correo,
                    Telefono = usuario.Telefono,
                    FechaNac = usuario.Fecha_Nac,
                    Privilegio = usuario.Privilegio,
                    Activo = usuario.Activo,
                    Usuario = usuario.Usuario
                };

                return Ok(ApiResponse<UsuarioDTO>.Ok(usuarioDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UsuarioDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Busca usuarios por nombre o apellido
        /// </summary>
        [HttpGet("buscar")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioDTO>>>> Buscar([FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    return BadRequest(ApiResponse<IEnumerable<UsuarioDTO>>.Error("El término de búsqueda es requerido"));
                }

                var usuarios = await _unitOfWork.Usuarios.BuscarPorNombreAsync(termino);

                var usuariosDTO = usuarios.Select(u => new UsuarioDTO
                {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Dni = u.Dni,
                    Correo = u.Correo,
                    Telefono = u.Telefono,
                    FechaNac = u.Fecha_Nac,
                    Privilegio = u.Privilegio,
                    Activo = u.Activo,
                    Usuario = u.Usuario
                });

                return Ok(ApiResponse<IEnumerable<UsuarioDTO>>.Ok(usuariosDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<UsuarioDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Crea un nuevo usuario (solo Admin)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<UsuarioDTO>>> Create([FromBody] UsuarioCreateDTO dto)
        {
            try
            {
                // Verificar si el usuario ya existe
                var existente = await _unitOfWork.Usuarios.GetByUsuarioAsync(dto.Usuario);
                if (existente != null)
                {
                    return BadRequest(ApiResponse<UsuarioDTO>.Error("Ya existe un usuario con ese nombre de usuario"));
                }

                // Verificar DNI
                var existenteDni = await _unitOfWork.Usuarios.GetByDniAsync(dto.Dni);
                if (existenteDni != null)
                {
                    return BadRequest(ApiResponse<UsuarioDTO>.Error("Ya existe un usuario con ese DNI"));
                }

                // Convertir fecha a UTC para PostgreSQL
                var fechaNacUtc = DateTime.SpecifyKind(dto.FechaNac, DateTimeKind.Utc);

                // Si no se proporciona patron, usar uno por defecto (usado para encriptacion legacy)
                var patron = string.IsNullOrEmpty(dto.Patron) ? "PuntoDeVenta" : dto.Patron;

                var usuario = new CE_Usuarios
                {
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    Dni = dto.Dni,
                    Correo = dto.Correo ?? string.Empty,
                    Telefono = dto.Telefono ?? string.Empty,
                    Fecha_Nac = fechaNacUtc,
                    Privilegio = dto.Privilegio,
                    Usuario = dto.Usuario,
                    Patron = patron,
                    Activo = true
                };

                // Usar metodo especial para insertar con contrasena encriptada
                await _unitOfWork.Usuarios.CreateWithPasswordAsync(usuario, dto.Contrasena);

                var usuarioDTO = new UsuarioDTO
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Dni = usuario.Dni,
                    Correo = usuario.Correo,
                    Telefono = usuario.Telefono,
                    FechaNac = usuario.Fecha_Nac,
                    Privilegio = usuario.Privilegio,
                    Activo = usuario.Activo,
                    Usuario = usuario.Usuario
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = usuario.IdUsuario },
                    ApiResponse<UsuarioDTO>.Ok(usuarioDTO, "Usuario creado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UsuarioDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Actualiza un usuario
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<UsuarioDTO>>> Update(int id, [FromBody] UsuarioUpdateDTO dto)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);

                if (usuario == null)
                {
                    return NotFound(ApiResponse<UsuarioDTO>.Error("Usuario no encontrado"));
                }

                if (!string.IsNullOrEmpty(dto.Nombre)) usuario.Nombre = dto.Nombre;
                if (!string.IsNullOrEmpty(dto.Apellido)) usuario.Apellido = dto.Apellido;
                if (dto.Dni.HasValue) usuario.Dni = dto.Dni.Value;
                if (!string.IsNullOrEmpty(dto.Correo)) usuario.Correo = dto.Correo;
                if (!string.IsNullOrEmpty(dto.Telefono)) usuario.Telefono = dto.Telefono;
                if (dto.FechaNac.HasValue) usuario.Fecha_Nac = dto.FechaNac.Value;
                if (dto.Privilegio.HasValue) usuario.Privilegio = dto.Privilegio.Value;
                if (dto.Activo.HasValue) usuario.Activo = dto.Activo.Value;

                await _unitOfWork.Usuarios.UpdateAsync(usuario);
                await _unitOfWork.SaveChangesAsync();

                var usuarioDTO = new UsuarioDTO
                {
                    IdUsuario = usuario.IdUsuario,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    Dni = usuario.Dni,
                    Correo = usuario.Correo,
                    Telefono = usuario.Telefono,
                    FechaNac = usuario.Fecha_Nac,
                    Privilegio = usuario.Privilegio,
                    Activo = usuario.Activo,
                    Usuario = usuario.Usuario
                };

                return Ok(ApiResponse<UsuarioDTO>.Ok(usuarioDTO, "Usuario actualizado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<UsuarioDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Elimina (desactiva) un usuario
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);

                if (usuario == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Usuario no encontrado"));
                }

                await _unitOfWork.Usuarios.CambiarEstadoAsync(id, false);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.Ok(true, "Usuario eliminado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Reactiva un usuario inactivo
        /// </summary>
        [HttpPost("{id}/reactivar")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> Reactivar(int id)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);

                if (usuario == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Usuario no encontrado"));
                }

                if (usuario.Activo)
                {
                    return BadRequest(ApiResponse<bool>.Error("El usuario ya está activo"));
                }

                await _unitOfWork.Usuarios.CambiarEstadoAsync(id, true);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.Ok(true, "Usuario reactivado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Cambia la contrasena de un usuario (solo Admin)
        /// </summary>
        [HttpPost("{id}/cambiar-contrasena")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<bool>>> CambiarContrasena(int id, [FromBody] CambiarContrasenaDTO dto)
        {
            try
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(id);

                if (usuario == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Usuario no encontrado"));
                }

                // Usar el patron del usuario para encriptar la nueva contrasena
                var patron = usuario.Patron ?? "PuntoDeVenta";

                // Actualizar contraseña usando pgp_sym_encrypt
                await _unitOfWork.Usuarios.ActualizarContrasenaAsync(id, dto.NuevaContrasena, patron);

                return Ok(ApiResponse<bool>.Ok(true, "Contraseña actualizada exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error(ex.Message));
            }
        }
    }
}
