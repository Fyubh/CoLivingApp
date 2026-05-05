import { useState } from 'react';
import { KeyboardTypeOptions, StyleSheet, Text, TextInput, View } from 'react-native';
import { ReactNode } from 'react';
import { RADIUS, T, TYPE } from '../theme/tokens';

export function TextField({
  value,
  onChangeText,
  placeholder,
  leading,
  trailing,
  secureTextEntry,
  autoCapitalize = 'none',
  keyboardType,
  error,
  autoFocus,
}: {
  value: string;
  onChangeText: (v: string) => void;
  placeholder?: string;
  leading?: ReactNode;
  trailing?: ReactNode;
  secureTextEntry?: boolean;
  autoCapitalize?: 'none' | 'sentences' | 'words' | 'characters';
  keyboardType?: KeyboardTypeOptions;
  error?: string | null;
  autoFocus?: boolean;
}) {
  const [focused, setFocused] = useState(false);
  return (
    <View>
      <View
        style={{
          flexDirection: 'row',
          alignItems: 'center',
          gap: 10,
          backgroundColor: '#fff',
          borderColor: error ? T.danger : focused ? T.accent : T.border,
          borderWidth: 1,
          borderRadius: RADIUS.input,
          paddingHorizontal: 14,
          height: 50,
        }}
      >
        {leading && <View>{leading}</View>}
        <TextInput
          value={value}
          onChangeText={onChangeText}
          placeholder={placeholder}
          placeholderTextColor={T.text3}
          secureTextEntry={secureTextEntry}
          autoCapitalize={autoCapitalize}
          keyboardType={keyboardType}
          autoFocus={autoFocus}
          autoCorrect={false}
          onFocus={() => setFocused(true)}
          onBlur={() => setFocused(false)}
          style={{ flex: 1, fontSize: 15, color: T.text, padding: 0 }}
        />
        {trailing && <View>{trailing}</View>}
      </View>
      {error && (
        <Text style={[TYPE.footnote, { color: T.danger, marginTop: 6, paddingLeft: 4 }]}>
          {error}
        </Text>
      )}
    </View>
  );
}
