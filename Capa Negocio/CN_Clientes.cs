using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capa_Datos;
using Capa_Entidad;
using System.Data;

namespace Capa_Negocio
{
    public class CN_Clientes
    {
        private readonly CD_Clientes objDatos = new CD_Clientes();

        #region CONSULTAR
        public CE_Clientes Consultar(int idCliente)
        {
            return objDatos.CD_Consultar(idCliente);
        }
        #endregion

        #region INSERTAR
        public void Insertar(CE_Clientes cliente)
        {
            this.objDatos.CD_Insertar(cliente);
        }
        #endregion

        #region ELIMINAR
        public void Eliminar(CE_Clientes cliente)
        {
            // No se puede eliminar "Consumidor Final" (ID = 1)
            if (cliente.IdCliente == 1)
            {
                throw new InvalidOperationException("No se puede eliminar el cliente 'Consumidor Final'.");
            }
            this.objDatos.CD_Eliminar(cliente);
        }
        #endregion

        #region REACTIVAR
        public void Reactivar(CE_Clientes cliente)
        {
            this.objDatos.CD_Reactivar(cliente);
        }
        #endregion

        #region ACTUALIZAR DATOS
        public void ActualizarDatos(CE_Clientes cliente)
        {
            // No se puede modificar "Consumidor Final" (ID = 1)
            if (cliente.IdCliente == 1)
            {
                throw new InvalidOperationException("No se puede modificar el cliente 'Consumidor Final'.");
            }
            this.objDatos.CD_ActualizarDatos(cliente);
        }
        #endregion

        #region BUSCAR CLIENTES
        public DataTable Buscar(string buscar)
        {
            return this.objDatos.Buscar(buscar);
        }
        #endregion

        #region LISTAR ACTIVOS (Para ComboBox)
        public List<CE_Clientes> ListarActivos()
        {
            return this.objDatos.ListarActivos();
        }
        #endregion
    }
}
