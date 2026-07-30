import { useState, useEffect } from 'react';
import { ticketsApi, X_USER } from '../../api/client';
import type { Ticket, Comment } from '../../types';

const STATUS_FLOW = ['Open', 'InProgress', 'Resolved', 'Closed'];
const S_LABELS: Record<string, string> = { Open: 'Abierto', InProgress: 'En Progreso', Resolved: 'Resuelto', Closed: 'Cerrado' };
const P_LABELS: Record<string, string> = { Low: 'Baja', Medium: 'Media', High: 'Alta', Critical: 'Crítica' };

interface Props {
  ticketId: string;
  onBack: () => void;
}

export default function TicketDetail({ ticketId, onBack }: Props) {
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [comments, setComments] = useState<Comment[]>([]);
  const [newComment, setNewComment] = useState('');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = () => {
    setLoading(true);
    setError('');
    Promise.all([
      ticketsApi.getById(ticketId),
      ticketsApi.getComments(ticketId),
    ])
      .then(([t, c]) => { setTicket(t); setComments(c); })
      .catch(e => setError(e.error || 'Error al cargar el ticket'))
      .finally(() => setLoading(false));
  };

  useEffect(load, [ticketId]);

  const handleAddComment = async () => {
    if (!newComment.trim()) return;
    try {
      await ticketsApi.addComment(ticketId, { text: newComment, createdBy: '' });
      setNewComment('');
      load();
    } catch (e: any) {
      setError(e.error || 'Error al añadir comentario');
    }
  };

  const handleStatusChange = async (status: string) => {
    try {
      await ticketsApi.updateStatus(ticketId, { status });
      load();
    } catch (e: any) {
      setError(e.error || 'Error al actualizar estado');
    }
  };

  if (loading) return <p>Cargando...</p>;
  if (error) return <p style={{ color: '#dc3545' }}>{error}</p>;
  if (!ticket) return <p>Ticket no encontrado</p>;

  const currentIdx = STATUS_FLOW.indexOf(ticket.status);

  return (
    <div style={styles.container}>
      <button onClick={onBack} style={styles.btn}>← Volver</button>

      <h2>{ticket.title}</h2>
      <div className="meta" style={styles.meta}>
        <span style={badge(ticket.priority)}>{P_LABELS[ticket.priority]}</span>
        <span>Estado: {S_LABELS[ticket.status]}</span>
        <span>Creado: {new Date(ticket.createdAt).toLocaleString()}</span>
        <span>Por: {ticket.createdBy}</span>
      </div>
      <p style={styles.desc}>{ticket.description}</p>

      <div className="status-flow" style={styles.statusFlow}>
        {STATUS_FLOW.map((s, i) => (
          <span key={s}>
            <button
              style={{
                ...styles.statusBtn,
                background: i === currentIdx ? '#007bff' : i < currentIdx ? '#28a745' : '#eee',
                color: i <= currentIdx ? '#fff' : '#333',
                cursor: i === currentIdx + 1 ? 'pointer' : 'default',
              }}
              disabled={i !== currentIdx + 1}
              onClick={() => handleStatusChange(s)}
            >
              {S_LABELS[s]}
            </button>
            {i < STATUS_FLOW.length - 1 && <span style={{ margin: '0 4px' }}>→</span>}
          </span>
        ))}
      </div>

      <h3>Comentarios ({comments.length})</h3>
      <div style={styles.comments}>
        {comments.map(c => (
          <div key={c.id} style={styles.comment}>
            <strong>{c.createdBy}</strong> <span style={{ color: '#666', fontSize: '0.85rem' }}>
              {new Date(c.createdAt).toLocaleString()}
            </span>
            <p style={{ margin: '0.25rem 0' }}>{c.text}</p>
          </div>
        ))}
      </div>

      <div style={styles.addComment}>
        <textarea
          value={newComment}
          onChange={e => setNewComment(e.target.value)}
          placeholder="Añadir comentario..."
          rows={3}
          style={styles.textarea}
        />
        <button onClick={handleAddComment} style={styles.btn} disabled={!newComment.trim()}>Enviar</button>
      </div>
    </div>
  );
}

const badge = (p: string) => ({
  padding: '2px 8px', borderRadius: '4px', fontSize: '0.85rem',
  backgroundColor: p === 'Critical' ? '#dc3545' : p === 'High' ? '#fd7e14' : p === 'Medium' ? '#ffc107' : '#28a745',
  color: '#fff', display: 'inline-block',
});

const styles: Record<string, React.CSSProperties> = {
  container: { padding: '1rem', maxWidth: '800px' },
  meta: { display: 'flex', gap: '1rem', alignItems: 'center', margin: '0.5rem 0', flexWrap: 'wrap' },
  desc: { background: '#f9f9f9', padding: '1rem', borderRadius: '4px', margin: '1rem 0' },
  statusFlow: { display: 'flex', alignItems: 'center', gap: '0', margin: '1rem 0', flexWrap: 'wrap' },
  statusBtn: { padding: '0.4rem 0.8rem', border: '1px solid #ccc', borderRadius: '4px', fontSize: '0.85rem' },
  comments: { margin: '1rem 0' },
  comment: { padding: '0.5rem', borderBottom: '1px solid #eee' },
  addComment: { marginTop: '1rem' },
  textarea: { width: '100%', padding: '0.5rem', border: '1px solid #ccc', borderRadius: '4px', boxSizing: 'border-box' },
  btn: { padding: '0.4rem 1rem', background: '#007bff', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer', marginBottom: '1rem' },
};
