import { Platform, StyleSheet, View, ViewStyle } from 'react-native';
import { BlurView } from 'expo-blur';
import { ReactNode } from 'react';
import { RADIUS, SHADOW, T } from '../theme/tokens';

export function Glass({
  children,
  padding = 16,
  radius = RADIUS.card,
  style,
  strong = false,
  noShadow = false,
}: {
  children?: ReactNode;
  padding?: number;
  radius?: number;
  style?: ViewStyle | ViewStyle[];
  strong?: boolean;
  noShadow?: boolean;
}) {
  return (
    <View
      style={[
        {
          borderRadius: radius,
          backgroundColor: strong ? T.glassFillStrong : T.glassFill,
          borderColor: T.glassBorder,
          borderWidth: StyleSheet.hairlineWidth,
          overflow: 'hidden',
        },
        !noShadow && SHADOW.card,
        style,
      ]}
    >
      {Platform.OS === 'ios' && (
        <BlurView intensity={20} tint="light" style={StyleSheet.absoluteFill} />
      )}
      <View style={{ padding }}>{children}</View>
    </View>
  );
}
