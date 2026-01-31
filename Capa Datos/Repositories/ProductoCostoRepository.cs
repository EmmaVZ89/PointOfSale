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
    /// Implementacion del repositorio de costos históricos de productos.
    /// Solo utilizado por administradores.
    /// </summary>
    public class ProductoCostoRepository : Repository<CE_ProductoCostoHistorico>, IProductoCostoRepository
    {
        public ProductoCostoRepository(ApplicationDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene el último costo registrado para un producto
        /// </summary>
        public async Task<CE_ProductoCostoHistorico> GetUltimoCostoAsync(int idArticulo)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IdArticulo == idArticulo)
                .OrderByDescending(c => c.FechaRegistro)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Obtiene el histórico de costos de un producto ordenado por fecha descendente
        /// </summary>
        public async Task<IEnumerable<CE_ProductoCostoHistorico>> GetHistoricoAsync(int idArticulo)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(c => c.IdArticulo == idArticulo)
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene los últimos costos de múltiples productos (para reportes)
        /// </summary>
        public async Task<Dictionary<int, decimal>> GetUltimosCostosAsync(IEnumerable<int> idArticulos)
        {
            var idsList = idArticulos.ToList();

            // Obtener todos los costos de los artículos solicitados
            var costos = await _dbSet
                .AsNoTracking()
                .Where(c => idsList.Contains(c.IdArticulo))
                .ToListAsync();

            // Agrupar por artículo y obtener el último costo de cada uno
            return costos
                .GroupBy(c => c.IdArticulo)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(c => c.FechaRegistro).First().CostoUnitario
                );
        }

        /// <summary>
        /// Obtiene el costo vigente de un producto a una fecha específica.
        /// Busca el registro de costo más reciente antes o igual a la fecha indicada.
        /// </summary>
        public async Task<decimal?> GetCostoVigenteAsync(int idArticulo, DateTime fecha)
        {
            var costo = await _dbSet
                .AsNoTracking()
                .Where(c => c.IdArticulo == idArticulo && c.FechaRegistro <= fecha)
                .OrderByDescending(c => c.FechaRegistro)
                .FirstOrDefaultAsync();

            return costo?.CostoUnitario;
        }

        /// <summary>
        /// Obtiene los costos vigentes de múltiples productos a una fecha específica.
        /// Para cada producto, busca el costo más reciente antes o igual a la fecha.
        /// </summary>
        public async Task<Dictionary<int, decimal>> GetCostosVigentesAsync(IEnumerable<int> idArticulos, DateTime fecha)
        {
            var idsList = idArticulos.ToList();

            // Obtener todos los costos de los artículos solicitados hasta la fecha
            var costos = await _dbSet
                .AsNoTracking()
                .Where(c => idsList.Contains(c.IdArticulo) && c.FechaRegistro <= fecha)
                .ToListAsync();

            // Agrupar por artículo y obtener el costo vigente (más reciente antes de la fecha)
            return costos
                .GroupBy(c => c.IdArticulo)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(c => c.FechaRegistro).First().CostoUnitario
                );
        }
    }
}
