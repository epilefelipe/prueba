import { useState } from 'react';
import TicketList from './features/tickets/TicketList';
import TicketDetail from './features/tickets/TicketDetail';
import CreateTicket from './features/tickets/CreateTicket';

type View =
  | { page: 'list' }
  | { page: 'detail'; ticketId: string }
  | { page: 'create' };

export default function App() {
  const [view, setView] = useState<View>({ page: 'list' });

  return (
    <div style={{ fontFamily: 'system-ui, sans-serif', maxWidth: '1200px', margin: '0 auto' }}>
      <style>{`
        @media (max-width: 600px) {
          .filters { flex-direction: column; }
          .filters select, .filters input { width: 100%; min-width: 0; }
          .ticket-table thead { display: none; }
          .ticket-table tr { display: block; margin-bottom: 0.5rem; border: 1px solid #ddd; border-radius: 4px; padding: 0.5rem; }
          .ticket-table td { display: block; border: none; padding: 0.25rem 0; }
          .ticket-table td::before { content: attr(data-label); font-weight: bold; display: inline-block; width: 100px; }
          .meta { flex-direction: column; align-items: flex-start; }
          .status-flow { flex-direction: column; gap: 0.25rem; }
          .pagination { flex-direction: column; gap: 0.5rem; }
          .actions { flex-direction: column; }
          .actions button { width: 100%; }
        }
      `}</style>
      <h1 style={{ padding: '1rem 1rem 0' }}>Gestión de Tickets</h1>

      {view.page === 'list' && (
        <TicketList
          onSelect={id => setView({ page: 'detail', ticketId: id })}
          onCreate={() => setView({ page: 'create' })}
        />
      )}
      {view.page === 'detail' && (
        <TicketDetail
          ticketId={view.ticketId}
          onBack={() => setView({ page: 'list' })}
        />
      )}
      {view.page === 'create' && (
        <CreateTicket
          onCreated={() => setView({ page: 'list' })}
          onCancel={() => setView({ page: 'list' })}
        />
      )}
    </div>
  );
}
