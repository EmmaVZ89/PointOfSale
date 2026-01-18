using System;
using System.Windows;

namespace PuntoDeVenta.src.Boxes
{
    public enum AccionPresupuesto
    {
        Cancelar,
        Imprimir,
        Guardar
    }

    public partial class DialogoPresupuesto : Window
    {
        public AccionPresupuesto Accion { get; private set; } = AccionPresupuesto.Cancelar;
        public string RutaArchivo { get; set; }

        public DialogoPresupuesto(string nroPresupuesto, decimal total)
        {
            InitializeComponent();
            txtInfo.Text = $"Presupuesto N° {nroPresupuesto}\nTotal: {total:C}";
        }

        private void BtnImprimir_Click(object sender, RoutedEventArgs e)
        {
            Accion = AccionPresupuesto.Imprimir;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            Accion = AccionPresupuesto.Guardar;
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            Accion = AccionPresupuesto.Cancelar;
            this.DialogResult = false;
            this.Close();
        }
    }
}
