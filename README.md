# Prueba técnica — API de Gastos

API REST para gestión de usuarios, categorías y gastos, construida con **ASP.NET Core**, **Entity Framework Core** y **SQL Server**.

---

## Requisitos

- **Docker Desktop** (incluye Docker Compose)

> Nota: La forma recomendada de ejecutar/probar esta entrega es con **Docker** (sin necesidad de instalar .NET ni SQL Server localmente).

---

## Ejecutar (API + SQL Server)

En la **raíz** del repositorio:

```bash
docker compose up --build
```

Servicios y puertos:
- API: `http://localhost:5124`
- SQL Server: `localhost:1433` (usuario `sa`)

---

## Probar con Swagger

- Swagger UI:
- Ir a la dirección:
```bash
  http://localhost:5124/swagger
```
- OpenAPI JSON:  
  `http://localhost:5124/swagger/v1/swagger.json`

---

## Autenticación (Fake)

La API usa autenticación simulada mediante el header **`X-User-Id`**.

En Swagger:
1. Click **Authorize**
2. Pegar uno de estos GUID:

- `11111111-1111-1111-1111-111111111111` (Usuario Demo 1)
- `22222222-2222-2222-2222-222222222222` (Usuario Demo 2)

---

## Seed / Base de datos con datos ficticios

En **Development**, al iniciar:
- Se aplican **migraciones**
- Se cargan **usuarios demo** y **datos de prueba** (categorías/gastos) para facilitar la evaluación

---

## Endpoints principales

### Users
- `GET /users/me` — devuelve el usuario actual (según `X-User-Id`)
- `PUT /users/me` — actualiza datos del usuario actual

### Categories
- `GET /categories` — lista categorías del usuario actual
- `POST /categories` — crea categoría
- `PUT /categories/{id}` — actualiza categoría
- `DELETE /categories/{id}` — elimina categoría  
  - Devuelve **409 Conflict** si la categoría tiene **gastos asociados**
  - Devuelve **403 Forbidden** si la categoría pertenece a otro usuario

### Expenses
- `GET /expenses` — listado paginado + filtros + búsqueda + orden
- `POST /expenses` — crea gasto
- `PUT /expenses/{id}` — actualiza gasto
- `DELETE /expenses/{id}` — elimina gasto

#### Parámetros soportados en `GET /expenses`
- `page` (default 1)
- `pageSize` (default 10)
- `categoryId` (opcional)
- `search` (opcional)
- `sortBy` (`monto` | `fecha`)
- `sortOrder` (`asc` | `desc`)

Búsqueda (`search`):
- Es **case-insensitive** y **accent-insensitive** (por collation / normalización).

---

## Códigos HTTP esperados

- **200** OK (lecturas/actualizaciones)
- **201** Created (creaciones)
- **204** No Content (borrados)
- **400** Bad Request (validación: montos negativos, parámetros inválidos, etc.)
- **401** Unauthorized (sin `X-User-Id`)
- **403** Forbidden (intento de acceder/modificar recursos de otro usuario)
- **404** Not Found (recurso no existe)
- **409** Conflict (p.ej. borrar categoría con gastos)

---

## Conexión a la base (opcional)

Desde SSMS
- Server: `localhost,1433`
- Authentication: SQL Login
- User: `sa`
- Password: `Your_password123`
- Database: `GastosDb`

> El password está en `docker-compose.yml` y es solo para la prueba.

---

## Estructura del repo

- `Gastos.Api/` — API (Controllers, Swagger, Middleware)
- `Gastos.Application/` — DTOs / lógica de aplicación
- `Gastos.Domain/` — entidades y reglas de dominio
- `Gastos.Infrastructure/` — DbContext, EF Core, persistencia, seed
- `docker-compose.yml` — levanta SQL Server + API
- `Gastos.Api/Dockerfile` — build/run de la API en contenedor

## Decisiones técnicas e inconvenientes encontrados

- **Ejecución con Docker:** Se decidió ejecutar la API y SQL Server con **Docker Compose** para que la persona que tenga que verificar la solución, pueda levantar todo de manera sencilla, sin instalar .NET ni configurar una base local.
- **Bloqueos de seguridad en Windows:** Durante la configuración inicial del ambiente de desarrollo, y las pruebas, el antivirus de Windows llegó a bloquear varias veces o eliminar archivos generados por el build (por ejemplo, `.dll`/`.exe`), impidiendo ejecutar la API localmente. Esta fue la parte más frustrante. Así que se priorizó la ejecución en Docker para saltarnos esos problemas de seguridad.
- **Versión de .NET:** inicialmente comencé con **.NET 10 (preview)**, la versión más actual, pero luego decidi cambiar a **.NET 8 (LTS)** debido a incompatibilidades/conflictos de dependencias con algunas librerías, principalmente las relacionadas con OpenAPI/Swagger. La versión final es **.NET 8** por estabilidad y compatibilidad.
