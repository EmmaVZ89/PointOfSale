using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Capa_Negocio;
using PuntoDeVenta.src.Boxes;

namespace PuntoDeVenta.Views
{
    public partial class Productos : UserControl
    {
        readonly CN_Productos obj_CN_Productos = new CN_Productos();
        private DataTable dtProductos; 

        public Productos()
        {
            InitializeComponent();
            this.Loaded += Productos_Loaded;
        }

        private async void Productos_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatosAsync("");
        }

        #region BUSCADOR
        public async Task CargarDatosAsync(string buscar)
        {
            MainWindow.ShowLoader();
            try
            {
                await Task.Run(() =>
                {
                    dtProductos = this.obj_CN_Productos.BuscarProducto(buscar);
                    
                    if (!dtProductos.Columns.Contains("EstadoStr") && dtProductos.Columns.Contains("Activo"))
                    {
                        dtProductos.Columns.Add("EstadoStr", typeof(string));
                        foreach (DataRow row in dtProductos.Rows)
                        {
                            bool activo = row["Activo"] != DBNull.Value ? Convert.ToBoolean(row["Activo"]) : true;
                            row["EstadoStr"] = activo ? "Activo" : "Inactivo";
                        }
                    }
                });

                AplicarFiltros();
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
            }
            finally
            {
                MainWindow.HideLoader();
            }
        }

        public async void Buscar(string buscar)
        {
            await CargarDatosAsync(buscar);
        }

        private void AplicarFiltros()
        {
            if (dtProductos == null) return;

            DataView dv = dtProductos.DefaultView;
            
            if (chkMostrarEliminados.IsChecked == false)
            {
                dv.RowFilter = "Activo = true";
            }
            else
            {
                dv.RowFilter = ""; 
            }

            this.GridDatos.ItemsSource = dv;
        }

        private void Buscando(object sender, TextChangedEventArgs e)
        {
            // Para busqueda en tiempo real, no usamos el loader pesado, filtramos en memoria si ya trajimos todo
            // O si queremos ir a BD cada vez (lento en cloud), usamos CargarDatosAsync.
            // Dado que optimizamos la query para ser liviana, ir a BD esta bien pero mejor con debounce.
            // Por simplicidad, mantendremos la llamada directa pero sin loader bloqueante para fluidez al escribir
            // Ojo: Si la BD esta lejos, esto lageará. Mejor filtrar en memoria lo que ya trajimos.
            
            // FILTRADO EN MEMORIA (Mucho mas rapido para Cloud)
            if (dtProductos != null)
            {
                string filtro = $"Nombre LIKE '%{this.tbBuscar.Text}%' OR Codigo LIKE '%{this.tbBuscar.Text}%'";
                
                // Combinar con el filtro de activos
                if (chkMostrarEliminados.IsChecked == false)
                {
                    filtro = $"({filtro}) AND Activo = true";
                }
                
                dtProductos.DefaultView.RowFilter = filtro;
                this.GridDatos.ItemsSource = dtProductos.DefaultView;
            }
        }

        private void Filtro_Changed(object sender, RoutedEventArgs e)
        {
            // Re-aplicar filtro actual (incluyendo texto de busqueda)
            Buscando(sender, null);
        }
        #endregion

        #region CRUD PRODUCTO (Navegacion)

        private void BtnCrearProducto_Click(object sender, RoutedEventArgs e)
        {
            AbrirCRUD(0, "Crear");
        }

        private void BtnConsultar_Click(object sender, RoutedEventArgs e)
        {
            int id = (int)((Button)sender).CommandParameter;
            AbrirCRUD(id, "Consultar");
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
                CRUDProductos ventana = new CRUDProductos();
                this.FrameProductos.Content = ventana;
                this.Contenido.Visibility = Visibility.Hidden;
                ventana.IdProducto = id;

                // Cargar datos asíncronos (Grupos)
                await ventana.CargarAsync();

                if (modo != "Crear")
                {
                    // Ejecutar consulta en background (ahora manejado por ConsultarAsync)
                    await ventana.ConsultarAsync();
                }

                switch (modo)
                {
                    case "Crear":
                        ventana.Titulo.Text = "Nuevo Producto";
                        ventana.BtnCrear.Visibility = Visibility.Visible;
                        break;
                    case "Consultar":
                        ventana.Titulo.Text = "Consulta de Producto";
                        ventana.BloquearTodo();
                        break;
                    case "Modificar":
                        ventana.Titulo.Text = "Modificar Producto";
                        ventana.BtnModificar.Visibility = Visibility.Visible;
                        break;
                    case "Eliminar":
                        ventana.Titulo.Text = "Eliminar Producto";
                        ventana.BloquearTodo();
                        ventana.BtnEliminar.Visibility = Visibility.Visible;
                        break;
                }
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
    }
}
