# Caso de Análisis — Gestión de Incidencias (Logística)

## A) Requerimientos

### Funcionales
1. RF-1: Operadores pueden crear incidencias con datos de entrega fallida.
2. RF-2: Supervisores pueden cambiar estado de incidencias y asignar responsables.
3. RF-3: Sistema registra historial de cambios (auditoría) con who/when/what.
4. RF-4: API REST expuesta para frontend web y consumo de terceros (con API Key).
5. RF-5: Integración (solo lectura) con portal legado Java para obtener datos de entregas.
6. RF-6: Dashboard con incidencias abiertas, atrasadas, top creadores.

### No funcionales
7. RNF-1: Debe soportar picos de 10.000 incidencias/día (~1 cada 8 segundos).
8. RNF-2: Secure-by-default: autenticación JWT, autorización por rol (Operador/Supervisor/Admin).
9. RNF-3: Auditoría inmutable de cambios de estado.
10. RNF-4: Disponibilidad 99.9% en horario laboral.

### Supuestos y preguntas abiertas
1. **Supuesto:** El portal legado expone una API o vista de solo lectura (no se puede modificar).
2. **Supuesto:** Los operadores tienen un dispositivo móvil con conectividad intermitente.
3. **Pregunta:** ¿El volumen de 10k/día es pico sostenido o solo en ciertas horas?
4. **Pregunta:** ¿Se requiere soporte multi-idioma?
5. **Pregunta:** ¿Qué latencia máxima se acepta para la integración con el sistema legado?
6. **Pregunta:** ¿Los terceros que consumen la API están identificados o son anónimos?
7. **Supuesto:** El equipo es de 5-6 personas (2 backend, 2 frontend, 1 QA, 1 PO).

---

## B) Propuesta técnica

### Arquitectura en alto nivel

```
[Web Frontend (React)] ──→ [API Gateway] ──→ [Incidencias API (.NET 8)]
                                 │                    │
[App Móvil / Terceros] ──────────┘                    │
                                                      ↓
                                           [Monolito Modular]
                                         ┌─────────┼─────────┐
                                    [Incidencias] [Auditoría] [Legado Adapter]
                                         │
                                    [SQL Server]  ←→ [Legado Java DB] (read replica)
```

### Capas / Módulos
- **Incidencias API:** Controllers + Application Services + Domain + Infrastructure
- **Módulo de Auditoría:** Event sourcing simple (tabla AuditLog con before/after JSON)
- **Legado Adapter:** Servicio que consulta DB legado (read replica o API) y cachea resultados

### Persistencia y auditoría
- **SQL Server** con esquema normalizado para incidencias.
- **Auditoría:** Tabla `AuditLog` con `EntityType, EntityId, Action, OldValues, NewValues, Timestamp, UserId`.
- **Cache** distribuido (Redis) para datos del legado con TTL de 5 min.

### Integración con sistema legado
- **Read replica** de la DB legado (si es posible) para evitar impacto en sistema productivo.
- **API Gateway** en frente de ambas APIs (nueva y legado) con ruteo por path.
- Si no hay replica, usar **carga batch nocturna** + polling cada 5 min.

### Observabilidad
- **Logs estructurados** con Serilog + sinks a Elasticsearch.
- **Métricas** con Prometheus + Grafana (latencia, throughput, errores por endpoint).
- **Trazas distribuidas** con OpenTelemetry entre API Gateway y backend.

---

## C) Decisiones de Arquitectura (ADR)

### ADR-001: Monolito modular vs Microservicios

**Contexto:** Necesitamos escalar para 10k incidencias/día con un equipo pequeño (5-6 personas).

**Decisión:** Monolito modular con boundaries bien definidos (Incidencias, Auditoría, Legado Adapter).

**Consecuencias:**
- + Despliegue simple (1 artifact), menor latencia interna (in-process).
- + El equipo pequeño no sufre sobrecarga de DevOps de microservicios.
- - Si escala más allá de 50k/día, habrá que extraer módulos a servicios independientes.

### ADR-002: EF Core vs Dapper

**Contexto:** Necesitamos persistencia con auditoría y consultas complejas.

**Decisión:** EF Core para el módulo de Incidencias (CRUD estándar) y Dapper para consultas pesadas (dashboard, reportes).

**Consecuencias:**
- + EF Core da productividad con migraciones, change tracking para auditoría.
- + Dapper da máximo performance en queries específicas.
- - Mantener dos ORMs aumenta la complejidad; se justifica por el mix de workload.

---

## D) Enfoque Ágil — Sprint Plan (2 semanas)

### User Stories

| ID | Historia | Est. | Rol |
|----|----------|------|-----|
| US-01 | Como operador quiero crear una incidencia con datos de entrega para reportar fallas | M | Operador |
| US-02 | Como operador quiero ver mis incidencias creadas y su estado para dar seguimiento | S | Operador |
| US-03 | Como supervisor quiero cambiar el estado de una incidencia para gestionar el flujo de resolución | M | Supervisor |
| US-04 | Como supervisor quiero asignar un responsable a una incidencia para escalar | S | Supervisor |
| US-05 | Como sistema quiero registrar cada cambio de estado en un log inmutable para cumplir con auditoría | L | Sistema |
| US-06 | Como supervisor quiero un dashboard con incidencias abiertas y atrasadas para priorizar | L | Supervisor |
| US-07 | Como operador quiero buscar incidencias por texto para encontrar rápidamente una falla | S | Operador |
| US-08 | Como equipo técnico quiero integrar la lectura de datos del sistema legado para evitar duplicación | L | Técnico |

### Criterios de aceptación (US-01 y US-03)

**US-01 — Crear incidencia:**
- [ ] Formulario con campos: Tipo de falla, Fecha entrega, Código envío, Descripción.
- [ ] Validación: todos los campos requeridos, descripción >= 10 caracteres.
- [ ] Al guardar, incidencia queda en estado "Abierta" con fecha/hora y usuario autenticado.
- [ ] Si el servicio de integración legado está caído, se permite crear sin datos de envío (modo offline).

**US-03 — Cambiar estado:**
- [ ] Solo transiciones válidas: Abierta → En Proceso, En Proceso → Resuelta, Resuelta → Cerrada.
- [ ] Al cambiar a "Resuelta" se requiere comentario obligatorio.
- [ ] Queda registrado en AuditLog: usuario, fecha, estado anterior, estado nuevo.
- [ ] Si la incidencia está "Cerrada", no se puede cambiar a otro estado.

### Estimación relativa

- **S (Small):** US-02, US-04, US-07 → 1-2 días
- **M (Medium):** US-01, US-03 → 3-4 días
- **L (Large):** US-05, US-06, US-08 → 5-7 días

### Riesgos

1. **Disponibilidad del legado Java** — Si su DB está caída, la creación de incidencias no debe bloquearse. *Mitigación:* cache con datos del día anterior y modo offline.
2. **Volumen pico** — 10k/día puede saturar la DB si no hay índices adecuados. *Mitigación:* índices cubrientes y paginación obligatoria. Monitorear con alertas.
3. **Curva de aprendizaje del equipo** — Si el equipo no conoce .NET 8 / React, el sprint 1 será más lento. *Mitigación:* pair programming los primeros 3 días y tener un spike de arquitectura.
