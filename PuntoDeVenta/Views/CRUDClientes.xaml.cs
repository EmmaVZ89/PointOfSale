using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Capa_Entidad;
using Capa_Negocio;
using PuntoDeVenta.src.Boxes;

namespace PuntoDeVenta.Views
{
    public partial class CRUDClientes : Page
    {
        readonly CN_Clientes objeto_CN_Clientes = new CN_Clientes();
        readonly CE_Clientes objeto_CE_Clientes = new CE_Clientes();

        public int IdCliente { get; set; }

        #region INICIAL
        public CRUDClientes()
        {
            InitializeComponent();
        }
        #endregion

        #region REGRESAR
        private void BtnRegresar_Click(object sender, RoutedEventArgs e)
        {
            this.Content = new Clientes();
        }
        #endregion

        #region VALIDAR CAMPOS
        public bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(tbRazonSocial.Text))
            {
                return false;
            }
            return true;
        }

        public void BloquearInputs()
        {
            tbRazonSocial.IsEnabled = false;
            tbDocumento.IsEnabled = false;
            tbTelefono.IsEnabled = false;
            tbEmail.IsEnabled = false;
            tbDomicilio.IsEnabled = false;
        }
        #endregion

        #region CRUD COMPLETO

        #region CREATE
        private async void BtnCrear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.ValidarCampos())
                {
                    LlenarObjetoCliente();

                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        this.objeto_CN_Clientes.Insertar(this.objeto_CE_Clientes);
                    });
                    MainWindow.HideLoader();

                    Mensaje.Mostrar("Cliente creado correctamente.", TipoMensaje.Exito);
                    this.Content = new Clientes();
                }
                else
                {
                    Mensaje.Mostrar("La Razon Social es obligatoria.", TipoMensaje.Advertencia);
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
                CE_Clientes auxCN = null;

                await Task.Run(() =>
                {
                    auxCN = this.objeto_CN_Clientes.Consultar(this.IdCliente);
                });

                if (auxCN != null)
                {
                    this.tbRazonSocial.Text = auxCN.RazonSocial;
                    this.tbDocumento.Text = auxCN.Documento;
                    this.tbTelefono.Text = auxCN.Telefono;
                    this.tbEmail.Text = auxCN.Email;
                    this.tbDomicilio.Text = auxCN.Domicilio;
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
                if (this.ValidarCampos())
                {
                    LlenarObjetoCliente();
                    this.objeto_CE_Clientes.IdCliente = this.IdCliente;

                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        this.objeto_CN_Clientes.ActualizarDatos(this.objeto_CE_Clientes);
                    });
                    MainWindow.HideLoader();

                    Mensaje.Mostrar("Cliente actualizado correctamente.", TipoMensaje.Exito);
                    this.Content = new Clientes();
                }
                else
                {
                    Mensaje.Mostrar("La Razon Social es obligatoria.", TipoMensaje.Advertencia);
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
                if (Mensaje.Confirmar("Esta seguro de dar de baja este cliente?"))
                {
                    this.objeto_CE_Clientes.IdCliente = this.IdCliente;

                    MainWindow.ShowLoader();
                    await Task.Run(() =>
                    {
                        this.objeto_CN_Clientes.Eliminar(this.objeto_CE_Clientes);
                    });
                    MainWindow.HideLoader();

                    Mensaje.Mostrar("Cliente eliminado.", TipoMensaje.Exito);
                    this.Content = new Clientes();
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
        private void LlenarObjetoCliente()
        {
            this.objeto_CE_Clientes.RazonSocial = this.tbRazonSocial.Text.Trim();
            this.objeto_CE_Clientes.Documento = this.tbDocumento.Text.Trim();
            this.objeto_CE_Clientes.Telefono = this.tbTelefono.Text.Trim();
            this.objeto_CE_Clientes.Email = this.tbEmail.Text.Trim();
            this.objeto_CE_Clientes.Domicilio = this.tbDomicilio.Text.Trim();
        }

        private void MostrarError(string mensaje)
        {
            Mensaje.Mostrar(mensaje, TipoMensaje.Error);
        }
        #endregion
    }
}
