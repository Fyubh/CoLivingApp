export const API_BASE_URL =
  process.env.EXPO_PUBLIC_API_BASE_URL?.replace(/\/$/, '') ||
  'http://localhost:5130/api';

export const PRIVACY_POLICY_URL =
  process.env.EXPO_PUBLIC_PRIVACY_POLICY_URL ||
  'https://example.com/privacy';

export const SUPPORT_EMAIL =
  process.env.EXPO_PUBLIC_SUPPORT_EMAIL ||
  'support@coliving-os.example';

// Внешнее приложение Ключи (NFC unlock). Карточка двери на Главной
// открывает его через Linking. До релиза подменить на реальный URL App Store /
// universal link после публикации Keys-приложения.
export const KEYS_APP_URL =
  process.env.EXPO_PUBLIC_KEYS_APP_URL ||
  'https://apps.apple.com/';
