import { StyleSheet, View } from 'react-native';
import { LinearGradient } from 'expo-linear-gradient';
import { ReactNode } from 'react';
import { GRADIENTS, T } from '../theme/tokens';

export function GradientBackground({ children }: { children: ReactNode }) {
  return (
    <View style={{ flex: 1, backgroundColor: T.bg }}>
      <LinearGradient colors={[...GRADIENTS.bg]} style={StyleSheet.absoluteFill} />
      {children}
    </View>
  );
}
