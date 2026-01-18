using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.IO;
using Capa_Negocio;
using Capa_Entidad;
using PuntoDeVenta.src.Boxes;
using System.Windows.Input;
using System.Threading.Tasks;

namespace PuntoDeVenta.Views
{
    public partial class CRUDProductos : Page
    {
        public int IdProducto;
        public string Patron = "PuntoDeVenta";
        private byte[] data;
        private bool imagenSubida = false;
        CN_Grupos objeto_CN_Grupos = new CN_Grupos();
        CN_Productos objeto_CN_Productos = new CN_Productos();
        CE_Productos objeto_CE_Productos = new CE_Productos();

        #region INICIAL
        public CRUDProductos()
        {
            InitializeComponent();
        }
        #endregion

        #region REGRESAR
        private void BtnRegresar_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Productos();
        }
        #endregion

        #region LLENAR GRUPOS
        public async Task CargarAsync()
        {
            try
            {
                List<string> grupos = null;
                await Task.Run(() =>
                {
                    grupos = this.objeto_CN_Grupos.ListarGrupos();
                });

                if (grupos != null)
                {
                    foreach (string g in grupos)
                    {
                        this.cbGrupo.Items.Add(g);
                    }
                }
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
            }
        }
        #endregion

        #region VALIDAR CAMPOS
        public bool ValidarCampos()
        {
            if(string.IsNullOrWhiteSpace(this.tbNombre.Text) || 
               string.IsNullOrWhiteSpace(this.tbCodigo.Text) || 
               string.IsNullOrWhiteSpace(this.cbGrupo.Text) || 
               string.IsNullOrWhiteSpace(this.tbPrecio.Text) ||
               string.IsNullOrWhiteSpace(this.tbCantidad.Text) || 
               string.IsNullOrWhiteSpace(this.tbUnidadMedida.Text) || 
               string.IsNullOrWhiteSpace(this.tbDescripcion.Text))
            {
                return false;
            }
            return true;
        }

        private void IntegerValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Solo permite numeros del 0 al 9
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void DecimalValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Permite numeros y coma
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex("[^0-9,]+");
            bool isCharValid = !regex.IsMatch(e.Text);

            // Evitar mas de una coma
            if (e.Text == "," && (sender as TextBox).Text.Contains(","))
            {
                isCharValid = false;
            }

