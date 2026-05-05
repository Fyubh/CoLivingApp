import { StyleSheet, View } from 'react-native';
import { T } from '../theme/tokens';

export function Divider({ inset = 0 }: { inset?: number }) {
  return (
    <View
      style={{ height: StyleSheet.hairlineWidth, backgroundColor: T.divider, marginLeft: inset }}
    />
  );
}
