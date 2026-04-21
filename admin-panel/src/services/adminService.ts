import { api } from './api';

export interface TenantDto {
    id: string;
    name: string;
    email: string;
    karmaScore: number;
}


// Описываем тип данных, который возвращает C# (ASP.NET по умолчанию переводит ключи в camelCase)
export interface DashboardStats {
    totalTenants: number;
    averageAiCleanlinessScore: number;
    pendingIncidentsCount: number;
    criticalAlerts: string[];
}

export const adminService = {
    getDashboardStats: async (): Promise<DashboardStats> => {
        const response = await api.get<DashboardStats>('/admin/dashboard');
        return response.data;
    }
    getTenants: async (search?: string, sortByKarmaAsc?: boolean): Promise<TenantDto[]> => {
        // Формируем query-параметры
        const params = new URLSearchParams();
        if (search) params.append('search', search);
        if (sortByKarmaAsc !== undefined) params.append('sortByKarmaAsc', String(sortByKarmaAsc));

        const response = await api.get<TenantDto[]>(`/admin/tenants?${params.toString()}`);
        return response.data;
    }
};

