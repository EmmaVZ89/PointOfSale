using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Capa_Entidad;
using Capa_Negocio;
using PuntoDeVenta.src.Boxes;

namespace PuntoDeVenta.Views
{
    public partial class Clientes : UserControl
    {
        readonly CN_Clientes objeto_CN_Clientes = new CN_Clientes();

        public Clientes()
        {
            InitializeComponent();
            this.Loaded += Clientes_Loaded;
        }

        private async void Clientes_Loaded(object sender, RoutedEventArgs e)
        {
            await BuscarAsync("");
        }

        #region BUSCAR Y CARGAR
        private async Task BuscarAsync(string buscar)
        {
            MainWindow.ShowLoader();
            try
            {
                DataTable dt = null;
                await Task.Run(() =>
                {
                    dt = this.objeto_CN_Clientes.Buscar(buscar);
                    if (!dt.Columns.Contains("EstadoStr") && dt.Columns.Contains("Activo"))
                    {
                        dt.Columns.Add("EstadoStr", typeof(string));
                        foreach (DataRow row in dt.Rows)
                        {
                            bool activo = row["Activo"] != DBNull.Value ? Convert.ToBoolean(row["Activo"]) : true;
                            row["EstadoStr"] = activo ? "Activo" : "Inactivo";
                        }
                    }
                });

                this.GridDatos.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error cargando clientes: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
        }

        private void Buscando(object sender, TextChangedEventArgs e)
        {
            if (GridDatos.ItemsSource is DataView dv)
            {
                dv.RowFilter = $"RazonSocial LIKE '%{this.tbBuscar.Text}%' OR Documento LIKE '%{this.tbBuscar.Text}%'";
            }
        }
        #endregion

        #region ACCIONES CRUD (Navegacion)
        private void BtnCrearCliente_Click(object sender, RoutedEventArgs e)
        {
            AbrirCRUD(0, "Crear");
        }

        private void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;

            // No permitir modificar "Consumidor Final"
            if (id == 1)
            {
                Mensaje.Mostrar("No se puede modificar el cliente 'Consumidor Final'.", TipoMensaje.Advertencia);
                return;
            }

            AbrirCRUD(id, "Modificar");
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;

            // No permitir eliminar "Consumidor Final"
            if (id == 1)
            {
                Mensaje.Mostrar("No se puede eliminar el cliente 'Consumidor Final'.", TipoMensaje.Advertencia);
                return;
            }

            AbrirCRUD(id, "Eliminar");
        }

        private async void AbrirCRUD(int id, string modo)
        {
            MainWindow.ShowLoader();
            try
            {
                CRUDClientes ventana = new CRUDClientes();
                ventana.IdCliente = id;

                if (modo != "Crear")
                    await ventana.ConsultarAsync();

                switch (modo)
                {
                    case "Crear":
                        ventana.Titulo.Text = "Nuevo Cliente";
                        ventana.BtnCrear.Visibility = Visibility.Visible;
                        break;
                    case "Modificar":
                        ventana.Titulo.Text = "Modificar Cliente";
                        ventana.BtnModificar.Visibility = Visibility.Visible;
                        break;
                    case "Eliminar":
                        ventana.Titulo.Text = "Confirmar Baja";
                        ventana.BloquearInputs();
                        ventana.BtnEliminar.Visibility = Visibility.Visible;
                        break;
                }

                this.FrameClientes.Content = ventana;
                this.Contenido.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar("Error: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
        }
        #endregion

        #region REACTIVAR
        private async void BtnReactivar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int id = (int)((Button)sender).CommandParameter;

                // No permitir reactivar "Consumidor Final" (ya esta activo siempre)
                if (id == 1)
                {
                    Mensaje.Mostrar("El cliente 'Consumidor Final' siempre esta activo.", TipoMensaje.Advertencia);
                    return;
                }

                if (Mensaje.Confirmar("Desea reactivar este cliente?"))
                {
                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        CE_Clientes cliente = new CE_Clientes { IdCliente = id };
                        objeto_CN_Clientes.Reactivar(cliente);
                    });

                    await BuscarAsync(tbBuscar.Text);
                    Mensaje.Mostrar("Cliente reactivado correctamente.", TipoMensaje.Exito);
                }
            }
            catch (Exception ex)
            {
                MainWindow.HideLoader();
                Mensaje.Mostrar("Error al reactivar: " + ex.Message, TipoMensaje.Error);
            }
        }
        #endregion
    }
}
