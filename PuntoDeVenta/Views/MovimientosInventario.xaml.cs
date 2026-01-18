using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Capa_Negocio;
using Capa_Entidad;
using LiveCharts;
using LiveCharts.Wpf;
using System.IO;
using Microsoft.Win32;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PuntoDeVenta.src.Boxes;

namespace PuntoDeVenta.Views
{
        public partial class MovimientosInventario : UserControl
        {
            // Se elimina la instancia global para evitar conflictos de conexión (NpgsqlConnection no es thread-safe)
            // CN_Inventario cnInventario = new CN_Inventario(); 
            
            private bool _cargando = false;
            public List<string> TopProductosLabels { get; set; }
    
            public MovimientosInventario()
            {
                InitializeComponent();
                dpDesde.SelectedDate = DateTime.Now.AddDays(-7);
                dpHasta.SelectedDate = DateTime.Now;
                
                chartTopProductos.Series = new SeriesCollection();
                chartResumen.Series = new SeriesCollection();
    
                this.Loaded += MovimientosInventario_Loaded;
            }
    
            private async void MovimientosInventario_Loaded(object sender, RoutedEventArgs e)
            {
                await CargarTodoAsync();
            }
    
            private async Task CargarTodoAsync()
            {
                if (_cargando) return; // Evitar re-entrancia
                _cargando = true;
    
                MainWindow.ShowLoader();
                // Pequeña pausa para asegurar que el loader se renderice antes de bloquear/cargar
                await Task.Delay(100); 
    
                try
                {
                    // Ejecución secuencial para evitar "Connection already in state 'Connecting'"
                    await CargarDatosAsync();
                    await CargarDashboardAsync();
                }
                catch(Exception ex)
                {
                    Mensaje.Mostrar("Error cargando inventario: " + ex.Message, TipoMensaje.Error);
                }
                finally
                {
                    MainWindow.HideLoader();
                    _cargando = false;
                }
            }
    
            private async Task CargarDatosAsync()
            {
                string buscar = tbBuscar.Text;
                DateTime? desde = dpDesde.SelectedDate;
                DateTime? hasta = dpHasta.SelectedDate;
                if(hasta.HasValue) hasta = hasta.Value.AddDays(1).AddSeconds(-1);
    
                List<CE_Movimiento> lista = null;
                
                // Instancia local para asegurar conexión limpia
                await Task.Run(() => 
                {
                    CN_Inventario cn = new CN_Inventario();
                    lista = cn.ListarMovimientos(buscar, desde, hasta);
                });
                
                GridMovimientos.ItemsSource = lista;
            }
    
            private async Task CargarDashboardAsync()
            {
                DataSet ds = null;
                
                // Instancia local para asegurar conexión limpia
                await Task.Run(() => 
                {
                    CN_Inventario cn = new CN_Inventario();
                    ds = cn.ObtenerDatosDashboard();
                });
    
                // UI Update
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    var values = new ChartValues<decimal>();
                    var labels = new List<string>();
    
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        labels.Add(row["Nombre"].ToString());
                        values.Add(Convert.ToDecimal(row["TotalMovido"]));
                    }
    
                    TopProductosLabels = labels;
                    chartTopProductos.Series.Clear();
                    chartTopProductos.Series.Add(new ColumnSeries { Title = "Unidades", Values = values, DataLabels = true });
                    chartTopProductos.AxisX[0].Labels = TopProductosLabels;
                }
    
