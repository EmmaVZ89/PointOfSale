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
    /// Implementacion del repositorio de Productos usando Entity Framework Core.
    /// </summary>
    public class ProductoRepository : Repository<CE_Productos>, IProductoRepository
    {
        public ProductoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CE_Productos> GetByCodigoAsync(string codigo)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Codigo == codigo);
        }

        public async Task<IEnumerable<CE_Productos>> GetActivosAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Productos>> GetByGrupoAsync(int idGrupo)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.Grupo == idGrupo && p.Activo)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Productos>> BuscarPorNombreAsync(string termino)
        {
            var terminoLower = termino.ToLower();
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.Nombre.ToLower().Contains(terminoLower))
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Productos>> GetConStockBajoAsync(decimal cantidadMinima)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.Activo && p.Cantidad < cantidadMinima)
                .OrderBy(p => p.Cantidad)
                .ToListAsync();
        }

        public async Task ActualizarStockAsync(int idArticulo, decimal nuevaCantidad)
        {
            var producto = await _dbSet.FindAsync(idArticulo);
            if (producto != null)
            {
                producto.Cantidad = nuevaCantidad;
                _context.Entry(producto).Property(p => p.Cantidad).IsModified = true;
            }
        }

        public async Task CambiarEstadoAsync(int idArticulo, bool activo)
        {
            var producto = await _dbSet.FindAsync(idArticulo);
            if (producto != null)
            {
                producto.Activo = activo;
                _context.Entry(producto).Property(p => p.Activo).IsModified = true;
            }
        }

        public async Task<IEnumerable<CE_Productos>> GetPaginadoAsync(int pagina, int tamanioPagina)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.Activo)
                .OrderBy(p => p.Nombre)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();
        }

        public async Task<int> ContarTotalAsync()
        {
            return await _dbSet.CountAsync(p => p.Activo);
        }
    }
}
