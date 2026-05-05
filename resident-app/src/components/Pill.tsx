import { Text, View } from 'react-native';
import { ReactNode } from 'react';
import { RADIUS, TYPE } from '../theme/tokens';

export function Pill({
  children,
  color,
  soft,
  leading,
}: {
  children: string;
  color: string;
  soft: string;
  leading?: ReactNode;
}) {
  return (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        gap: 6,
        height: 24,
        paddingHorizontal: 10,
        paddingLeft: leading ? 8 : 10,
        borderRadius: RADIUS.pill,
        backgroundColor: soft,
        alignSelf: 'flex-start',
      }}
    >
      {leading}
      <Text style={[TYPE.caption, { color, fontWeight: '700', fontSize: 11 }]}>{children}</Text>
    </View>
  );
}
