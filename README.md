# Point of Sale (POS) System

Sistema de Punto de Venta completo para pequenas y medianas empresas, desarrollado con arquitectura moderna y mejores practicas de desarrollo.

## Stack Tecnologico

| Capa | Tecnologia |
|------|------------|
| **Frontend Desktop** | WPF (.NET 8) + XAML |
| **Frontend Web** | Blazor WebAssembly (proximamente) |
| **Backend API** | ASP.NET Core 8 Web API |
| **ORM** | Entity Framework Core 8 |
| **Base de Datos** | PostgreSQL (Neon Cloud) |
| **Autenticacion** | JWT Bearer Tokens |
| **Documentacion API** | Swagger / OpenAPI |

## Arquitectura

```
PuntoDeVenta/
├── Capa Entidad/          # Modelos de dominio
├── Capa Datos/            # Acceso a datos (EF Core + Repository Pattern)
│   ├── Context/           # DbContext
│   ├── Interfaces/        # Contratos de repositorios
│   └── Repositories/      # Implementaciones
├── Capa Negocio/          # Logica de negocio
├── PuntoDeVenta/          # Cliente WPF
│   └── Views/             # Vistas XAML
├── PuntoDeVenta.API/      # API REST
│   ├── Controllers/       # Endpoints
│   ├── DTOs/              # Data Transfer Objects
│   └── Auth/              # JWT Authentication
└── docs/                  # Documentacion tecnica
```

## Patrones de Diseno Implementados

- **Repository Pattern**: Abstraccion del acceso a datos
- **Unit of Work**: Coordinacion de transacciones
- **Dependency Injection**: Desacoplamiento de dependencias
- **DTO Pattern**: Transferencia de datos entre capas
- **Clean Architecture**: Separacion de responsabilidades

## Funcionalidades

### Gestion de Usuarios
- CRUD completo con imagenes de perfil
- Autenticacion por usuario/contrasena
- Autenticacion por patron de desbloqueo
- Niveles de privilegio (Admin/Usuario)

### Gestion de Productos
- CRUD con imagenes de producto
- Categorias/Grupos
- Control de stock
- Unidades de medida

### Punto de Venta
- Carrito de compras
- Busqueda por codigo de barras
- Generacion de tickets PDF
- Generacion de presupuestos

### Inventario
- Registro de movimientos (entradas/salidas)
- Historial por producto
- Ajustes de inventario

### Dashboard
- Resumen de ventas
- Graficos con LiveCharts
- Estadisticas en tiempo real

## Requisitos

- .NET 8 SDK
- PostgreSQL (o cuenta en Neon.tech)
- Visual Studio 2022 / VS Code / Rider

## Instalacion

1. Clonar el repositorio:
```bash
git clone https://github.com/EmmaVZ89/PointOfSale.git
cd PointOfSale
```

2. Configurar la base de datos:
```bash
cp appsettings.example.json appsettings.json
# Editar appsettings.json con tus credenciales de PostgreSQL
```

3. Compilar y ejecutar:
```bash
# Cliente WPF
dotnet run --project PuntoDeVenta/Capa\ Presentacion.csproj

# API REST (Swagger en http://localhost:5207/swagger)
dotnet run --project PuntoDeVenta.API/PuntoDeVenta.API.csproj
```

## API Endpoints

| Metodo | Endpoint | Descripcion |
|--------|----------|-------------|
| POST | `/api/auth/login` | Autenticacion |
| GET | `/api/productos` | Listar productos |
| GET | `/api/productos/{id}` | Obtener producto |
| POST | `/api/productos` | Crear producto |
| PUT | `/api/productos/{id}` | Actualizar producto |
| DELETE | `/api/productos/{id}` | Eliminar producto |
| GET | `/api/usuarios` | Listar usuarios |
| GET | `/api/clientes` | Listar clientes |
| GET | `/api/movimientos` | Listar movimientos |

## Documentacion

- [Fase 1: Seguridad y Configuracion](docs/Fase1_TecnologiasNuevas.md)
- [Fase 2: Entity Framework Core y Patrones](docs/Fase2_TecnologiasNuevas.md)
- [Fase 3: API REST y JWT](docs/Fase3_TecnologiasNuevas.md)

## Roadmap

- [x] Fase 1: Seguridad (configuracion externa, .gitignore)
- [x] Fase 2: Migracion a .NET 8 + Entity Framework Core
- [x] Fase 3: API REST con JWT Authentication
- [ ] Fase 4: Frontend Web (Blazor)
- [ ] Fase 5: Testing y DevOps

## Licencia

MIT License

## Autor

**Emmanuel Valdez**
- GitHub: [@EmmaVZ89](https://github.com/EmmaVZ89)
