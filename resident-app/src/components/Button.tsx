import { Pressable, StyleSheet, Text, ViewStyle } from 'react-native';
import { ReactNode } from 'react';
import { RADIUS, T, TYPE } from '../theme/tokens';

export type ButtonKind =
  | 'primary'
  | 'secondary'
  | 'glass'
  | 'success'
  | 'destructive'
  | 'ghost';

export type ButtonSize = 'sm' | 'md' | 'lg';

const heights: Record<ButtonSize, number> = { sm: 36, md: 48, lg: 54 };
const fontSizes: Record<ButtonSize, number> = { sm: 14, md: 16, lg: 17 };
const radii: Record<ButtonSize, number> = { sm: 999, md: RADIUS.input, lg: RADIUS.btn };

type Palette = { bg: string; fg: string; border?: string };

const palette: Record<ButtonKind, Palette> = {
  primary: { bg: T.accent, fg: '#fff' },
  secondary: { bg: T.accentSoft, fg: T.accent },
  glass: { bg: T.glassFillStrong, fg: T.text, border: T.glassBorder },
  success: { bg: T.success, fg: '#fff' },
  destructive: { bg: T.danger, fg: '#fff' },
  ghost: { bg: 'transparent', fg: T.accent },
};

export function Button({
  kind = 'primary',
  size = 'md',
  children,
  full,
  leading,
  trailing,
  onPress,
  disabled,
  style,
  textColor,
}: {
  kind?: ButtonKind;
  size?: ButtonSize;
  children: ReactNode;
  full?: boolean;
  leading?: ReactNode;
  trailing?: ReactNode;
  onPress?: () => void;
  disabled?: boolean;
  style?: ViewStyle | ViewStyle[];
  textColor?: string;
}) {
  const p = palette[kind];
  const padding = size === 'sm' ? 14 : 20;
  return (
    <Pressable
      onPress={onPress}
      disabled={disabled}
      style={({ pressed }) => [
        {
          height: heights[size],
          borderRadius: radii[size],
          paddingHorizontal: padding,
          backgroundColor: p.bg,
          borderColor: p.border,
          borderWidth: p.border ? StyleSheet.hairlineWidth : 0,
          flexDirection: 'row',
          alignItems: 'center',
          justifyContent: 'center',
          gap: 8,
          alignSelf: full ? 'stretch' : 'auto',
          opacity: disabled ? 0.5 : pressed ? 0.85 : 1,
        },
        kind === 'primary' && {
          shadowColor: T.accent,
          shadowOpacity: 0.28,
          shadowRadius: 6,
          shadowOffset: { width: 0, height: 2 },
          elevation: 2,
        },
        style,
      ]}
    >
      {leading}
      {typeof children === 'string' ? (
        <Text
          style={[
            TYPE.bodyMed,
            {
              fontSize: fontSizes[size],
              fontWeight: '700',
              color: textColor ?? p.fg,
              letterSpacing: -0.2,
            },
          ]}
        >
          {children}
        </Text>
      ) : (
        children
      )}
      {trailing}
    </Pressable>
  );
}
