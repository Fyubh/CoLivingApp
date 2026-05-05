import { StyleSheet, Text, View } from 'react-native';
import { useSafeAreaInsets } from 'react-native-safe-area-context';
import { ReactNode } from 'react';
import { T, TYPE } from '../theme/tokens';

export function PageHeader({
  title,
  trailing,
}: {
  title: string;
  trailing?: ReactNode;
}) {
  const insets = useSafeAreaInsets();
  return (
    <View
      style={{
        paddingTop: insets.top + 8,
        paddingBottom: 14,
        paddingHorizontal: 20,
        borderBottomColor: T.divider,
        borderBottomWidth: StyleSheet.hairlineWidth,
        backgroundColor: 'rgba(255,255,255,0.55)',
        flexDirection: 'row',
        alignItems: 'flex-end',
        justifyContent: 'space-between',
        gap: 12,
      }}
    >
      <Text style={[TYPE.hero, { color: T.text, flex: 1 }]} numberOfLines={1}>
        {title}
      </Text>
      {trailing}
    </View>
  );
}
