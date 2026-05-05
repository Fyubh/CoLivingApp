import type { TextStyle, ViewStyle } from 'react-native';

export const T = {
  bg: '#F2F4F8',
  surface: '#FFFFFF',
  surfaceAlt: '#F7F8FB',
  surfaceGroup: 'rgba(118,124,140,0.08)',

  glassFill: 'rgba(255,255,255,0.62)',
  glassFillStrong: 'rgba(255,255,255,0.78)',
  glassBorder: 'rgba(255,255,255,0.7)',

  text: '#0E1220',
  text2: '#5A6178',
  text3: '#8C92A6',
  textInverse: '#FFFFFF',

  accent: '#1F6BFF',
  accentPressed: '#1655CC',
  accentSoft: '#E5EEFF',
  accentSoftStrong: '#D4E2FF',

  violet: '#7D6BFF',
  violetSoft: '#EAE6FF',

  success: '#22A06B',
  successSoft: '#DCF1E6',
  warning: '#E89A2A',
  warningSoft: '#FCEFD8',
  danger: '#E5484D',
  dangerSoft: '#FBE0E1',
  info: '#1F6BFF',
  infoSoft: '#E5EEFF',

  border: 'rgba(20,24,40,0.08)',
  divider: 'rgba(20,24,40,0.06)',
} as const;

export const RADIUS = {
  chip: 999,
  pill: 999,
  input: 16,
  btn: 18,
  cardSm: 20,
  card: 26,
  sheet: 32,
  fab: 28,
} as const;

export const SPACING = {
  s4: 4,
  s8: 8,
  s12: 12,
  s16: 16,
  s20: 20,
  s24: 24,
  s32: 32,
} as const;

export const TYPE: Record<
  | 'hero'
  | 'display'
  | 'title'
  | 'headline'
  | 'body'
  | 'bodyMed'
  | 'footnote'
  | 'footMed'
  | 'caption'
  | 'capUpper',
  TextStyle
> = {
  hero: { fontSize: 34, lineHeight: 40, fontWeight: '800', letterSpacing: -0.6 },
  display: { fontSize: 28, lineHeight: 34, fontWeight: '800', letterSpacing: -0.5 },
  title: { fontSize: 22, lineHeight: 28, fontWeight: '700', letterSpacing: -0.3 },
  headline: { fontSize: 17, lineHeight: 22, fontWeight: '600', letterSpacing: -0.2 },
  body: { fontSize: 15, lineHeight: 20, fontWeight: '400', letterSpacing: -0.1 },
  bodyMed: { fontSize: 15, lineHeight: 20, fontWeight: '600', letterSpacing: -0.1 },
  footnote: { fontSize: 13, lineHeight: 18, fontWeight: '400', letterSpacing: 0 },
  footMed: { fontSize: 13, lineHeight: 18, fontWeight: '600', letterSpacing: 0 },
  caption: { fontSize: 11, lineHeight: 14, fontWeight: '600', letterSpacing: 0.1 },
  capUpper: {
    fontSize: 11,
    lineHeight: 14,
    fontWeight: '700',
    letterSpacing: 0.7,
    textTransform: 'uppercase',
  },
};

export const SHADOW: Record<'card' | 'raised' | 'fab' | 'doorCard', ViewStyle> = {
  card: {
    shadowColor: '#141828',
    shadowOffset: { width: 0, height: 8 },
    shadowOpacity: 0.06,
    shadowRadius: 24,
    elevation: 4,
  },
  raised: {
    shadowColor: '#141828',
    shadowOffset: { width: 0, height: 16 },
    shadowOpacity: 0.1,
    shadowRadius: 40,
    elevation: 8,
  },
  fab: {
    shadowColor: '#1F6BFF',
    shadowOffset: { width: 0, height: 10 },
    shadowOpacity: 0.32,
    shadowRadius: 24,
    elevation: 12,
  },
  doorCard: {
    shadowColor: '#5656C8',
    shadowOffset: { width: 0, height: 18 },
    shadowOpacity: 0.3,
    shadowRadius: 40,
    elevation: 16,
  },
};

export const GRADIENTS = {
  bg: ['#EAF1F8', '#F0EAF6', '#FBF0EA'] as const,
  door: ['#4F7BFF', '#6E6BFF', '#B884E0'] as const,
};
