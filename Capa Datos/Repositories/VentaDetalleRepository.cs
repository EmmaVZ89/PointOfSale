using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Capa_Datos.Context;
using Capa_Datos.Interfaces;
using Capa_Entidad;

namespace Capa_Datos.Repositories
{
    /// <summary>
    /// Implementacion del repositorio de VentaDetalle usando Entity Framework Core.
    /// </summary>
    public class VentaDetalleRepository : Repository<CE_VentaDetalle>, IVentaDetalleRepository
    {
        public VentaDetalleRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CE_VentaDetalle>> GetByVentaAsync(int idVenta)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(d => d.Id_Venta == idVenta)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_VentaDetalle>> GetByVentaConProductoAsync(int idVenta)
        {
            // Obtener detalles con info de producto mediante join manual
            var detalles = await _dbSet
                .AsNoTracking()
                .Where(d => d.Id_Venta == idVenta)
                .ToListAsync();

            // Cargar info de productos
            var idArticulos = detalles.Select(d => d.Id_Articulo).Distinct().ToList();
            var productos = await _context.Productos
                .AsNoTracking()
                .Where(p => idArticulos.Contains(p.IdArticulo))
                .ToDictionaryAsync(p => p.IdArticulo, p => new { p.Nombre, p.Codigo, p.Precio });

            // Enriquecer detalles y manejar precios nulos
            foreach (var detalle in detalles)
            {
                if (detalle.Id_Articulo.HasValue && productos.TryGetValue(detalle.Id_Articulo.Value, out var prod))
                {
                    detalle.NombreProducto = prod.Nombre;
                    detalle.CodigoProducto = prod.Codigo;

                    // Si Precio_Venta es null, usar precio actual del producto
                    if (!detalle.Precio_Venta.HasValue)
                    {
                        detalle.Precio_Venta = prod.Precio;
                    }

                    // Si Monto_Total es null, calcularlo
                    if (!detalle.Monto_Total.HasValue)
                    {
                        detalle.Monto_Total = detalle.Cantidad * (detalle.Precio_Venta ?? 0);
                    }
                }
            }

            return detalles;
        }
    }
}
