using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Capa_Datos.Interfaces
{
    /// <summary>
    /// Interfaz generica base para el patron Repository.
    ///
    /// TECNOLOGIA NUEVA: Patron Repository + Generics
    /// - El patron Repository abstrae el acceso a datos
    /// - Permite cambiar la implementacion (EF Core, Dapper, ADO.NET) sin afectar el codigo que lo usa
    /// - Los generics (T) permiten reutilizar la misma interfaz para diferentes entidades
    /// - Expression{Func{T, bool}} permite pasar condiciones LINQ como parametros
    /// </summary>
    /// <typeparam name="T">Tipo de entidad (ej: CE_Usuarios, CE_Productos)</typeparam>
    public interface IRepository<T> where T : class
    {
        #region Operaciones de Lectura

        /// <summary>
        /// Obtiene una entidad por su ID
        /// </summary>
        Task<T> GetByIdAsync(int id);

        /// <summary>
        /// Obtiene todas las entidades
        /// </summary>
        Task<IEnumerable<T>> GetAllAsync();

        /// <summary>
        /// Busca entidades que cumplan una condicion
        /// </summary>
        /// <param name="predicate">Condicion LINQ (ej: x => x.Activo == true)</param>
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Obtiene la primera entidad que cumpla una condicion, o null si no existe
        /// </summary>
        Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Verifica si existe alguna entidad que cumpla la condicion
        /// </summary>
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Cuenta las entidades que cumplen una condicion
        /// </summary>
        Task<int> CountAsync(Expression<Func<T, bool>> predicate = null);

        #endregion

        #region Operaciones de Escritura

        /// <summary>
        /// Agrega una nueva entidad
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Agrega multiples entidades
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Actualiza una entidad existente
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Elimina una entidad
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Elimina una entidad por su ID
        /// </summary>
        Task DeleteByIdAsync(int id);

        #endregion

        #region Unit of Work

        /// <summary>
        /// Guarda todos los cambios pendientes en la base de datos.
        /// Implementa el patron Unit of Work.
        /// </summary>
        Task<int> SaveChangesAsync();

        #endregion
    }
}
