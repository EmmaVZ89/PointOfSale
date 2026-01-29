# =========================================
# Dockerfile para PuntoDeVenta.Web
# Blazor WASM + nginx para hosting estatico
# =========================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["PuntoDeVenta.Web/PuntoDeVenta.Web.csproj", "PuntoDeVenta.Web/"]

# Restaurar dependencias
RUN dotnet restore "PuntoDeVenta.Web/PuntoDeVenta.Web.csproj"

# Copiar todo el codigo fuente
COPY . .

# Build en modo Release
WORKDIR "/src/PuntoDeVenta.Web"
RUN dotnet build "PuntoDeVenta.Web.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "PuntoDeVenta.Web.csproj" -c Release -o /app/publish

# Stage 3: nginx para hosting estatico
FROM nginx:alpine AS final

# Copiar configuracion nginx con security headers
COPY PuntoDeVenta.Web/nginx.conf /etc/nginx/nginx.conf

# Copiar archivos publicados de Blazor WASM
COPY --from=publish /app/publish/wwwroot /usr/share/nginx/html

# Crear usuario non-root
RUN addgroup -g 1001 -S appgroup && \
    adduser -S -D -H -u 1001 -h /var/cache/nginx -s /sbin/nologin -G appgroup appuser && \
    chown -R appuser:appgroup /var/cache/nginx && \
    chown -R appuser:appgroup /var/log/nginx && \
    chown -R appuser:appgroup /etc/nginx/conf.d && \
    touch /var/run/nginx.pid && \
    chown -R appuser:appgroup /var/run/nginx.pid && \
    chown -R appuser:appgroup /usr/share/nginx/html

# Puerto Railway
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/ || exit 1

# Ejecutar nginx
CMD ["nginx", "-g", "daemon off;"]
