import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AdminLayout } from './layouts/AdminLayout';
import { Dashboard } from './features/dashboard/Dashboard';

// Заглушка для страницы жильцов (потом сделаем настоящую)
const TenantsPlaceholder = () => <div className="glass-card p-8 text-center text-ios-muted">Модуль Tenant CRM в разработке...</div>;

function App() {
    return (
        <BrowserRouter>
            <AdminLayout>
                <Routes>
                    <Route path="/" element={<Dashboard />} />
                    <Route path="/tenants" element={<TenantsPlaceholder />} />
                    <Route path="/incidents" element={<div className="glass-card p-8">Модуль Дисциплины (ИИ)</div>} />
                    <Route path="/maintenance" element={<div className="glass-card p-8">Модуль Заявок</div>} />
                </Routes>
            </AdminLayout>
        </BrowserRouter>
    );
}

export default App;