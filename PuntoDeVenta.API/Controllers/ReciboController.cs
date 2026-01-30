using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Capa_Datos;
using Capa_Datos.Interfaces;
using PuntoDeVenta.API.DTOs;

namespace PuntoDeVenta.API.Controllers
{
    /// <summary>
    /// Controller para generar recibos/presupuestos en PDF.
    /// Replica la funcionalidad del legacy ImprimirTicket/Presupuesto.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReciboController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ReciboController> _logger;

        // Datos del negocio/vendedor (configurables via appsettings.json)
        private string NombreNegocio => _configuration["Negocio:Nombre"] ?? "Distribuidora LA FAMILIA";
        private string DomicilioNegocio => _configuration["Negocio:Domicilio"] ?? "Coronel Terrada 4840 - Isidro Casanova";
        private string TelefonoNegocio => _configuration["Negocio:Telefono"] ?? "1125594005";
        private string InstagramNegocio => _configuration["Negocio:Instagram"] ?? "@lafamiliabebidas.bsas";
        private string CuitNegocio => _configuration["Negocio:CUIT"] ?? "";

        // Logo cacheado
        private static byte[] _logoBytes = null;
        private static byte[] GetLogoBytes()
        {
            if (_logoBytes == null)
            {
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "PuntoDeVenta.API.Resources.logo_presupuesto.png";
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            _logoBytes = ms.ToArray();
                        }
                    }
                }
            }
            return _logoBytes;
        }

        public ReciboController(IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<ReciboController> logger)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
            // Configurar licencia de QuestPDF (Community = gratis para uso comercial)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Genera un PDF de presupuesto/recibo para una venta.
        /// </summary>
        [HttpGet("venta/{idVenta}")]
        public async Task<IActionResult> GenerarRecibo(int idVenta)
        {
            try
            {
                _logger.LogInformation("[RECIBO DEBUG] GenerarRecibo llamado con idVenta: {IdVenta}", idVenta);

                // Obtener la venta
                var venta = await _unitOfWork.Ventas.GetByIdAsync(idVenta);
                if (venta == null)
                {
                    _logger.LogWarning("[RECIBO DEBUG] Venta no encontrada para idVenta: {IdVenta}", idVenta);
                    return NotFound(ApiResponse<object>.Error("Venta no encontrada"));
                }
                _logger.LogInformation("[RECIBO DEBUG] Venta encontrada: No_Factura={NoFactura}, Monto={Monto}", venta.No_Factura, venta.Monto_Total);

                // Obtener detalles
                var detalles = await _unitOfWork.VentaDetalles.GetByVentaConProductoAsync(idVenta);
                var detallesList = detalles.ToList();
                _logger.LogInformation("[RECIBO DEBUG] Detalles obtenidos: {Count} registros", detallesList.Count);

                // Log detallado de cada item
                foreach (var det in detallesList)
                {
                    _logger.LogInformation("[RECIBO DEBUG] Detalle: Id_Detalle={IdDetalle}, Id_Venta={IdVenta}, Id_Articulo={IdArticulo}, Cantidad={Cantidad}, Precio={Precio}, Monto={Monto}, Nombre={Nombre}",
                        det.Id_Detalle, det.Id_Venta, det.Id_Articulo, det.Cantidad, det.Precio_Venta, det.Monto_Total, det.NombreProducto ?? "NULL");
                }

                // Cargar nombres de presentacion para cada detalle
                foreach (var detalle in detallesList)
                {
                    if (detalle.IdPresentacion.HasValue)
                    {
                        var presentacion = await _unitOfWork.Presentaciones.GetByIdAsync(detalle.IdPresentacion.Value);
                        detalle.PresentacionNombre = presentacion?.Nombre;
                    }
                }

                // Obtener cliente
                string clienteNombre = "Consumidor Final";
                string clienteDocumento = "0";
                string clienteDomicilio = "";
                if (venta.Id_Cliente.HasValue)
                {
                    var cliente = await _unitOfWork.Clientes.GetByIdAsync(venta.Id_Cliente.Value);
                    if (cliente != null)
                    {
                        clienteNombre = cliente.RazonSocial;
                        clienteDocumento = cliente.Documento ?? "0";
                        clienteDomicilio = cliente.Domicilio ?? "";
                    }
                }

                // Obtener usuario
                string vendedor = "Usuario";
                if (venta.Id_Usuario.HasValue)
                {
                    var usuario = await _unitOfWork.Usuarios.GetByIdAsync(venta.Id_Usuario.Value);
                    if (usuario != null)
                    {
                        vendedor = $"{usuario.Nombre} {usuario.Apellido}".Trim();
                    }
                }

                _logger.LogInformation("[RECIBO DEBUG] Llamando a GenerarPdfPresupuesto con {Count} detalles", detallesList.Count);

                // Generar PDF
                var pdfBytes = GenerarPdfPresupuesto(
                    venta.No_Factura,
                    venta.Fecha_Venta.HasValue ? DateTimeHelper.ConvertUtcToArgentina(venta.Fecha_Venta.Value) : DateTimeHelper.GetArgentinaNow(),
                    clienteNombre,
                    clienteDocumento,
                    clienteDomicilio,
                    vendedor,
                    detallesList,
                    venta.Monto_Total
                );

                // Retornar PDF como descarga
                return File(pdfBytes, "application/pdf", $"Presupuesto_{venta.No_Factura}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Error al generar PDF: {ex.Message}"));
            }
        }

        /// <summary>
        /// Genera un reporte PDF de caja diario con resumen de ventas.
        /// </summary>
        [HttpGet("caja/{fecha}")]
        public async Task<IActionResult> GenerarReporteCaja(DateTime fecha)
        {
            try
            {
                // Obtener todas las ventas del dia
                var fechaInicio = DateTime.SpecifyKind(fecha.Date, DateTimeKind.Utc);
                var fechaFin = DateTime.SpecifyKind(fecha.Date.AddDays(1).AddTicks(-1), DateTimeKind.Utc);

                var ventas = await _unitOfWork.Ventas.GetByFechaAsync(fechaInicio, fechaFin);
                var ventasList = ventas.ToList();

                // Obtener detalles de todas las ventas para productos top
                var productosVendidos = new Dictionary<string, (string Nombre, decimal Cantidad, decimal Total)>();

                foreach (var venta in ventasList.Where(v => !v.Cancelada))
                {
                    var detalles = await _unitOfWork.VentaDetalles.GetByVentaConProductoAsync(venta.Id_Venta);
                    foreach (var det in detalles)
                    {
                        // Cargar nombre de presentación si existe
                        if (det.IdPresentacion.HasValue)
                        {
                            var presentacion = await _unitOfWork.Presentaciones.GetByIdAsync(det.IdPresentacion.Value);
                            det.PresentacionNombre = presentacion?.Nombre;
                        }

                        // Construir key única por producto + presentación
                        var key = det.IdPresentacion.HasValue
                            ? $"{det.CodigoProducto ?? det.Id_Articulo?.ToString() ?? "N/A"}_{det.IdPresentacion}"
                            : det.CodigoProducto ?? det.Id_Articulo?.ToString() ?? "N/A";

                        // Construir nombre con presentación si existe
                        var nombre = det.NombreProducto ?? "Producto";
                        if (!string.IsNullOrEmpty(det.PresentacionNombre) && det.CantidadUnidadesPorPresentacion > 1)
                        {
                            nombre += $" - {det.PresentacionNombre}";
                        }

                        var cantidad = det.Cantidad;
                        var total = det.Monto_Total ?? 0;

                        if (productosVendidos.ContainsKey(key))
                        {
                            var existing = productosVendidos[key];
                            productosVendidos[key] = (nombre, existing.Cantidad + cantidad, existing.Total + total);
                        }
                        else
                        {
                            productosVendidos[key] = (nombre, cantidad, total);
                        }
                    }
                }

                // Obtener nombres de usuarios
                var usuarioIds = ventasList.Where(v => v.Id_Usuario.HasValue).Select(v => v.Id_Usuario!.Value).Distinct();
                var usuarios = new Dictionary<int, string>();
                foreach (var uid in usuarioIds)
                {
                    var u = await _unitOfWork.Usuarios.GetByIdAsync(uid);
                    if (u != null)
                        usuarios[uid] = $"{u.Nombre} {u.Apellido}".Trim();
                }

                // Obtener nombres de clientes
                var clienteIds = ventasList.Where(v => v.Id_Cliente.HasValue).Select(v => v.Id_Cliente!.Value).Distinct();
                var clientes = new Dictionary<int, string>();
                foreach (var cid in clienteIds)
                {
                    var c = await _unitOfWork.Clientes.GetByIdAsync(cid);
                    if (c != null)
                        clientes[cid] = c.RazonSocial;
                }

                // Generar PDF
                var pdfBytes = GenerarPdfReporteCaja(fecha, ventasList, productosVendidos, usuarios, clientes);

                return File(pdfBytes, "application/pdf", $"ReporteCaja_{fecha:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Error al generar reporte: {ex.Message}"));
            }
        }

        /// <summary>
        /// Genera un PDF de presupuesto directamente desde los datos del carrito (sin guardar venta).
        /// Util para generar presupuestos antes de confirmar la venta.
        /// </summary>
        [HttpPost("presupuesto")]
        public async Task<IActionResult> GenerarPresupuesto([FromBody] PresupuestoRequestDTO request)
        {
            try
            {
                // Obtener cliente
                string clienteNombre = "Consumidor Final";
                string clienteDocumento = "0";
                string clienteDomicilio = "";
                if (request.IdCliente.HasValue)
                {
                    var cliente = await _unitOfWork.Clientes.GetByIdAsync(request.IdCliente.Value);
                    if (cliente != null)
                    {
                        clienteNombre = cliente.RazonSocial;
                        clienteDocumento = cliente.Documento ?? "0";
                        clienteDomicilio = cliente.Domicilio ?? "";
                    }
                }

                // Construir lista de detalles con info de productos
                var detalles = new List<Capa_Entidad.CE_VentaDetalle>();
                foreach (var item in request.Items)
                {
                    var producto = await _unitOfWork.Productos.GetByIdAsync(item.IdArticulo);

                    // Obtener nombre de presentacion si existe
                    string? presentacionNombre = null;
                    if (item.IdPresentacion.HasValue)
                    {
                        var presentacion = await _unitOfWork.Presentaciones.GetByIdAsync(item.IdPresentacion.Value);
                        presentacionNombre = presentacion?.Nombre;
                    }

                    detalles.Add(new Capa_Entidad.CE_VentaDetalle
                    {
                        Id_Articulo = item.IdArticulo,
                        Cantidad = item.Cantidad,
                        Precio_Venta = item.PrecioUnitario,
                        Monto_Total = item.Cantidad * item.PrecioUnitario,
                        NombreProducto = producto?.Nombre ?? "Producto",
                        CodigoProducto = producto?.Codigo ?? "",
                        IdPresentacion = item.IdPresentacion,
                        CantidadUnidadesPorPresentacion = item.CantidadUnidadesPorPresentacion,
                        PresentacionNombre = presentacionNombre
                    });
                }

                decimal total = detalles.Sum(d => d.Monto_Total ?? 0);
                var ahora = DateTimeHelper.GetArgentinaNow();
                string noPresupuesto = $"P-{ahora:yyMMddHHmmss}";

                // Generar PDF
                var pdfBytes = GenerarPdfPresupuesto(
                    noPresupuesto,
                    ahora,
                    clienteNombre,
                    clienteDocumento,
                    clienteDomicilio,
                    "Vendedor",
                    detalles,
                    total
                );

                return File(pdfBytes, "application/pdf", $"Presupuesto_{noPresupuesto}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error($"Error al generar presupuesto: {ex.Message}"));
            }
        }

        /// <summary>
        /// Genera el PDF usando QuestPDF.
        /// Diseño similar al legacy pero modernizado.
        /// </summary>
        private byte[] GenerarPdfPresupuesto(
            string noDocumento,
            DateTime fecha,
            string clienteNombre,
            string clienteDocumento,
            string clienteDomicilio,
            string vendedor,
            List<Capa_Entidad.CE_VentaDetalle> detalles,
            decimal total)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(25);
                    page.DefaultTextStyle(x => x.FontSize(10).FontColor(Colors.Grey.Darken3));

                    page.Header().Element(c => ComposeHeader(c, noDocumento));
                    page.Content().Element(c => ComposeContent(c, noDocumento, fecha, clienteNombre, clienteDocumento, clienteDomicilio, vendedor, detalles, total));
                    page.Footer().Element(ComposeFooter);
                });
            });

            return document.GeneratePdf();
        }

        // Color azul del diseño legacy
        private static readonly string ColorAzul = "#4A90D9";
        private static readonly string ColorAzulClaro = "#E8F4FD";

        private void ComposeHeader(IContainer container, string noDocumento)
        {
            container.Column(column =>
            {
                // Encabezado azul con título (similar al legacy)
                column.Item().Border(1).BorderColor(ColorAzul).Row(row =>
                {
                    // Logo (80px como en el legacy)
                    var logoBytes = GetLogoBytes();
                    if (logoBytes != null && logoBytes.Length > 0)
                    {
                        row.ConstantItem(80).Padding(5).AlignCenter().AlignMiddle()
                            .Image(logoBytes).FitArea();
                    }
                    else
                    {
                        row.ConstantItem(80).Padding(10).AlignCenter().AlignMiddle()
                            .Text("LOGO").FontSize(8).FontColor(Colors.Grey.Medium);
                    }

                    // Título en fondo azul
                    row.RelativeItem().Background(ColorAzul).Padding(15).AlignCenter().AlignMiddle()
                        .Text($"Presupuesto ({noDocumento})")
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.White);
                });

                column.Item().PaddingBottom(15);
            });
        }

        private void ComposeDatosEmpresaCliente(IContainer container, string clienteNombre, string clienteDocumento, string clienteDomicilio)
        {
            // Helper para mostrar "-" si el valor es vacío o nulo
            string ValorOGuion(string valor) => string.IsNullOrWhiteSpace(valor) ? "-" : valor;

            container.Border(1).BorderColor(ColorAzul).Row(row =>
            {
                // DATOS EMPRESA (columna izquierda)
                row.RelativeItem().BorderRight(1).BorderColor(ColorAzul).Column(col =>
                {
                    col.Item().Background(ColorAzulClaro).Padding(8)
                        .Text("Datos Empresa")
                        .FontSize(9)
                        .Bold()
                        .FontColor(ColorAzul);

                    col.Item().Padding(8).Column(c =>
                    {
                        c.Item().Text(ValorOGuion(NombreNegocio)).Bold().FontSize(9);
                        c.Item().PaddingTop(4).Text(ValorOGuion(DomicilioNegocio)).FontSize(9);
                        c.Item().PaddingTop(4).Text($"Tel: {ValorOGuion(TelefonoNegocio)}").FontSize(9);
                        c.Item().PaddingTop(4).Text($"Instagram: {ValorOGuion(InstagramNegocio)}").FontSize(9);
                    });
                });

                // DATOS CLIENTE (columna derecha)
                row.RelativeItem().Column(col =>
                {
                    col.Item().Background(ColorAzulClaro).Padding(8)
                        .Text("Datos Cliente")
                        .FontSize(9)
                        .Bold()
                        .FontColor(ColorAzul);

                    col.Item().Padding(8).Column(c =>
                    {
                        c.Item().Text(ValorOGuion(clienteNombre)).Bold().FontSize(9);
                        c.Item().PaddingTop(4).Text($"Documento: {ValorOGuion(clienteDocumento)}").FontSize(9);
                        c.Item().PaddingTop(4).Text($"Domicilio: {ValorOGuion(clienteDomicilio)}").FontSize(9);
                    });
                });
            });
        }

        private void ComposeContent(
            IContainer container,
            string noDocumento,
            DateTime fecha,
            string clienteNombre,
            string clienteDocumento,
            string clienteDomicilio,
            string vendedor,
            List<Capa_Entidad.CE_VentaDetalle> detalles,
            decimal total)
        {
            container.Column(col =>
            {
                // Seccion Datos Empresa / Datos Cliente (formato legacy)
                col.Item().Element(c => ComposeDatosEmpresaCliente(c, clienteNombre, clienteDocumento, clienteDomicilio));

                // Fila Fecha del presupuesto
                col.Item().PaddingTop(15).Border(1).BorderColor(ColorAzul).Padding(10)
                    .Text(text =>
                    {
                        text.Span("Fecha del presupuesto: ").Bold();
                        text.Span(fecha.ToString("dd/MM/yyyy HH:mm"));
                    });

                col.Item().PaddingTop(20);

                // DEBUG: Mostrar información de diagnóstico
                col.Item().Text($"[DEBUG] IdVenta: {noDocumento}").FontSize(8).FontColor(Colors.Red.Medium);
                col.Item().Text($"[DEBUG] Detalles encontrados: {detalles.Count}").FontSize(8).FontColor(Colors.Red.Medium);
                if (detalles.Count > 0)
                {
                    col.Item().Text($"[DEBUG] Primer detalle - IdArticulo: {detalles[0].Id_Articulo}, Nombre: {detalles[0].NombreProducto ?? "NULL"}, Cantidad: {detalles[0].Cantidad}").FontSize(7).FontColor(Colors.Red.Medium);
                }

                // Tabla de productos (formato legacy)
                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(45); // Descripcion 45%
                        columns.ConstantColumn(70); // Unidades 15%
                        columns.ConstantColumn(90); // Precio 20%
                        columns.ConstantColumn(90); // Total 20%
                    });

                    // Header azul (como legacy)
                    table.Header(header =>
                    {
                        header.Cell().Background(ColorAzul).Padding(10)
                            .Text("DESCRIPCION").Bold().FontSize(10).FontColor(Colors.White);
                        header.Cell().Background(ColorAzul).Padding(10).AlignCenter()
                            .Text("UNIDADES").Bold().FontSize(10).FontColor(Colors.White);
                        header.Cell().Background(ColorAzul).Padding(10).AlignRight()
                            .Text("PRECIO").Bold().FontSize(10).FontColor(Colors.White);
                        header.Cell().Background(ColorAzul).Padding(10).AlignRight()
                            .Text("TOTAL").Bold().FontSize(10).FontColor(Colors.White);
                    });

                    // Filas de productos
                    bool alternate = false;
                    foreach (var detalle in detalles)
                    {
                        var bgColor = alternate ? "#F9F9F9" : "#FFFFFF";
                        alternate = !alternate;

                        // Construir descripcion con presentacion si existe
                        var descripcion = detalle.NombreProducto ?? "Producto";
                        if (!string.IsNullOrEmpty(detalle.PresentacionNombre) && detalle.CantidadUnidadesPorPresentacion > 1)
                        {
                            descripcion += $" - {detalle.PresentacionNombre}";
                        }

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(8).Text(descripcion).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(8).AlignCenter().Text(detalle.Cantidad.ToString("N0")).FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(8).AlignRight().Text($"${detalle.Precio_Venta ?? 0:N2}").FontSize(9);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                            .Padding(8).AlignRight().Text($"${detalle.Monto_Total ?? 0:N2}").FontSize(9);
                    }
                });

                col.Item().PaddingTop(20);

                // Seccion Totales (formato legacy - alineado a la derecha)
                col.Item().Row(row =>
                {
                    row.RelativeItem(55); // Espacio vacío izquierda

                    row.RelativeItem(45).Border(1).BorderColor(ColorAzul).Column(totCol =>
                    {
                        // Sub-total
                        totCol.Item().Background("#F5F5F5").Padding(10).Row(r =>
                        {
                            r.RelativeItem().AlignRight().PaddingRight(20).Text("SUB-TOTAL").FontSize(10);
                            r.ConstantItem(100).AlignRight().Text($"${total:N2}").FontSize(10);
                        });

                        // Total Presupuesto
                        totCol.Item().Background(ColorAzul).Padding(10).Row(r =>
                        {
                            r.RelativeItem().AlignRight().PaddingRight(20)
                                .Text("TOTAL PRESUPUESTO").Bold().FontSize(12).FontColor(Colors.White);
                            r.ConstantItem(100).AlignRight()
                                .Text($"${total:N2}").Bold().FontSize(12).FontColor(Colors.White);
                        });
                    });
                });
            });
        }

        private void ComposeFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(5).AlignCenter()
                    .Text("Este documento no tiene validez fiscal - Presupuesto generado por Gestión POS")
                    .FontSize(8)
                    .FontColor(Colors.Grey.Darken1);
                col.Item().AlignCenter()
                    .Text("MUCHAS GRACIAS POR SU COMPRA")
                    .FontSize(9)
                    .Bold()
                    .FontColor(Colors.Indigo.Medium);
            });
        }

        #region Reporte de Caja

        /// <summary>
        /// Genera el PDF del reporte de caja diario
        /// </summary>
        private byte[] GenerarPdfReporteCaja(
            DateTime fecha,
            List<Capa_Entidad.CE_Ventas> ventas,
            Dictionary<string, (string Nombre, decimal Cantidad, decimal Total)> productos,
            Dictionary<int, string> usuarios,
            Dictionary<int, string> clientes)
        {
            // Calcular estadisticas
            var ventasCompletadas = ventas.Where(v => !v.Cancelada).ToList();
            var ventasCanceladas = ventas.Where(v => v.Cancelada).ToList();

            var totalVentas = ventasCompletadas.Count;
            var totalIngresos = ventasCompletadas.Sum(v => v.Monto_Total);
            var ticketPromedio = totalVentas > 0 ? totalIngresos / totalVentas : 0;
            var totalCanceladas = ventasCanceladas.Count;
            var montoCancelado = ventasCanceladas.Sum(v => v.Monto_Total);

            // Totales por forma de pago
            var totalEfectivo = ventasCompletadas.Where(v => v.FormaPago != "T").Sum(v => v.Monto_Total);
            var totalTransferencia = ventasCompletadas.Where(v => v.FormaPago == "T").Sum(v => v.Monto_Total);
            var cantidadEfectivo = ventasCompletadas.Count(v => v.FormaPago != "T");
            var cantidadTransferencia = ventasCompletadas.Count(v => v.FormaPago == "T");

            // Agrupar por hora (usando tuplas en lugar de anonymous types)
            var ventasPorHora = ventasCompletadas
                .GroupBy(v => (v.Fecha_Venta ?? DateTime.MinValue).Hour)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => (Cantidad: g.Count(), Total: g.Sum(v => v.Monto_Total)));

            // Agrupar por vendedor
            var ventasPorVendedor = ventasCompletadas
                .GroupBy(v => v.Id_Usuario ?? 0)
                .OrderByDescending(g => g.Sum(v => v.Monto_Total))
                .ToDictionary(
                    g => usuarios.TryGetValue(g.Key, out var n) ? n : "Sin vendedor",
                    g => (Cantidad: g.Count(), Total: g.Sum(v => v.Monto_Total)));

            // Top 10 productos
            var topProductos = productos
                .OrderByDescending(p => p.Value.Total)
                .Take(10)
                .ToList();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Element(c => ComposeCajaHeader(c, fecha));
                    page.Content().Element(c => ComposeCajaContent(
                        c, fecha, totalVentas, totalIngresos, ticketPromedio,
                        totalCanceladas, montoCancelado,
                        totalEfectivo, cantidadEfectivo, totalTransferencia, cantidadTransferencia,
                        ventasPorHora, ventasPorVendedor, topProductos, ventas, usuarios, clientes));
                    page.Footer().Element(ComposeCajaFooter);
                });
            });

            return document.GeneratePdf();
        }

        private void ComposeCajaHeader(IContainer container, DateTime fecha)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("PUNTO DE VENTA")
                            .FontSize(22)
                            .Bold()
                            .FontColor(Colors.Indigo.Darken3);
                        col.Item().Text("Sistema de Gestión Comercial")
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(180).AlignRight().Column(col =>
                    {
                        col.Item().Text("REPORTE DE CAJA")
                            .FontSize(16)
                            .Bold()
                            .FontColor(Colors.Indigo.Medium);
                        col.Item().Text(fecha.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                            new System.Globalization.CultureInfo("es-ES")))
                            .FontSize(10)
                            .FontColor(Colors.Grey.Darken2);
                    });
                });

                column.Item().PaddingVertical(10).LineHorizontal(2).LineColor(Colors.Indigo.Darken3);
            });
        }

        private void ComposeCajaContent(
            IContainer container,
            DateTime fecha,
            int totalVentas,
            decimal totalIngresos,
            decimal ticketPromedio,
            int totalCanceladas,
            decimal montoCancelado,
            decimal totalEfectivo,
            int cantidadEfectivo,
            decimal totalTransferencia,
            int cantidadTransferencia,
            Dictionary<int, (int Cantidad, decimal Total)> ventasPorHora,
            Dictionary<string, (int Cantidad, decimal Total)> ventasPorVendedor,
            List<KeyValuePair<string, (string Nombre, decimal Cantidad, decimal Total)>> topProductos,
            List<Capa_Entidad.CE_Ventas> ventas,
            Dictionary<int, string> usuarios,
            Dictionary<int, string> clientes)
        {
            container.Column(mainCol =>
            {
                // SECCION 1: RESUMEN GENERAL
                mainCol.Item().PaddingBottom(15).Column(col =>
                {
                    col.Item().Text("RESUMEN DEL DÍA")
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Indigo.Darken2);
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().PaddingTop(10).Row(row =>
                    {
                        // KPI Ventas
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Green.Lighten5).Padding(10).Column(c =>
                        {
                            c.Item().Text("VENTAS COMPLETADAS").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(totalVentas.ToString()).FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                            c.Item().Text("transacciones").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(10);

                        // KPI Ingresos
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Blue.Lighten5).Padding(10).Column(c =>
                        {
                            c.Item().Text("INGRESOS TOTALES").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"${totalIngresos:N0}").FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text("pesos").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(10);

                        // KPI Ticket Promedio
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Purple.Lighten5).Padding(10).Column(c =>
                        {
                            c.Item().Text("TICKET PROMEDIO").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"${ticketPromedio:N0}").FontSize(18).Bold().FontColor(Colors.Purple.Darken2);
                            c.Item().Text("por venta").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(10);

                        // KPI Canceladas
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(totalCanceladas > 0 ? Colors.Red.Lighten5 : Colors.Grey.Lighten4).Padding(10).Column(c =>
                        {
                            c.Item().Text("CANCELADAS").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text(totalCanceladas.ToString()).FontSize(18).Bold()
                                .FontColor(totalCanceladas > 0 ? Colors.Red.Darken2 : Colors.Grey.Darken1);
                            c.Item().Text($"(${montoCancelado:N0})").FontSize(8)
                                .FontColor(totalCanceladas > 0 ? Colors.Red.Darken1 : Colors.Grey.Darken1);
                        });
                    });

                    // Fila de totales por forma de pago
                    col.Item().PaddingTop(10).Row(row =>
                    {
                        // KPI Efectivo
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Amber.Lighten5).Padding(10).Column(c =>
                        {
                            c.Item().Text("EFECTIVO").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"${totalEfectivo:N0}").FontSize(18).Bold().FontColor(Colors.Amber.Darken3);
                            c.Item().Text($"{cantidadEfectivo} ventas").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(10);

                        // KPI Transferencia
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                            .Background(Colors.Cyan.Lighten5).Padding(10).Column(c =>
                        {
                            c.Item().Text("TRANSFERENCIA").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().Text($"${totalTransferencia:N0}").FontSize(18).Bold().FontColor(Colors.Cyan.Darken3);
                            c.Item().Text($"{cantidadTransferencia} ventas").FontSize(8).FontColor(Colors.Grey.Darken1);
                        });

                        row.ConstantItem(10);

                        // Espacio vacío para mantener el layout
                        row.RelativeItem();
                        row.ConstantItem(10);
                        row.RelativeItem();
                    });
                });

                // SECCION 2: VENTAS POR HORA
                if (ventasPorHora.Any())
                {
                    mainCol.Item().PaddingBottom(15).Column(col =>
                    {
                        col.Item().Text("DISTRIBUCIÓN POR HORA")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Indigo.Darken2);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5)
                                    .Text("HORA").Bold().FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5)
                                    .Text("").FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignCenter()
                                    .Text("VENTAS").Bold().FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight()
                                    .Text("MONTO").Bold().FontSize(8);
                            });

                            var maxVentas = ventasPorHora.Values.Max(v => (int)v.Cantidad);

                            foreach (var hora in ventasPorHora.OrderBy(h => h.Key))
                            {
                                var porcentaje = maxVentas > 0 ? (float)hora.Value.Cantidad / maxVentas : 0;

                                table.Cell().Padding(3).Text($"{hora.Key:00}:00 - {hora.Key:00}:59").FontSize(8);
                                table.Cell().Padding(3).PaddingRight(10).Column(c =>
                                {
                                    c.Item().PaddingTop(3).Height(12).Row(row =>
                                    {
                                        row.RelativeItem(porcentaje).Background(Colors.Indigo.Medium).MinWidth(2);
                                        row.RelativeItem(1 - porcentaje);
                                    });
                                });
                                table.Cell().Padding(3).AlignCenter().Text(hora.Value.Cantidad.ToString()).FontSize(8);
                                table.Cell().Padding(3).AlignRight().Text($"${hora.Value.Total:N0}").FontSize(8);
                            }
                        });
                    });
                }

                // SECCION 3: VENTAS POR VENDEDOR
                if (ventasPorVendedor.Any())
                {
                    mainCol.Item().PaddingBottom(15).Column(col =>
                    {
                        col.Item().Text("VENTAS POR VENDEDOR")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Indigo.Darken2);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5)
                                    .Text("VENDEDOR").Bold().FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignCenter()
                                    .Text("TRANSACCIONES").Bold().FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight()
                                    .Text("MONTO TOTAL").Bold().FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight()
                                    .Text("% DEL DÍA").Bold().FontSize(8);
                            });

                            bool alternate = false;
                            foreach (var vendedor in ventasPorVendedor)
                            {
                                var bgColor = alternate ? Colors.Grey.Lighten4 : Colors.White;
                                alternate = !alternate;
                                var porcentaje = totalIngresos > 0 ? (decimal)vendedor.Value.Total / totalIngresos * 100 : 0;

                                table.Cell().Background(bgColor).Padding(5).Text(vendedor.Key).FontSize(8);
                                table.Cell().Background(bgColor).Padding(5).AlignCenter()
                                    .Text(vendedor.Value.Cantidad.ToString()).FontSize(8);
                                table.Cell().Background(bgColor).Padding(5).AlignRight()
                                    .Text($"${vendedor.Value.Total:N0}").FontSize(8).Bold();
                                table.Cell().Background(bgColor).Padding(5).AlignRight()
                                    .Text($"{porcentaje:N1}%").FontSize(8);
                            }
                        });
                    });
                }

                // SECCION 4: TOP PRODUCTOS
                if (topProductos.Any())
                {
                    mainCol.Item().PaddingBottom(15).Column(col =>
                    {
                        col.Item().Text("TOP 10 PRODUCTOS MÁS VENDIDOS")
                            .FontSize(12)
                            .Bold()
                            .FontColor(Colors.Indigo.Darken2);
                        col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(70);
                                columns.RelativeColumn();
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5)
                                    .Text("#").Bold().FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5)
                                    .Text("CODIGO").Bold().FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5)
                                    .Text("PRODUCTO").Bold().FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignCenter()
                                    .Text("CANTIDAD").Bold().FontSize(8);
                                header.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight()
                                    .Text("TOTAL").Bold().FontSize(8);
                            });

                            int rank = 1;
                            bool alternate = false;
                            foreach (var prod in topProductos)
                            {
                                var bgColor = alternate ? Colors.Grey.Lighten4 : Colors.White;
                                alternate = !alternate;

                                table.Cell().Background(bgColor).Padding(5)
                                    .Text(rank.ToString()).FontSize(8).Bold();
                                table.Cell().Background(bgColor).Padding(5)
                                    .Text(prod.Key).FontSize(7).FontColor(Colors.Grey.Darken1);
                                table.Cell().Background(bgColor).Padding(5)
                                    .Text(prod.Value.Nombre).FontSize(8);
                                table.Cell().Background(bgColor).Padding(5).AlignCenter()
                                    .Text(prod.Value.Cantidad.ToString("N0")).FontSize(8);
                                table.Cell().Background(bgColor).Padding(5).AlignRight()
                                    .Text($"${prod.Value.Total:N0}").FontSize(8).Bold();
                                rank++;
                            }
                        });
                    });
                }

                // SECCION 5: DETALLE DE TRANSACCIONES
                mainCol.Item().Column(col =>
                {
                    col.Item().Text("DETALLE DE TRANSACCIONES")
                        .FontSize(12)
                        .Bold()
                        .FontColor(Colors.Indigo.Darken2);
                    col.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    col.Item().PaddingTop(10).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(80);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.ConstantColumn(70);
                            columns.ConstantColumn(60);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Indigo.Lighten4).Padding(4)
                                .Text("HORA").Bold().FontSize(7);
                            header.Cell().Background(Colors.Indigo.Lighten4).Padding(4)
                                .Text("FACTURA").Bold().FontSize(7);
                            header.Cell().Background(Colors.Indigo.Lighten4).Padding(4)
                                .Text("CLIENTE").Bold().FontSize(7);
                            header.Cell().Background(Colors.Indigo.Lighten4).Padding(4)
                                .Text("VENDEDOR").Bold().FontSize(7);
                            header.Cell().Background(Colors.Indigo.Lighten4).Padding(4).AlignRight()
                                .Text("MONTO").Bold().FontSize(7);
                            header.Cell().Background(Colors.Indigo.Lighten4).Padding(4).AlignCenter()
                                .Text("ESTADO").Bold().FontSize(7);
                        });

                        bool alternate = false;
                        foreach (var venta in ventas.OrderBy(v => v.Fecha_Venta))
                        {
                            var bgColor = venta.Cancelada
                                ? Colors.Red.Lighten5
                                : (alternate ? Colors.Grey.Lighten4 : Colors.White);
                            alternate = !alternate;

                            var clienteNombre = venta.Id_Cliente.HasValue && clientes.TryGetValue(venta.Id_Cliente.Value, out var cn)
                                ? cn : "Consumidor Final";
                            var vendedorNombre = venta.Id_Usuario.HasValue && usuarios.TryGetValue(venta.Id_Usuario.Value, out var vn)
                                ? vn : "-";

                            table.Cell().Background(bgColor).Padding(4)
                                .Text((venta.Fecha_Venta ?? DateTime.MinValue).ToString("HH:mm")).FontSize(7);
                            table.Cell().Background(bgColor).Padding(4)
                                .Text(venta.No_Factura ?? "-").FontSize(7);
                            table.Cell().Background(bgColor).Padding(4)
                                .Text(clienteNombre.Length > 20 ? clienteNombre.Substring(0, 20) + "..." : clienteNombre).FontSize(7);
                            table.Cell().Background(bgColor).Padding(4)
                                .Text(vendedorNombre).FontSize(7);
                            table.Cell().Background(bgColor).Padding(4).AlignRight()
                                .Text($"${venta.Monto_Total:N0}").FontSize(7)
                                .FontColor(venta.Cancelada ? Colors.Red.Darken2 : Colors.Black);
                            table.Cell().Background(bgColor).Padding(4).AlignCenter()
                                .Text(venta.Cancelada ? "CANCELADA" : "OK").FontSize(6)
                                .FontColor(venta.Cancelada ? Colors.Red.Darken2 : Colors.Green.Darken2);
                        }

                        // Fila de totales
                        table.Cell().ColumnSpan(4).Background(Colors.Indigo.Lighten4).Padding(5)
                            .Text($"TOTAL: {ventas.Count} transacciones").Bold().FontSize(8);
                        table.Cell().Background(Colors.Indigo.Lighten4).Padding(5).AlignRight()
                            .Text($"${totalIngresos:N0}").Bold().FontSize(8).FontColor(Colors.Indigo.Darken3);
                        table.Cell().Background(Colors.Indigo.Lighten4).Padding(5);
                    });
                });
            });
        }

        private void ComposeCajaFooter(IContainer container)
        {
            container.Column(col =>
            {
                col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                col.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"Generado: {DateTimeHelper.GetArgentinaNow():dd/MM/yyyy HH:mm:ss}")
                        .FontSize(7)
                        .FontColor(Colors.Grey.Darken1);
                    row.RelativeItem().AlignRight().Text("Gestión POS")
                        .FontSize(7)
                        .FontColor(Colors.Grey.Darken1);
                });
            });
        }

        #endregion
    }

    /// <summary>
    /// DTO para solicitar un presupuesto sin guardar venta
    /// </summary>
    public class PresupuestoRequestDTO
    {
        public int? IdCliente { get; set; }
        public List<PresupuestoItemDTO> Items { get; set; } = new();
    }

    public class PresupuestoItemDTO
    {
        public int IdArticulo { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int? IdPresentacion { get; set; }
        public int CantidadUnidadesPorPresentacion { get; set; } = 1;
    }
}
