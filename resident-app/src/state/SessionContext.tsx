import { createContext, ReactNode, useCallback, useContext, useEffect, useState } from 'react';
import { api } from '../api/client';
import { sessionStorage } from '../storage/sessionStorage';

type SessionValue = {
  booting: boolean;
  token: string | null;
  mustChangePassword: boolean;
  busy: boolean;
  error: string | null;
  login: (email: string, password: string) => Promise<boolean>;
  changePassword: (oldPassword: string, newPassword: string) => Promise<boolean>;
  logout: () => Promise<void>;
  clearError: () => void;
};

const SessionContext = createContext<SessionValue | null>(null);

export function SessionProvider({ children }: { children: ReactNode }) {
  const [booting, setBooting] = useState(true);
  const [busy, setBusy] = useState(false);
  const [token, setToken] = useState<string | null>(null);
  const [mustChangePassword, setMustChangePassword] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    (async () => {
      const stored = await sessionStorage.getToken();
      if (stored) setToken(stored);
      setBooting(false);
    })();
  }, []);

  const clearError = useCallback(() => setError(null), []);

  async function login(email: string, password: string): Promise<boolean> {
    setBusy(true);
    setError(null);
    try {
      const result = await api.login(email, password);
      setToken(result.token);
      setMustChangePassword(result.mustChangePassword);
      if (!result.mustChangePassword) {
        await sessionStorage.setToken(result.token);
      }
      return true;
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось войти');
      return false;
    } finally {
      setBusy(false);
    }
  }

  async function changePassword(oldPassword: string, newPassword: string): Promise<boolean> {
    if (!token) return false;
    setBusy(true);
    setError(null);
    try {
      const result = await api.changePassword(token, oldPassword, newPassword);
      await sessionStorage.setToken(result.token);
      setToken(result.token);
      setMustChangePassword(false);
      return true;
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Не удалось сменить пароль');
      return false;
    } finally {
      setBusy(false);
    }
  }

  async function logout() {
    await sessionStorage.clearToken();
    setToken(null);
    setMustChangePassword(false);
    setError(null);
  }

  return (
    <SessionContext.Provider
      value={{ booting, token, mustChangePassword, busy, error, login, changePassword, logout, clearError }}
    >
      {children}
    </SessionContext.Provider>
  );
}

export function useSession(): SessionValue {
  const ctx = useContext(SessionContext);
  if (!ctx) throw new Error('useSession must be used inside SessionProvider');
  return ctx;
}
