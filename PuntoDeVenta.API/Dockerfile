# =========================================
# Dockerfile para PuntoDeVenta.API
# Multi-stage build para produccion
# =========================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto primero (mejor cache)
COPY ["PuntoDeVenta.API/PuntoDeVenta.API.csproj", "PuntoDeVenta.API/"]
COPY ["Capa Datos/Capa Datos.csproj", "Capa Datos/"]
COPY ["Capa Entidad/Capa Entidad.csproj", "Capa Entidad/"]
COPY ["Capa Negocio/Capa Negocio.csproj", "Capa Negocio/"]

# Restaurar dependencias
RUN dotnet restore "PuntoDeVenta.API/PuntoDeVenta.API.csproj"

# Copiar todo el codigo fuente
COPY . .

# Build en modo Release
WORKDIR "/src/PuntoDeVenta.API"
RUN dotnet build "PuntoDeVenta.API.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "PuntoDeVenta.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Instalar dependencias nativas para QuestPDF/SkiaSharp
RUN apt-get update && apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libfreetype6 \
    libx11-6 \
    libxext6 \
    libxrender1 \
    libc6 \
    libgdiplus \
    && rm -rf /var/lib/apt/lists/*

# Crear usuario non-root para seguridad (OWASP)
RUN groupadd -r appgroup && useradd -r -g appgroup appuser

# Copiar archivos publicados
COPY --from=publish /app/publish .

# Cambiar ownership de archivos
RUN chown -R appuser:appgroup /app

# Cambiar a usuario non-root
USER appuser

# Puerto estandar Railway
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Ejecutar la aplicacion
ENTRYPOINT ["dotnet", "PuntoDeVenta.API.dll"]
