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
    /// Repositorio para gestionar presentaciones de productos
    /// </summary>
    public class PresentacionRepository : Repository<CE_ProductoPresentacion>, IPresentacionRepository
    {
        public PresentacionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<CE_ProductoPresentacion>> GetByProductoAsync(int idArticulo)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.IdArticulo == idArticulo)
                .OrderBy(p => p.CantidadUnidades)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_ProductoPresentacion>> GetActivasByProductoAsync(int idArticulo)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.IdArticulo == idArticulo && p.Activo)
                .OrderBy(p => p.CantidadUnidades)
                .ToListAsync();
        }

        public async Task<CE_ProductoPresentacion> GetPresentacionUnitariaAsync(int idArticulo)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdArticulo == idArticulo && p.CantidadUnidades == 1);
        }

        public async Task<bool> ExistePresentacionAsync(int idArticulo, int cantidadUnidades)
        {
            return await _dbSet
                .AsNoTracking()
                .AnyAsync(p => p.IdArticulo == idArticulo && p.CantidadUnidades == cantidadUnidades);
        }

        public async Task<IEnumerable<CE_ProductoPresentacion>> GetAllActivasAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(p => p.Activo)
                .OrderBy(p => p.IdArticulo)
                .ThenBy(p => p.CantidadUnidades)
                .ToListAsync();
        }
    }
}
