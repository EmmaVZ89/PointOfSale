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
        private IVentaRepository _ventas;
        private IVentaDetalleRepository _ventaDetalles;
        private IPresentacionRepository _presentaciones;
        private IProductoCostoRepository _productoCostos;

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

        /// <summary>
        /// Repositorio de Ventas
        /// </summary>
        public IVentaRepository Ventas
        {
            get
            {
                if (_ventas == null)
                    _ventas = new VentaRepository(_context);
                return _ventas;
            }
        }

        /// <summary>
        /// Repositorio de Detalles de Venta
        /// </summary>
        public IVentaDetalleRepository VentaDetalles
        {
            get
            {
                if (_ventaDetalles == null)
                    _ventaDetalles = new VentaDetalleRepository(_context);
                return _ventaDetalles;
            }
        }

        /// <summary>
        /// Repositorio de Presentaciones de Producto
        /// </summary>
        public IPresentacionRepository Presentaciones
        {
            get
            {
                if (_presentaciones == null)
                    _presentaciones = new PresentacionRepository(_context);
                return _presentaciones;
            }
        }

        /// <summary>
        /// Repositorio de Costos Históricos de Producto
        /// </summary>
        public IProductoCostoRepository ProductoCostos
        {
            get
            {
                if (_productoCostos == null)
                    _productoCostos = new ProductoCostoRepository(_context);
                return _productoCostos;
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