            e.Handled = !isCharValid;
        }

        public void BloquearTodo()
        {
            tbNombre.IsEnabled = false;
            tbCodigo.IsEnabled = false;
            tbCantidad.IsEnabled = false;
            tbActivo.IsEnabled = false;
            tbPrecio.IsEnabled = false;
            cbGrupo.IsEnabled = false;
            tbUnidadMedida.IsEnabled = false;
            tbDescripcion.IsEnabled = false;
            BtnCambiarImagen.IsEnabled = false;
        }
        #endregion

        #region CRUD PRODUCTOS

        #region CREATE
        private async void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ValidarCampos() == true)
                {
                    int idGrupo = this.objeto_CN_Grupos.IdGrupo(this.cbGrupo.Text);

                    this.objeto_CE_Productos.Nombre = this.tbNombre.Text;
                    this.objeto_CE_Productos.Codigo = this.tbCodigo.Text;
                    this.objeto_CE_Productos.Precio = Decimal.Parse(this.tbPrecio.Text);
                    this.objeto_CE_Productos.Cantidad = Decimal.Parse(this.tbCantidad.Text);
                    this.objeto_CE_Productos.Activo = (bool)this.tbActivo.IsChecked;
                    this.objeto_CE_Productos.UnidadMedida = this.tbUnidadMedida.Text;
                    this.objeto_CE_Productos.Img = this.data;
                    this.objeto_CE_Productos.Descripcion = this.tbDescripcion.Text;
                    this.objeto_CE_Productos.Grupo = idGrupo;

                    // Pasar ID de usuario logueado para registrar el movimiento de stock inicial
                    int idUsuario = Properties.Settings.Default.IdUsuario;
                    
                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        this.objeto_CN_Productos.Insertar(this.objeto_CE_Productos, idUsuario);
                    });
                    MainWindow.HideLoader();

                    Mensaje.Mostrar("Producto creado correctamente.", TipoMensaje.Exito);
                    this.Content = new Productos();
                }
                else
                {
                    Mensaje.Mostrar("Los campos no pueden quedar vacíos!", TipoMensaje.Advertencia);
                }
            }
            catch (Exception ex)
            {
                MainWindow.HideLoader();
                Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
            }
        }
        #endregion

        #region READ
        public async Task ConsultarAsync()
        {
            try
            {
                CE_Productos cnProd = null;
                string nombreGrupo = null;

                await Task.Run(() =>
                {
                    cnProd = this.objeto_CN_Productos.Consultar(this.IdProducto);
                    if (cnProd != null)
                    {
                        var cnGro = this.objeto_CN_Grupos.Nombre(cnProd.Grupo);
                        nombreGrupo = cnGro.Nombre;
                    }
                });

                if (cnProd != null)
                {
                    this.tbNombre.Text = cnProd.Nombre.ToString();
                    this.tbCodigo.Text = cnProd.Codigo.ToString();
                    this.tbPrecio.Text = cnProd.Precio.ToString();
                    this.tbActivo.IsChecked = cnProd.Activo;
                    this.tbCantidad.Text = cnProd.Cantidad.ToString();
                    
                    // Bloquear edición de stock en modo Modificar
                    this.tbCantidad.IsEnabled = false;
                    this.tbCantidad.ToolTip = "El stock solo se puede modificar desde el módulo de Inventario.";
                    
                    this.tbUnidadMedida.Text = cnProd.UnidadMedida.ToString();
                    
                    if (cnProd.Img != null)
                    {
                        ImageSourceConverter imgs = new ImageSourceConverter();
                        this.imagen.Source = (ImageSource)imgs.ConvertFrom(cnProd.Img);
                    }
                    
                    this.tbDescripcion.Text = cnProd.Descripcion.ToString();
                    
                    if (nombreGrupo != null)
                    {
                        this.cbGrupo.Text = nombreGrupo;
                    }
                }
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
            }
        }
        #endregion

        #region DELETE
        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(Mensaje.Confirmar("¿Seguro de eliminar este producto?"))
                {
                    this.objeto_CE_Productos.IdArticulo = this.IdProducto;
                    
                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        this.objeto_CN_Productos.Eliminar(this.objeto_CE_Productos);
                    });
                    MainWindow.HideLoader();

                    Mensaje.Mostrar("Producto eliminado.", TipoMensaje.Exito);
                    this.Content = new Productos();
                }
            }
            catch (Exception ex)
            {
                MainWindow.HideLoader();
                Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
            }
        }
        #endregion

        #region UPDATE
        private async void BtnModificar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ValidarCampos() == true)
                {
                    int idGrupo = this.objeto_CN_Grupos.IdGrupo(this.cbGrupo.Text);

                    this.objeto_CE_Productos.IdArticulo = this.IdProducto;
                    this.objeto_CE_Productos.Nombre = this.tbNombre.Text;
                    this.objeto_CE_Productos.Codigo = this.tbCodigo.Text;
                    this.objeto_CE_Productos.Precio = Decimal.Parse(this.tbPrecio.Text);
                    // Stock no se actualiza desde aquí, pero mantenemos el valor para el objeto
                    this.objeto_CE_Productos.Cantidad = Decimal.Parse(this.tbCantidad.Text);
                    this.objeto_CE_Productos.Activo = (bool)this.tbActivo.IsChecked;
                    this.objeto_CE_Productos.UnidadMedida = this.tbUnidadMedida.Text;
                    this.objeto_CE_Productos.Descripcion = this.tbDescripcion.Text;
                    this.objeto_CE_Productos.Grupo = idGrupo;

                    bool hayImagen = this.imagenSubida;
                    if (hayImagen)
                    {
                        this.objeto_CE_Productos.Img = this.data;
                    }

                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        this.objeto_CN_Productos.ActualizarDatos(this.objeto_CE_Productos);
                        if (hayImagen)
                        {
                            this.objeto_CN_Productos.ActualizarIMG(this.objeto_CE_Productos);
                        }
                    });
                    MainWindow.HideLoader();

                    Mensaje.Mostrar("Producto actualizado correctamente.", TipoMensaje.Exito);
                    this.Content = new Productos();
                }
                else
                {
                    Mensaje.Mostrar("Los campos no pueden quedar vacíos!", TipoMensaje.Advertencia);
                }
            }
            catch (Exception ex)
            {
                MainWindow.HideLoader();
                Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
            }
        }
        #endregion

        #endregion

        #region SUBIR IMAGEN
        private void BtnCambiarImagen_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openFD = new OpenFileDialog();
                if (openFD.ShowDialog() == true)
                {
                    FileStream fStream = new FileStream(openFD.FileName, FileMode.Open, FileAccess.Read);
                    this.data = new byte[fStream.Length];
                    fStream.Read(this.data, 0, Convert.ToInt32(fStream.Length));
                    fStream.Close();
                    ImageSourceConverter imgs = new ImageSourceConverter();
                    this.imagen.SetValue(Image.SourceProperty, imgs.ConvertFromString(openFD.FileName.ToString()));
                }
                this.imagenSubida = true;
            }
            catch (Exception ex)
            {
                Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
            }
        }
        #endregion
    }
}