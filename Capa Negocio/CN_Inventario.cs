using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capa_Datos;
using Capa_Entidad;

namespace Capa_Negocio
{
    public class CN_Inventario
    {
        private CD_Inventario objDatos = new CD_Inventario();

        public void RegistrarMovimiento(CE_Movimiento movimiento)
        {
            if (movimiento.Cantidad <= 0)
            {
                throw new ArgumentException("La cantidad debe ser mayor a cero.");
            }

            if (movimiento.IdArticulo <= 0)
            {
                throw new ArgumentException("Debe seleccionar un producto válido.");
            }

            if (string.IsNullOrEmpty(movimiento.TipoMovimiento))
            {
                throw new ArgumentException("El tipo de movimiento es requerido.");
            }

            objDatos.RegistrarMovimiento(movimiento);
        }

        public List<CE_Movimiento> ListarMovimientos(string buscar, DateTime? desde, DateTime? hasta)
        {
            return objDatos.ListarMovimientos(buscar, desde, hasta);
        }

        public DataSet ObtenerDatosDashboard()
        {
            return objDatos.ObtenerDatosDashboard();
        }
    }
}
