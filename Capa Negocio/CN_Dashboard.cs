using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Capa_Datos;

namespace Capa_Negocio
{
    public class CN_Dashboard
    {
        private CD_Dashboard objDatos = new CD_Dashboard();

        public Dictionary<string, string> ObtenerKPIs()
        {
            var kpis = new Dictionary<string, string>();

            // 1. Resumen Día
            DataTable dtDia = objDatos.ObtenerResumenDia();
            if (dtDia.Rows.Count > 0)
            {
                kpis.Add("TotalVendido", Convert.ToDecimal(dtDia.Rows[0]["TotalVendido"]).ToString("C"));
                kpis.Add("CantidadVentas", dtDia.Rows[0]["CantidadVentas"].ToString());
            }

            // 2. Resumen Inventario
            DataTable dtInv = objDatos.ObtenerResumenInventario();
            if (dtInv.Rows.Count > 0)
            {
                kpis.Add("TotalProductos", dtInv.Rows[0]["TotalProductos"].ToString());
                kpis.Add("StockBajo", dtInv.Rows[0]["StockBajo"].ToString());
            }

            return kpis;
        }

        public DataTable ObtenerVentasSemanales()
        {
            return objDatos.ObtenerVentasSemanales();
        }
    }
}