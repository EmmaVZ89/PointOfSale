using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using Capa_Negocio;
using Capa_Entidad;
using PuntoDeVenta.src.Boxes;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.Threading.Tasks;
using System.Linq;

namespace PuntoDeVenta.Views
{
    public partial class Reportes : UserControl
    {
        private CN_Reportes cnReportes = new CN_Reportes();
        private CN_Clientes cnClientes = new CN_Clientes();
        private string reporteActual = "Ventas";

        public Reportes()
        {
            InitializeComponent();
            dpDesde.SelectedDate = DateTime.Now.AddDays(-30);
            dpHasta.SelectedDate = DateTime.Now;
            this.Loaded += Reportes_Loaded;
        }

        private async void Reportes_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarClientes();
        }

        private async Task CargarClientes()
        {
            try
            {
                List<CE_Clientes> clientes = null;
                await Task.Run(() =>
                {
                    clientes = cnClientes.ListarActivos();
                });

                // Agregar opción "Todos" al inicio
                var lista = new List<CE_Clientes>();
                lista.Add(new CE_Clientes { IdCliente = 0, RazonSocial = "-- Todos --" });
                if (clientes != null)
                {
                    lista.AddRange(clientes);
                }

                cbClientes.ItemsSource = lista;
                cbClientes.SelectedIndex = 0; // "Todos" por defecto
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error cargando clientes: " + ex.Message, TipoMensaje.Error);
            }
        }

        private void Radio_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            if (radio == null || !IsLoaded) return;

            string contenido = radio.Content.ToString();

            if (contenido.Contains("Detalladas"))
            {
                reporteActual = "Ventas";
                MostrarFiltros(true, mostrarCliente: true);
                MostrarBotonesReporte(true);
            }
            else if (contenido.Contains("Stock"))
            {
                reporteActual = "Stock";
                MostrarFiltros(false, mostrarCliente: false);
                MostrarBotonesReporte(true);
            }
            else if (contenido.Contains("Top"))
            {
                reporteActual = "Top";
                MostrarFiltros(true, mostrarCliente: false);
                MostrarBotonesReporte(true);
            }
            else if (contenido.Contains("Reimprimir"))
            {
                reporteActual = "Reimprimir";
                MostrarFiltros(true, mostrarCliente: true);
                MostrarBotonesReporte(false);
            }

