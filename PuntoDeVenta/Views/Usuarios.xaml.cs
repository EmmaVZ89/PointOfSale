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
    public partial class Usuarios : UserControl
    {
        readonly CN_Usuarios objeto_CN_Usuarios = new CN_Usuarios();

        public Usuarios()
        {
            InitializeComponent();
            this.Loaded += Usuarios_Loaded;
        }

        private async void Usuarios_Loaded(object sender, RoutedEventArgs e)
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
                    dt = this.objeto_CN_Usuarios.Buscar(buscar);
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
                Mensaje.Mostrar("Error cargando usuarios: " + ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
        }

        private void Buscando(object sender, TextChangedEventArgs e)
        {
            // Filtrado en memoria si ya tenemos datos, o busqueda async
            // Para usuarios, como son pocos, podemos filtrar el DefaultView existente
            if (GridDatos.ItemsSource is DataView dv)
            {
                dv.RowFilter = $"Nombre LIKE '%{this.tbBuscar.Text}%' OR Apellido LIKE '%{this.tbBuscar.Text}%' OR Usuario LIKE '%{this.tbBuscar.Text}%'";
            }
        }
        #endregion

        #region ACCIONES CRUD (Navegación)
        private void BtnCrearUsuario_Click(object sender, RoutedEventArgs e)
        {
            AbrirCRUD(0, "Crear");
        }

        private void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            AbrirCRUD(id, "Modificar");
        }

        private void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            AbrirCRUD(id, "Eliminar");
        }

        private async void AbrirCRUD(int id, string modo)
        {
            MainWindow.ShowLoader();
            try
            {
                CRUDUsuarios ventana = new CRUDUsuarios();
                ventana.IdUsuario = id;
                
                await ventana.CargarCBAsync();

                if(modo != "Crear")
                    await ventana.ConsultarAsync();

                switch (modo)
                {
                    case "Crear":
                        ventana.Titulo.Text = "Nuevo Usuario";
                        ventana.BtnCrear.Visibility = Visibility.Visible;
                        break;
                    case "Modificar":
                        ventana.Titulo.Text = "Modificar Usuario";
                        ventana.BtnModificar.Visibility = Visibility.Visible;
                        break;
                    case "Eliminar":
                        ventana.Titulo.Text = "Confirmar Baja";
                        ventana.BloquearInputs(); 
                        ventana.BtnEliminar.Visibility = Visibility.Visible;
                        break;
                }

                this.FrameUsuarios.Content = ventana;
                this.Contenido.Visibility = Visibility.Hidden;
            }
            catch(Exception ex)
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
                
                if (Mensaje.Confirmar("¿Desea reactivar este usuario?"))
                {
                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        CE_Usuarios user = new CE_Usuarios { IdUsuario = id };
                        objeto_CN_Usuarios.Reactivar(user);
                    });
                    
                    await BuscarAsync(tbBuscar.Text); // Recargar
                    Mensaje.Mostrar("Usuario reactivado correctamente.", TipoMensaje.Exito);
                }
            }
            catch (Exception ex)
            {
                MainWindow.HideLoader(); // Asegurar cierre si falla antes
                Mensaje.Mostrar("Error al reactivar: " + ex.Message, TipoMensaje.Error);
            }
        }
        #endregion
    }
}
