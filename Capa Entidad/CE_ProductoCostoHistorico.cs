using System;

namespace Capa_Entidad
{
    /// <summary>
    /// Entidad para el histórico de costos de compra por producto.
    /// Solo visible y editable por administradores.
    /// </summary>
    public class CE_ProductoCostoHistorico
    {
        public int IdCostoHistorico { get; set; }
        public int IdArticulo { get; set; }
        public decimal CostoUnitario { get; set; }
        public DateTime FechaRegistro { get; set; }
        public int? IdUsuarioRegistro { get; set; }
    }
}
