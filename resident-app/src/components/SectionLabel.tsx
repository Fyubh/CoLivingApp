import { Text, View } from 'react-native';
import { ReactNode } from 'react';
import { T, TYPE } from '../theme/tokens';

export function SectionLabel({
  children,
  trailing,
}: {
  children: string;
  trailing?: ReactNode;
}) {
  return (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        justifyContent: 'space-between',
        paddingHorizontal: 6,
        paddingBottom: 8,
        gap: 8,
      }}
    >
      <Text style={[TYPE.capUpper, { color: T.text2, flex: 1 }]} numberOfLines={1}>
        {children}
      </Text>
      {trailing}
    </View>
  );
}
