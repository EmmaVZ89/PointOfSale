using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capa_Entidad
{
    public class CE_Movimiento
    {
        private int idMovimiento;
        private int idArticulo;
        private int? idUsuario;
        private string tipoMovimiento;
        private decimal cantidad;
        private DateTime fechaMovimiento;
        private string observacion;

        // Propiedades de navegación (para mostrar en Grillas sin hacer joins extras)
        private string nombreProducto;
        private string codigoProducto;
        private string usuarioResponsable;

        public int IdMovimiento { get => idMovimiento; set => idMovimiento = value; }
        public int IdArticulo { get => idArticulo; set => idArticulo = value; }
        public int? IdUsuario { get => idUsuario; set => idUsuario = value; }
        public string TipoMovimiento { get => tipoMovimiento; set => tipoMovimiento = value; }
        public decimal Cantidad { get => cantidad; set => cantidad = value; }
        public DateTime FechaMovimiento { get => fechaMovimiento; set => fechaMovimiento = value; }
        public string Observacion { get => observacion; set => observacion = value; }
        
        public string NombreProducto { get => nombreProducto; set => nombreProducto = value; }
        public string CodigoProducto { get => codigoProducto; set => codigoProducto = value; }
        public string UsuarioResponsable { get => usuarioResponsable; set => usuarioResponsable = value; }
    }
}
