using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Capa_Datos.Context;
using Capa_Datos.Interfaces;

namespace Capa_Datos.Repositories
{
    /// <summary>
    /// Implementacion del patron Unit of Work usando Entity Framework Core.
    ///
    /// TECNOLOGIA NUEVA: Patron Unit of Work
    /// - Coordina multiples repositorios en una sola transaccion
    /// - IDbContextTransaction: Permite control explicito de transacciones
    /// - Lazy initialization: Los repositorios se crean solo cuando se necesitan
    /// - IDisposable: Libera recursos automaticamente con "using"
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;
        private bool _disposed = false;

        // Repositorios - Lazy initialization
        private IUsuarioRepository _usuarios;
        private IProductoRepository _productos;
        private IClienteRepository _clientes;
        private IGrupoRepository _grupos;
        private IMovimientoRepository _movimientos;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Repositorios (Lazy Loading)

        /// <summary>
        /// Repositorio de Usuarios.
        /// Se crea la primera vez que se accede (Lazy Loading).
        /// </summary>
        public IUsuarioRepository Usuarios
        {
            get
            {
                if (_usuarios == null)
                    _usuarios = new UsuarioRepository(_context);
                return _usuarios;
            }
        }

        /// <summary>
        /// Repositorio de Productos
        /// </summary>
        public IProductoRepository Productos
        {
            get
            {
                if (_productos == null)
                    _productos = new ProductoRepository(_context);
                return _productos;
            }
        }

        /// <summary>
        /// Repositorio de Clientes
        /// </summary>
        public IClienteRepository Clientes
        {
            get
            {
                if (_clientes == null)
                    _clientes = new ClienteRepository(_context);
                return _clientes;
            }
        }

        /// <summary>
        /// Repositorio de Grupos
        /// </summary>
        public IGrupoRepository Grupos
        {
            get
            {
                if (_grupos == null)
                    _grupos = new GrupoRepository(_context);
                return _grupos;
            }
        }

        /// <summary>
        /// Repositorio de Movimientos
        /// </summary>
        public IMovimientoRepository Movimientos
        {
            get
            {
                if (_movimientos == null)
                    _movimientos = new MovimientoRepository(_context);
                return _movimientos;
            }
        }

        #endregion

        #region Transacciones

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        #endregion

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _transaction?.Dispose();
                    _context.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
