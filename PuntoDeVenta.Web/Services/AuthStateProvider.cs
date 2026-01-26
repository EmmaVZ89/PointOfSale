using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace PuntoDeVenta.Web.Services
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly HttpClient _httpClient;
        private readonly AuthenticationState _anonymous;

        private const string TokenKey = "authToken";

        public AuthStateProvider(ILocalStorageService localStorage, HttpClient httpClient)
        {
            _localStorage = localStorage;
            _httpClient = httpClient;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsStringAsync(TokenKey);

                if (string.IsNullOrWhiteSpace(token))
                {
                    return _anonymous;
                }

                var claims = ParseClaimsFromJwt(token);

                // Verificar si el token ha expirado
                var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
                if (expClaim != null)
                {
                    var expTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaim.Value));
                    if (expTime <= DateTimeOffset.UtcNow)
                    {
                        await _localStorage.RemoveItemAsync(TokenKey);
                        return _anonymous;
                    }
                }

                // Configurar el header de autorizacion
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                return new AuthenticationState(
                    new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
            }
            catch
            {
                return _anonymous;
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            _httpClient.DefaultRequestHeaders.Authorization = null;
            var authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(jwt);

                foreach (var claim in token.Claims)
                {
                    // Mapear claims de JWT a claims de ClaimTypes
                    var claimType = claim.Type switch
                    {
                        "sub" => ClaimTypes.NameIdentifier,
                        "unique_name" => ClaimTypes.Name,
                        "role" => ClaimTypes.Role,
                        "email" => ClaimTypes.Email,
                        _ => claim.Type
                    };

                    claims.Add(new Claim(claimType, claim.Value));
                }
            }
            catch
            {
                // Si falla el parsing, retornar lista vacia
            }

            return claims;
        }
    }
}
