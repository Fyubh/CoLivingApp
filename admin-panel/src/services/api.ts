import axios from 'axios';

// Замени порт 7000 на тот, на котором запускается твой C# API (посмотри в Swagger)
const API_BASE_URL = 'https://localhost:7000/api';

export const api = axios.create({
    baseURL: API_BASE_URL,
});

// Interceptor: Автоматически добавляет токен во все запросы
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token');
    if (token) {
        config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
});