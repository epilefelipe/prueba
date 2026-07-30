import { useState, useEffect } from 'react';
import { ticketsApi } from '../../api/client';
import type { TicketListItem, PagedResult } from '../../types';

const PRIORITIES = ['', 'Low', 'Medium', 'High', 'Critical'];
const STATUSES = ['', 'Open', 'InProgress', 'Resolved', 'Closed'];
const P_LABELS: Record<string, string> = { '': 'Todas', Low: 'Baja', Medium: 'Media', High: 'Alta', Critical: 'Crítica' };
const S_LABELS: Record<string, string> = { '': 'Todos', Open: 'Abierto', InProgress: 'En Progreso', Resolved: 'Resuelto', Closed: 'Cerrado' };

interface Props {
  onSelect: (id: string) => void;
  onCreate: () => void;
}

export default function TicketList({ onSelect, onCreate }: Props) {
  const [data, setData] = useState<PagedResult<TicketListItem> | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [status, setStatus] = useState('');
  const [priority, setPriority] = useState('');
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);

  useEffect(() => {
    setLoading(true);
    setError('');
    ticketsApi.list({ status, priority, q: search, page, pageSize: 10 })
      .then(setData)
      .catch(e => setError(e.error || 'Error al cargar tickets'))
      .finally(() => setLoading(false));
  }, [status, priority, search, page]);

  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h2>Tickets</h2>
        <button onClick={onCreate} style={styles.btn}>+ Nuevo Ticket</button>
      </div>

      <div style={styles.filters}>
        <select value={status} onChange={e => { setStatus(e.target.value); setPage(1); }} style={styles.select}>
          {STATUSES.map(s => <option key={s} value={s}>{S_LABELS[s]}</option>)}
        </select>
        <select value={priority} onChange={e => { setPriority(e.target.value); setPage(1); }} style={styles.select}>
          {PRIORITIES.map(p => <option key={p} value={p}>{P_LABELS[p]}</option>)}
        </select>
        <input
          placeholder="Buscar..."
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1); }}
          style={styles.input}
        />
      </div>

      {loading && <p>Cargando...</p>}
      {error && <p style={styles.error}>{error}</p>}

      {data && (
        <>
          <table style={styles.table}>
            <thead>
              <tr>
                <th>Título</th>
                <th>Prioridad</th>
                <th>Estado</th>
                <th>Creado</th>
                <th>Comentarios</th>
              </tr>
            </thead>
            <tbody>
              {data.items.map(t => (
                <tr key={t.id} onClick={() => onSelect(t.id)} style={styles.row}>
                  <td>{t.title}</td>
                  <td><span style={badge(t.priority)}>{P_LABELS[t.priority]}</span></td>
                  <td>{S_LABELS[t.status]}</td>
                  <td>{new Date(t.createdAt).toLocaleDateString()}</td>
                  <td>{t.commentCount}</td>
                </tr>
              ))}
              {data.items.length === 0 && (
                <tr><td colSpan={5} style={{ textAlign: 'center' }}>No se encontraron tickets</td></tr>
              )}
            </tbody>
          </table>

          <div style={styles.pagination}>
            <button disabled={page <= 1} onClick={() => setPage(p => p - 1)} style={styles.btn}>Anterior</button>
            <span>Página {data.page} de {data.totalPages}</span>
            <button disabled={page >= data.totalPages} onClick={() => setPage(p => p + 1)} style={styles.btn}>Siguiente</button>
          </div>
        </>
      )}
    </div>
  );
}

const badge = (p: string) => ({
  padding: '2px 8px', borderRadius: '4px', fontSize: '0.85rem',
  backgroundColor: p === 'Critical' ? '#dc3545' : p === 'High' ? '#fd7e14' : p === 'Medium' ? '#ffc107' : '#28a745',
  color: '#fff',
});

const styles: Record<string, React.CSSProperties> = {
  container: { padding: '1rem' },
  header: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' },
  filters: { display: 'flex', gap: '0.5rem', marginBottom: '1rem', flexWrap: 'wrap' },
  select: { padding: '0.4rem', border: '1px solid #ccc', borderRadius: '4px' },
  input: { padding: '0.4rem', border: '1px solid #ccc', borderRadius: '4px', flex: 1, minWidth: '200px' },
  table: { width: '100%', borderCollapse: 'collapse' },
  row: { cursor: 'pointer', borderBottom: '1px solid #eee' },
  pagination: { display: 'flex', justifyContent: 'center', alignItems: 'center', gap: '1rem', marginTop: '1rem' },
  btn: { padding: '0.4rem 1rem', background: '#007bff', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  error: { color: '#dc3545' },
};
