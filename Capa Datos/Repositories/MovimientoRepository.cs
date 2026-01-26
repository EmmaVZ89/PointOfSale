using System;
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
    /// Implementacion del repositorio de Movimientos usando Entity Framework Core.
    /// </summary>
    public class MovimientoRepository : Repository<CE_Movimiento>, IMovimientoRepository
    {
        public MovimientoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<CE_Movimiento>> GetByArticuloAsync(int idArticulo)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.IdArticulo == idArticulo)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Movimiento>> GetByUsuarioAsync(int idUsuario)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.IdUsuario == idUsuario)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Movimiento>> GetByTipoAsync(string tipoMovimiento)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.TipoMovimiento == tipoMovimiento)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Movimiento>> GetByFechaAsync(DateTime desde, DateTime hasta)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.FechaMovimiento >= desde && m.FechaMovimiento <= hasta)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Movimiento>> GetConDetallesAsync()
        {
            // Usamos una consulta que trae los datos de navegacion
            // mediante un join manual ya que no tenemos relaciones de navegacion
            var query = from m in _context.Movimientos
                        join p in _context.Productos on m.IdArticulo equals p.IdArticulo into productos
                        from producto in productos.DefaultIfEmpty()
                        join u in _context.Usuarios on m.IdUsuario equals u.IdUsuario into usuarios
                        from usuario in usuarios.DefaultIfEmpty()
                        orderby m.FechaMovimiento descending
                        select new CE_Movimiento
                        {
                            IdMovimiento = m.IdMovimiento,
                            IdArticulo = m.IdArticulo,
                            IdUsuario = m.IdUsuario,
                            TipoMovimiento = m.TipoMovimiento ?? "N/A",
                            Cantidad = m.Cantidad,
                            FechaMovimiento = m.FechaMovimiento,
                            Observacion = m.Observacion ?? "",
                            NombreProducto = producto != null ? producto.Nombre : "N/A",
                            CodigoProducto = producto != null ? producto.Codigo : "N/A",
                            UsuarioResponsable = usuario != null ? (usuario.Nombre ?? "") + " " + (usuario.Apellido ?? "") : "Sistema"
                        };

            return await query.AsNoTracking().ToListAsync();
        }

        public async Task<IEnumerable<CE_Movimiento>> GetByArticuloYFechaAsync(int idArticulo, DateTime desde, DateTime hasta)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.IdArticulo == idArticulo &&
                           m.FechaMovimiento >= desde &&
                           m.FechaMovimiento <= hasta)
                .OrderByDescending(m => m.FechaMovimiento)
                .ToListAsync();
        }

        public async Task<CE_Movimiento> GetUltimoMovimientoAsync(int idArticulo)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(m => m.IdArticulo == idArticulo)
                .OrderByDescending(m => m.FechaMovimiento)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetTotalEntradasAsync(int idArticulo)
        {
            // Asumimos que "ENTRADA" es el tipo para entradas
            return await _dbSet
                .Where(m => m.IdArticulo == idArticulo && m.TipoMovimiento == "ENTRADA")
                .SumAsync(m => m.Cantidad);
        }

        public async Task<decimal> GetTotalSalidasAsync(int idArticulo)
        {
            // Asumimos que "SALIDA" es el tipo para salidas
            return await _dbSet
                .Where(m => m.IdArticulo == idArticulo && m.TipoMovimiento == "SALIDA")
                .SumAsync(m => m.Cantidad);
        }
    }
}