            // Limpiar grilla al cambiar
            GridReportes.ItemsSource = null;
        }

        private void MostrarBotonesReporte(bool mostrarExport)
        {
            // Mostrar/ocultar botones según el modo
            btnExportarCSV.Visibility = mostrarExport ? Visibility.Visible : Visibility.Collapsed;
            btnExportarPDF.Visibility = mostrarExport ? Visibility.Visible : Visibility.Collapsed;
            btnReimprimir.Visibility = mostrarExport ? Visibility.Collapsed : Visibility.Visible;
        }

        private void MostrarFiltros(bool visible, bool mostrarCliente = false)
        {
            Visibility v = visible ? Visibility.Visible : Visibility.Collapsed;
            pnlFechaDesde.Visibility = v;
            pnlFechaHasta.Visibility = v;

            // Cliente solo visible en reporte de Ventas
            pnlCliente.Visibility = mostrarCliente ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void BtnGenerar_Click(object sender, RoutedEventArgs e)
        {
            DateTime desde = dpDesde.SelectedDate ?? DateTime.MinValue;
            DateTime hasta = dpHasta.SelectedDate ?? DateTime.MaxValue;
            DataTable dt = new DataTable();

            // Obtener cliente seleccionado (0 = Todos)
            int? idCliente = null;
            if (cbClientes.SelectedItem is CE_Clientes clienteSeleccionado && clienteSeleccionado.IdCliente > 0)
            {
                idCliente = clienteSeleccionado.IdCliente;
            }

            MainWindow.ShowLoader(); // MOSTRAR LOADER

            try
            {
                // Ejecutar en background para no congelar la UI
                await Task.Run(() =>
                {
                    switch (reporteActual)
                    {
                        case "Ventas":
                            dt = cnReportes.ObtenerVentas(desde, hasta, null, idCliente);
                            break;
                        case "Stock":
                            dt = cnReportes.ObtenerStockValorizado();
                            break;
                        case "Top":
                            dt = cnReportes.ObtenerTopProductos(desde, hasta);
                            break;
                        case "Reimprimir":
                            dt = cnReportes.ObtenerVentasResumen(desde, hasta, idCliente);
                            break;
                    }
                });

                // Asignar en UI Thread
                GridReportes.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error generando reporte: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader(); // OCULTAR LOADER SIEMPRE
            }
        }

        private async void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            // Reutilizamos logica CSV generica
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv" };
            saveFileDialog.FileName = $"Reporte_{reporteActual}_{DateTime.Now:yyyyMMddHHmmssffff}.csv";

            if (saveFileDialog.ShowDialog() == true)
            {
                MainWindow.ShowLoader();
                try
                {
                    await Task.Run(() =>
                    {
                        // IMPORTANTE: Acceder a GridReportes desde otro hilo requiere Dispatcher
                        // Para simplificar y evitar cross-thread, tomamos los datos ANTES o usamos Dispatcher dentro
                        // Como DataTable es thread-safe para lectura, mejor tomamos la tabla base.
                        
                        DataTable dt = null;
                        this.Dispatcher.Invoke(() => 
                        { 
                            var view = GridReportes.ItemsSource as DataView;
                            if (view != null) dt = view.Table;
                        });

                        if (dt == null) return;

                        StringBuilder csv = new StringBuilder();
                        // Cabeceras
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            csv.Append(dt.Columns[i].ColumnName + (i < dt.Columns.Count - 1 ? "," : ""));
                        }
                        csv.AppendLine();

                        // Filas
                        foreach (DataRow row in dt.Rows)
                        {
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                string valor = row[i].ToString().Replace(",", "."); // Simple escape
                                csv.Append(valor + (i < dt.Columns.Count - 1 ? "," : ""));
                            }
                            csv.AppendLine();
                        }

                        File.WriteAllText(saveFileDialog.FileName, csv.ToString());
                    });

                    Mensaje.Mostrar("Exportado correctamente.", TipoMensaje.Exito);
                }
                catch (Exception ex) { Mensaje.Mostrar("Error: " + ex.Message, TipoMensaje.Error); }
                finally { MainWindow.HideLoader(); }
            }
        }

        private async void BtnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "PDF file (*.pdf)|*.pdf" };
            saveFileDialog.FileName = $"Reporte_{reporteActual}_{DateTime.Now:yyyyMMddHHmmssffff}.pdf";

            if (saveFileDialog.ShowDialog() == true)
            {
                MainWindow.ShowLoader();
                try
                {
                    // Capturar datos en UI Thread
                    DataTable dt = null;
                    var view = GridReportes.ItemsSource as DataView;
                    if (view != null) dt = view.Table;

                    if (dt != null)
                    {
                        await Task.Run(() =>
                        {
                            Document doc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10); // Horizontal
                            PdfWriter.GetInstance(doc, new FileStream(saveFileDialog.FileName, FileMode.Create));
                            doc.Open();

                            // Titulo
                            doc.Add(new Paragraph($"Reporte: {reporteActual}", FontFactory.GetFont("Arial", 16, Font.BOLD)));
                            doc.Add(new Paragraph($"Generado: {DateTime.Now}", FontFactory.GetFont("Arial", 10)));
                            doc.Add(new Paragraph(" "));

                            // Tabla
                            PdfPTable table = new PdfPTable(dt.Columns.Count);
                            table.WidthPercentage = 100;

                            // Cabeceras
                            foreach (DataColumn col in dt.Columns)
                            {
                                PdfPCell cell = new PdfPCell(new Phrase(col.ColumnName, FontFactory.GetFont("Arial", 10, Font.BOLD)));
                                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                                table.AddCell(cell);
                            }

                            // Datos
                            foreach (DataRow row in dt.Rows)
                            {
                                foreach (var item in row.ItemArray)
                                {
                                    table.AddCell(new Phrase(item.ToString(), FontFactory.GetFont("Arial", 8)));
                                }
                            }
                            doc.Add(table);
                            doc.Close();
                        });

                        Mensaje.Mostrar("PDF Generado.", TipoMensaje.Exito);
                    }
                }
                catch (Exception ex) { Mensaje.Mostrar("Error PDF: " + ex.Message, TipoMensaje.Error); }
                finally { MainWindow.HideLoader(); }
            }
        }

        #region EVENTOS GRILLA
        private void GridReportes_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            // Si la columna es numerica (decimal, double, float, int)
            Type type = e.PropertyType;

            if (type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
                type == typeof(int) || type == typeof(long))
            {
                var column = e.Column as DataGridTextColumn;
                if (column != null)
                {
                    // Aplicar formato de 2 decimales si es dinero o cantidad decimal
                    if (type != typeof(int) && type != typeof(long))
                    {
                        column.Binding.StringFormat = "{0:N2}";
                    }
                    else
                    {
                        column.Binding.StringFormat = "{0:N0}";
                    }

                    // Alinear a la derecha
                    Style style = new Style(typeof(TextBlock));
                    style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Right));
                    style.Setters.Add(new Setter(TextBlock.PaddingProperty, new Thickness(0, 0, 10, 0)));
                    style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
                    column.ElementStyle = style;
                }
            }
        }
        #endregion

        #region REIMPRIMIR TICKET
        private async void BtnReimprimirTicket_Click(object sender, RoutedEventArgs e)
        {
            // Obtener la fila seleccionada
            if (GridReportes.SelectedItem == null)
            {
                Mensaje.Mostrar("Seleccione una venta de la lista para reimprimir.", TipoMensaje.Advertencia);
                return;
            }

            // Obtener el número de factura de la fila seleccionada
            DataRowView row = GridReportes.SelectedItem as DataRowView;
            if (row == null) return;

            string noFactura = row["Nro Factura"]?.ToString();
            if (string.IsNullOrEmpty(noFactura))
            {
                Mensaje.Mostrar("No se pudo obtener el número de factura.", TipoMensaje.Error);
                return;
            }

            // Obtener cliente de la fila
            string clienteNombre = row["Cliente"]?.ToString() ?? "Consumidor Final";

            MainWindow.ShowLoader();

            try
            {
                DataTable dtCabecera = null;
                DataTable dtDetalle = null;

                // Obtener datos en background
                await Task.Run(() =>
                {
                    dtCabecera = cnReportes.ObtenerCabeceraVenta(noFactura);
                    dtDetalle = cnReportes.ObtenerDetalleVentaParaTicket(noFactura);
                });

                if (dtCabecera == null || dtCabecera.Rows.Count == 0)
                {
                    Mensaje.Mostrar("No se encontraron datos de la venta.", TipoMensaje.Error);
                    return;
                }

                if (dtDetalle == null || dtDetalle.Rows.Count == 0)
                {
                    Mensaje.Mostrar("No se encontró el detalle de la venta.", TipoMensaje.Error);
                    return;
                }

                // Datos de cabecera
                DataRow cabecera = dtCabecera.Rows[0];
                DateTime fechaVenta = Convert.ToDateTime(cabecera["Fecha_Venta"]);
                decimal montoTotal = Convert.ToDecimal(cabecera["Monto_Total"]);

                // Datos del cliente (ahora vienen de la consulta de cabecera)
                clienteNombre = cabecera["Cliente"]?.ToString() ?? "Consumidor Final";
                string clienteDocumento = cabecera["Documento"] != DBNull.Value && !string.IsNullOrWhiteSpace(cabecera["Documento"]?.ToString())
                    ? cabecera["Documento"].ToString() : "-";
                string clienteDomicilio = cabecera["Domicilio"] != DBNull.Value && !string.IsNullOrWhiteSpace(cabecera["Domicilio"]?.ToString())
                    ? cabecera["Domicilio"].ToString() : "-";

                // Convertir DataTable a List<ItemVenta>
                List<ItemVenta> items = new List<ItemVenta>();
                foreach (DataRow item in dtDetalle.Rows)
                {
                    items.Add(new ItemVenta
                    {
                        Codigo = item["Codigo"]?.ToString() ?? "",
                        Nombre = item["Nombre"]?.ToString() ?? "",
                        Cantidad = Convert.ToDecimal(item["Cantidad"]),
                        Precio = Convert.ToDecimal(item["Precio"]),
                        Total = Convert.ToDecimal(item["Total"])
                    });
                }

                // Usar el mismo método de POS para generar el presupuesto A4
                POS.GenerarPresupuestoPDF(noFactura, fechaVenta, clienteNombre, clienteDocumento, clienteDomicilio, items, montoTotal);
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error al reimprimir presupuesto: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
        }
        #endregion
    }
}