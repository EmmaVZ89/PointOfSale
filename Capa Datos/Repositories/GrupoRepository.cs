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
    /// Implementacion del repositorio de Grupos usando Entity Framework Core.
    /// </summary>
    public class GrupoRepository : Repository<CE_Grupos>, IGrupoRepository
    {
        public GrupoRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CE_Grupos> GetByNombreAsync(string nombre)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(g => g.Nombre == nombre);
        }

        public async Task<IEnumerable<CE_Grupos>> BuscarPorNombreAsync(string termino)
        {
            var terminoLower = termino.ToLower();
            return await _dbSet
                .AsNoTracking()
                .Where(g => g.Nombre.ToLower().Contains(terminoLower))
                .OrderBy(g => g.Nombre)
                .ToListAsync();
        }

        public async Task<bool> TieneProductosAsync(int idGrupo)
        {
            return await _context.Productos
                .AnyAsync(p => p.Grupo == idGrupo);
        }

        public async Task<int> ContarProductosAsync(int idGrupo)
        {
            return await _context.Productos
                .CountAsync(p => p.Grupo == idGrupo);
        }
    }
}
