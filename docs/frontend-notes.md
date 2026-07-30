# Ticket Manager — Frontend Notes

## Cómo ejecutar

```bash
# Development
cd frontend
npm install
npm run dev

# Con Docker
docker-compose up
```

## Estructura de carpetas

```
frontend/
├── src/
│   ├── api/           # Cliente HTTP (ticketsApi)
│   ├── types/         # TypeScript interfaces
│   ├── features/
│   │   └── tickets/   # Componentes del feature
│   │       ├── TicketList.tsx
│   │       ├── TicketDetail.tsx
│   │       └── CreateTicket.tsx
│   ├── App.tsx        # Routing simple (state-based)
│   └── main.tsx       # Entry point
├── Dockerfile
├── package.json
├── tsconfig.json
└── vite.config.ts
```

## Mejoras futuras (próximos 3 sprints)

1. **Caching / React Query** — Implementar react-query para cachear listados, evitar re-fetch innecesarios y optimistic updates en creación/comentarios. Reduciría latencia percibida y carga en API.

2. **Optimistic updates** — Al cambiar estado o agregar comentario, reflejar el cambio en UI inmediatamente (sin esperar response) y hacer rollback si falla. Mejora drásticamente la UX.

3. **Testing + Storybook** — Agregar tests unitarios con Vitest + Testing Library para componentes críticos (validación de formularios, render condicional) y Storybook para desarrollo visual de componentes.

## Decisiones técnicas

- **React puro** sin librería de estado externa — useState/useEffect son suficientes para este alcance. Si creciera, migraría a Zustand o React Query.
- **fetch nativo** en vez de Axios — Para mantener dependencias al mínimo. Un interceptor de error se implementó en el cliente HTTP.
- **Sin CSS framework** — Se usó CSS-in-JS inline para mantener todo autocontenido. En producción usaría Tailwind o MUI.
