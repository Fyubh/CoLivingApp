import { Pressable, StyleSheet, View } from 'react-native';
import { T } from '../theme/tokens';
import { Icon, IconName } from './Icon';

export function GlassIconBtn({
  icon,
  badge,
  onPress,
}: {
  icon: IconName;
  badge?: boolean;
  onPress?: () => void;
}) {
  return (
    <Pressable
      onPress={onPress}
      style={({ pressed }) => ({
        width: 40,
        height: 40,
        borderRadius: 20,
        backgroundColor: T.glassFillStrong,
        borderColor: T.glassBorder,
        borderWidth: StyleSheet.hairlineWidth,
        alignItems: 'center',
        justifyContent: 'center',
        opacity: pressed ? 0.85 : 1,
      })}
    >
      <Icon name={icon} size={20} color={T.text} />
      {badge && (
        <View
          style={{
            position: 'absolute',
            top: 4,
            right: 4,
            width: 9,
            height: 9,
            borderRadius: 5,
            backgroundColor: T.danger,
            borderWidth: 2,
            borderColor: 'rgba(255,255,255,0.95)',
          }}
        />
      )}
    </Pressable>
  );
}
