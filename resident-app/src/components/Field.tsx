import { Text, View } from 'react-native';
import { ReactNode } from 'react';
import { T, TYPE } from '../theme/tokens';

export function Field({
  label,
  hint,
  children,
}: {
  label: string;
  hint?: string;
  children: ReactNode;
}) {
  return (
    <View>
      <Text style={[TYPE.footMed, { color: T.text2, marginBottom: 6, paddingLeft: 4 }]}>
        {label}
      </Text>
      {children}
      {hint && (
        <Text style={[TYPE.footnote, { color: T.text2, marginTop: 6, paddingLeft: 4 }]}>
          {hint}
        </Text>
      )}
    </View>
  );
}
