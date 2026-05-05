import { API_BASE_URL } from '../config';
import {
  ChatMessageDto,
  CheckoutRequest,
  ChoreDto,
  CreateChoreRequest,
  CreateExpenseRequest,
  CreateItemRequest,
  CreateMaintenanceRequest,
  ExpenseDto,
  ItemDto,
  ItemStatusInt,
  LoginResponse,
  MaintenanceRequestDto,
  MyApartmentContextDto,
  RecurringExpenseDto,
  ResidentNotificationDto,
  SendMessageRequest,
  SettleDebtRequest,
  SettlementDto,
  UserBalanceDto,
} from './types';

type RequestOptions = {
  method?: 'GET' | 'POST' | 'PATCH' | 'DELETE';
  token?: string | null;
  body?: unknown;
};

async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const headers: Record<string, string> = {
    Accept: 'application/json',
  };

  if (options.body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }

  if (options.token) {
    headers.Authorization = `Bearer ${options.token}`;
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method || 'GET',
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  });

  // Empty body is allowed (204 / no-content responses).
  const text = await response.text();
  const data = text ? safeParse(text) : null;

  if (!response.ok) {
    const err =
      (typeof data === 'object' && data && 'error' in data && (data as { error: string }).error) ||
      `Request failed (${response.status})`;
    throw new Error(err);
  }

  return data as T;
}

function safeParse(text: string): unknown {
  try {
    return JSON.parse(text);
  } catch {
    return null;
  }
}

export const api = {
  login: (email: string, password: string) =>
    request<LoginResponse>('/Auth/login', {
      method: 'POST',
      body: { email, password },
    }),

  changePassword: (token: string, oldPassword: string, newPassword: string) =>
    request<LoginResponse>('/Users/change-password', {
      method: 'POST',
      token,
      body: { oldPassword, newPassword },
    }),

  getMyContext: (token: string) =>
    request<MyApartmentContextDto | null>('/Apartments/my-context', { token }),

  getNotifications: (token: string) =>
    request<ResidentNotificationDto[]>('/Notifications/my?take=20', { token }),

  markNotificationRead: (token: string, id: string) =>
    request<{ notificationId: string }>(`/Notifications/${id}/read`, {
      method: 'POST',
      token,
    }),

  // ─── Maintenance ─────────────────────────────────────────────────────

  getMaintenance: (token: string) =>
    request<MaintenanceRequestDto[]>('/Maintenance/my', { token }),

  createMaintenance: (token: string, payload: CreateMaintenanceRequest) =>
    request<{ maintenanceRequestId: string }>('/Maintenance', {
      method: 'POST',
      token,
      body: { reportedByUserId: '', ...payload },
    }),

  cancelMaintenance: (token: string, id: string, reason?: string) =>
    request<{ maintenanceId: string; status: string }>(`/Maintenance/${id}/cancel`, {
      method: 'POST',
      token,
      body: { reason: reason || null },
    }),

  rateMaintenance: (token: string, id: string, rating: number, feedback?: string) =>
    request<{ maintenanceId: string; status: string }>(`/Maintenance/${id}/rate`, {
      method: 'POST',
      token,
      body: { rating, feedback: feedback || null },
    }),

  // ─── Expenses ────────────────────────────────────────────────────────

  getBalances: (token: string, apartmentId: string) =>
    request<UserBalanceDto[]>(`/Expenses/balance/${apartmentId}`, { token }),

  getExpenses: (token: string, apartmentId: string) =>
    request<ExpenseDto[]>(`/Expenses/${apartmentId}`, { token }),

  getRecurring: (token: string, apartmentId: string) =>
    request<RecurringExpenseDto[]>(`/Expenses/recurring/${apartmentId}`, { token }),

  getSettlements: (token: string, apartmentId: string) =>
    request<SettlementDto[]>(`/Expenses/settlements/${apartmentId}`, { token }),

  createExpense: (token: string, payload: CreateExpenseRequest) =>
    request<{ expenseId: string }>('/Expenses', {
      method: 'POST',
      token,
      body: { payerId: '', ...payload },
    }),

  settleDebt: (token: string, payload: SettleDebtRequest) =>
    request<{ settlementId: string }>('/Expenses/settle', {
      method: 'POST',
      token,
      body: { senderId: '', ...payload },
    }),

  // ─── Inventory ───────────────────────────────────────────────────────

  getItems: (token: string, apartmentId: string, status: ItemStatusInt) =>
    request<ItemDto[]>(`/Inventory/${apartmentId}?status=${status}`, { token }),

  createItem: (token: string, payload: CreateItemRequest) =>
    request<{ itemId: string }>('/Inventory', {
      method: 'POST',
      token,
      body: { userId: '', ...payload },
    }),

  moveItemToCart: (token: string, itemId: string, apartmentId: string) =>
    request<void>('/Inventory/move-to-cart', {
      method: 'POST',
      token,
      body: { itemId, apartmentId, userId: '' },
    }),

  removeItem: (token: string, itemId: string, apartmentId: string) =>
    request<void>(`/Inventory/${itemId}/${apartmentId}`, {
      method: 'DELETE',
      token,
    }),

  checkout: (token: string, payload: CheckoutRequest) =>
    request<{ expenseId: string }>('/Inventory/checkout', {
      method: 'POST',
      token,
      body: { payerId: '', ...payload },
    }),

  consumeItem: (token: string, itemId: string, apartmentId: string) =>
    request<void>('/Inventory/consume', {
      method: 'POST',
      token,
      body: { itemId, apartmentId, userId: '' },
    }),

  // ─── Chores ──────────────────────────────────────────────────────────

  getChores: (token: string, apartmentId: string) =>
    request<ChoreDto[]>(`/Chores/${apartmentId}`, { token }),

  createChore: (token: string, payload: CreateChoreRequest) =>
    request<{ choreId: string }>('/Chores', {
      method: 'POST',
      token,
      body: payload,
    }),

  completeChore: (token: string, choreId: string, apartmentId: string) =>
    request<void>(`/Chores/${choreId}/complete`, {
      method: 'POST',
      token,
      body: { choreId, apartmentId, userId: '' },
    }),

  confirmChore: (token: string, choreId: string, apartmentId: string) =>
    request<void>(`/Chores/${choreId}/confirm`, {
      method: 'POST',
      token,
      body: { choreId, apartmentId, userId: '' },
    }),

  rejectChore: (token: string, choreId: string, apartmentId: string) =>
    request<void>(`/Chores/${choreId}/reject`, {
      method: 'POST',
      token,
      body: { choreId, apartmentId, userId: '' },
    }),

  // ─── Chat ────────────────────────────────────────────────────────────

  getChatHistory: (token: string, apartmentId: string) =>
    request<ChatMessageDto[]>(`/Chat/${apartmentId}`, { token }),

  sendMessage: (token: string, payload: SendMessageRequest) =>
    request<void>('/Chat', {
      method: 'POST',
      token,
      body: payload,
    }),
};
