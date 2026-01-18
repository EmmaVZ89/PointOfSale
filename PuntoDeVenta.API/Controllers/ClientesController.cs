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
    public class ClientesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClienteDTO>>>> GetAll()
        {
            try
            {
                var clientes = await _unitOfWork.Clientes.GetActivosAsync();

                var clientesDTO = clientes.Select(c => new ClienteDTO
                {
                    IdCliente = c.IdCliente,
                    RazonSocial = c.RazonSocial,
                    Documento = c.Documento,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Activo = c.Activo,
                    FechaAlta = c.FechaAlta
                });

                return Ok(ApiResponse<IEnumerable<ClienteDTO>>.Ok(clientesDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ClienteDTO>>.Error(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ClienteDTO>>> GetById(int id)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);

                if (cliente == null)
                {
                    return NotFound(ApiResponse<ClienteDTO>.Error("Cliente no encontrado"));
                }

                var clienteDTO = new ClienteDTO
                {
                    IdCliente = cliente.IdCliente,
                    RazonSocial = cliente.RazonSocial,
                    Documento = cliente.Documento,
                    Telefono = cliente.Telefono,
                    Email = cliente.Email,
                    Activo = cliente.Activo,
                    FechaAlta = cliente.FechaAlta
                };

                return Ok(ApiResponse<ClienteDTO>.Ok(clienteDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ClienteDTO>.Error(ex.Message));
            }
        }

        [HttpGet("documento/{documento}")]
        public async Task<ActionResult<ApiResponse<ClienteDTO>>> GetByDocumento(string documento)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByDocumentoAsync(documento);

                if (cliente == null)
                {
                    return NotFound(ApiResponse<ClienteDTO>.Error("Cliente no encontrado"));
                }

                var clienteDTO = new ClienteDTO
                {
                    IdCliente = cliente.IdCliente,
                    RazonSocial = cliente.RazonSocial,
                    Documento = cliente.Documento,
                    Telefono = cliente.Telefono,
                    Email = cliente.Email,
                    Activo = cliente.Activo,
                    FechaAlta = cliente.FechaAlta
                };

                return Ok(ApiResponse<ClienteDTO>.Ok(clienteDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ClienteDTO>.Error(ex.Message));
            }
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClienteDTO>>>> Buscar([FromQuery] string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    return BadRequest(ApiResponse<IEnumerable<ClienteDTO>>.Error("El termino de busqueda es requerido"));
                }

                var clientes = await _unitOfWork.Clientes.BuscarPorRazonSocialAsync(termino);

                var clientesDTO = clientes.Select(c => new ClienteDTO
                {
                    IdCliente = c.IdCliente,
                    RazonSocial = c.RazonSocial,
                    Documento = c.Documento,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Activo = c.Activo,
                    FechaAlta = c.FechaAlta
                });

                return Ok(ApiResponse<IEnumerable<ClienteDTO>>.Ok(clientesDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ClienteDTO>>.Error(ex.Message));
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<ClienteDTO>>> Create([FromBody] ClienteCreateDTO dto)
        {
            try
            {
                var existente = await _unitOfWork.Clientes.GetByDocumentoAsync(dto.Documento);
                if (existente != null)
                {
                    return BadRequest(ApiResponse<ClienteDTO>.Error("Ya existe un cliente con ese documento"));
                }

                var cliente = new CE_Clientes
                {
                    RazonSocial = dto.RazonSocial,
                    Documento = dto.Documento,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    Activo = true,
                    FechaAlta = DateTime.Now
                };

                await _unitOfWork.Clientes.AddAsync(cliente);
                await _unitOfWork.SaveChangesAsync();

                var clienteDTO = new ClienteDTO
                {
                    IdCliente = cliente.IdCliente,
                    RazonSocial = cliente.RazonSocial,
                    Documento = cliente.Documento,
                    Telefono = cliente.Telefono,
                    Email = cliente.Email,
                    Activo = cliente.Activo,
                    FechaAlta = cliente.FechaAlta
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = cliente.IdCliente },
                    ApiResponse<ClienteDTO>.Ok(clienteDTO, "Cliente creado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ClienteDTO>.Error(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ClienteDTO>>> Update(int id, [FromBody] ClienteUpdateDTO dto)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);

                if (cliente == null)
                {
                    return NotFound(ApiResponse<ClienteDTO>.Error("Cliente no encontrado"));
                }

                if (!string.IsNullOrEmpty(dto.RazonSocial)) cliente.RazonSocial = dto.RazonSocial;
                if (!string.IsNullOrEmpty(dto.Documento)) cliente.Documento = dto.Documento;
                if (!string.IsNullOrEmpty(dto.Telefono)) cliente.Telefono = dto.Telefono;
                if (!string.IsNullOrEmpty(dto.Email)) cliente.Email = dto.Email;
                if (dto.Activo.HasValue) cliente.Activo = dto.Activo.Value;

                await _unitOfWork.Clientes.UpdateAsync(cliente);
                await _unitOfWork.SaveChangesAsync();

                var clienteDTO = new ClienteDTO
                {
                    IdCliente = cliente.IdCliente,
                    RazonSocial = cliente.RazonSocial,
                    Documento = cliente.Documento,
                    Telefono = cliente.Telefono,
                    Email = cliente.Email,
                    Activo = cliente.Activo,
                    FechaAlta = cliente.FechaAlta
                };

                return Ok(ApiResponse<ClienteDTO>.Ok(clienteDTO, "Cliente actualizado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ClienteDTO>.Error(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<bool>>> Delete(int id)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);

                if (cliente == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Cliente no encontrado"));
                }

                await _unitOfWork.Clientes.CambiarEstadoAsync(id, false);
                await _unitOfWork.SaveChangesAsync();

                return Ok(ApiResponse<bool>.Ok(true, "Cliente eliminado exitosamente"));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error(ex.Message));
            }
        }
    }
}
