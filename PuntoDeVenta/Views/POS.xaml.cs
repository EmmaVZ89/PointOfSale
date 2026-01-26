using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Capa_Negocio;
using Capa_Entidad;
using PuntoDeVenta.src.Boxes;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.pipeline.html;
using System.IO;
using System.Diagnostics;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;

namespace PuntoDeVenta.Views
{
    public partial class POS : UserControl
    {
        private CN_Productos cnProductos = new CN_Productos();
        private CN_Carrito cnCarrito = new CN_Carrito();
        private CN_Clientes cnClientes = new CN_Clientes();

        private List<ItemVenta> carritoItems = new List<ItemVenta>();
        private DataRowView productoSeleccionado;
        private decimal stockDisponible = 0;

        public POS()
        {
            InitializeComponent();
            this.Loaded += POS_Loaded;
        }

        private async void POS_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarProductos();
            await CargarClientes();
        }

        #region CARGA INICIAL
        private async Task CargarProductos()
        {
            MainWindow.ShowLoader();
            try
            {
                DataTable dt = null;
                await Task.Run(() =>
                {
                    dt = cnProductos.BuscarProducto("");
                });

                if (dt != null && dt.Rows.Count > 0)
                {
                    DataView dv = dt.DefaultView;
                    dv.RowFilter = "Activo = true AND Cantidad > 0";
                    cbProductos.ItemsSource = dv;
                }
                else
                {
                    cbProductos.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error cargando productos: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
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

                if (clientes != null && clientes.Count > 0)
                {
                    cbClientes.ItemsSource = clientes;
                    cbClientes.SelectedIndex = 0; // Consumidor Final preseleccionado
                }
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error cargando clientes: " + ex.Message, TipoMensaje.Error);
            }
        }
        #endregion

        #region SELECCION PRODUCTO
        private void CbProductos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbProductos.SelectedItem != null)
            {
                productoSeleccionado = (DataRowView)cbProductos.SelectedItem;
                tbPrecio.Text = productoSeleccionado["Precio"].ToString();
                stockDisponible = Convert.ToDecimal(productoSeleccionado["Cantidad"]);
            }
            else
            {
                productoSeleccionado = null;
                tbPrecio.Text = "";
                stockDisponible = 0;
            }
        }

        private void CbProductos_DropDownClosed(object sender, EventArgs e)
        {
            if (cbProductos.SelectedItem != null)
            {
                tbCantidad.SelectAll();
                tbCantidad.Focus();
            }
        }

        private void CbProductos_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && cbProductos.SelectedItem != null)
            {
                tbCantidad.SelectAll();
                tbCantidad.Focus();
            }
        }
        #endregion

        #region AGREGAR AL CARRITO
        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            AgregarAlCarrito();
        }

        private void TbCantidad_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AgregarAlCarrito();
            }
        }

        private void AgregarAlCarrito()
        {
            if (productoSeleccionado == null)
            {
                Mensaje.Mostrar("Seleccione un producto.", TipoMensaje.Advertencia);
                return;
            }

            if (!decimal.TryParse(tbCantidad.Text, out decimal cantidad) || cantidad <= 0)
            {
                Mensaje.Mostrar("Ingrese una cantidad válida.", TipoMensaje.Advertencia);
                return;
            }

            if (!decimal.TryParse(tbPrecio.Text, out decimal precio) || precio < 0)
            {
                Mensaje.Mostrar("El precio no es válido.", TipoMensaje.Advertencia);
                return;
            }

            var itemExistente = carritoItems.FirstOrDefault(x => x.IdArticulo == (int)productoSeleccionado["IdArticulo"]);
            decimal cantidadEnCarrito = itemExistente != null ? itemExistente.Cantidad : 0;

            if (cantidad + cantidadEnCarrito > stockDisponible)
            {
                Mensaje.Mostrar($"Stock insuficiente. Disponible: {stockDisponible}. En carrito: {cantidadEnCarrito}.", TipoMensaje.Advertencia);
                return;
            }

            if (itemExistente != null)
            {
                itemExistente.Cantidad += cantidad;
                itemExistente.Precio = precio;
                itemExistente.Total = itemExistente.Cantidad * itemExistente.Precio;
            }
            else
            {
                carritoItems.Add(new ItemVenta
                {
                    IdArticulo = (int)productoSeleccionado["IdArticulo"],
                    Codigo = productoSeleccionado["Codigo"].ToString(),
                    Nombre = productoSeleccionado["Nombre"].ToString(),
                    Precio = precio,
                    Cantidad = cantidad,
                    Total = cantidad * precio
                });
            }

            RefrescarGrid();
            tbCantidad.Text = "1";
            cbProductos.SelectedItem = null;
            cbProductos.Focus();
        }
        #endregion

        #region GESTION GRILLA
        private void RefrescarGrid()
        {
            GridProductos.ItemsSource = null;
            GridProductos.ItemsSource = carritoItems;
            CalcularTotales();
        }

        private void BtnEliminarFila_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as FrameworkElement).DataContext as ItemVenta;
            if (item != null)
            {
                carritoItems.Remove(item);
                RefrescarGrid();
            }
        }

        private void BtnAnular_Click(object sender, RoutedEventArgs e)
        {
            if(Mensaje.Confirmar("¿Seguro de vaciar el carrito?"))
            {
                carritoItems.Clear();
                RefrescarGrid();
                tbPagaCon.Text = "";
            }
        }
        #endregion

        #region CALCULOS Y PAGO
        private void CalcularTotales()
        {
            decimal total = carritoItems.Sum(x => x.Total);
            lblSubtotal.Text = total.ToString("C");
            lblTotal.Text = total.ToString("C");
            CalcularVuelto();
        }

        private void TbPagaCon_TextChanged(object sender, TextChangedEventArgs e)
        {
            CalcularVuelto();
        }

        private void CalcularVuelto()
        {
            decimal total = carritoItems.Sum(x => x.Total);
            if (decimal.TryParse(tbPagaCon.Text, out decimal pago))
            {
                decimal vuelto = pago - total;
                lblVuelto.Text = vuelto.ToString("C");
                if (vuelto < 0) lblVuelto.Foreground = System.Windows.Media.Brushes.Red;
                else lblVuelto.Foreground = System.Windows.Media.Brushes.Green;
            }
            else
            {
                lblVuelto.Text = "$0.00";
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        #endregion

        #region COBRAR
        private async void BtnCobrar_Click(object sender, RoutedEventArgs e)
        {
            if (carritoItems.Count == 0)
            {
                Mensaje.Mostrar("No hay productos en la venta.", TipoMensaje.Advertencia);
                return;
            }

            decimal total = carritoItems.Sum(x => x.Total);
            decimal pago = 0;
            decimal.TryParse(tbPagaCon.Text, out pago);

            if (pago < total)
            {
                Mensaje.Mostrar("El pago es insuficiente.", TipoMensaje.Advertencia);
                return;
            }

            MainWindow.ShowLoader();
            try
            {
                string nroFactura = "F-" + DateTime.Now.ToString("yyMMddHHmm");
                int idUsuario = Properties.Settings.Default.IdUsuario;
                DateTime fecha = DateTime.Now;

                // Obtener cliente seleccionado (default: 1 = Consumidor Final)
                int idCliente = 1;
                if (cbClientes.SelectedItem is CE_Clientes clienteSeleccionado)
                {
                    idCliente = clienteSeleccionado.IdCliente;
                }

                // Capturar datos necesarios para el thread seguro
                var itemsParaVenta = carritoItems.ToList();

                await Task.Run(() =>
                {
                    cnCarrito.Venta(nroFactura, total, fecha, idUsuario, idCliente);

                    foreach (var item in itemsParaVenta)
                    {
                        cnCarrito.Venta_Detalle(item.Codigo, item.Cantidad, nroFactura, item.Total);
                    }
                });

                ImprimirTicket(nroFactura, pago, pago - total);

                carritoItems.Clear();
                RefrescarGrid();
                tbPagaCon.Text = "";
                cbClientes.SelectedIndex = 0; // Resetear a Consumidor Final
                await CargarProductos(); // Recargar productos para actualizar stock

                Mensaje.Mostrar("Venta registrada correctamente.", TipoMensaje.Exito);
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error al procesar venta: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
        }

        private void ImprimirTicket(string factura, decimal efectivo, decimal cambio)
        {
            try
            {
                // Obtener datos del cliente seleccionado
                string clienteNombre = "Consumidor Final";
                string clienteDocumento = "-";
                string clienteDomicilio = "-";
                if (cbClientes.SelectedItem is CE_Clientes cliente)
                {
                    clienteNombre = !string.IsNullOrEmpty(cliente.RazonSocial) ? cliente.RazonSocial : "Consumidor Final";
                    clienteDocumento = !string.IsNullOrEmpty(cliente.Documento) ? cliente.Documento : "-";
                    clienteDomicilio = !string.IsNullOrEmpty(cliente.Domicilio) ? cliente.Domicilio : "-";
                }

                // Generar PDF usando el nuevo formato Presupuesto A4
                GenerarPresupuestoPDF(factura, DateTime.Now, clienteNombre, clienteDocumento, clienteDomicilio,
                    carritoItems.ToList(), carritoItems.Sum(x => x.Total));
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error generando presupuesto PDF: " + ex.Message, TipoMensaje.Error);
            }
        }
        #endregion

        #region GENERACION PDF PRESUPUESTO
        /// <summary>
        /// Genera el PDF del presupuesto en formato A4 con opciones de imprimir o guardar
        /// </summary>
        public static void GenerarPresupuestoPDF(string nroPresupuesto, DateTime fecha,
            string clienteNombre, string clienteDocumento, string clienteDomicilio, List<ItemVenta> items, decimal total,
            string rutaArchivo = null)
        {
            // Mostrar diálogo de opciones si no se especifica ruta
            AccionPresupuesto accion = AccionPresupuesto.Guardar;
            if (string.IsNullOrEmpty(rutaArchivo))
            {
                var dialogo = new DialogoPresupuesto(nroPresupuesto, total);
                if (dialogo.ShowDialog() != true)
                    return;
                accion = dialogo.Accion;
            }

            // Generar PDF en archivo temporal
            string pdfTempPath = Path.Combine(Path.GetTempPath(), $"Presupuesto_{nroPresupuesto}_{Guid.NewGuid()}.pdf");
            string logoTempPath = null;

            try
            {
                // Guardar logo a archivo temporal
                try { logoTempPath = GuardarLogoTemporal(); } catch { }

                // Generar contenido HTML
                string pagina = GenerarContenidoHTML(nroPresupuesto, fecha, clienteNombre,
                    clienteDocumento, clienteDomicilio, items, total, logoTempPath);

                // Crear PDF
                CrearArchivoPDF(pdfTempPath, pagina);

                // Ejecutar acción según elección del usuario
                switch (accion)
                {
                    case AccionPresupuesto.Imprimir:
                        ImprimirPDF(pdfTempPath);
                        break;

                    case AccionPresupuesto.Guardar:
                        GuardarPDF(pdfTempPath, nroPresupuesto);
                        break;
                }
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error al generar presupuesto: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                // Limpiar archivos temporales (el PDF se limpia después de imprimir/guardar)
                if (!string.IsNullOrEmpty(logoTempPath) && File.Exists(logoTempPath))
                {
                    try { File.Delete(logoTempPath); } catch { }
                }
            }
        }

        /// <summary>
        /// Genera el contenido HTML del presupuesto
        /// </summary>
        private static string GenerarContenidoHTML(string nroPresupuesto, DateTime fecha,
            string clienteNombre, string clienteDocumento, string clienteDomicilio, List<ItemVenta> items,
            decimal total, string logoTempPath)
        {
            string pagina = Properties.Resources.Presupuesto.ToString();

            // Logo
            string logoHtml = !string.IsNullOrEmpty(logoTempPath)
                ? $"<img src=\"file:///{logoTempPath.Replace("\\", "/")}\" width=\"80\" />"
                : "";
            pagina = pagina.Replace("@LOGO_BASE64", logoHtml);

            // Datos del documento
            pagina = pagina.Replace("@NRO_PRESUPUESTO", nroPresupuesto);
            pagina = pagina.Replace("@FECHA", fecha.ToString("dd/MM/yyyy HH:mm"));
            pagina = pagina.Replace("@CLIENTE_NOMBRE", clienteNombre);
            pagina = pagina.Replace("@CLIENTE_DOCUMENTO", clienteDocumento);
            pagina = pagina.Replace("@CLIENTE_DOMICILIO", clienteDomicilio);

            // Generar filas de productos
            string filas = "";
            bool alternar = false;
            foreach (var item in items)
            {
                string bgColor = alternar ? "#F9F9F9" : "#FFFFFF";
                filas += $"<tr style=\"background-color: {bgColor};\">";
                filas += $"<td style=\"border: 1px solid #DDD; padding: 8px 10px; text-align: left; font-size: 9px;\">{item.Nombre}</td>";
                filas += $"<td style=\"border: 1px solid #DDD; padding: 8px; text-align: center; font-size: 9px;\">{item.Cantidad}</td>";
                filas += $"<td style=\"border: 1px solid #DDD; padding: 8px 10px; text-align: right; font-size: 9px;\">{item.Precio:C}</td>";
                filas += $"<td style=\"border: 1px solid #DDD; padding: 8px 10px; text-align: right; font-size: 9px;\">{item.Total:C}</td>";
                filas += "</tr>";
                alternar = !alternar;
            }

            // Filas vacías para completar mínimo visual
            int filasVacias = Math.Max(0, 6 - items.Count);
            for (int i = 0; i < filasVacias; i++)
            {
                string bgColor = alternar ? "#F9F9F9" : "#FFFFFF";
                filas += $"<tr style=\"background-color: {bgColor};\">";
                filas += "<td style=\"border: 1px solid #DDD; padding: 8px;\">&nbsp;</td>";
                filas += "<td style=\"border: 1px solid #DDD; padding: 8px;\"></td>";
                filas += "<td style=\"border: 1px solid #DDD; padding: 8px;\"></td>";
                filas += "<td style=\"border: 1px solid #DDD; padding: 8px;\"></td>";
                filas += "</tr>";
                alternar = !alternar;
            }

            pagina = pagina.Replace("@PRODUCTOS_GRID", filas);
            pagina = pagina.Replace("@TOTAL", total.ToString("C"));

            return pagina;
        }

        /// <summary>
        /// Crea el archivo PDF a partir del contenido HTML
        /// </summary>
        private static void CrearArchivoPDF(string rutaArchivo, string contenidoHTML)
        {
            using (FileStream stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                Document pdfDoc = new Document(PageSize.A4, 30, 30, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();

                using (StringReader sr = new StringReader(contenidoHTML))
                {
                    XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                }
                pdfDoc.Close();
            }
        }

        /// <summary>
        /// Abre el PDF para imprimir
        /// </summary>
        private static void ImprimirPDF(string rutaPDF)
        {
            try
            {
                // Copiar a ubicación temporal accesible
                string tempPrint = Path.Combine(Path.GetTempPath(), "presupuesto_imprimir.pdf");
                File.Copy(rutaPDF, tempPrint, true);

                // Abrir el PDF con el visor predeterminado
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPrint,
                    UseShellExecute = true
                });

                // Mostrar instrucción al usuario
                Mensaje.Mostrar("Se abrió el presupuesto.\n\nPresione Ctrl+P para imprimir\no use el menú Archivo > Imprimir", TipoMensaje.Info);

                // Eliminar el PDF temporal original
                try { File.Delete(rutaPDF); } catch { }
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error al abrir documento: " + ex.Message, TipoMensaje.Error);
            }
        }

        /// <summary>
        /// Muestra el diálogo para guardar el PDF
        /// </summary>
        private static void GuardarPDF(string pdfTempPath, string nroPresupuesto)
        {
            SaveFileDialog saveFile = new SaveFileDialog
            {
                Filter = "PDF file (*.pdf)|*.pdf",
                FileName = $"Presupuesto_{nroPresupuesto}_{DateTime.Now:yyyyMMddHHmmss}.pdf"
            };

            if (saveFile.ShowDialog() == true)
            {
                File.Copy(pdfTempPath, saveFile.FileName, true);
                Mensaje.Mostrar("Presupuesto guardado correctamente.", TipoMensaje.Exito);
            }

            // Eliminar archivo temporal
            try { File.Delete(pdfTempPath); } catch { }
        }

        /// <summary>
        /// Guarda el logo a un archivo temporal y devuelve la ruta
        /// </summary>
        private static string GuardarLogoTemporal()
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), $"logo_presupuesto_{Guid.NewGuid()}.png");
                using (Bitmap logo = Properties.Resources.LogoPresupuesto)
                {
                    logo.Save(tempPath, ImageFormat.Png);
                }
                return tempPath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Convierte un número a su representación en letras (español)
        /// </summary>
        public static string NumeroALetras(decimal numero)
        {
            string[] unidades = { "", "UN", "DOS", "TRES", "CUATRO", "CINCO", "SEIS", "SIETE", "OCHO", "NUEVE" };
            string[] decenas = { "", "DIEZ", "VEINTE", "TREINTA", "CUARENTA", "CINCUENTA", "SESENTA", "SETENTA", "OCHENTA", "NOVENTA" };
            string[] especiales = { "DIEZ", "ONCE", "DOCE", "TRECE", "CATORCE", "QUINCE", "DIECISEIS", "DIECISIETE", "DIECIOCHO", "DIECINUEVE" };
            string[] centenas = { "", "CIENTO", "DOSCIENTOS", "TRESCIENTOS", "CUATROCIENTOS", "QUINIENTOS", "SEISCIENTOS", "SETECIENTOS", "OCHOCIENTOS", "NOVECIENTOS" };

            long entero = (long)Math.Floor(numero);
            int centavos = (int)((numero - entero) * 100);

            if (entero == 0) return $"CERO PESOS CON {centavos:00}/100";

            string resultado = "";

            // Millones
            if (entero >= 1000000)
            {
                long millones = entero / 1000000;
                if (millones == 1)
                    resultado += "UN MILLON ";
                else
                    resultado += ConvertirGrupo((int)millones, unidades, decenas, especiales, centenas) + " MILLONES ";
                entero %= 1000000;
            }

            // Miles
            if (entero >= 1000)
            {
                long miles = entero / 1000;
                if (miles == 1)
                    resultado += "MIL ";
                else
                    resultado += ConvertirGrupo((int)miles, unidades, decenas, especiales, centenas) + " MIL ";
                entero %= 1000;
            }

            // Cientos/Decenas/Unidades
            if (entero > 0)
            {
                if (entero == 100)
                    resultado += "CIEN ";
                else
                    resultado += ConvertirGrupo((int)entero, unidades, decenas, especiales, centenas) + " ";
            }

            resultado = resultado.Trim() + $" PESOS CON {centavos:00}/100";
            return resultado;
        }

        private static string ConvertirGrupo(int numero, string[] unidades, string[] decenas, string[] especiales, string[] centenas)
        {
            string resultado = "";

            int c = numero / 100;
            int d = (numero % 100) / 10;
            int u = numero % 10;

            if (c > 0)
            {
                if (numero == 100)
                    return "CIEN";
                resultado += centenas[c] + " ";
            }

            if (d == 1)
            {
                resultado += especiales[u];
            }
            else if (d == 2 && u > 0)
            {
                resultado += "VEINTI" + unidades[u].ToLower();
            }
            else
            {
                if (d > 0)
                {
                    resultado += decenas[d];
                    if (u > 0) resultado += " Y ";
                }
                if (u > 0 || (d == 0 && c == 0))
                {
                    resultado += unidades[u];
                }
            }

            return resultado.Trim();
        }
        #endregion
    }

    // Clase auxiliar para el DataGrid
    public class ItemVenta
    {
        public int IdArticulo { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Total { get; set; }
    }
}
