import { ReactNode } from 'react';
import { Link, useLocation } from 'react-router-dom';

interface AdminLayoutProps {
    children: ReactNode;
}

export const AdminLayout = ({ children }: AdminLayoutProps) => {
    const location = useLocation();

    const menuItems = [
        { path: '/', icon: 'bi-grid-1x2-fill', label: 'Command Center' },
        { path: '/tenants', icon: 'bi-people-fill', label: 'Tenant CRM' },
        { path: '/incidents', icon: 'bi-shield-exclamation', label: 'Дисциплина (ИИ)' },
        { path: '/maintenance', icon: 'bi-tools', label: 'Заявки' },
    ];

    return (
        <div className="min-h-screen flex">
            {/* Sidebar - Glassmorphism */}
            <aside className="w-64 fixed h-full left-0 top-0 p-4 z-20">
                <div className="glass-card h-full flex flex-col pt-8 px-4">
                    <div className="flex items-center gap-3 mb-10 px-2">
                        <div className="w-10 h-10 bg-ios-blue rounded-xl flex items-center justify-center text-white font-bold text-xl shadow-ios-btn">
                            C
                        </div>
                        <h1 className="text-xl font-bold tracking-tight">Co-Living OS</h1>
                    </div>

                    <nav className="flex flex-col gap-2">
                        {menuItems.map((item) => {
                            const isActive = location.pathname === item.path;
                            return (
                                <Link
                                    key={item.path}
                                    to={item.path}
                                    className={`flex items-center gap-3 px-4 py-3 rounded-ios-btn font-semibold transition-all ${
                                        isActive
                                            ? 'bg-ios-card-solid text-ios-blue shadow-sm'
                                            : 'text-ios-muted hover:bg-white/40 hover:text-ios-text'
                                    }`}
                                >
                                    <i className={`bi ${item.icon} text-lg`}></i>
                                    {item.label}
                                </Link>
                            );
                        })}
                    </nav>

                    <div className="mt-auto mb-4">
                        <button className="w-full flex items-center gap-3 px-4 py-3 rounded-ios-btn text-ios-red font-semibold hover:bg-red-50 transition-all">
                            <i className="bi bi-box-arrow-left text-lg"></i>
                            Выход
                        </button>
                    </div>
                </div>
            </aside>

            {/* Main Content Area */}
            <main className="ml-64 flex-1 p-8">
                {/* Topbar */}
                <header className="flex justify-between items-center mb-8 glass-card py-4 px-6">
                    <h2 className="text-2xl font-bold">
                        {menuItems.find(m => m.path === location.pathname)?.label || 'Dashboard'}
                    </h2>
                    <div className="flex items-center gap-4">
                        <div className="w-10 h-10 rounded-full bg-ios-card-solid flex items-center justify-center cursor-pointer shadow-sm relative">
                            <i className="bi bi-bell-fill text-ios-muted"></i>
                            <span className="absolute top-0 right-0 w-3 h-3 bg-ios-red rounded-full border-2 border-white"></span>
                        </div>
                        <div className="flex items-center gap-3 bg-ios-card-solid py-1 px-1 pr-4 rounded-full cursor-pointer shadow-sm">
                            <div className="w-8 h-8 rounded-full bg-ios-purple text-white flex items-center justify-center font-bold">
                                А
                            </div>
                            <span className="font-semibold text-sm">Администратор</span>
                        </div>
                    </div>
                </header>

                {/* Сюда будут подставляться страницы (Dashboard, Tenants и т.д.) */}
                <div className="animate-[fadeUp_0.4s_ease-out]">
                    {children}
                </div>
            </main>
        </div>
    );
};