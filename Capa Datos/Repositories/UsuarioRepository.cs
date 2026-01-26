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
            // La BD usa pgcrypto para encriptar passwords.
            // El Patron del usuario se usa como clave de encriptacion.
            // Primero buscamos el usuario para obtener su Patron
            var user = await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Usuario == usuario && u.Activo);

            if (user == null || string.IsNullOrEmpty(user.Patron))
                return null;

            // Validamos usando pgp_sym_decrypt con raw SQL
            var sql = @"SELECT COUNT(*)::int AS ""Value"" FROM ""Usuarios""
                       WHERE ""usuario"" = {0}
                       AND pgp_sym_decrypt(""contrasenia"", {1}) = {2}
                       AND ""Activo"" = true";

            var count = await _context.Database
                .SqlQueryRaw<int>(sql, usuario, user.Patron, contrasena)
                .FirstOrDefaultAsync();

            return count > 0 ? user : null;
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

        public async Task<CE_Usuarios> CreateWithPasswordAsync(CE_Usuarios usuario, string contrasena)
        {
            // Usar SQL raw para insertar con contrasena encriptada usando pgp_sym_encrypt
            var sql = @"
                INSERT INTO ""Usuarios""
                (""Nombre"", ""Apellido"", ""DNI"", ""CUIT"", ""Correo"", ""Telefono"",
                 ""Fecha_Nac"", ""Privilegio"", ""usuario"", ""contrasenia"", ""Patron"", ""Activo"")
                VALUES
                (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8, pgp_sym_encrypt(@p9, @p10), @p10, @p11)
                RETURNING ""IdUsuario""";

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = sql;

            var parameters = new object[]
            {
                usuario.Nombre,
                usuario.Apellido,
                usuario.Dni,
                usuario.Cuit,
                usuario.Correo ?? "",
                usuario.Telefono ?? "",
                usuario.Fecha_Nac,
                usuario.Privilegio,
                usuario.Usuario,
                contrasena,
                usuario.Patron,
                usuario.Activo
            };

            for (int i = 0; i < parameters.Length; i++)
            {
                var param = command.CreateParameter();
                param.ParameterName = $"@p{i}";
                param.Value = parameters[i] ?? DBNull.Value;
                command.Parameters.Add(param);
            }

            await _context.Database.OpenConnectionAsync();
            try
            {
                var result = await command.ExecuteScalarAsync();
                usuario.IdUsuario = Convert.ToInt32(result);
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }

            return usuario;
        }

        public async Task ActualizarContrasenaAsync(int idUsuario, string nuevaContrasena, string patron)
        {
            var sql = @"
                UPDATE ""Usuarios""
                SET ""contrasenia"" = pgp_sym_encrypt({0}, {1})
                WHERE ""IdUsuario"" = {2}";

            await _context.Database.ExecuteSqlRawAsync(sql, nuevaContrasena, patron, idUsuario);
        }
    }
}
