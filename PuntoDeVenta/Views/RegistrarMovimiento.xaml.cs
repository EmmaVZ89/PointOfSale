using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Capa_Entidad;
using Capa_Negocio;
using PuntoDeVenta.src.Boxes;

namespace PuntoDeVenta.Views
{
    public partial class RegistrarMovimiento : Page
    {
        CN_Productos cnProductos = new CN_Productos();
        CN_Inventario cnInventario = new CN_Inventario();

        public RegistrarMovimiento()
        {
            InitializeComponent();
            
            // Foco inicial y carga
            this.Loaded += RegistrarMovimiento_Loaded;

            // Evento global de teclas
            this.KeyDown += new KeyEventHandler(Page_KeyDown);
        }

        private async void RegistrarMovimiento_Loaded(object sender, RoutedEventArgs e)
        {
            cbProductos.Focus();
            await CargarProductos();
        }

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

                if (dt != null)
                {
                    cbProductos.ItemsSource = dt.DefaultView;
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

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9,]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        #region EVENTOS DE CIERRE (OVERLAY)
        
        // Al hacer click en el fondo oscuro, cerrar
        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CerrarModal();
        }

        // Al hacer click en la tarjeta, detener propagación para que no cierre
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void CerrarModal()
        {
            try
            {
                // 1. Intentar limpiar vía NavigationService (método estándar)
                if (this.NavigationService != null)
                {
                    this.NavigationService.Content = null;
                    return;
                }

                // 2. Si falla, buscar visualmente el Frame padre
                DependencyObject parent = System.Windows.Media.VisualTreeHelper.GetParent(this);
                while (parent != null && !(parent is Frame))
                {
                    parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                }

                if (parent is Frame frame)
                {
                    frame.Content = null;
                }
            }
            catch
            {
                // Si todo falla, intentar ocultar la página (último recurso)
                this.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Guardar();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            CerrarModal();
        }

        private async void Guardar()
        {
            if (cbProductos.SelectedValue == null)
            {
                Mensaje.Mostrar("Seleccione un producto.", TipoMensaje.Advertencia);
                return;
            }

            if (string.IsNullOrWhiteSpace(tbCantidad.Text) || !decimal.TryParse(tbCantidad.Text, out decimal cant) || cant <= 0)
            {
                Mensaje.Mostrar("Ingrese una cantidad válida.", TipoMensaje.Advertencia);
                return;
            }

            try
            {
                CE_Movimiento mov = new CE_Movimiento();
                mov.IdArticulo = Convert.ToInt32(cbProductos.SelectedValue);
                // ID usuario logueado
                mov.IdUsuario = Properties.Settings.Default.IdUsuario;
                
                mov.TipoMovimiento = (rbEntrada.IsChecked == true) ? "ENTRADA" : "SALIDA";
                mov.Cantidad = Convert.ToDecimal(tbCantidad.Text);
                mov.Observacion = tbObservacion.Text;

                MainWindow.ShowLoader();
                await Task.Run(() =>
                {
                    cnInventario.RegistrarMovimiento(mov);
                });
                MainWindow.HideLoader();

                Mensaje.Mostrar("Movimiento registrado correctamente.", TipoMensaje.Exito);
                
                // Cerrar automáticamente tras éxito
                CerrarModal();
            }
            catch (Exception ex)
            {
                MainWindow.HideLoader();
                Mensaje.Mostrar("Error al registrar: " + ex.Message, TipoMensaje.Error);
            }
        }

        private void Page_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CerrarModal();
            }
            else if (e.Key == Key.F1)
            {
                Guardar();
            }
            else if (e.Key == Key.Enter)
            {
                // Mover foco al siguiente elemento
                TraversalRequest request = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement elementWithFocus = Keyboard.FocusedElement as UIElement;
                if (elementWithFocus != null)
                {
                    elementWithFocus.MoveFocus(request);
                }
            }
        }
    }
}