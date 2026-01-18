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
    /// Implementacion del repositorio de Clientes usando Entity Framework Core.
    /// </summary>
    public class ClienteRepository : Repository<CE_Clientes>, IClienteRepository
    {
        public ClienteRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CE_Clientes> GetByDocumentoAsync(string documento)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Documento == documento);
        }

        public async Task<IEnumerable<CE_Clientes>> GetActivosAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.Activo)
                .OrderBy(c => c.RazonSocial)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Clientes>> BuscarPorRazonSocialAsync(string termino)
        {
            var terminoLower = termino.ToLower();
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.RazonSocial.ToLower().Contains(terminoLower))
                .OrderBy(c => c.RazonSocial)
                .ToListAsync();
        }

        public async Task<CE_Clientes> GetByEmailAsync(string email)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task CambiarEstadoAsync(int idCliente, bool activo)
        {
            var cliente = await _dbSet.FindAsync(idCliente);
            if (cliente != null)
            {
                cliente.Activo = activo;
                _context.Entry(cliente).Property(c => c.Activo).IsModified = true;
            }
        }

        public async Task<IEnumerable<CE_Clientes>> GetByFechaAltaAsync(DateTime desde, DateTime hasta)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.FechaAlta >= desde && c.FechaAlta <= hasta)
                .OrderByDescending(c => c.FechaAlta)
                .ToListAsync();
        }
    }
}