                if (ds != null && ds.Tables.Count > 1 && ds.Tables[1].Rows.Count > 0)
                {
                    DataRow row = ds.Tables[1].Rows[0];
                    decimal entradas = row["EntradasHoy"] != DBNull.Value ? Convert.ToDecimal(row["EntradasHoy"]) : 0;
                    decimal salidas = row["SalidasHoy"] != DBNull.Value ? Convert.ToDecimal(row["SalidasHoy"]) : 0;
    
                    chartResumen.Series.Clear();
                    chartResumen.Series.Add(new PieSeries { Title = "Entradas", Values = new ChartValues<decimal> { entradas }, DataLabels = true, Fill = System.Windows.Media.Brushes.Green });
                    chartResumen.Series.Add(new PieSeries { Title = "Salidas", Values = new ChartValues<decimal> { salidas }, DataLabels = true, Fill = System.Windows.Media.Brushes.Red });
                }
            }
        // Eventos sincrónicos wrappers
        private async void Buscando(object sender, TextChangedEventArgs e) { await CargarTodoAsync(); }
        private async void Filtro_Changed(object sender, SelectionChangedEventArgs e) { await CargarTodoAsync(); }

        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                RegistrarMovimiento modal = new RegistrarMovimiento();
                modal.Unloaded += async (s, args) => 
                {
                     await CargarTodoAsync();
                     FrameModal.Content = null;
                };
                FrameModal.NavigationService.Navigate(modal);
            }
            catch(Exception ex) { Mensaje.Mostrar(ex.Message, TipoMensaje.Error); }
        }

                private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
                {
                    // Reutilizamos logica previa
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
                    saveFileDialog.FileName = "Movimientos_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".csv";
                    
                    if (saveFileDialog.ShowDialog() == true)
                    {                try
                {
                    StringBuilder csv = new StringBuilder();
                    csv.AppendLine("Id,Fecha,Tipo,Producto,Codigo,Cantidad,Usuario,Observacion");
                    var lista = GridMovimientos.ItemsSource as List<CE_Movimiento>;
                    if(lista != null)
                    {
                        foreach (var item in lista)
                        {
                            csv.AppendLine($"{item.IdMovimiento},{item.FechaMovimiento},{item.TipoMovimiento},\"{item.NombreProducto}\",{item.CodigoProducto},{item.Cantidad},\"{item.UsuarioResponsable}\",\"{item.Observacion}\"");
                        }
                        File.WriteAllText(saveFileDialog.FileName, csv.ToString());
                        Mensaje.Mostrar("Exportado correctamente.", TipoMensaje.Exito);
                    }
                }
                catch(Exception ex) { Mensaje.Mostrar("Error: " + ex.Message, TipoMensaje.Error); }
            }
        }

        private void BtnReportePDF_Click(object sender, RoutedEventArgs e)
        {
            // Misma lógica previa
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "PDF file (*.pdf)|*.pdf";
            saveFileDialog.FileName = "ReporteInventario_" + DateTime.Now.ToString("yyyyMMddHHmmssffff") + ".pdf";

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Crear Documento
                    Document doc = new Document(PageSize.A4, 25, 25, 30, 30);
                    PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(saveFileDialog.FileName, FileMode.Create));
                    doc.Open();
                    // ... (resto del PDF igual que antes)
                    // Titulo
                    iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("Reporte de Movimientos de Inventario", FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD));
                    title.Alignment = Element.ALIGN_CENTER;
                    doc.Add(title);
                    doc.Add(new iTextSharp.text.Paragraph($"Generado el: {DateTime.Now.ToString("dd/MM/yyyy HH:mm")}", FontFactory.GetFont("Arial", 10)));
                    doc.Add(new iTextSharp.text.Paragraph(" ")); 

                    // Tabla
                    PdfPTable table = new PdfPTable(7);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 15f, 10f, 25f, 10f, 10f, 15f, 15f });

                    string[] headers = { "Fecha", "Tipo", "Producto", "Cód", "Cant", "Usuario", "Obs" };
                    foreach (string header in headers)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont("Arial", 10, iTextSharp.text.Font.BOLD)));
                        cell.BackgroundColor = new BaseColor(240, 240, 240);
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        table.AddCell(cell);
                    }

                    var lista = GridMovimientos.ItemsSource as List<CE_Movimiento>;
                    if (lista != null)
                    {
                        foreach (var item in lista)
                        {
                            table.AddCell(new Phrase(item.FechaMovimiento.ToString("dd/MM/yy HH:mm"), FontFactory.GetFont("Arial", 8)));
                            table.AddCell(new Phrase(item.TipoMovimiento, FontFactory.GetFont("Arial", 8)));
                            table.AddCell(new Phrase(item.NombreProducto, FontFactory.GetFont("Arial", 8)));
                            table.AddCell(new Phrase(item.CodigoProducto, FontFactory.GetFont("Arial", 8)));
                            table.AddCell(new Phrase(item.Cantidad.ToString(), FontFactory.GetFont("Arial", 8)));
                            table.AddCell(new Phrase(item.UsuarioResponsable, FontFactory.GetFont("Arial", 8)));
                            table.AddCell(new Phrase(item.Observacion, FontFactory.GetFont("Arial", 8)));
                        }
                    }
                    doc.Add(table);
                    doc.Close();
                    Mensaje.Mostrar("Reporte PDF generado correctamente.", TipoMensaje.Exito);
                }
                catch (Exception ex)
                {
                    Mensaje.Mostrar("Error al generar PDF: " + ex.Message, TipoMensaje.Error);
                }
            }
        }
    }
}