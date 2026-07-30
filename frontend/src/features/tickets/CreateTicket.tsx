import { useState } from 'react';
import { ticketsApi } from '../../api/client';

interface Props {
  onCreated: () => void;
  onCancel: () => void;
}

const PRIORITIES = ['Low', 'Medium', 'High', 'Critical'];
const P_LABELS: Record<string, string> = { Low: 'Baja', Medium: 'Media', High: 'Alta', Critical: 'Crítica' };

export default function CreateTicket({ onCreated, onCancel }: Props) {
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const [priority, setPriority] = useState('Medium');
  const [error, setError] = useState('');
  const [sending, setSending] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (title.length < 5 || title.length > 120) { setError('El título debe tener entre 5 y 120 caracteres'); return; }
    if (description.length < 10 || description.length > 2000) { setError('La descripción debe tener entre 10 y 2000 caracteres'); return; }

    setSending(true);
    try {
      await ticketsApi.create({ title, description, priority, createdBy: '' });
      onCreated();
    } catch (e: any) {
      setError(e.error || 'Error al crear ticket');
    } finally {
      setSending(false);
    }
  };

  return (
    <div style={styles.container}>
      <h2>Crear Ticket</h2>
      <form onSubmit={handleSubmit}>
        <div style={styles.field}>
          <label>Título</label>
          <input value={title} onChange={e => setTitle(e.target.value)} style={styles.input} />
        </div>
        <div style={styles.field}>
          <label>Descripción</label>
          <textarea value={description} onChange={e => setDescription(e.target.value)} rows={4} style={styles.textarea} />
        </div>
        <div style={styles.field}>
          <label>Prioridad</label>
          <select value={priority} onChange={e => setPriority(e.target.value)} style={styles.input}>
            {PRIORITIES.map(p => <option key={p} value={p}>{P_LABELS[p]}</option>)}
          </select>
        </div>

        {error && <p style={styles.error}>{error}</p>}

        <div className="actions" style={styles.actions}>
          <button type="submit" disabled={sending} style={styles.btn}>{sending ? 'Creando...' : 'Crear'}</button>
          <button type="button" onClick={onCancel} style={{ ...styles.btn, background: '#6c757d' }}>Cancelar</button>
        </div>
      </form>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  container: { padding: '1rem', maxWidth: '600px' },
  field: { marginBottom: '1rem' },
  input: { width: '100%', padding: '0.4rem', border: '1px solid #ccc', borderRadius: '4px', boxSizing: 'border-box' },
  textarea: { width: '100%', padding: '0.4rem', border: '1px solid #ccc', borderRadius: '4px', boxSizing: 'border-box' },
  actions: { display: 'flex', gap: '0.5rem' },
  btn: { padding: '0.4rem 1rem', background: '#007bff', color: '#fff', border: 'none', borderRadius: '4px', cursor: 'pointer' },
  error: { color: '#dc3545' },
};
