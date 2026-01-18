using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace PuntoDeVenta.src.Boxes
{
    public enum TipoMensaje
    {
        Info,
        Exito,
        Error,
        Advertencia,
        Confirmacion
    }

    public partial class Mensaje : Window
    {
        public bool Confirmado { get; private set; } = false;

        public Mensaje(string mensaje, TipoMensaje tipo)
        {
            InitializeComponent();
            Configurar(mensaje, tipo);
        }

        private void Configurar(string mensaje, TipoMensaje tipo)
        {
            txtMensaje.Text = mensaje;

            switch (tipo)
            {
                case TipoMensaje.Info:
                    txtTitulo.Text = "Información";
                    SetColor("#3498DB"); // Azul
                    IconPath.Data = Geometry.Parse("M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z");
                    break;
                case TipoMensaje.Exito:
                    txtTitulo.Text = "Éxito";
                    SetColor("#2ECC71"); // Verde
                    IconPath.Data = Geometry.Parse("M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z");
                    break;
                case TipoMensaje.Error:
                    txtTitulo.Text = "Error";
                    SetColor("#E74C3C"); // Rojo
                    IconPath.Data = Geometry.Parse("M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2C6.47,2 2,6.47 2,12C2,17.53 6.47,22 12,22C17.53,22 22,17.53 22,12C22,6.47 17.53,2 12,2M14.59,8L12,10.59L9.41,8L8,9.41L10.59,12L8,14.59L9.41,16L12,13.41L14.59,16L16,14.59L13.41,12L16,9.41L14.59,8Z");
                    break;
                case TipoMensaje.Advertencia:
                    txtTitulo.Text = "Advertencia";
                    SetColor("#F39C12"); // Naranja
                    IconPath.Data = Geometry.Parse("M13,14H11V9H13M13,18H11V16H13M1,21H23L12,2L1,21Z");
                    break;
                case TipoMensaje.Confirmacion:
                    txtTitulo.Text = "Confirmar";
                    SetColor("#34495E"); // Gris Oscuro
                    IconPath.Data = Geometry.Parse("M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M12,4A8,8 0 0,1 20,12A8,8 0 0,1 12,20A8,8 0 0,1 4,12A8,8 0 0,1 12,4M11,16.5L6.5,12L7.91,10.59L11,13.67L16.59,8.09L18,9.5L11,16.5Z");
                    btnCancelar.Visibility = Visibility.Visible;
                    btnAceptar.Content = "Sí, continuar";
                    break;
            }
        }

        private void SetColor(string hex)
        {
            var brush = (SolidColorBrush)new BrushConverter().ConvertFromString(hex);
            ColorBar.Background = brush;
            IconPath.Fill = brush;
            btnAceptar.Background = brush;
        }

        private void Overlay_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void BtnAceptar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = true;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Confirmado = false;
            this.Close();
        }

        // Método estático para uso fácil
        public static void Mostrar(string mensaje, TipoMensaje tipo)
        {
            new Mensaje(mensaje, tipo).ShowDialog();
        }

        public static bool Confirmar(string mensaje)
        {
            var m = new Mensaje(mensaje, TipoMensaje.Confirmacion);
            m.ShowDialog();
            return m.Confirmado;
        }
    }
}
