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
    /// Implementacion del repositorio de Usuarios usando Entity Framework Core.
    /// </summary>
    public class UsuarioRepository : Repository<CE_Usuarios>, IUsuarioRepository
    {
        public UsuarioRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CE_Usuarios> ValidarCredencialesAsync(string usuario, string contrasena)
        {
            // Nota: La contrasena deberia estar hasheada en la BD
            // y compararse de forma segura (ej: BCrypt)
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Usuario == usuario &&
                    u.Contrasenia == contrasena &&
                    u.Activo);
        }

        public async Task<CE_Usuarios> ValidarPatronAsync(string patron)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Patron == patron && u.Activo);
        }

        public async Task<CE_Usuarios> GetByDniAsync(int dni)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Dni == dni);
        }

        public async Task<CE_Usuarios> GetByUsuarioAsync(string usuario)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Usuario == usuario);
        }

        public async Task<IEnumerable<CE_Usuarios>> GetActivosAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .Where(u => u.Activo)
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Usuarios>> BuscarPorNombreAsync(string termino)
        {
            var terminoLower = termino.ToLower();
            return await _dbSet
                .AsNoTracking()
                .Where(u =>
                    u.Nombre.ToLower().Contains(terminoLower) ||
                    u.Apellido.ToLower().Contains(terminoLower))
                .OrderBy(u => u.Apellido)
                .ThenBy(u => u.Nombre)
                .ToListAsync();
        }

        public async Task CambiarEstadoAsync(int idUsuario, bool activo)
        {
            var usuario = await _dbSet.FindAsync(idUsuario);
            if (usuario != null)
            {
                usuario.Activo = activo;
                _context.Entry(usuario).Property(u => u.Activo).IsModified = true;
            }
        }
    }
}
