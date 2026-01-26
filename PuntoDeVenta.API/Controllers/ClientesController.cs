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

        /// <summary>
        /// Obtiene todos los clientes. Usa ?soloActivos=true para filtrar solo activos.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClienteDTO>>>> GetAll([FromQuery] bool soloActivos = false)
        {
            try
            {
                IEnumerable<CE_Clientes> clientes;

                if (soloActivos)
                {
                    clientes = await _unitOfWork.Clientes.GetActivosAsync();
                }
                else
                {
                    clientes = await _unitOfWork.Clientes.GetAllAsync();
                }

                var clientesDTO = clientes.Select(c => new ClienteDTO
                {
                    IdCliente = c.IdCliente,
                    RazonSocial = c.RazonSocial,
                    Documento = c.Documento,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Domicilio = c.Domicilio,
                    Activo = c.Activo,
                    FechaAlta = c.FechaAlta
                }).OrderBy(c => c.RazonSocial);

                return Ok(ApiResponse<IEnumerable<ClienteDTO>>.Ok(clientesDTO));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ClienteDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Activa o desactiva un cliente
        /// </summary>
        [HttpPatch("{id}/estado")]
        public async Task<ActionResult<ApiResponse<bool>>> CambiarEstado(int id, [FromBody] ClienteCambiarEstadoDTO dto)
        {
            try
            {
                // Proteger cliente "Consumidor Final" (ID=1)
                if (id == 1)
                {
                    return BadRequest(ApiResponse<bool>.Error("No se puede modificar el estado del cliente 'Consumidor Final'"));
                }

                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);

                if (cliente == null)
                {
                    return NotFound(ApiResponse<bool>.Error("Cliente no encontrado"));
                }

                await _unitOfWork.Clientes.CambiarEstadoAsync(id, dto.Activo);
                await _unitOfWork.SaveChangesAsync();

                var mensaje = dto.Activo ? "Cliente activado" : "Cliente desactivado";
                return Ok(ApiResponse<bool>.Ok(true, mensaje));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<bool>.Error(ex.Message));
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
                    Domicilio = cliente.Domicilio,
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
                    Domicilio = cliente.Domicilio,
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
                    return BadRequest(ApiResponse<IEnumerable<ClienteDTO>>.Error("El término de búsqueda es requerido"));
                }

                var clientes = await _unitOfWork.Clientes.BuscarPorRazonSocialAsync(termino);

                var clientesDTO = clientes.Select(c => new ClienteDTO
                {
                    IdCliente = c.IdCliente,
                    RazonSocial = c.RazonSocial,
                    Documento = c.Documento,
                    Telefono = c.Telefono,
                    Email = c.Email,
                    Domicilio = c.Domicilio,
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
                    Domicilio = dto.Domicilio,
                    Activo = true,
                    FechaAlta = DateTime.UtcNow
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
                    Domicilio = cliente.Domicilio,
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
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                return StatusCode(500, ApiResponse<ClienteDTO>.Error(errorMsg));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<ClienteDTO>>> Update(int id, [FromBody] ClienteUpdateDTO dto)
        {
            try
            {
                // Proteger cliente "Consumidor Final" (ID=1)
                if (id == 1)
                {
                    return BadRequest(ApiResponse<ClienteDTO>.Error("No se puede modificar el cliente 'Consumidor Final'"));
                }

                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);

                if (cliente == null)
                {
                    return NotFound(ApiResponse<ClienteDTO>.Error("Cliente no encontrado"));
                }

                if (!string.IsNullOrEmpty(dto.RazonSocial)) cliente.RazonSocial = dto.RazonSocial;
                if (!string.IsNullOrEmpty(dto.Documento)) cliente.Documento = dto.Documento;
                if (!string.IsNullOrEmpty(dto.Telefono)) cliente.Telefono = dto.Telefono;
                if (!string.IsNullOrEmpty(dto.Email)) cliente.Email = dto.Email;
                if (dto.Domicilio != null) cliente.Domicilio = dto.Domicilio;
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
                    Domicilio = cliente.Domicilio,
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
                // Proteger cliente "Consumidor Final" (ID=1)
                if (id == 1)
                {
                    return BadRequest(ApiResponse<bool>.Error("No se puede eliminar el cliente 'Consumidor Final'"));
                }

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

        /// <summary>
        /// Obtiene estadisticas de compras del cliente
        /// </summary>
        [HttpGet("{id}/estadisticas")]
        public async Task<ActionResult<ApiResponse<ClienteEstadisticasDTO>>> GetEstadisticas(int id)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(ApiResponse<ClienteEstadisticasDTO>.Error("Cliente no encontrado"));
                }

                // Obtener ventas del cliente (no canceladas)
                var ventas = await _unitOfWork.Ventas.GetByClienteAsync(id);
                var ventasActivas = ventas.Where(v => !v.Cancelada).ToList();

                var estadisticas = new ClienteEstadisticasDTO
                {
                    IdCliente = id,
                    TotalCompras = ventasActivas.Count,
                    MontoAcumulado = ventasActivas.Sum(v => v.Monto_Total),
                    UltimaCompra = ventasActivas.OrderByDescending(v => v.Fecha_Venta).FirstOrDefault()?.Fecha_Venta,
                    EsFrecuente = ventasActivas.Count >= 5 // Cliente frecuente: 5+ compras
                };

                return Ok(ApiResponse<ClienteEstadisticasDTO>.Ok(estadisticas));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ClienteEstadisticasDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene historial de compras del cliente (ultimas N)
        /// </summary>
        [HttpGet("{id}/compras")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ClienteCompraDTO>>>> GetCompras(int id, [FromQuery] int limite = 10)
        {
            try
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(id);
                if (cliente == null)
                {
                    return NotFound(ApiResponse<IEnumerable<ClienteCompraDTO>>.Error("Cliente no encontrado"));
                }

                var ventas = await _unitOfWork.Ventas.GetByClienteAsync(id);

                var compras = ventas
                    .OrderByDescending(v => v.Fecha_Venta)
                    .Take(limite)
                    .Select(v => new ClienteCompraDTO
                    {
                        IdVenta = v.Id_Venta,
                        NoFactura = v.No_Factura,
                        Fecha = v.Fecha_Venta ?? DateTime.MinValue,
                        Monto = v.Monto_Total,
                        CantidadArticulos = v.Detalles?.Count ?? 0,
                        Cancelada = v.Cancelada
                    });

                return Ok(ApiResponse<IEnumerable<ClienteCompraDTO>>.Ok(compras));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<IEnumerable<ClienteCompraDTO>>.Error(ex.Message));
            }
        }
    }
}
