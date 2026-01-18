using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using Capa_Negocio;
using PuntoDeVenta.src.Boxes;
using System.Threading.Tasks;

namespace PuntoDeVenta.Views
{
    public partial class Dashboard : UserControl
    {
        private CN_Dashboard cnDashboard = new CN_Dashboard();
        public List<string> FechasLabels { get; set; }
        public Func<double, string> Formatter { get; set; }

        public Dashboard()
        {
            InitializeComponent();
            ConfigurarAccesos();
            this.Loaded += Dashboard_Loaded;
            DataContext = this;
        }

        private async void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private void ConfigurarAccesos()
        {
            // 1 = Admin, 2 = Vendedor (segun logica previa)
            int privilegio = Properties.Settings.Default.Privilegio;

            if (privilegio != 1)
            {
                // Ocultar opciones de admin
                btnAccessInventario.Visibility = Visibility.Collapsed;
                btnAccessReportes.Visibility = Visibility.Collapsed;
            }
        }

        private async Task CargarDatos()
        {
            MainWindow.ShowLoader();
            try
            {
                Dictionary<string, string> kpis = null;
                DataTable dtVentas = null;

                await Task.Run(() =>
                {
                    // 1. Cargar KPIs
                    kpis = cnDashboard.ObtenerKPIs();
                    // 2. Cargar Gráfico
                    dtVentas = cnDashboard.ObtenerVentasSemanales();
                });

                if (kpis != null)
                {
                    if (kpis.ContainsKey("TotalVendido")) lblVentasHoy.Text = kpis["TotalVendido"];
                    if (kpis.ContainsKey("CantidadVentas")) lblTransacciones.Text = kpis["CantidadVentas"];
                    if (kpis.ContainsKey("TotalProductos")) lblProductos.Text = kpis["TotalProductos"];
                    if (kpis.ContainsKey("StockBajo")) lblStockBajo.Text = kpis["StockBajo"];
                }

                if (dtVentas != null && dtVentas.Rows.Count > 0)
                {
                    var values = new ChartValues<decimal>();
                    FechasLabels = new List<string>();

                    foreach (DataRow row in dtVentas.Rows)
                    {
                        DateTime fecha = Convert.ToDateTime(row["Fecha"]);
                        FechasLabels.Add(fecha.ToString("dd/MM"));
                        values.Add(Convert.ToDecimal(row["Total"]));
                    }

                    chartVentas.Series = new SeriesCollection
                    {
                        new LineSeries
                        {
                            Title = "Ventas",
                            Values = values,
                            PointGeometry = DefaultGeometries.Circle,
                            PointGeometrySize = 10,
                            LineSmoothness = 0, 
                            StrokeThickness = 3,
                            Stroke = System.Windows.Media.Brushes.DodgerBlue,
                            Fill = System.Windows.Media.Brushes.Transparent
                        }
                    };

                    Formatter = value => value.ToString("C");
                    // Forzar actualizacion de bindings si es necesario, 
                    // pero al setear DataContext = this en ctor y properties notify (si hubiera) deberia andar.
                    // Aqui asignamos propiedades directas y Collections, deberia refrescar.
                    // Como FechasLabels es property, quizas necesite OnPropertyChanged si no es observable.
                    // Pero Chart de LiveCharts suele reaccionar a cambios en collections.
                    // Reasignar DataContext para refrescar FechasLabels si no es observablecollection
                    // o RaisePropertyChanged si implementaramos INotifyPropertyChanged.
                    // Por simplicidad, reasignamos chart axis labels si es posible o asumimos que LiveCharts lo toma.
                    // LiveCharts Axis Labels binding requires INPC usually.
                    // Hack rapido:
                    chartVentas.Update(true, true); 
                }
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error cargando dashboard: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
        }

        #region NAVEGACION
        private void BtnNuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            NavegarA("POS");
        }

        private void BtnMovimiento_Click(object sender, RoutedEventArgs e)
        {
            NavegarA("Inventario");
        }

        private void BtnReportes_Click(object sender, RoutedEventArgs e)
        {
            NavegarA("Reportes");
        }

        private void NavegarA(string destino)
        {
            // Obtener la ventana que contiene este UserControl
            Window parentWindow = Window.GetWindow(this);
            
            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.Navegar(destino);
            }
        }
        #endregion
    }
}
