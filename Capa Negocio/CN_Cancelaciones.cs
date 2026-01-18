using System;
using System.Data;
using Capa_Datos;

namespace Capa_Negocio
{
    public class CN_Cancelaciones
    {
        private readonly CD_Cancelaciones objDatos = new CD_Cancelaciones();

        #region BUSCAR VENTAS CANCELABLES
        public DataTable BuscarVentas(string buscar)
        {
            return objDatos.BuscarVentas(buscar);
        }
        #endregion

        #region OBTENER DETALLE DE VENTA
        public DataTable ObtenerDetalleVenta(int idVenta)
        {
            return objDatos.ObtenerDetalleVenta(idVenta);
        }
        #endregion

        #region CANCELAR VENTA
        public bool CancelarVenta(int idVenta, int idUsuario, string motivo)
        {
            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new ArgumentException("Debe ingresar un motivo de cancelacion.");
            }

            return objDatos.CancelarVenta(idVenta, idUsuario, motivo);
        }
        #endregion
    }
}
