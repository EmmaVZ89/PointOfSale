using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Capa_Negocio;
using System.IO;
using Capa_Entidad;
using System.Threading.Tasks;

namespace PuntoDeVenta.Views
{
    public partial class MiCuenta : Window
    {
        public MiCuenta()
        {
            InitializeComponent();
            this.Loaded += MiCuenta_Loaded;
        }

        private async void MiCuenta_Loaded(object sender, RoutedEventArgs e)
        {
            await CargarDatos();
        }

        private async Task CargarDatos()
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            try
            {
                CE_Usuarios user = null;
                await Task.Run(() =>
                {
                    CN_Usuarios cn = new CN_Usuarios();
                    user = cn.Cargar(Properties.Settings.Default.IdUsuario);
                });

                if (user != null)
                {
                    lblNombre.Text = $"{user.Nombre} {user.Apellido}";
                    lblUsuario.Text = user.Usuario;
                    lblCorreo.Text = user.Correo;
                    
                    // Rol (esto es un parche rápido, idealmente traer nombre rol)
                    lblRol.Text = user.Privilegio == 1 ? "Administrador" : "Vendedor";

                    if (user.Img != null)
                    {
                        using (var ms = new MemoryStream(user.Img))
                        {
                            var image = new BitmapImage();
                            image.BeginInit();
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.StreamSource = ms;
                            image.EndInit();
                            imagen.ImageSource = image;
                        }
                    }
                }
            }
            catch { }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}