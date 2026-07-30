export interface Ticket {
  id: string;
  title: string;
  description: string;
  priority: string;
  status: string;
  createdAt: string;
  updatedAt: string;
  createdBy: string;
  commentCount: number;
}

export interface TicketListItem {
  id: string;
  title: string;
  priority: string;
  status: string;
  createdAt: string;
  createdBy: string;
  commentCount: number;
}

export interface CreateTicketDto {
  title: string;
  description: string;
  priority: string;
  createdBy: string;
}

export interface UpdateTicketDto {
  title: string;
  description: string;
  priority: string;
}

export interface UpdateStatusDto {
  status: string;
}

export interface Comment {
  id: string;
  text: string;
  createdAt: string;
  createdBy: string;
}

export interface CreateCommentDto {
  text: string;
  createdBy: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  error: string;
  details?: { propertyName: string; errorMessage: string }[];
}
