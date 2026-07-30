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
      <h1 style={{ padding: '1rem 1rem 0' }}>Ticket Manager</h1>

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
