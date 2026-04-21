import React, { useEffect, useState } from 'react';
import { adminService, DashboardStats } from '../../services/adminService';

export const Dashboard = () => {
    // Состояния для хранения данных
    const[stats, setStats] = useState<DashboardStats | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    // Скачиваем данные при открытии страницы
    useEffect(() => {
        const fetchStats = async () => {
            try {
                const data = await adminService.getDashboardStats();
                setStats(data);
            } catch (err: any) {
                // Если ловим 401 Unauthorized - значит нет токена
                if (err.response?.status === 401) {
                    setError("Нет доступа. Вы не авторизованы (Нужен токен Admin).");
                } else {
                    setError("Ошибка при загрузке данных с сервера.");
                }
            } finally {
                setIsLoading(false);
            }
        };

        fetchStats();
    },[]);

    if (isLoading) {
        return (
            <div className="flex items-center justify-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-4 border-ios-blue border-t-transparent"></div>
            </div>
        );
    }

    if (error || !stats) {
        return (
            <div className="glass-card bg-red-50/50 p-8 text-center text-ios-red border-red-200">
                <i className="bi bi-x-circle text-4xl mb-2 block"></i>
                <h3 className="font-bold text-lg">{error}</h3>
                <p className="text-sm text-black/50 mt-2">Убедись, что C# бэкенд запущен, а токен добавлен в localStorage.</p>
            </div>
        );
    }

    return (
        <div className="flex flex-col gap-6">

            {/* Секция Алертов */}
            {stats.criticalAlerts.length > 0 && (
                <div className="flex flex-col gap-2">
                    {stats.criticalAlerts.map((alert, idx) => (
                        <div key={idx} className="bg-red-50/80 border-l-4 border-ios-red p-4 rounded-xl flex items-center gap-4 shadow-sm backdrop-blur-md">
                            <i className="bi bi-exclamation-triangle-fill text-ios-red text-xl"></i>
                            <span className="font-semibold text-red-900">{alert}</span>
                        </div>
                    ))}
                </div>
            )}

            {/* Верхние карточки (KPIs) */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6">

                {/* Карточка: Жильцы */}
                <div className="glass-card flex flex-col justify-center">
                    <div className="flex justify-between items-start mb-2">
                        <div className="w-10 h-10 rounded-full bg-ios-blue/10 flex items-center justify-center text-ios-blue">
                            <i className="bi bi-people-fill text-xl"></i>
                        </div>
                    </div>
                    <h3 className="text-ios-muted font-semibold mb-1">Всего резидентов</h3>
                    <p className="text-4xl font-bold">{stats.totalTenants}</p>
                </div>

                {/* Карточка: ИИ Индекс чистоты */}
                <div className="glass-card flex flex-col justify-center relative overflow-hidden">
                    <div className="flex justify-between items-start mb-2">
                        <div className="w-10 h-10 rounded-full bg-ios-green/10 flex items-center justify-center text-ios-green">
                            <i className="bi bi-stars text-xl"></i>
                        </div>
                    </div>
                    <h3 className="text-ios-muted font-semibold mb-1">ИИ Индекс чистоты (Карма)</h3>
                    <div className="flex items-end gap-2">
                        <p className="text-4xl font-bold">{stats.averageAiCleanlinessScore}</p>
                        <span className="text-ios-muted font-semibold mb-1">/ 100</span>
                    </div>
                    <div className="absolute -bottom-6 -right-6 w-24 h-24 bg-ios-green/10 rounded-full blur-xl"></div>
                </div>

                {/* Карточка: Инциденты */}
                <div className="glass-card flex flex-col justify-center">
                    <div className="flex justify-between items-start mb-2">
                        <div className="w-10 h-10 rounded-full bg-ios-orange/10 flex items-center justify-center text-ios-orange">
                            <i className="bi bi-shield-exclamation text-xl"></i>
                        </div>
                        {stats.pendingIncidentsCount > 0 && (
                            <span className="text-ios-red text-sm font-bold bg-ios-red/10 px-2 py-1 rounded-full">Требует внимания</span>
                        )}
                    </div>
                    <h3 className="text-ios-muted font-semibold mb-1">Новые инциденты (ИИ)</h3>
                    <p className="text-4xl font-bold">{stats.pendingIncidentsCount}</p>
                </div>

            </div>

            {/* ... Остальная нижняя часть (Финансы и Карта) остается без изменений, 
          так как там пока статический дизайн ... */}

        </div>
    );
};