using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Capa_Datos;
using Capa_Datos.Interfaces;
using Capa_Entidad;
using PuntoDeVenta.API.DTOs;

namespace PuntoDeVenta.API.Controllers
{
    /// <summary>
    /// Controller para gestion de ventas.
    /// Persiste datos en las tablas Ventas y Ventas_Detalle.
    /// Replica la funcionalidad del legacy (CD_Carrito.Venta/Venta_Detalle).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public VentasController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Obtiene todas las ventas con filtros opcionales
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<VentaDTO>>>> GetAll([FromQuery] VentaFiltroDTO filtro)
        {
            try
            {
                // Determinar si mostrar canceladas segun estado
                bool? cancelada = null;
                if (!string.IsNullOrEmpty(filtro.Estado))
                {
                    cancelada = filtro.Estado.ToUpper() == "CANCELADA";
                }

                // Convertir fechas de Argentina a UTC para PostgreSQL
                DateTime? fechaDesdeUtc = filtro.FechaDesde.HasValue
                    ? DateTimeHelper.GetArgentinaDayStartUtc(filtro.FechaDesde.Value)
                    : null;
                DateTime? fechaHastaUtc = filtro.FechaHasta.HasValue
                    ? DateTimeHelper.GetArgentinaDayEndUtc(filtro.FechaHasta.Value)
                    : null;

                var (ventas, totalCount) = await _unitOfWork.Ventas.GetPaginadoAsync(
                    filtro.IdCliente,
                    filtro.IdUsuario,
                    fechaDesdeUtc,
                    fechaHastaUtc,
                    cancelada,
                    filtro.Page,
                    filtro.PageSize);

                // Cargar nombres de clientes y usuarios
                var ventaDtos = new List<VentaDTO>();
                foreach (var venta in ventas)
                {
                    var dto = await MapVentaToDTO(venta);
                    ventaDtos.Add(dto);
                }

                var response = new PaginatedResponse<VentaDTO>
                {
                    Items = ventaDtos,
                    Page = filtro.Page,
                    PageSize = filtro.PageSize,
                    TotalItems = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filtro.PageSize)
                };

                return Ok(ApiResponse<PaginatedResponse<VentaDTO>>.Ok(response));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<PaginatedResponse<VentaDTO>>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene una venta por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<VentaDTO>>> GetById(int id)
        {
            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdAsync(id);

                if (venta == null)
                {
                    return NotFound(ApiResponse<VentaDTO>.Error("Venta no encontrada"));
                }

                var dto = await MapVentaToDTO(venta, incluirDetalles: true);
                return Ok(ApiResponse<VentaDTO>.Ok(dto));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VentaDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Registra una nueva venta.
        /// Persiste en Ventas y Ventas_Detalle, actualiza stock y crea movimientos.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<VentaDTO>>> Create([FromBody] VentaCreateDTO dto)
        {
            try
            {
                if (dto.Detalles == null || !dto.Detalles.Any())
                {
                    return BadRequest(ApiResponse<VentaDTO>.Error("La venta debe tener al menos un producto"));
                }

                // Obtener el usuario actual del token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userNameClaim = User.FindFirst(ClaimTypes.Name);
                var userId = int.TryParse(userIdClaim?.Value, out var id) ? id : 1;
                var userName = userNameClaim?.Value ?? "Usuario";

                // Validar cliente (default: Consumidor Final = 1)
                var idCliente = dto.IdCliente ?? 1;
                string? clienteNombre = null;
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(idCliente);
                if (cliente != null)
                {
                    clienteNombre = cliente.RazonSocial;
                }

                // Iniciar transaccion para garantizar atomicidad
                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Calcular total
                    decimal subtotal = 0;
                    foreach (var detalle in dto.Detalles)
                    {
                        subtotal += detalle.Cantidad * detalle.PrecioUnitario;
                    }
                    var total = subtotal - dto.Descuento;

                    // Generar numero de factura (formato legacy: F-YYMMDDHHmm)
                    var noFactura = _unitOfWork.Ventas.GenerarNumeroFactura();

                    // Crear venta en BD
                    var venta = new CE_Ventas
                    {
                        No_Factura = noFactura,
                        Fecha_Venta = DateTime.UtcNow,
                        Monto_Total = total,
                        Id_Usuario = userId,
                        Id_Cliente = idCliente,
                        Cancelada = false,
                        FormaPago = dto.FormaPago ?? "E",
                        MontoRecibido = dto.MontoRecibido
                    };

                    await _unitOfWork.Ventas.AddAsync(venta);
                    await _unitOfWork.SaveChangesAsync();

                    // Crear detalles y actualizar stock
                    var detallesDto = new List<VentaDetalleDTO>();

                    foreach (var detalle in dto.Detalles)
                    {
                        var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.IdArticulo);

                        if (producto == null)
                        {
                            throw new Exception($"Producto {detalle.IdArticulo} no encontrado");
                        }

                        // Calcular unidades reales a descontar del stock
                        // Por ejemplo: 2 packs x6 = 12 unidades
                        var unidadesPorPresentacion = detalle.CantidadUnidadesPorPresentacion > 0
                            ? detalle.CantidadUnidadesPorPresentacion
                            : 1;
                        var unidadesADescontar = detalle.Cantidad * unidadesPorPresentacion;

                        if (producto.Cantidad < unidadesADescontar)
                        {
                            throw new Exception($"Stock insuficiente para {producto.Nombre}. Disponible: {producto.Cantidad}, Requerido: {unidadesADescontar}");
                        }

                        var montoTotal = detalle.Cantidad * detalle.PrecioUnitario;

                        // Crear detalle de venta (guardamos la cantidad de presentaciones vendidas)
                        var ventaDetalle = new CE_VentaDetalle
                        {
                            Id_Venta = venta.Id_Venta,
                            Id_Articulo = detalle.IdArticulo,
                            Cantidad = detalle.Cantidad,
                            Precio_Venta = detalle.PrecioUnitario,
                            Monto_Total = montoTotal,
                            // Guardar info de presentacion para cancelaciones futuras
                            IdPresentacion = detalle.IdPresentacion,
                            CantidadUnidadesPorPresentacion = unidadesPorPresentacion
                        };

                        await _unitOfWork.VentaDetalles.AddAsync(ventaDetalle);

                        // Actualizar stock del producto (descontamos las unidades reales)
                        // La entidad ya esta tracked por FindAsync, los cambios se guardan con CommitTransaction
                        producto.Cantidad -= unidadesADescontar;

                        // Crear movimiento de inventario (SALIDA) - registramos las unidades reales
                        var movimiento = new CE_Movimiento
                        {
                            IdArticulo = detalle.IdArticulo,
                            IdUsuario = userId,
                            TipoMovimiento = "SALIDA",
                            Cantidad = unidadesADescontar,
                            FechaMovimiento = DateTime.UtcNow,
                            Observacion = $"Venta Factura: {noFactura}"
                        };

                        await _unitOfWork.Movimientos.AddAsync(movimiento);

                        // Obtener nombre de presentacion si existe
                        string? presentacionNombre = null;
                        if (detalle.IdPresentacion.HasValue)
                        {
                            var presentacion = await _unitOfWork.Presentaciones.GetByIdAsync(detalle.IdPresentacion.Value);
                            presentacionNombre = presentacion?.Nombre;
                        }

                        // Agregar a lista de DTOs
                        detallesDto.Add(new VentaDetalleDTO
                        {
                            IdDetalle = ventaDetalle.Id_Detalle,
                            IdArticulo = detalle.IdArticulo,
                            NombreProducto = producto.Nombre,
                            CodigoProducto = producto.Codigo,
                            Cantidad = detalle.Cantidad,
                            PrecioUnitario = detalle.PrecioUnitario,
                            IdPresentacion = detalle.IdPresentacion,
                            PresentacionNombre = presentacionNombre,
                            CantidadUnidadesPorPresentacion = unidadesPorPresentacion
                        });
                    }

                    await _unitOfWork.CommitTransactionAsync();

                    // Crear DTO de respuesta
                    var ventaDto = new VentaDTO
                    {
                        IdVenta = venta.Id_Venta,
                        NoFactura = noFactura,
                        IdCliente = idCliente,
                        ClienteNombre = clienteNombre,
                        IdUsuario = userId,
                        UsuarioNombre = userName,
                        Fecha = venta.Fecha_Venta ?? DateTime.UtcNow,
                        Subtotal = subtotal,
                        Descuento = dto.Descuento,
                        Total = total,
                        Estado = "COMPLETADA",
                        Observaciones = dto.Observaciones,
                        FormaPago = venta.FormaPago,
                        MontoRecibido = venta.MontoRecibido,
                        Detalles = detallesDto
                    };

                    return CreatedAtAction(
                        nameof(GetById),
                        new { id = venta.Id_Venta },
                        ApiResponse<VentaDTO>.Ok(ventaDto, "Venta registrada exitosamente"));
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VentaDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Cancela una venta.
        /// Restaura el stock de los productos y registra movimientos de entrada.
        /// </summary>
        [HttpPost("{id}/cancelar")]
        public async Task<ActionResult<ApiResponse<VentaDTO>>> Cancelar(int id, [FromBody] VentaCancelarDTO? dto = null)
        {
            try
            {
                var venta = await _unitOfWork.Ventas.GetByIdAsync(id);

                if (venta == null)
                {
                    return NotFound(ApiResponse<VentaDTO>.Error("Venta no encontrada"));
                }

                if (venta.Cancelada)
                {
                    return BadRequest(ApiResponse<VentaDTO>.Error("La venta ya está cancelada"));
                }

                // Obtener usuario actual
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                var userId = int.TryParse(userIdClaim?.Value, out var uid) ? uid : 1;

                await _unitOfWork.BeginTransactionAsync();

                try
                {
                    // Obtener detalles de la venta
                    var detalles = await _unitOfWork.VentaDetalles.GetByVentaAsync(id);

                    // Devolver el stock de cada producto
                    foreach (var detalle in detalles)
                    {
                        if (detalle.Id_Articulo.HasValue)
                        {
                            var producto = await _unitOfWork.Productos.GetByIdAsync(detalle.Id_Articulo.Value);
                            if (producto != null)
                            {
                                // Calcular unidades reales a devolver
                                // Para ventas nuevas: usa CantidadUnidadesPorPresentacion
                                // Para ventas viejas (WPF): CantidadUnidadesPorPresentacion = 1
                                var unidadesPorPresentacion = detalle.CantidadUnidadesPorPresentacion > 0
                                    ? detalle.CantidadUnidadesPorPresentacion
                                    : 1;
                                var unidadesADevolver = detalle.Cantidad * unidadesPorPresentacion;

                                producto.Cantidad += unidadesADevolver;

                                // Crear movimiento de inventario (ENTRADA por cancelacion)
                                var movimiento = new CE_Movimiento
                                {
                                    IdArticulo = detalle.Id_Articulo.Value,
                                    IdUsuario = userId,
                                    TipoMovimiento = "ENTRADA",
                                    Cantidad = unidadesADevolver,
                                    FechaMovimiento = DateTime.UtcNow,
                                    Observacion = $"Cancelación Factura: {venta.No_Factura}"
                                };

                                await _unitOfWork.Movimientos.AddAsync(movimiento);
                            }
                        }
                    }

                    // Marcar venta como cancelada
                    await _unitOfWork.Ventas.CancelarVentaAsync(
                        id,
                        userId,
                        dto?.Motivo ?? "Cancelada por usuario");

                    await _unitOfWork.CommitTransactionAsync();

                    // Recargar venta actualizada
                    venta = await _unitOfWork.Ventas.GetByIdAsync(id);
                    var ventaDto = await MapVentaToDTO(venta!, incluirDetalles: true);

                    return Ok(ApiResponse<VentaDTO>.Ok(ventaDto, "Venta cancelada exitosamente"));
                }
                catch (Exception)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<VentaDTO>.Error(ex.Message));
            }
        }

        /// <summary>
        /// Obtiene los productos mas vendidos en un rango de fechas
        /// </summary>
        [HttpGet("top")]
        public async Task<ActionResult<ApiResponse<List<TopProductoVentaDTO>>>> GetTopProductos(
            [FromQuery] DateTime? desde = null,
            [FromQuery] DateTime? hasta = null,
            [FromQuery] int top = 10)
        {
            try
            {
                // Usar fecha de Argentina si no se especifica
                var fechaDesdeArg = desde ?? DateTimeHelper.GetArgentinaToday().AddDays(-30);
                var fechaHastaArg = hasta ?? DateTimeHelper.GetArgentinaToday();

                // Convertir fechas de Argentina a UTC para PostgreSQL
                var fechaDesde = DateTimeHelper.GetArgentinaDayStartUtc(fechaDesdeArg);
                var fechaHasta = DateTimeHelper.GetArgentinaDayEndUtc(fechaHastaArg);

                // Obtener todas las ventas no canceladas en el rango
                var (ventas, _) = await _unitOfWork.Ventas.GetPaginadoAsync(
                    idCliente: null,
                    idUsuario: null,
                    fechaDesde: fechaDesde,
                    fechaHasta: fechaHasta,
                    cancelada: false,
                    pagina: 1,
                    tamanioPagina: 10000);

                // Obtener detalles de todas las ventas
                var topProductos = new Dictionary<int, (string Codigo, string Nombre, decimal Cantidad, decimal Ingresos)>();

                foreach (var venta in ventas)
                {
                    var detalles = await _unitOfWork.VentaDetalles.GetByVentaConProductoAsync(venta.Id_Venta);

                    foreach (var detalle in detalles)
                    {
                        var idArticulo = detalle.Id_Articulo ?? 0;
                        if (idArticulo == 0) continue;

                        var montoTotal = detalle.Cantidad * (detalle.Precio_Venta ?? 0);

                        if (topProductos.ContainsKey(idArticulo))
                        {
                            var existing = topProductos[idArticulo];
                            topProductos[idArticulo] = (
                                existing.Codigo,
                                existing.Nombre,
                                existing.Cantidad + detalle.Cantidad,
                                existing.Ingresos + montoTotal
                            );
                        }
                        else
                        {
                            topProductos[idArticulo] = (
                                detalle.CodigoProducto ?? "",
                                detalle.NombreProducto ?? "Producto",
                                detalle.Cantidad,
                                montoTotal
                            );
                        }
                    }
                }

                var result = topProductos
                    .Select(kvp => new TopProductoVentaDTO
                    {
                        IdArticulo = kvp.Key,
                        Codigo = kvp.Value.Codigo,
                        Nombre = kvp.Value.Nombre,
                        CantidadVendida = kvp.Value.Cantidad,
                        TotalIngresos = kvp.Value.Ingresos
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(top)
                    .ToList();

                return Ok(ApiResponse<List<TopProductoVentaDTO>>.Ok(result));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<TopProductoVentaDTO>>.Error(ex.Message));
            }
        }

        #region Ganancias (Solo Admin)

        /// <summary>
        /// Obtiene el reporte de ganancias de un día específico (Admin).
        /// Calcula ganancia bruta por producto basada en costo de compra vs precio de venta.
        /// </summary>
        [HttpGet("ganancias")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<ApiResponse<ReporteGananciasDTO>>> GetGanancias([FromQuery] DateTime fecha)
        {
            try
            {
                // Convertir fecha a UTC para PostgreSQL
                var fechaDesde = DateTimeHelper.GetArgentinaDayStartUtc(fecha);
                var fechaHasta = DateTimeHelper.GetArgentinaDayEndUtc(fecha);

                // Obtener ventas no canceladas del día
                var (ventas, _) = await _unitOfWork.Ventas.GetPaginadoAsync(
                    idCliente: null,
                    idUsuario: null,
                    fechaDesde: fechaDesde,
                    fechaHasta: fechaHasta,
                    cancelada: false,
                    pagina: 1,
                    tamanioPagina: 10000);

                if (!ventas.Any())
                {
                    return Ok(ApiResponse<ReporteGananciasDTO>.Ok(new ReporteGananciasDTO
                    {
                        Fecha = fecha,
                        Productos = new List<GananciaProductoDTO>(),
                        TotalIngresos = 0,
                        TotalCostos = 0,
                        TotalGanancias = 0,
                        MargenPromedioGlobal = 0,
                        TotalProductosVendidos = 0,
                        ProductosSinCosto = 0,
                        MensajeProductosSinCosto = ""
                    }, "No hay ventas para la fecha seleccionada"));
                }

                // Diccionario para agrupar por producto+presentación
                var gananciasDict = new Dictionary<string, GananciaProductoDTO>();
                var productosSinCosto = new HashSet<int>();

                // Procesar cada venta
                foreach (var venta in ventas)
                {
                    var detalles = await _unitOfWork.VentaDetalles.GetByVentaConProductoAsync(venta.Id_Venta);

                    foreach (var detalle in detalles)
                    {
                        var idArticulo = detalle.Id_Articulo ?? 0;
                        if (idArticulo == 0) continue;

                        // Obtener producto para el costo
                        var producto = await _unitOfWork.Productos.GetByIdAsync(idArticulo);
                        if (producto == null) continue;

                        // Obtener nombre de presentación
                        string presentacionNombre = "Unidad";
                        if (detalle.IdPresentacion.HasValue)
                        {
                            var presentacion = await _unitOfWork.Presentaciones.GetByIdAsync(detalle.IdPresentacion.Value);
                            presentacionNombre = presentacion?.Nombre ?? "Unidad";
                        }

                        // Calcular unidades totales
                        var unidadesPorPresentacion = detalle.CantidadUnidadesPorPresentacion > 0
                            ? detalle.CantidadUnidadesPorPresentacion
                            : 1;
                        var unidadesTotales = detalle.Cantidad * unidadesPorPresentacion;

                        // Calcular costos e ingresos
                        var costoUnitario = producto.CostoUnitario ?? 0;
                        if (!producto.CostoUnitario.HasValue || producto.CostoUnitario == 0)
                        {
                            productosSinCosto.Add(idArticulo);
                        }

                        var costoTotal = unidadesTotales * costoUnitario;
                        var ingresoTotal = detalle.Monto_Total ?? 0m;
                        var ganancia = ingresoTotal - costoTotal;

                        // Clave única por producto + presentación
                        var key = $"{idArticulo}_{detalle.IdPresentacion ?? 0}";

                        if (gananciasDict.ContainsKey(key))
                        {
                            var existing = gananciasDict[key];
                            existing.CantidadVendida += detalle.Cantidad;
                            existing.UnidadesTotales += unidadesTotales;
                            existing.CostoTotal += costoTotal;
                            existing.IngresoTotal += ingresoTotal;
                            existing.GananciaBruta += ganancia;
                        }
                        else
                        {
                            gananciasDict[key] = new GananciaProductoDTO
                            {
                                IdArticulo = idArticulo,
                                Codigo = producto.Codigo,
                                Nombre = producto.Nombre,
                                Presentacion = presentacionNombre,
                                UnidadesPorPresentacion = unidadesPorPresentacion,
                                CantidadVendida = detalle.Cantidad,
                                UnidadesTotales = unidadesTotales,
                                PrecioVenta = detalle.Precio_Venta ?? 0,
                                CostoUnitario = costoUnitario,
                                CostoTotal = costoTotal,
                                IngresoTotal = ingresoTotal,
                                GananciaBruta = ganancia,
                                MargenPorcentaje = 0 // Se calcula después
                            };
                        }
                    }
                }

                // Calcular margen porcentaje para cada producto
                foreach (var item in gananciasDict.Values)
                {
                    if (item.CostoTotal > 0)
                    {
                        item.MargenPorcentaje = (item.GananciaBruta / item.CostoTotal) * 100;
                    }
                    else if (item.IngresoTotal > 0)
                    {
                        item.MargenPorcentaje = 100; // Sin costo = 100% margen
                    }
                }

                // Calcular totales
                var totalIngresos = gananciasDict.Values.Sum(g => g.IngresoTotal);
                var totalCostos = gananciasDict.Values.Sum(g => g.CostoTotal);
                var totalGanancias = totalIngresos - totalCostos;
                var margenGlobal = totalCostos > 0 ? (totalGanancias / totalCostos) * 100 : 100;

                var reporte = new ReporteGananciasDTO
                {
                    Fecha = fecha,
                    Productos = gananciasDict.Values.OrderByDescending(g => g.GananciaBruta).ToList(),
                    TotalIngresos = totalIngresos,
                    TotalCostos = totalCostos,
                    TotalGanancias = totalGanancias,
                    MargenPromedioGlobal = Math.Round(margenGlobal, 2),
                    TotalProductosVendidos = gananciasDict.Count,
                    ProductosSinCosto = productosSinCosto.Count,
                    MensajeProductosSinCosto = productosSinCosto.Count > 0
                        ? $"{productosSinCosto.Count} producto(s) no tienen costo registrado. Su ganancia se calcula como 100% del ingreso."
                        : ""
                };

                return Ok(ApiResponse<ReporteGananciasDTO>.Ok(reporte));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<ReporteGananciasDTO>.Error(ex.Message));
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Mapea una entidad Venta a DTO
        /// </summary>
        private async Task<VentaDTO> MapVentaToDTO(CE_Ventas venta, bool incluirDetalles = false)
        {
            // Cargar nombre de cliente
            string? clienteNombre = null;
            if (venta.Id_Cliente.HasValue)
            {
                var cliente = await _unitOfWork.Clientes.GetByIdAsync(venta.Id_Cliente.Value);
                clienteNombre = cliente?.RazonSocial;
            }

            // Cargar nombre de usuario
            string? usuarioNombre = null;
            if (venta.Id_Usuario.HasValue)
            {
                var usuario = await _unitOfWork.Usuarios.GetByIdAsync(venta.Id_Usuario.Value);
                usuarioNombre = usuario?.Nombre;
            }

            var dto = new VentaDTO
            {
                IdVenta = venta.Id_Venta,
                NoFactura = venta.No_Factura,
                IdCliente = venta.Id_Cliente,
                ClienteNombre = clienteNombre,
                IdUsuario = venta.Id_Usuario ?? 0,
                UsuarioNombre = usuarioNombre ?? "Usuario",
                Fecha = venta.Fecha_Venta ?? DateTime.UtcNow,
                Subtotal = venta.Monto_Total,
                Descuento = 0, // El legacy no guarda descuento separado
                Total = venta.Monto_Total,
                Estado = venta.Cancelada ? "CANCELADA" : "COMPLETADA",
                MotivoCancelacion = venta.MotivoCancelacion,
                FormaPago = venta.FormaPago ?? "E",
                MontoRecibido = venta.MontoRecibido,
                Detalles = new List<VentaDetalleDTO>()
            };

            if (incluirDetalles)
            {
                var detalles = await _unitOfWork.VentaDetalles.GetByVentaConProductoAsync(venta.Id_Venta);
                var detallesList = new List<VentaDetalleDTO>();

                foreach (var d in detalles)
                {
                    // Obtener nombre de presentacion si existe
                    string? presentacionNombre = null;
                    if (d.IdPresentacion.HasValue)
                    {
                        var presentacion = await _unitOfWork.Presentaciones.GetByIdAsync(d.IdPresentacion.Value);
                        presentacionNombre = presentacion?.Nombre;
                    }

                    detallesList.Add(new VentaDetalleDTO
                    {
                        IdDetalle = d.Id_Detalle,
                        IdArticulo = d.Id_Articulo ?? 0,
                        NombreProducto = d.NombreProducto ?? "Producto",
                        CodigoProducto = d.CodigoProducto ?? "",
                        Cantidad = d.Cantidad,
                        PrecioUnitario = d.Precio_Venta ?? 0,
                        IdPresentacion = d.IdPresentacion,
                        PresentacionNombre = presentacionNombre,
                        CantidadUnidadesPorPresentacion = d.CantidadUnidadesPorPresentacion
                    });
                }

                dto.Detalles = detallesList;
            }

            return dto;
        }

        #endregion
    }

    /// <summary>
    /// DTO para cancelar una venta
    /// </summary>
    public class VentaCancelarDTO
    {
        public string? Motivo { get; set; }
    }
}
