using System;

namespace Capa_Entidad
{
    /// <summary>
    /// Representa una presentacion de producto (unidad, pack, caja, etc.)
    /// Permite manejar multiples precios por producto segun la presentacion
    /// </summary>
    public class CE_ProductoPresentacion
    {
        public int IdPresentacion { get; set; }
        public int IdArticulo { get; set; }
        public string Nombre { get; set; }
        public int CantidadUnidades { get; set; }
        public decimal Precio { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Navegacion (no mapeada en EF, solo para uso en memoria)
        public CE_Productos Producto { get; set; }
    }
}
