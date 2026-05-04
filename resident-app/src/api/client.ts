import { API_BASE_URL } from '../config';
import {
  CreateMaintenanceRequest,
  LoginResponse,
  MaintenanceRequestDto,
  MyApartmentContextDto,
  ResidentNotificationDto,
} from './types';

type RequestOptions = {
  method?: 'GET' | 'POST';
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

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    throw new Error(data?.error || `Request failed (${response.status})`);
  }

  return data as T;
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

  getMaintenance: (token: string) =>
    request<MaintenanceRequestDto[]>('/Maintenance/my', { token }),

  createMaintenance: (token: string, payload: CreateMaintenanceRequest) =>
    request<{ maintenanceRequestId: string }>('/Maintenance', {
      method: 'POST',
      token,
      body: {
        reportedByUserId: '',
        ...payload,
      },
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
};
