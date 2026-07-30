import type {
  Ticket, TicketListItem, CreateTicketDto, UpdateTicketDto,
  UpdateStatusDto, Comment, CreateCommentDto, PagedResult
} from '../types';

const BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:8080';

export const X_USER = 'user@example.com';

async function request<T>(url: string, options?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE_URL}${url}`, {
    headers: { 'Content-Type': 'application/json', 'X-User': X_USER, ...options?.headers },
    ...options,
  });

  if (!res.ok) {
    const body = await res.json().catch(() => ({ error: res.statusText }));
    throw { status: res.status, ...body };
  }

  if (res.status === 204) return undefined as T;
  return res.json();
}

export const ticketsApi = {
  list(params: {
    status?: string; priority?: string; q?: string; page?: number; pageSize?: number;
  }): Promise<PagedResult<TicketListItem>> {
    const qs = new URLSearchParams();
    if (params.status) qs.set('status', params.status);
    if (params.priority) qs.set('priority', params.priority);
    if (params.q) qs.set('q', params.q);
    if (params.page) qs.set('page', String(params.page));
    if (params.pageSize) qs.set('pageSize', String(params.pageSize));
    return request(`/api/tickets?${qs}`);
  },

  getById(id: string): Promise<Ticket> {
    return request(`/api/tickets/${id}`);
  },

  create(dto: CreateTicketDto): Promise<Ticket> {
    return request('/api/tickets', { method: 'POST', body: JSON.stringify(dto) });
  },

  update(id: string, dto: UpdateTicketDto): Promise<Ticket> {
    return request(`/api/tickets/${id}`, { method: 'PUT', body: JSON.stringify(dto) });
  },

  updateStatus(id: string, dto: UpdateStatusDto): Promise<Ticket> {
    return request(`/api/tickets/${id}/status`, { method: 'PATCH', body: JSON.stringify(dto) });
  },

  getComments(ticketId: string): Promise<Comment[]> {
    return request(`/api/tickets/${ticketId}/comments`);
  },

  addComment(ticketId: string, dto: CreateCommentDto): Promise<Comment> {
    return request(`/api/tickets/${ticketId}/comments`, { method: 'POST', body: JSON.stringify(dto) });
  },
};
