using Capa_Negocio;
using PuntoDeVenta.src.Boxes;
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
using System.Windows.Shapes;
using Capa_Entidad;
using PuntoDeVenta.src;

namespace PuntoDeVenta.Views
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            CargarTema();
            this.tbUsuario.Focus();
        }

        public void CargarTema()
        {
            Temas temas = new Temas();
            Application.Current.Resources.MergedDictionaries.Clear();
            Application.Current.Resources.MergedDictionaries.Add(temas.CargarTema());
        }

        private async void Acceder(object sender, RoutedEventArgs e)
        {
            if(this.tbUsuario.Text != "" && this.pbContra.Password != "")
            {
                LoadingOverlay.Visibility = Visibility.Visible; // Mostrar Loader
                try
                {
                    string u = this.tbUsuario.Text;
                    string p = this.pbContra.Password;
                    
                    CE_Usuarios usuario = null;

                    // Ejecutar DB en hilo secundario
                    await Task.Run(() =>
                    {
                        CN_Usuarios capaN = new CN_Usuarios();
                        usuario = capaN.Login(u, p);
                    });

                    if (usuario.IdUsuario > 0)
                    {
                        if (!usuario.Activo)
                        {
                            Mensaje.Mostrar("Usuario inactivo. Contacte al administrador.", TipoMensaje.Advertencia);
                            return;
                        }

                        Properties.Settings.Default.IdUsuario = usuario.IdUsuario;
                        Properties.Settings.Default.Privilegio = usuario.Privilegio;
                        MainWindow mainWindow = new MainWindow();
                        Application.Current.MainWindow = mainWindow; // IMPORTANTE: Para que ShowLoader encuentre la ventana
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        Mensaje.Mostrar("Usuario y/o Contraseña incorrectos!", TipoMensaje.Error);
                    }
                }
                catch (Exception ex)
                {
                    Mensaje.Mostrar(ex.Message, TipoMensaje.Error);
                }
                finally
                {
                    LoadingOverlay.Visibility = Visibility.Collapsed; // Ocultar Loader
                }
            }
            else
            {
                Mensaje.Mostrar("Los campos no pueden quedar vacíos", TipoMensaje.Advertencia);
            }
        }

        private void Cerrar(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region EVENTOS TECLADO
        private void TbUsuario_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                pbContra.Focus();
            }
        }

        private void PbContra_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Acceder(sender, e);
            }
        }
        #endregion
    }
}