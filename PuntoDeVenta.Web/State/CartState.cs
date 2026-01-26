using PuntoDeVenta.Web.Models;

namespace PuntoDeVenta.Web.State
{
    public class CartItem
    {
        public int IdArticulo { get; set; }
        public int? IdPresentacion { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string? PresentacionNombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Cantidad { get; set; }
        public int CantidadUnidadesPorPresentacion { get; set; } = 1;
        public decimal StockDisponible { get; set; }
        public decimal Subtotal => PrecioUnitario * Cantidad;
        // Total de unidades que se descontaran del stock
        public decimal UnidadesTotales => Cantidad * CantidadUnidadesPorPresentacion;
        // Clave unica para identificar item (producto + presentacion)
        public string ItemKey => IdPresentacion.HasValue ? $"{IdArticulo}_{IdPresentacion}" : $"{IdArticulo}_0";
    }

    public class CartState
    {
        private readonly List<CartItem> _items = new();

        public event Action? OnChange;

        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

        public ClienteDTO? Cliente { get; private set; }

        public decimal Subtotal => _items.Sum(i => i.Subtotal);

        public decimal Descuento { get; private set; }

        public decimal Total => Subtotal - Descuento;

        public int TotalItems => _items.Count;

        public int TotalUnidades => (int)_items.Sum(i => i.Cantidad);

        public void AddItem(ProductoDTO producto, decimal cantidad = 1)
        {
            AddItem(producto, null, cantidad);
        }

        public void AddItem(ProductoDTO producto, PresentacionDTO? presentacion, decimal cantidad = 1)
        {
            var idPresentacion = presentacion?.IdPresentacion;
            var cantidadUnidades = presentacion?.CantidadUnidades ?? 1;
            var precio = presentacion?.Precio ?? producto.Precio;
            var itemKey = idPresentacion.HasValue ? $"{producto.IdArticulo}_{idPresentacion}" : $"{producto.IdArticulo}_0";

            var existingItem = _items.FirstOrDefault(i => i.ItemKey == itemKey);

            // Calcular unidades totales en carrito para este producto
            var unidadesEnCarrito = _items.Where(i => i.IdArticulo == producto.IdArticulo)
                                         .Sum(i => i.UnidadesTotales);
            var unidadesAdicionales = cantidad * cantidadUnidades;

            if (existingItem != null)
            {
                var nuevasUnidadesTotal = unidadesEnCarrito + unidadesAdicionales - existingItem.UnidadesTotales + (existingItem.Cantidad + cantidad) * cantidadUnidades;
                if (unidadesEnCarrito + unidadesAdicionales <= producto.Cantidad)
                {
                    existingItem.Cantidad += cantidad;
                }
            }
            else
            {
                if (unidadesEnCarrito + unidadesAdicionales <= producto.Cantidad)
                {
                    _items.Add(new CartItem
                    {
                        IdArticulo = producto.IdArticulo,
                        IdPresentacion = idPresentacion,
                        Nombre = producto.Nombre,
                        Codigo = producto.Codigo,
                        PresentacionNombre = presentacion?.Nombre,
                        PrecioUnitario = precio,
                        Cantidad = cantidad,
                        CantidadUnidadesPorPresentacion = cantidadUnidades,
                        StockDisponible = producto.Cantidad
                    });
                }
            }

            NotifyStateChanged();
        }

        public void UpdateQuantity(int idArticulo, decimal cantidad)
        {
            UpdateQuantity(idArticulo, null, cantidad);
        }

        public void UpdateQuantity(int idArticulo, int? idPresentacion, decimal cantidad)
        {
            var itemKey = idPresentacion.HasValue ? $"{idArticulo}_{idPresentacion}" : $"{idArticulo}_0";
            var item = _items.FirstOrDefault(i => i.ItemKey == itemKey);
            if (item != null)
            {
                if (cantidad <= 0)
                {
                    _items.Remove(item);
                }
                else
                {
                    // Verificar stock disponible considerando todas las presentaciones del producto
                    var unidadesOtrosItems = _items.Where(i => i.IdArticulo == idArticulo && i.ItemKey != itemKey)
                                                   .Sum(i => i.UnidadesTotales);
                    var unidadesNecesarias = cantidad * item.CantidadUnidadesPorPresentacion;
                    if (unidadesOtrosItems + unidadesNecesarias <= item.StockDisponible)
                    {
                        item.Cantidad = cantidad;
                    }
                }
            }
            NotifyStateChanged();
        }

        public void RemoveItem(int idArticulo)
        {
            RemoveItem(idArticulo, null);
        }

        public void RemoveItem(int idArticulo, int? idPresentacion)
        {
            var itemKey = idPresentacion.HasValue ? $"{idArticulo}_{idPresentacion}" : $"{idArticulo}_0";
            var item = _items.FirstOrDefault(i => i.ItemKey == itemKey);
            if (item != null)
            {
                _items.Remove(item);
                NotifyStateChanged();
            }
        }

        public void SetCliente(ClienteDTO? cliente)
        {
            Cliente = cliente;
            NotifyStateChanged();
        }

        public void SetDescuento(decimal descuento)
        {
            if (descuento >= 0 && descuento <= Subtotal)
            {
                Descuento = descuento;
                NotifyStateChanged();
            }
        }

        public void Clear()
        {
            _items.Clear();
            Cliente = null;
            Descuento = 0;
            NotifyStateChanged();
        }

        public VentaCreateDTO ToVentaCreateDTO(string formaPago = "E", decimal? montoRecibido = null)
        {
            return new VentaCreateDTO
            {
                IdCliente = Cliente?.IdCliente,
                Descuento = Descuento,
                FormaPago = formaPago,
                MontoRecibido = montoRecibido,
                Detalles = _items.Select(i => new VentaDetalleCreateDTO
                {
                    IdArticulo = i.IdArticulo,
                    IdPresentacion = i.IdPresentacion,
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario,
                    CantidadUnidadesPorPresentacion = i.CantidadUnidadesPorPresentacion
                }).ToList()
            };
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
