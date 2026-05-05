import { BottomTabBarProps } from '@react-navigation/bottom-tabs';
import { BlurView } from 'expo-blur';
import { Platform, Pressable, StyleSheet, Text, View } from 'react-native';
import { T, TYPE } from '../theme/tokens';

export function TabBar({ state, descriptors, navigation, insets }: BottomTabBarProps) {
  return (
    <View
      style={{
        position: 'absolute',
        left: 12,
        right: 12,
        bottom: 12 + insets.bottom,
        height: 70,
        borderRadius: 36,
        backgroundColor: 'rgba(255,255,255,0.78)',
        borderColor: T.glassBorder,
        borderWidth: StyleSheet.hairlineWidth,
        overflow: 'hidden',
        flexDirection: 'row',
        alignItems: 'center',
        paddingHorizontal: 6,
        shadowColor: '#141828',
        shadowOffset: { width: 0, height: 12 },
        shadowOpacity: 0.1,
        shadowRadius: 28,
        elevation: 12,
      }}
    >
      {Platform.OS === 'ios' && (
        <BlurView intensity={28} tint="light" style={StyleSheet.absoluteFill} />
      )}
      {state.routes.map((route, index) => {
        const { options } = descriptors[route.key];
        const isFocused = state.index === index;
        const tint = isFocused ? T.accent : T.text2;

        const onPress = () => {
          const event = navigation.emit({
            type: 'tabPress',
            target: route.key,
            canPreventDefault: true,
          });
          if (!isFocused && !event.defaultPrevented) {
            navigation.navigate(route.name as never);
          }
        };

        const labelRaw = options.tabBarLabel ?? options.title ?? route.name;
        const label = typeof labelRaw === 'string' ? labelRaw : route.name;
        const TabIcon = options.tabBarIcon;

        return (
          <Pressable
            key={route.key}
            onPress={onPress}
            style={{
              flex: 1,
              alignItems: 'center',
              justifyContent: 'center',
              height: '100%',
              gap: 3,
            }}
          >
            {TabIcon ? TabIcon({ focused: isFocused, color: tint, size: 24 }) : null}
            <Text
              style={[
                TYPE.caption,
                { color: tint, fontWeight: isFocused ? '700' : '500' },
              ]}
            >
              {label}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}
