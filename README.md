# Ticket Manager — Fullstack Exam

API REST para administración de Tickets de Soporte (helpdesk) con .NET 8 (backend), React (frontend) y SQL Server (DB).

## Requisitos

- **Docker** (recomendado) — para correr backend y frontend
- O alternativamente:
  - .NET 8 SDK (`dotnet --version`)
  - Node.js 20+ (`node --version`)

## Cómo ejecutar

### Con Docker (recomendado)

```bash
docker-compose up --build
```

- Backend: http://localhost:8080/api/tickets
- Frontend: http://localhost:5173
- Swagger: http://localhost:8080/swagger

### Sin Docker

**Backend:**
```bash
cd backend
dotnet restore
dotnet run
```

**Frontend:**
```bash
cd frontend
npm install
npm run dev
```

## Endpoints principales

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/tickets?status=&priority=&q=&page=&pageSize=` | Lista paginada |
| GET | `/api/tickets/{id}` | Detalle |
| POST | `/api/tickets` | Crear |
| PUT | `/api/tickets/{id}` | Actualizar |
| PATCH | `/api/tickets/{id}/status` | Cambiar estado |
| GET | `/api/tickets/{id}/comments` | Comentarios |
| POST | `/api/tickets/{id}/comments` | Agregar comentario |

### Ejemplo de request

```bash
# Crear ticket
curl -X POST http://localhost:8080/api/tickets \
  -H "Content-Type: application/json" \
  -d '{"title":"Mi ticket de prueba","description":"Descripción del problema","priority":"High","createdBy":"user@example.com"}'

# Listar tickets
curl "http://localhost:8080/api/tickets?status=Open&page=1&pageSize=10"

# Cambiar estado
curl -X PATCH http://localhost:8080/api/tickets/{id}/status \
  -H "Content-Type: application/json" \
  -d '{"status":"InProgress"}'
```

## Estructura del proyecto

```
/
├── backend/          # API .NET 8 (clean architecture)
│   ├── Domain/       # Entidades, Enums, reglas
│   ├── Application/  # DTOs, Services, Validators
│   ├── Infrastructure/# EF Core, Repositories
│   └── API/          # Controllers, Middleware
├── frontend/         # React + TypeScript + Vite
│   └── src/
│       ├── api/      # Cliente HTTP
│       ├── types/    # Interfaces TS
│       └── features/tickets/  # Componentes
├── db/               # Scripts SQL
└── docs/             # Documentación
```

## DB

Los scripts SQL están en `/db/`:
- `01-ddl.sql` — CREATE TABLE
- `02-queries.sql` — Consultas (paginación, top, búsqueda, atrasados)
- `03-performance.md` — Índices, validación, antipatrones
