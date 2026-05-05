import { Text, View } from 'react-native';
import { T } from '../theme/tokens';

export function Avatar({
  initial,
  color = T.accent,
  size = 36,
}: {
  initial: string;
  color?: string;
  size?: number;
}) {
  return (
    <View
      style={{
        width: size,
        height: size,
        borderRadius: size / 2,
        backgroundColor: color,
        alignItems: 'center',
        justifyContent: 'center',
      }}
    >
      <Text style={{ color: '#fff', fontWeight: '700', fontSize: Math.round(size * 0.42) }}>
        {(initial || '?').charAt(0).toUpperCase()}
      </Text>
    </View>
  );
}
