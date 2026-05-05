import { Pressable, Text, View } from 'react-native';
import { ReactNode } from 'react';
import { T, TYPE } from '../theme/tokens';
import { Icon } from './Icon';
import { Divider } from './Divider';

export function ListRow({
  icon,
  iconBg,
  title,
  subtitle,
  trailing,
  chevron = false,
  danger = false,
  isLast = false,
  onPress,
}: {
  icon?: ReactNode;
  iconBg?: string;
  title: string;
  subtitle?: string;
  trailing?: ReactNode | string;
  chevron?: boolean;
  danger?: boolean;
  isLast?: boolean;
  onPress?: () => void;
}) {
  const Inner = (
    <View
      style={{
        flexDirection: 'row',
        alignItems: 'center',
        gap: 12,
        paddingHorizontal: 16,
        paddingVertical: 14,
      }}
    >
      {icon && (
        <View
          style={{
            width: 36,
            height: 36,
            borderRadius: 10,
            backgroundColor: iconBg ?? 'rgba(20,24,40,0.05)',
            alignItems: 'center',
            justifyContent: 'center',
          }}
        >
          {icon}
        </View>
      )}
      <View style={{ flex: 1 }}>
        <Text style={[TYPE.bodyMed, { color: danger ? T.danger : T.text }]}>{title}</Text>
        {subtitle && (
          <Text style={[TYPE.footnote, { color: T.text2, marginTop: 2 }]}>{subtitle}</Text>
        )}
      </View>
      {typeof trailing === 'string' ? (
        <Text style={[TYPE.body, { color: T.text2 }]}>{trailing}</Text>
      ) : (
        trailing
      )}
      {chevron && <Icon name="chevron-right" size={16} color={T.text3} />}
    </View>
  );

  return (
    <View>
      {onPress ? <Pressable onPress={onPress}>{Inner}</Pressable> : Inner}
      {!isLast && <Divider inset={icon ? 64 : 16} />}
    </View>
  );
}
