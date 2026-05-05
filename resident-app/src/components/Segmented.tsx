import { Pressable, StyleSheet, Text, View } from 'react-native';
import { T, TYPE } from '../theme/tokens';

export function Segmented<O extends string>({
  options,
  value,
  onChange,
}: {
  options: readonly O[];
  value: O;
  onChange: (v: O) => void;
}) {
  return (
    <View
      style={{
        flexDirection: 'row',
        padding: 4,
        borderRadius: 14,
        backgroundColor: 'rgba(20,24,40,0.05)',
        borderColor: 'rgba(20,24,40,0.04)',
        borderWidth: StyleSheet.hairlineWidth,
      }}
    >
      {options.map((o) => {
        const active = o === value;
        return (
          <Pressable
            key={o}
            onPress={() => onChange(o)}
            style={{
              flex: 1,
              height: 34,
              borderRadius: 10,
              alignItems: 'center',
              justifyContent: 'center',
              backgroundColor: active ? '#fff' : 'transparent',
              shadowColor: '#141828',
              shadowOffset: { width: 0, height: 1 },
              shadowOpacity: active ? 0.08 : 0,
              shadowRadius: 3,
              elevation: active ? 1 : 0,
            }}
          >
            <Text
              style={[
                TYPE.footnote,
                { color: active ? T.text : T.text2, fontWeight: active ? '700' : '500' },
              ]}
            >
              {o}
            </Text>
          </Pressable>
        );
      })}
    </View>
  );
}
