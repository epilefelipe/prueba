# Ticket Manager — Backend Decisions

## Decisiones y Trade-offs

1. **EF Core InMemory vs SQL Server real** — Usamos InMemory para el ejercicio práctico porque no requiere instalación de DB. En producción usaríamos SQL Server con migraciones EF Core. Trade-off: InMemory no valida constraints reales.

2. **Proyecto único con carpetas vs multi-proyecto** — Elegí un solo proyecto .NET con carpetas (Domain/, Application/, Infrastructure/, API/) para simplificar la compilación con Docker. En producción usaría proyectos separados para enforcear dependencias.

3. **FluentValidation** — Se integró FluentValidation para validación declarativa y separada de los modelos. Más mantenible que DataAnnotations para reglas complejas.

4. **InMemory + GUIDs** — Usé GUIDs para Ids porque funcionan bien con InMemory y evitan conflictos en distributed scenarios. En SQL Server preferiría INT con identity por performance.

5. **Middleware global de errores** — Centraliza el manejo de errores (400/404/409/500) en un solo lugar, devolviendo JSON consistente. Evita try/catch en cada controller.

6. **Sin autenticación real** — Se implementó un stub de auth (header X-User) listo para integrar JWT. Decisión consciente para mantener el scope manejable.

7. **PATCH para cambio de estado** — Usamos PATCH semántico para /status en vez de PUT, reflejando que es una actualización parcial. La validación de transiciones está en Domain.

## Endpoints principales

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | /api/tickets | Lista paginada con filtros |
| GET | /api/tickets/{id} | Detalle de ticket |
| POST | /api/tickets | Crear ticket |
| PUT | /api/tickets/{id} | Actualizar ticket |
| PATCH | /api/tickets/{id}/status | Cambiar estado |
| GET | /api/tickets/{id}/comments | Comentarios |
| POST | /api/tickets/{id}/comments | Agregar comentario |
