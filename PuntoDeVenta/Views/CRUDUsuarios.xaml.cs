using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Capa_Entidad;
using Capa_Negocio;
using PuntoDeVenta.src.Boxes;
using System.Threading.Tasks;

namespace PuntoDeVenta.Views
{
    public partial class CRUDUsuarios : Page
    {
        readonly CN_Usuarios objeto_CN_Usuarios = new CN_Usuarios();
        readonly CE_Usuarios objeto_CE_Usuarios = new CE_Usuarios();
        readonly CN_Privilegios objeto_CN_Privilegios = new CN_Privilegios();

        // PROPIEDADES RESTAURADAS
        public int IdUsuario { get; set; }
        public string Patron = "PuntoDeVenta";
        private byte[] data;
        private bool imagenSubida = false;

        #region INICIAL
        public CRUDUsuarios()
        {
            InitializeComponent();
        }
        #endregion

        #region REGRESAR
        private void BtnRegresar_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Usuarios(); 
        }
        #endregion

        #region CARGAR PRIVILEGIOS
        public async Task CargarCBAsync()
        {
            try
            {
                List<string> privilegios = null;
                await Task.Run(() =>
                {
                    privilegios = this.objeto_CN_Privilegios.ListarPrivilegios();
                });

                if (privilegios != null)
                {
                    foreach (string p in privilegios)
                    {
                        this.cbPrivilegio.Items.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }
        #endregion

        #region VALIDAR CAMPOS
        public bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(tbNombre.Text) || 
                string.IsNullOrWhiteSpace(tbApellido.Text) || 
                string.IsNullOrWhiteSpace(tbDNI.Text) ||
                string.IsNullOrWhiteSpace(tbCUIT.Text) || 
                string.IsNullOrWhiteSpace(tbCorreo.Text) || 
                string.IsNullOrWhiteSpace(tbTelefono.Text) ||
                string.IsNullOrWhiteSpace(tbFecha.Text) || 
                string.IsNullOrWhiteSpace(cbPrivilegio.Text) || 
                string.IsNullOrWhiteSpace(tbUsuario.Text))
            {
                return false;
            }
            return true;
        }

        // Método auxiliar para bloquear inputs en modo eliminar
        public void BloquearInputs()
        {
            tbNombre.IsEnabled = false;
            tbApellido.IsEnabled = false;
            tbDNI.IsEnabled = false;
            tbCUIT.IsEnabled = false;
            tbFecha.IsEnabled = false;
            tbTelefono.IsEnabled = false;
            tbCorreo.IsEnabled = false;
            cbPrivilegio.IsEnabled = false;
            tbUsuario.IsEnabled = false;
            tbContrasenia.IsEnabled = false;
            BtnCambiarImagen.IsEnabled = false;
        }
        #endregion

        #region CRUD COMPLETO

        #region CREATE
        private async void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ValidarCampos() == true && !string.IsNullOrWhiteSpace(this.tbContrasenia.Text))
                {
                    LlenarObjetoUsuario();
                    
                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        this.objeto_CN_Usuarios.Insertar(this.objeto_CE_Usuarios);
                    });
                    MainWindow.HideLoader();

                    Mensaje.Mostrar("Usuario creado correctamente.", TipoMensaje.Exito);
                    this.Content = new Usuarios();
                }
                else
                {
                    Mensaje.Mostrar("Los campos no pueden quedar vacíos", TipoMensaje.Advertencia);
                }
            }
            catch (Exception ex)
            {
                MainWindow.HideLoader();
                MostrarError(ex.Message);
            }
        }
        #endregion

        #region READ
        public async Task ConsultarAsync()
        {
            try
            {
                CE_Usuarios auxCN = null;
                CE_Privilegios privilegio = null;

                await Task.Run(() =>
                {
                    auxCN = this.objeto_CN_Usuarios.Consultar(this.IdUsuario);
                    if (auxCN != null)
                    {
                        privilegio = this.objeto_CN_Privilegios.NombrePrivilegio(auxCN.Privilegio);
                    }
                });

                if (auxCN != null)
                {
                    this.tbNombre.Text = auxCN.Nombre;
                    this.tbApellido.Text = auxCN.Apellido;
                    this.tbDNI.Text = auxCN.Dni.ToString();
                    this.tbCUIT.Text = auxCN.Cuit.ToString();
                    this.tbCorreo.Text = auxCN.Correo;
                    this.tbTelefono.Text = auxCN.Telefono.ToString();
                    this.tbFecha.Text = auxCN.Fecha_Nac.ToString(); 

                    if (privilegio != null)
                        this.cbPrivilegio.Text = privilegio.NombrePrivilegio;

                    if (auxCN.Img != null)
                    {
                        ImageSourceConverter imgs = new ImageSourceConverter();
                        this.imagen.Source = (ImageSource)imgs.ConvertFrom(auxCN.Img);
                    }
                    
                    this.tbUsuario.Text = auxCN.Usuario;
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
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
                    LlenarObjetoUsuario();
                    this.objeto_CE_Usuarios.IdUsuario = this.IdUsuario; // Asegurar ID
                    
                    bool updatePass = !string.IsNullOrWhiteSpace(this.tbContrasenia.Text);
                    bool updateImg = this.imagenSubida;

                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        // Actualizar Datos Básicos
                        this.objeto_CN_Usuarios.ActualizarDatos(this.objeto_CE_Usuarios);

                        // Actualizar Password si cambió
                        if (updatePass)
                        {
                            this.objeto_CN_Usuarios.ActualizarPass(this.objeto_CE_Usuarios);
                        }

                        // Actualizar Imagen si cambió
                        if (updateImg)
                        {
                            this.objeto_CN_Usuarios.ActualizarIMG(this.objeto_CE_Usuarios);
                        }
                    });
                    MainWindow.HideLoader();

                    Mensaje.Mostrar("Usuario actualizado correctamente.", TipoMensaje.Exito);
                    this.Content = new Usuarios();
                }
                else
                {
                    Mensaje.Mostrar("Los campos no pueden quedar vacíos", TipoMensaje.Advertencia);
                }
            }
            catch (Exception ex)
            {
                MainWindow.HideLoader();
                MostrarError(ex.Message);
            }
        }
        #endregion

        #region DELETE
        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // VALIDACIÓN DE SEGURIDAD
                if (this.IdUsuario == Properties.Settings.Default.IdUsuario)
                {
                    MostrarError("No puedes eliminar tu propio usuario mientras estás logueado.");
                    return;
                }

                if (Mensaje.Confirmar("¿Está seguro de dar de baja este usuario?"))
                {
                    this.objeto_CE_Usuarios.IdUsuario = this.IdUsuario;
                    
                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        this.objeto_CN_Usuarios.Eliminar(this.objeto_CE_Usuarios);
                    });
                    MainWindow.HideLoader();
                    
                    Mensaje.Mostrar("Usuario eliminado.", TipoMensaje.Exito);
                    this.Content = new Usuarios();
                }
            }
            catch (Exception ex)
            {
                MainWindow.HideLoader();
                MostrarError(ex.Message);
            }
        }
        #endregion

        #endregion

        #region HELPERS
        private void LlenarObjetoUsuario()
        {
            int privilegio = this.objeto_CN_Privilegios.IdPrivilegio(this.cbPrivilegio.Text);

            this.objeto_CE_Usuarios.Nombre = this.tbNombre.Text;
            this.objeto_CE_Usuarios.Apellido = this.tbApellido.Text;
            this.objeto_CE_Usuarios.Dni = int.Parse(this.tbDNI.Text);
            this.objeto_CE_Usuarios.Cuit = int.Parse(this.tbCUIT.Text); // Ojo con long/double si CUIT > int max
            this.objeto_CE_Usuarios.Correo = this.tbCorreo.Text;
            this.objeto_CE_Usuarios.Telefono = this.tbTelefono.Text;
            this.objeto_CE_Usuarios.Fecha_Nac = DateTime.Parse(this.tbFecha.Text);
            this.objeto_CE_Usuarios.Privilegio = privilegio;
            this.objeto_CE_Usuarios.Usuario = this.tbUsuario.Text;
            
            // Opcionales para Insert (update los maneja separado o igual)
            this.objeto_CE_Usuarios.Contrasenia = this.tbContrasenia.Text; 
            this.objeto_CE_Usuarios.Img = this.data;
            this.objeto_CE_Usuarios.Patron = this.Patron;
        }

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
                    this.imagenSubida = true;
                }
            }
            catch (Exception ex)
            {
                MostrarError(ex.Message);
            }
        }

        private void MostrarError(string mensaje)
        {
            Mensaje.Mostrar(mensaje, TipoMensaje.Error);
        }
        #endregion
    }
}
