namespace PuntoDeVenta.Web.Services
{
    /// <summary>
    /// Servicio para manejar el estado del sidebar (colapsado/expandido)
    /// </summary>
    public class SidebarState
    {
        private bool _isCollapsed = false;

        public bool IsCollapsed
        {
            get => _isCollapsed;
            set
            {
                if (_isCollapsed != value)
                {
                    _isCollapsed = value;
                    OnChange?.Invoke();
                }
            }
        }

        public event Action? OnChange;

        public void ToggleSidebar()
        {
            IsCollapsed = !IsCollapsed;
        }
    }
}
