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
    /// Implementacion del repositorio de Ventas usando Entity Framework Core.
    /// </summary>
    public class VentaRepository : Repository<CE_Ventas>, IVentaRepository
    {
        public VentaRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<CE_Ventas?> GetByFacturaAsync(string noFactura)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.No_Factura == noFactura);
        }

        public async Task<IEnumerable<CE_Ventas>> GetByClienteAsync(int idCliente)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(v => v.Id_Cliente == idCliente && !v.Cancelada)
                .OrderByDescending(v => v.Fecha_Venta)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Ventas>> GetByUsuarioAsync(int idUsuario)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(v => v.Id_Usuario == idUsuario && !v.Cancelada)
                .OrderByDescending(v => v.Fecha_Venta)
                .ToListAsync();
        }

        public async Task<IEnumerable<CE_Ventas>> GetByFechaAsync(DateTime fechaDesde, DateTime fechaHasta)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(v => v.Fecha_Venta >= fechaDesde && v.Fecha_Venta <= fechaHasta)
                .OrderByDescending(v => v.Fecha_Venta)
                .ToListAsync();
        }

        public async Task<(IEnumerable<CE_Ventas> Items, int TotalCount)> GetPaginadoAsync(
            int? idCliente,
            int? idUsuario,
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            bool? cancelada,
            int pagina,
            int tamanioPagina)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (idCliente.HasValue)
                query = query.Where(v => v.Id_Cliente == idCliente);

            if (idUsuario.HasValue)
                query = query.Where(v => v.Id_Usuario == idUsuario);

            if (fechaDesde.HasValue)
                query = query.Where(v => v.Fecha_Venta >= fechaDesde);

            if (fechaHasta.HasValue)
                query = query.Where(v => v.Fecha_Venta <= fechaHasta);

            if (cancelada.HasValue)
                query = query.Where(v => v.Cancelada == cancelada);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(v => v.Fecha_Venta)
                .Skip((pagina - 1) * tamanioPagina)
                .Take(tamanioPagina)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task CancelarVentaAsync(int idVenta, int idUsuarioCancelo, string motivo)
        {
            var venta = await _dbSet.FindAsync(idVenta);
            if (venta != null)
            {
                venta.Cancelada = true;
                venta.FechaCancelacion = DateTime.UtcNow;
                venta.IdUsuarioCancelo = idUsuarioCancelo;
                venta.MotivoCancelacion = motivo;
            }
        }

        public string GenerarNumeroFactura()
        {
            // Formato legacy: F-YYMMDDHHmm
            // Usa zona horaria Argentina (UTC-3)
            var argentinaTime = TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

            return $"F-{argentinaTime:yyMMddHHmm}";
        }
    }
}
