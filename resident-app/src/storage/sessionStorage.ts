import * as SecureStore from 'expo-secure-store';

const tokenKey = 'resident_jwt';

export const sessionStorage = {
  getToken: () => SecureStore.getItemAsync(tokenKey),
  setToken: (token: string) => SecureStore.setItemAsync(tokenKey, token),
  clearToken: () => SecureStore.deleteItemAsync(tokenKey),
};
