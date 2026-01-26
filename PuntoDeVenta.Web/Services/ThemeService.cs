using Blazored.LocalStorage;

namespace PuntoDeVenta.Web.Services
{
    public interface IThemeService
    {
        string CurrentTheme { get; }
        event Action? OnThemeChanged;
        Task InitializeAsync();
        Task SetThemeAsync(string theme);
        string[] AvailableThemes { get; }
    }

    public class ThemeService : IThemeService
    {
        private readonly ILocalStorageService _localStorage;
        private const string ThemeKey = "app-theme";
        private string _currentTheme = "default";

        public string CurrentTheme => _currentTheme;

        public string[] AvailableThemes => new[] { "default", "green", "red" };

        public event Action? OnThemeChanged;

        public ThemeService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        public async Task InitializeAsync()
        {
            try
            {
                var savedTheme = await _localStorage.GetItemAsStringAsync(ThemeKey);
                if (!string.IsNullOrEmpty(savedTheme) && AvailableThemes.Contains(savedTheme))
                {
                    _currentTheme = savedTheme;
                }
            }
            catch
            {
                _currentTheme = "default";
            }
        }

        public async Task SetThemeAsync(string theme)
        {
            if (AvailableThemes.Contains(theme))
            {
                _currentTheme = theme;
                await _localStorage.SetItemAsStringAsync(ThemeKey, theme);
                OnThemeChanged?.Invoke();
            }
        }
    }
}
