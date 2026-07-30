## Parte C — Performance básico

### 1. Índices propuestos

**Índice 1 — Filtrado + ordenamiento de tickets**
```sql
CREATE NONCLUSTERED INDEX IX_Ticket_Status_Priority_CreatedAt
ON Ticket (Status, Priority, CreatedAt DESC)
INCLUDE (Title, Description);
```
**Por qué:** La consulta principal de listado filtra por `Status` y `Priority`, y ordena por `CreatedAt DESC`. Este índice cubre el filtrado, ordenamiento y evita key lookups al incluir las columnas más consultadas.

**Índice 2 — FK en Comment**
```sql
CREATE NONCLUSTERED INDEX IX_Comment_TicketId
ON Comment (TicketId)
INCLUDE (Text, CreatedAt, CreatedBy);
```
**Por qué:** La tabla Comment se consulta siempre por `TicketId` (JOIN con Ticket y listado de comentarios de un ticket). Este índice acelera el JOIN y el COUNT de comentarios, y con INCLUDE evita accesos a la tabla base.

### 2. Cómo validar la mejora

1. **Plan de ejecución:** Ejecutar las consultas con `SET STATISTICS PROFILE ON` o `SET SHOWPLAN_XML ON` antes/después de crear índices. Buscar `Index Scan` vs `Index Seek`.
2. **Estadísticas de IO:** Usar `SET STATISTICS IO ON` y comparar `logical reads` / `scan count`.
3. **Estadísticas de tiempo:** Usar `SET STATISTICS TIME ON` para ver el tiempo de CPU y elapsed.
4. **DMV:** Consultar `sys.dm_db_index_usage_stats` para ver si los índices son usados.

### 3. Antipatrón identificado

**Falta de índice en FK:** La columna `TicketId` en `Comment` no tiene índice por defecto. Cada vez que se hace JOIN entre Ticket y Comment, o se cuentan comentarios, SQL Server debe hacer un `Table Scan` sobre Comment. Esto es un antipatrón clásico: asumir que la FK tiene índice automático.

**Otros antipatrones observables:**
- `SELECT *` en producción (trae columnas innecesarias)
- Funciones en columnas filtradas: `WHERE YEAR(CreatedAt) = 2024` impide usar índice en `CreatedAt`
- Falta de paginación con `OFFSET/FETCH` (usar `ROW_NUMBER` o traer todo)
