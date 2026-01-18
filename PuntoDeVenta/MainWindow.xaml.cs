using PuntoDeVenta.src;
using PuntoDeVenta.src.Boxes;
using PuntoDeVenta.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PuntoDeVenta
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Iniciar en Dashboard
                this.DataContext = new Dashboard();
                this.lvInicio.IsSelected = true;

                string tema = Properties.Settings.Default.Tema;

                if (Properties.Settings.Default.Privilegio != 1)
                {
                    this.lvProductos.Visibility = Visibility.Hidden;
                    this.lvClientes.Visibility = Visibility.Hidden;
                    this.lvUsuarios.Visibility = Visibility.Hidden;
                    this.lvInventario.Visibility = Visibility.Hidden;
                    this.lvReportes.Visibility = Visibility.Hidden;
                    this.lvCancelaciones.Visibility = Visibility.Hidden;
                }

                this.CargarTema();
                this.ActualizarIndicadorTema();

            }
            catch (Exception ex)
            {
                Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
            }
        }

        #region LOADER GLOBAL
        public static void ShowLoader()
        {
            if (Application.Current.MainWindow is MainWindow win)
            {
                win.LoadingOverlay.Visibility = Visibility.Visible;
            }
        }

        public static void HideLoader()
        {
            if (Application.Current.MainWindow is MainWindow win)
            {
                win.LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        public static async Task RunAsync(Action action)
        {
            ShowLoader();
            try
            {
                await Task.Run(action);
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() => 
                    Mensaje.Mostrar("Error: " + ex.Message, TipoMensaje.Error)
                );
            }
            finally
            {
                HideLoader();
            }
        }
        #endregion

        #region NAVEGACION CENTRALIZADA
        
        public async void Navegar(string destino)
        {
            DeseleccionarMenu();
            ShowLoader();
            
            // Pequeño delay para permitir que el loader se renderice visualmente
            await Task.Delay(100); 

            try
            {
                switch (destino)
                {
                    case "Dashboard":
                        this.DataContext = new Dashboard();
                        lvInicio.IsSelected = true;
                        break;
                    case "POS":
                        this.DataContext = new POS();
                        lvPOS.IsSelected = true;
                        break;
                    case "Productos":
                        this.DataContext = new Productos();
                        lvProductos.IsSelected = true;
                        break;
                    case "Clientes":
                        this.DataContext = new Clientes();
                        lvClientes.IsSelected = true;
                        break;
                    case "Inventario":
                        this.DataContext = new MovimientosInventario();
                        lvInventario.IsSelected = true;
                        break;
                    case "Reportes":
                        this.DataContext = new Reportes();
                        lvReportes.IsSelected = true;
                        break;
                    case "Cancelaciones":
                        this.DataContext = new Cancelaciones();
                        lvCancelaciones.IsSelected = true;
                        break;
                    case "Usuarios":
                        this.DataContext = new Usuarios();
                        lvUsuarios.IsSelected = true;
                        break;
                }
            }
            catch(Exception ex)
            {
                Mensaje.Mostrar("Error de navegación: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                HideLoader();
            }
        }

        private void DeseleccionarMenu()
        {
            lvInicio.IsSelected = false;
            lvPOS.IsSelected = false;
            lvProductos.IsSelected = false;
            lvClientes.IsSelected = false;
            lvInventario.IsSelected = false;
            lvReportes.IsSelected = false;
            lvCancelaciones.IsSelected = false;
            lvUsuarios.IsSelected = false;
        }

        // Eventos del Menu
        private void Dashboard(object sender, RoutedEventArgs e) => Navegar("Dashboard");
        private void POS(object sender, RoutedEventArgs e) => Navegar("POS");
        private void Productos_Click(object sender, RoutedEventArgs e) => Navegar("Productos");
        private void Clientes_Click(object sender, RoutedEventArgs e) => Navegar("Clientes");
        private void Inventario_Click(object sender, RoutedEventArgs e) => Navegar("Inventario");
        private void Reportes_Click(object sender, RoutedEventArgs e) => Navegar("Reportes");
        private void Cancelaciones_Click(object sender, RoutedEventArgs e) => Navegar("Cancelaciones");
        private void Usuarios_click(object sender, RoutedEventArgs e) => Navegar("Usuarios");

        #endregion

        private void TBShow(object sender, RoutedEventArgs e)
        {
            this.GridContent.Opacity = 0.8;
        }

        private void TBHide(object sender, RoutedEventArgs e)
        {
            this.GridContent.Opacity = 1;
        }

        private void PreviewMouseLeftButtonDownBG(object sender, MouseButtonEventArgs e)
        {
            this.BtnShowHide.IsChecked = false;
        }

        private void Minimizar(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Cerrar(object sender, RoutedEventArgs e)
        {
            Login lg = new Login();
            lg.Show();
            this.Close();
        }

        private void MiCuenta(object sender, RoutedEventArgs e)
        {
            MiCuenta miCuenta = new MiCuenta();
            miCuenta.ShowDialog();
        }

        private void AcercaDe(object sender, RoutedEventArgs e)
        {
            AcercaDe acercaDe = new AcercaDe();
            acercaDe.ShowDialog();
        }

        #region MOVER VENTANA

        private void Mover(Border header)
        {
            var restaurar = false;

            header.MouseLeftButtonDown += (s, e) =>
            {
                if (e.ClickCount == 2)
                {
                    if ((ResizeMode == ResizeMode.CanResize) || (ResizeMode == ResizeMode.CanResizeWithGrip))
                    {
                        this.CambiarEstado();
                    }
                }
                else
                {
                    if (WindowState == WindowState.Maximized)
                    {
                        restaurar = true;
                    }
                    this.DragMove();
                }
            };

            header.MouseLeftButtonUp += (s, e) =>
            {
                restaurar = false;
            };

            header.MouseMove += (s, e) =>
            {
                if (restaurar)
                {
                    try
                    {
                        restaurar = false;
                        var mouseX = e.GetPosition(this).X;
                        var width = RestoreBounds.Width;
                        var x = mouseX - width / 2;

                        if (x < 0)
                        {
                            x = 0;
                        }
                        else if (x + width > SystemParameters.PrimaryScreenWidth)
                        {
                            x = SystemParameters.PrimaryScreenWidth - width;
                        }

                        this.WindowState = WindowState.Normal;
                        this.Left = x;
                        this.Top = 0;
                        this.DragMove();
                    }
                    catch (Exception ex)
                    {
                        Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
                    }
                }
            };
        }

        private void CambiarEstado()
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                    {
                        this.WindowState = WindowState.Maximized;
                        break;
                    }
                case WindowState.Maximized:
                    {
                        this.WindowState = WindowState.Normal;
                        break;
                    }
            }
        }

        private void RestaurarVentana(object sender, RoutedEventArgs e)
        {
            this.Mover(sender as Border);
        }
        #endregion

        #region SELECTOR DE TEMA
        private void TemaGreen_Click(object sender, RoutedEventArgs e)
        {
            CambiarTema("Green");
        }

        private void TemaDark_Click(object sender, RoutedEventArgs e)
        {
            CambiarTema("Dark");
        }

        private void TemaRed_Click(object sender, RoutedEventArgs e)
        {
            CambiarTema("Red");
        }

        private void CambiarTema(string tema)
        {
            Properties.Settings.Default.Tema = tema;
            Properties.Settings.Default.Save();
            this.CargarTema();
            this.ActualizarIndicadorTema();
        }

        public void CargarTema()
        {
            Temas temas = new Temas();
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(temas.CargarTema());
        }

        private void ActualizarIndicadorTema()
        {
            string tema = Properties.Settings.Default.Tema ?? "Green";

            // Actualizar opacidad de los botones para indicar selección
            BtnTemaGreen.Opacity = tema == "Green" ? 1.0 : 0.6;
            BtnTemaDark.Opacity = tema == "Dark" ? 1.0 : 0.6;
            BtnTemaRed.Opacity = tema == "Red" ? 1.0 : 0.6;
        }
        #endregion
    }
}