using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Capa_Negocio;
using PuntoDeVenta.src.Boxes;

namespace PuntoDeVenta.Views
{
    public partial class Cancelaciones : UserControl
    {
        private readonly CN_Cancelaciones cnCancelaciones = new CN_Cancelaciones();
        private int ventaSeleccionadaId = 0;

        public Cancelaciones()
        {
            InitializeComponent();
            this.Loaded += Cancelaciones_Loaded;
        }

        private async void Cancelaciones_Loaded(object sender, RoutedEventArgs e)
        {
            await BuscarVentasAsync("");
        }

        #region BUSCAR VENTAS
        private async Task BuscarVentasAsync(string buscar)
        {
            MainWindow.ShowLoader();
            try
            {
                DataTable dt = null;
                await Task.Run(() =>
                {
                    dt = cnCancelaciones.BuscarVentas(buscar);
                });

                GridVentas.ItemsSource = dt?.DefaultView;
                LimpiarDetalle();
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error buscando ventas: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
        }

        private void TbBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                _ = BuscarVentasAsync(tbBuscar.Text);
            }
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            _ = BuscarVentasAsync(tbBuscar.Text);
        }
        #endregion

        #region SELECCION DE VENTA
        private async void GridVentas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridVentas.SelectedItem is DataRowView row)
            {
                ventaSeleccionadaId = Convert.ToInt32(row["Id_Venta"]);
                await CargarDetalleVenta(ventaSeleccionadaId);
                BtnCancelarVenta.IsEnabled = true;
            }
            else
            {
                LimpiarDetalle();
            }
        }

        private async Task CargarDetalleVenta(int idVenta)
        {
            try
            {
                DataTable dt = null;
                await Task.Run(() =>
                {
                    dt = cnCancelaciones.ObtenerDetalleVenta(idVenta);
                });

                GridDetalle.ItemsSource = dt?.DefaultView;
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error cargando detalle: " + ex.Message, TipoMensaje.Error);
            }
        }

        private void LimpiarDetalle()
        {
            ventaSeleccionadaId = 0;
            GridDetalle.ItemsSource = null;
            tbMotivo.Text = "";
            BtnCancelarVenta.IsEnabled = false;
        }
        #endregion

        #region CANCELAR VENTA
        private async void BtnCancelarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (ventaSeleccionadaId <= 0)
            {
                Mensaje.Mostrar("Seleccione una venta para cancelar.", TipoMensaje.Advertencia);
                return;
            }

            if (string.IsNullOrWhiteSpace(tbMotivo.Text))
            {
                Mensaje.Mostrar("Debe ingresar un motivo de cancelacion.", TipoMensaje.Advertencia);
                tbMotivo.Focus();
                return;
            }

            if (!Mensaje.Confirmar("Esta seguro de cancelar esta venta?\n\nEsta accion devolvera el stock de los productos y no se puede deshacer."))
            {
                return;
            }

            MainWindow.ShowLoader();
            try
            {
                int idUsuario = Properties.Settings.Default.IdUsuario;
                string motivo = tbMotivo.Text.Trim();
                bool resultado = false;

                await Task.Run(() =>
                {
                    resultado = cnCancelaciones.CancelarVenta(ventaSeleccionadaId, idUsuario, motivo);
                });

                if (resultado)
                {
                    Mensaje.Mostrar("Venta cancelada correctamente. El stock ha sido devuelto.", TipoMensaje.Exito);
                    await BuscarVentasAsync(tbBuscar.Text);
                }
                else
                {
                    Mensaje.Mostrar("No se pudo cancelar la venta. Verifique que no este ya cancelada.", TipoMensaje.Error);
                }
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error al cancelar venta: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
        }
        #endregion
    }
}
